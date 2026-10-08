using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Areas.KienThuc.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>
/// US-09: service truyền đúng tham số xuống truy vấn và gắn nhãn "Nâng cao" đúng (BR-04).
/// Dùng repository giả nên không cần Neo4j.
/// </summary>
public class ThuVienServiceTests
{
    /// <summary>Repository giả: ghi lại tham số nhận được và trả về danh sách định trước.</summary>
    private sealed class RepoGia : IThuVienRepository
    {
        private readonly IReadOnlyList<NoiDungThuVien> _ketQua;

        public RepoGia(params NoiDungThuVien[] ketQua) => _ketQua = ketQua;

        public int Lop { get; private set; }
        public string? Loai { get; private set; }
        public string? Cap { get; private set; }
        public int? LopLoc { get; private set; }
        public string? TaiKhoanId { get; private set; }

        public Task<IReadOnlyList<NoiDungThuVien>> DuyetAsync(
            int lop, string? loai, string? cap, int? lopLoc, string? taiKhoanId)
        {
            Lop = lop;
            Loai = loai;
            Cap = cap;
            LopLoc = lopLoc;
            TaiKhoanId = taiKhoanId;
            return Task.FromResult(_ketQua);
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

    private static NoiDungThuVien NoiDung(string ma, int lop, string loai = "KhaiNiem")
        => new(ma, ma, null, loai, lop, ThuVienQuyTac.CapTheoLop(lop), ma, false);

    [Fact]
    public async Task TruyenLopHienThiXuongTruyVanChuKhongPhaiLopThat()
    {
        // Học sinh lớp 4 bật xem trước nâng cao → truy vấn lấy tới lớp 12.
        var repo = new RepoGia();
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.Equal(12, repo.Lop);
        Assert.Equal(12, vm.LopHienThi);
        Assert.Equal(4, vm.LopHocSinh);
    }

    [Fact]
    public async Task KhongBatNangCaoThiChiLayToiLopThat()
    {
        var repo = new RepoGia();
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 4 });

        await service.DuyetAsync(null, null, null);

        Assert.Equal(4, repo.Lop);
    }

    [Fact]
    public async Task GiaTriLocLaBiBoTruocKhiXuongTruyVan()
    {
        var repo = new RepoGia();
        var service = new ThuVienService(repo, new NguoiDungGia());

        await service.DuyetAsync("KHONG_CO_LOAI_NAY", "CAP_9", 99);

        Assert.Null(repo.Loai);
        Assert.Null(repo.Cap);
        Assert.Null(repo.LopLoc);
    }

    [Fact]
    public async Task GiaTriLocHopLeDuocGiuVaChuanHoa()
    {
        var repo = new RepoGia();
        var service = new ThuVienService(repo, new NguoiDungGia());

        var vm = await service.DuyetAsync("cong_thuc", "cap_1", 3);

        Assert.Equal("CONG_THUC", repo.Loai);
        Assert.Equal("CAP_1", repo.Cap);
        Assert.Equal(3, repo.LopLoc);
        Assert.Equal("CONG_THUC", vm.Loai);
        Assert.True(vm.CoLoc);
    }

    [Fact]
    public async Task KhachKhongGuiTaiKhoanIdNenKhongCoHuyHieuDaHoc()
    {
        var repo = new RepoGia();
        var service = new ThuVienService(repo, new NguoiDungGia { DaDangNhap = false, TaiKhoanId = null });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.Null(repo.TaiKhoanId);   // BR-09: khách không có dữ liệu học tập
        Assert.False(vm.DaDangNhap);
    }

    [Fact]
    public async Task HocSinhGuiTaiKhoanIdDeTinhDaHoc()
    {
        var repo = new RepoGia();
        var service = new ThuVienService(
            repo, new NguoiDungGia { DaDangNhap = true, TaiKhoanId = "tk-1" });

        await service.DuyetAsync(null, null, null);

        Assert.Equal("tk-1", repo.TaiKhoanId);
    }

    [Fact]
    public async Task GanNhanNangCaoChoNoiDungVuotLopHocSinh()
    {
        // AC US-09: học sinh lớp 4 bật xem trước thì thấy Hình thang cân (lớp 6) kèm nhãn "Nâng cao".
        var repo = new RepoGia(
            NoiDung("HINH_THOI", 4),
            NoiDung("HINH_THANG_CAN", 6),
            NoiDung("TC_HBH_1", 8, "TinhChat"));
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.False(vm.DanhSach.Single(n => n.Ma == "HINH_THOI").NangCao);
        Assert.True(vm.DanhSach.Single(n => n.Ma == "HINH_THANG_CAN").NangCao);
        Assert.True(vm.DanhSach.Single(n => n.Ma == "TC_HBH_1").NangCao);
    }

    [Fact]
    public async Task HocSinhLop12KhongCoNoiDungNaoLaNangCao()
    {
        var repo = new RepoGia(NoiDung("HINH_THANG_CAN", 6), NoiDung("TC_HBH_1", 8));
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 12 });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.All(vm.DanhSach, n => Assert.False(n.NangCao));
    }

    [Fact]
    public async Task TheCoLienKetToiChiTietCuaHinhLienQuan()
    {
        var repo = new RepoGia(NoiDung("TC_HBH_1", 8, "TinhChat") with { });
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 8 });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.Equal("/KienThuc/ThuVien/ChiTiet/TC_HBH_1", vm.DanhSach[0].DuongDanChiTiet);
    }

    [Fact]
    public async Task KhongSuyRaDuocHinhThiTheKhongCoLienKet()
    {
        var repo = new RepoGia(new NoiDungThuVien("X", "X", null, "CongThuc", 8, "CAP_2", null, false));
        var service = new ThuVienService(repo, new NguoiDungGia { Lop = 8 });

        var vm = await service.DuyetAsync(null, null, null);

        Assert.Null(vm.DanhSach[0].DuongDanChiTiet);
    }

    [Fact]
    public async Task HocSinhCap1DuocDanhDauDeNutBamToHon()
    {
        var repo = new RepoGia();

        var cap1 = await new ThuVienService(repo, new NguoiDungGia { Lop = 4 }).DuyetAsync(null, null, null);
        var cap2 = await new ThuVienService(repo, new NguoiDungGia { Lop = 8 }).DuyetAsync(null, null, null);

        Assert.True(cap1.LaCap1);    // NFR-04: nút ≥ 48×48px
        Assert.False(cap2.LaCap1);
    }
}
