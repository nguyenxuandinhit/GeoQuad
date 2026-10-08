using System.Security.Cryptography;
using System.Text;
using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Neo4j.Driver;
using Microsoft.AspNetCore.DataProtection;

namespace GeoQuad.Tests.C_HocTap;

public sealed class LamBaiServiceTests
{
    [Fact]
    public void Token_ChiDungVoiCungBaiChuTheVaKhongQuaHan()
    {
        var keys = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "geoquad-keys-" + Guid.NewGuid().ToString("N")));
        keys.Create();
        var provider = DataProtectionProvider.Create(keys);
        var clock = new TestClock(DateTimeOffset.Parse("2026-10-08T00:00:00Z"));
        var service = new LanLamTokenService(provider, clock);
        var issued = service.Tao("BT-014", null, "abc");

        Assert.True(service.ThuGiaiMa(issued.Token, "BT-014", "guest:abc", out var claim));
        Assert.False(service.ThuGiaiMa(issued.Token, "BT-027", "guest:abc", out _));
        Assert.False(service.ThuGiaiMa(issued.Token, "BT-014", "guest:other", out _));
        Assert.False(service.ThuGiaiMa(issued.Token + "x", "BT-014", "guest:abc", out _));
        clock.Advance(TimeSpan.FromHours(25));
        Assert.False(service.ThuGiaiMa(issued.Token, "BT-014", "guest:abc", out _));
        keys.Delete(recursive: true);
    }

    [Fact]
    public void ReplayHash_KhongPhuThuocKhoangTrangDauCuoi()
    {
        Assert.Equal(LanLamTokenService.Hash(" B ", " cm² "), LanLamTokenService.Hash("B", "cm²"));
        Assert.NotEqual(LanLamTokenService.Hash("A", ""), LanLamTokenService.Hash("B", ""));
    }

    [Fact]
    public async Task KhachLamBai_DuocChamNhungKhongGhiTienDo()
    {
        var service = TaoService(new NguoiGia(null));
        var (bai, token, _) = await service.MoAsync("BT-014", null);
        var result = await service.NopAsync("BT-014", new BaiTapNopInput
        {
            Token = token!.Token, DapAn = "96,0", DonVi = "cm²"
        }, token.GuestNonce);

        Assert.True(result.HopLe);
        Assert.True(result.Dung);
    }

    [Fact]
    public async Task HocSinh_DungTokenChiGhiMotLanVaReplayKhacDapAnBiTuChoi()
    {
        var repo = new FakeRepo();
        var service = TaoService(new NguoiGia("account-8"), repo);
        var (_, token, _) = await service.MoAsync("BT-014", null);
        var first = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, null);
        var replay = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token.Token, DapAn = "95", DonVi = "cm²" }, null);

        Assert.True(first.HopLe);
        Assert.True(replay.XungDot);
        Assert.Single(repo.Attempts);
        Assert.Equal("96", repo.Attempts[0].DapAn);
    }

    [Fact]
    public async Task KetQuaKhach_LaKhachVaCoKienThucLienQuan()
    {
        var service = TaoService(new NguoiGia(null));
        var (_, token, _) = await service.MoAsync("BT-014", null);
        var nop = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, token.GuestNonce);

        var view = await service.KetQuaKhachAsync("BT-014", token.Token, token.GuestNonce, nop.DapAnDaChon, nop.Dung);

        Assert.NotNull(view);
        Assert.True(view.LaKhach);
        Assert.Equal(["CT_THOI_DT"], view.KienThuc.Select(x => x.Ma));
        Assert.Equal(["HINH_THOI"], view.KhaiNiem.Select(x => x.Ma));
        Assert.True(nop.View!.LaKhach);
    }

    [Fact]
    public async Task KetQuaTaiKhoan_SauNopVaDocLai_KhongLaKhachVaCoKienThuc()
    {
        var repo = new FakeRepo();
        var service = TaoService(new NguoiGia("account-8"), repo);
        var (_, token, _) = await service.MoAsync("BT-014", null);
        var nop = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, null);

        var view = await service.KetQuaTaiKhoanAsync(nop.MaLan);

        Assert.NotNull(view);
        Assert.False(view.LaKhach);
        Assert.False(nop.View!.LaKhach);
        Assert.Equal("96", view.DapAnDung);
        Assert.Single(view.KienThuc);
    }

    [Theory]
    [InlineData("TRAC_NGHIEM", "B", null, "B")]
    [InlineData("DAP_AN_SO", null, "96", "96")]
    [InlineData("DAP_AN_SO", null, "7.07", "7.07")]
    [InlineData("DAP_AN_SO", null, null, "")]
    public void DapAnDungDangChuoi_TheoLoai(string loai, string? chu, string? so, string expected)
    {
        decimal? soDecimal = so is null ? null : decimal.Parse(so, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(expected, ChamBai.DapAnDungDangChuoi(loai, chu, soDecimal));
    }

    // Cùng khóa Data Protection cho cả hai người, để token bị từ chối vì sai chủ thể chứ không vì sai khóa.
    [Fact]
    public async Task TokenCuaHocSinhKhac_BiTuChoiVaKhongGhi()
    {
        var repo = new FakeRepo();
        var provider = TaoProvider();
        var (_, token, _) = await TaoService(new NguoiGia("account-A"), repo, provider).MoAsync("BT-014", null);

        var result = await TaoService(new NguoiGia("account-B"), repo, provider).NopAsync("BT-014",
            new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, null);

        Assert.False(result.HopLe);
        Assert.Contains("hết hạn", result.Loi);
        Assert.Empty(repo.Attempts);
    }

    [Fact]
    public async Task BaiBiAnSauKhiMo_KhongChamKhongGhi()
    {
        var repo = new FakeRepo();
        var service = TaoService(new NguoiGia("account-8"), repo);
        var (_, token, _) = await service.MoAsync("BT-014", null);
        repo.AnBai = true;

        var result = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, null);

        Assert.False(result.HopLe);
        Assert.Contains("không còn hiển thị", result.Loi);
        Assert.Empty(repo.Attempts);
    }

    // Repository trả null khi lớp trong DB khác lớp trong cookie hoặc tài khoản đã bị xóa (khóa + kiểm tra lại trong giao dịch).
    [Fact]
    public async Task LopDoiHoacTaiKhoanBiXoa_KhongBaoDaLuu()
    {
        var repo = new FakeRepo { TuChoiGhi = true };
        var service = TaoService(new NguoiGia("account-8"), repo);
        var (_, token, _) = await service.MoAsync("BT-014", null);

        var result = await service.NopAsync("BT-014", new BaiTapNopInput { Token = token!.Token, DapAn = "96", DonVi = "cm²" }, null);

        Assert.False(result.HopLe);
        Assert.Contains("đăng nhập lại", result.Loi);
        Assert.Null(result.View);
    }

    private static IDataProtectionProvider TaoProvider()
    {
        var path = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "geoquad-token-" + Guid.NewGuid().ToString("N")));
        path.Create();
        return DataProtectionProvider.Create(path);
    }

    private static BaiTapLamService TaoService(ICurrentUser user, FakeRepo? repo = null, IDataProtectionProvider? provider = null)
    {
        var tokens = new LanLamTokenService(provider ?? TaoProvider(), TimeProvider.System);
        return new BaiTapLamService(repo ?? new FakeRepo(), user, tokens, TimeProvider.System);
    }

    private sealed class NguoiGia(string? id) : ICurrentUser
    {
        public bool DaDangNhap => id is not null;
        public string? TaiKhoanId => id;
        public string? BietDanh => null;
        public bool LaQuanTri => false;
        public int Lop => 8;
        public bool XemNangCao => false;
        public int LopHienThi => Lop;
    }

    private sealed class FakeRepo : ILamBaiRepository
    {
        public List<BanGhiLuotLam> Attempts { get; } = [];
        public bool AnBai { get; set; }
        public bool TuChoiGhi { get; set; }
        private static readonly BaiTapChiTiet Exercise = new("BT-014", "Tính diện tích.", "DAP_AN_SO", 8, 2,
            [], null, 96m, .01m, "cm²", "Dùng công thức diện tích.", null, null);
        public Task<BaiTapChiTiet?> ChiTietAsync(string ma, int lopHienThi) =>
            Task.FromResult<BaiTapChiTiet?>(!AnBai && ma == Exercise.Ma ? Exercise : null);
        public Task<bool> LaChungMinhCongKhaiAsync(string ma, int lopHienThi) => Task.FromResult(false);
        public Task<string?> BaiTiepTheoKhachAsync(string maHienTai, int lopHienThi) => Task.FromResult<string?>(null);
        public Task<KienThucBaiTap> KienThucLienQuanAsync(string ma, int lopHienThi) => Task.FromResult(new KienThucBaiTap(
            [new KienThucLienQuan("CT_THOI_DT", "Diện tích hình thoi", @"S = \frac{d_1 d_2}{2}", 8)],
            [new KhaiNiemLienQuan("HINH_THOI", "Hình thoi", 8)]));
        public Task<BanGhiLuotLam?> KetQuaAsync(string taiKhoanId, string maLan) => Task.FromResult(Attempts.FirstOrDefault(a => a.MaLan == maLan));
        public Task<BanGhiLuotLam?> GhiNhanAsync(string taiKhoanId, int lopThuc, BaiTapChiTiet bai, string maLan,
            string payloadHash, string dapAn, string donViDaChon, bool dung, int seconds)
        {
            if (TuChoiGhi) return Task.FromResult<BanGhiLuotLam?>(null);
            var existing = Attempts.FirstOrDefault(a => a.MaLan == maLan);
            if (existing is not null)
                return Task.FromResult<BanGhiLuotLam?>(existing with { XungDot = existing.PayloadHash != payloadHash });
            var saved = new BanGhiLuotLam(maLan, payloadHash, dapAn, "96", "cm²", dung, seconds, false, bai.Ma);
            Attempts.Add(saved);
            return Task.FromResult<BanGhiLuotLam?>(saved);
        }
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan span) => _now += span;
    }
}
