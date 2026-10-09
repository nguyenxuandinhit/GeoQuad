using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.C_HocTap;

public sealed class BaiTapServiceTests
{
    private sealed class RepoGia : IBaiTapRepository
    {
        public int SoLanLoc { get; private set; }
        public int LopNhan { get; private set; }
        public string? TaiKhoanNhan { get; private set; }
        public BaiTapLoc? BoLocNhan { get; private set; }

        public Task<IReadOnlyList<BaiTapTomTat>> LocAsync(int lop, BaiTapLoc boLoc, string? taiKhoanId)
        {
            SoLanLoc++;
            LopNhan = lop;
            TaiKhoanNhan = taiKhoanId;
            BoLocNhan = boLoc;
            return Task.FromResult<IReadOnlyList<BaiTapTomTat>>(
            [
                new("BT-014", "Hình thoi có hai đường chéo...", "DAP_AN_SO", 2, 8, "CAP_2", 1, true)
            ]);
        }

        public Task<IReadOnlyList<KhaiNiemLoc>> KhaiNiemAsync(int lop) =>
            Task.FromResult<IReadOnlyList<KhaiNiemLoc>>([new("HINH_THOI", "Hình thoi")]);
    }

    private sealed class NguoiGia : ICurrentUser
    {
        public bool DaDangNhap => TaiKhoanId is not null;
        public string? TaiKhoanId { get; init; }
        public string? BietDanh => null;
        public bool LaQuanTri => false;
        public int Lop { get; init; } = 8;
        public bool XemNangCao => false;
        public int LopHienThi => Lop;
    }

    [Fact]
    public async Task LocKetHop_ChuyenDuBoLocVaTaiKhoanChoRepository()
    {
        var repo = new RepoGia();
        var service = new BaiTapService(repo, new NguoiGia { TaiKhoanId = "uuid-8" });

        var ketQua = await service.DanhSachAsync(new BaiTapLoc("CAP_2", 8, "DAP_AN_SO", 2, "HINH_THOI"));

        Assert.Null(ketQua.Loi);
        Assert.Single(ketQua.BaiTap);
        Assert.Equal(8, repo.LopNhan);
        Assert.Equal("uuid-8", repo.TaiKhoanNhan);
        Assert.Equal("CAP_2", repo.BoLocNhan?.Cap);
        Assert.Equal("HINH_THOI", repo.BoLocNhan?.KhaiNiem);
    }

    [Fact]
    public async Task Khach_KhongGuiTaiKhoanChoRepository()
    {
        var repo = new RepoGia();
        var service = new BaiTapService(repo, new NguoiGia { TaiKhoanId = null });

        await service.DanhSachAsync(new BaiTapLoc());

        Assert.Null(repo.TaiKhoanNhan);
    }

    [Theory]
    [InlineData(9, "DAP_AN_SO", 2)]
    [InlineData(8, "KHONG_HOP_LE", 2)]
    [InlineData(8, "DAP_AN_SO", 4)]
    public async Task BoLocKhongHopLe_KhongTruyVanBaiTap(int lop, string loai, int doKho)
    {
        var repo = new RepoGia();
        var service = new BaiTapService(repo, new NguoiGia { Lop = 8 });

        var ketQua = await service.DanhSachAsync(new BaiTapLoc(null, lop, loai, doKho, null));

        Assert.NotNull(ketQua.Loi);
        Assert.Equal(0, repo.SoLanLoc);
    }

    // BR-03: lớp phải thuộc cấp đã chọn; mâu thuẫn thì nói rõ, không trả danh sách rỗng khó hiểu.
    [Fact]
    public async Task CapVaLopMauThuan_BaoLoiRoRangVaKhongTruyVan()
    {
        var repo = new RepoGia();
        var service = new BaiTapService(repo, new NguoiGia { Lop = 8 });

        var ketQua = await service.DanhSachAsync(new BaiTapLoc("CAP_1", 7, null, null, null));

        Assert.Contains("Lớp 7", ketQua.Loi);
        Assert.Contains("Cấp 1", ketQua.Loi);
        Assert.Equal(0, repo.SoLanLoc);
    }

    [Theory]
    [InlineData(null, 8, new[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
    [InlineData("CAP_1", 8, new[] { 1, 2, 3, 4, 5 })]
    [InlineData("CAP_2", 8, new[] { 6, 7, 8 })]
    [InlineData("CAP_2", 12, new[] { 6, 7, 8, 9 })]
    [InlineData("CAP_3", 8, new int[0])]
    [InlineData("CAP_1", 3, new[] { 1, 2, 3 })]
    public void CacLopTheoCap_ChiGomLopCuaCapVaKhongVuotLopHienThi(string? cap, int lopHienThi, int[] expected)
    {
        Assert.Equal(expected, QuyTacCapLop.CacLop(cap, lopHienThi));
    }

    [Fact]
    public async Task DanhSach_LopChonTheoCapDaChon()
    {
        var service = new BaiTapService(new RepoGia(), new NguoiGia { Lop = 8 });

        var ketQua = await service.DanhSachAsync(new BaiTapLoc("CAP_1", null, null, null, null));

        Assert.Equal([1, 2, 3, 4, 5], ketQua.LopChon);
    }
}
