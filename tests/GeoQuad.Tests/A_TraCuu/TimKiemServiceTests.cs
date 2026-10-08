using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Areas.KienThuc.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-11: service tìm kiếm — nới AND sang OR, gom nhóm, lọc lớp.</summary>
public class TimKiemServiceTests
{
    private sealed class RepoGia : ITimKiemRepository
    {
        private readonly Func<string, IReadOnlyList<KetQuaTimKiem>> _tra;

        public RepoGia(Func<string, IReadOnlyList<KetQuaTimKiem>> tra) => _tra = tra;

        public List<string> CacTruyVan { get; } = [];
        public int LopNhan { get; private set; }

        public Task<IReadOnlyList<KetQuaTimKiem>> TimAsync(string truyVanLucene, int lop)
        {
            CacTruyVan.Add(truyVanLucene);
            LopNhan = lop;
            return Task.FromResult(_tra(truyVanLucene));
        }
    }

    private sealed class NguoiDungGia : ICurrentUser
    {
        public bool DaDangNhap { get; init; }
        public string? TaiKhoanId { get; init; }
        public string? BietDanh { get; init; }
        public bool LaQuanTri { get; init; }
        public int Lop { get; init; } = 8;
        public bool XemNangCao { get; init; }
        public int LopHienThi => XemNangCao ? 12 : Lop;
    }

    private static KetQuaTimKiem Muc(string ma, string nhan, int lop = 8, double diem = 1.0)
        => new(ma, ma, "doan trich", [nhan], lop, diem, ma);

    [Fact]
    public async Task TuKhoaRongThiKhongGuiTruyVan()
    {
        var repo = new RepoGia(_ => []);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("   ");

        Assert.Empty(repo.CacTruyVan);
        Assert.True(vm.ChuaNhapTuKhoa);
        Assert.False(vm.KhongCoKetQua);
    }

    [Fact]
    public async Task TimBangAndTruoc()
    {
        var repo = new RepoGia(_ => [Muc("HINH_THOI", "KhaiNiem")]);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("hinh thoi");

        Assert.Single(repo.CacTruyVan);
        Assert.Equal("hinh AND thoi", repo.CacTruyVan[0]);
        Assert.False(vm.DaNoiLong);
        Assert.Equal(1, vm.TongSo);
    }

    [Fact]
    public async Task KhongCoKetQuaVoiAndThiThuOr()
    {
        var repo = new RepoGia(q => q.Contains("OR") ? [Muc("HINH_THOI", "KhaiNiem")] : []);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("hinh thoi");

        Assert.Equal(["hinh AND thoi", "hinh OR thoi"], repo.CacTruyVan);
        Assert.True(vm.DaNoiLong);
        Assert.Equal(1, vm.TongSo);
    }

    [Fact]
    public async Task MotTuThiKhongThuLaiVoiOr()
    {
        // Một từ thì chuỗi AND và OR giống nhau, không cần gọi truy vấn lần hai.
        var repo = new RepoGia(_ => []);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("khongcogi");

        Assert.Single(repo.CacTruyVan);
        Assert.True(vm.KhongCoKetQua);
        Assert.False(vm.DaNoiLong);
    }

    [Fact]
    public async Task CaAndVaOrDeuRongThiBaoKhongCoKetQua()
    {
        var repo = new RepoGia(_ => []);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("abc xyz");

        Assert.Equal(2, repo.CacTruyVan.Count);
        Assert.True(vm.KhongCoKetQua);
        Assert.False(vm.DaNoiLong);
        Assert.Empty(vm.TheoNhom);
    }

    [Fact]
    public async Task GomNhomTheoDungThuTu()
    {
        var repo = new RepoGia(_ =>
        [
            Muc("CT_THOI_DT", "CongThuc"),
            Muc("TC_THOI_1", "TinhChat"),
            Muc("HINH_THOI", "KhaiNiem")
        ]);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("thoi");

        Assert.Equal(
            new[] { "Khái niệm", "Tính chất & dấu hiệu", "Công thức" },
            vm.TheoNhom.Select(x => x.Nhom).ToArray());
        Assert.Equal(3, vm.TongSo);
    }

    [Fact]
    public async Task NhomRongThiKhongHien()
    {
        var repo = new RepoGia(_ => [Muc("HINH_THOI", "KhaiNiem")]);
        var service = new TimKiemService(repo, new NguoiDungGia());

        var vm = await service.TimAsync("thoi");

        Assert.Single(vm.TheoNhom);
        Assert.Equal("Khái niệm", vm.TheoNhom[0].Nhom);
    }

    [Fact]
    public async Task TruyenLopHienThiXuongTruyVan()
    {
        var repo = new RepoGia(_ => []);
        var service = new TimKiemService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        await service.TimAsync("thoi");

        Assert.Equal(12, repo.LopNhan);
    }

    [Fact]
    public async Task GanNhanNangCaoTheoLopThat()
    {
        var repo = new RepoGia(_ =>
        [
            Muc("HINH_THOI", "KhaiNiem", lop: 4),
            Muc("HINH_THANG_CAN", "KhaiNiem", lop: 6)
        ]);
        var service = new TimKiemService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        var vm = await service.TimAsync("hinh");
        var muc = vm.TheoNhom[0].Muc;

        Assert.False(muc.Single(m => m.Ma == "HINH_THOI").NangCao);
        Assert.True(muc.Single(m => m.Ma == "HINH_THANG_CAN").NangCao);
    }

    [Fact]
    public async Task KyTuDacBietDuocThoatTruocKhiXuongTruyVan()
    {
        var repo = new RepoGia(_ => []);
        var service = new TimKiemService(repo, new NguoiDungGia());

        await service.TimAsync("hinh(thoi) *");

        Assert.Equal(@"hinh\(thoi\) AND \*", repo.CacTruyVan[0]);
    }
}
