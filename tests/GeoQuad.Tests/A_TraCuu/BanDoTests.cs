using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Areas.KienThuc.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-12: bản đồ kiến thức (FR-13, UC-05, BR-11).</summary>
public class BanDoTests
{
    private static BanDoNut Nut(string ma, int lop, params string[] laDacBietCua)
        => new(ma, ma, $"dinh nghia {ma}", lop, ThuVienQuyTac.CapTheoLop(lop), laDacBietCua, false);

    // ---- BanDoQuyTac.LopXem ----

    [Fact]
    public void KhongChonLopThiDungLopDangHienThi()
        => Assert.Equal(8, BanDoQuyTac.LopXem(null, 8));

    [Fact]
    public void ChonLopNhoHonThiDungLopDaChon()
        => Assert.Equal(3, BanDoQuyTac.LopXem(3, 8));

    [Fact]
    public void ChonLopCaoHonLopDuocXemThiBiChanLai()
        // BR-04: không được xem vượt quá lớp của mình qua tham số URL.
        => Assert.Equal(4, BanDoQuyTac.LopXem(12, 4));

    // ---- BanDoQuyTac.DungCanh ----

    [Fact]
    public void DungCanhTheoChieuDacBietToiTongQuat()
    {
        var nut = new[] { Nut("HINH_VUONG", 1, "HINH_CHU_NHAT"), Nut("HINH_CHU_NHAT", 1) };

        var canh = BanDoQuyTac.DungCanh(nut);

        var c = Assert.Single(canh);
        Assert.Equal("HINH_VUONG", c.Tu);        // từ hình đặc biệt
        Assert.Equal("HINH_CHU_NHAT", c.Den);    // tới hình tổng quát
    }

    [Fact]
    public void HinhVuongNoiToiCaHinhChuNhatVaHinhThoi()
    {
        // AC US-12.
        var nut = new[]
        {
            Nut("HINH_VUONG", 1, "HINH_CHU_NHAT", "HINH_THOI"),
            Nut("HINH_CHU_NHAT", 1, "HINH_BINH_HANH"),
            Nut("HINH_THOI", 4, "HINH_BINH_HANH"),
            Nut("HINH_BINH_HANH", 4)
        };

        var canh = BanDoQuyTac.DungCanh(nut);

        Assert.Contains(new BanDoCanh("HINH_VUONG", "HINH_CHU_NHAT"), canh);
        Assert.Contains(new BanDoCanh("HINH_VUONG", "HINH_THOI"), canh);
        Assert.Equal(4, canh.Count);
    }

    [Fact]
    public void BoMuiTenTroToiNutKhongHienThi()
    {
        // Lọc lớp 3: hình thoi (lớp 4) không hiển thị nên mũi tên tới nó phải bị bỏ.
        var nut = new[] { Nut("HINH_VUONG", 1, "HINH_CHU_NHAT", "HINH_THOI"), Nut("HINH_CHU_NHAT", 1) };

        var canh = BanDoQuyTac.DungCanh(nut);

        Assert.Single(canh);
        Assert.DoesNotContain(canh, c => c.Den == "HINH_THOI");
    }

    [Fact]
    public void KhongCoQuanHeThiDanhSachCanhRong()
    {
        var nut = new[] { Nut("HINH_CHU_NHAT", 1), Nut("TU_GIAC", 3) };

        Assert.Empty(BanDoQuyTac.DungCanh(nut));
    }

    [Fact]
    public void DoThiKhongTaoChuTrinh()
    {
        // Bảy quan hệ của Phụ lục D.2 phải cho một đồ thị không chu trình (AC US-12).
        var nut = new[]
        {
            Nut("HINH_VUONG", 1, "HINH_CHU_NHAT", "HINH_THOI"),
            Nut("HINH_CHU_NHAT", 1, "HINH_BINH_HANH"),
            Nut("HINH_THOI", 4, "HINH_BINH_HANH"),
            Nut("HINH_BINH_HANH", 4, "HINH_THANG"),
            Nut("HINH_THANG_CAN", 6, "HINH_THANG"),
            Nut("HINH_THANG", 5, "TU_GIAC"),
            Nut("TU_GIAC", 3)
        };
        var canh = BanDoQuyTac.DungCanh(nut);

        Assert.Equal(7, canh.Count);
        Assert.False(CoChuTrinh(canh));
    }

    private static bool CoChuTrinh(IReadOnlyList<BanDoCanh> canh)
    {
        var ke = canh.GroupBy(c => c.Tu).ToDictionary(g => g.Key, g => g.Select(c => c.Den).ToList());
        var dangDi = new HashSet<string>();
        var xong = new HashSet<string>();

        bool Tham(string n)
        {
            if (dangDi.Contains(n)) return true;
            if (!xong.Add(n)) return false;
            dangDi.Add(n);
            if (ke.TryGetValue(n, out var ds) && ds.Any(Tham)) return true;
            dangDi.Remove(n);
            return false;
        }

        return ke.Keys.Any(Tham);
    }

    // ---- Màu và chú giải ----

    [Fact]
    public void MoiCapCoMauRieng()
    {
        var mau = ThuVienQuyTac.CacCap.Select(BanDoQuyTac.MauTheoCap).ToList();

        Assert.Equal(3, mau.Distinct().Count());
        Assert.All(mau, m => Assert.StartsWith("#", m));
    }

    [Fact]
    public void ChuGiaiCoDuBaCapKemTenChu()
    {
        var cg = BanDoQuyTac.ChuGiai();

        Assert.Equal(3, cg.Count);
        // Chú giải có chữ nên không chỉ truyền tin bằng màu (NFR-11).
        Assert.All(cg, x => Assert.False(string.IsNullOrWhiteSpace(x.Ten)));
    }

    // ---- Câu trả lời "Vì sao … là …?" ----

    [Fact]
    public void CauTraLoiTuChuoiBonHinh()
    {
        var chuoi = new[] { "Hình vuông", "Hình chữ nhật", "Hình bình hành", "Hình thang" };

        var cau = BanDoQuyTac.CauTraLoi(chuoi);

        Assert.StartsWith("Hình vuông là một trường hợp đặc biệt của hình chữ nhật", cau);
        Assert.Contains("Hình bình hành là một trường hợp đặc biệt của hình thang", cau);
        Assert.EndsWith(".", cau);
    }

    [Fact]
    public void ChuoiNganThiKhongCoCauTraLoi()
    {
        Assert.Equal(string.Empty, BanDoQuyTac.CauTraLoi([]));
        Assert.Equal(string.Empty, BanDoQuyTac.CauTraLoi(["Hình vuông"]));
    }

    // ---- Service ----

    private sealed class RepoGia : IBanDoRepository
    {
        public IReadOnlyList<BanDoNut> Nut { get; init; } = [];
        public IReadOnlyList<string> Chuoi { get; init; } = [];

        public int LopNhan { get; private set; }
        public int SoLanGoiViSao { get; private set; }

        public Task<IReadOnlyList<BanDoNut>> LayNutAsync(int lop, string? taiKhoanId)
        {
            LopNhan = lop;
            return Task.FromResult(Nut);
        }

        public Task<IReadOnlyList<LienKetKhaiNiem>> LayTongQuatHonAsync(string ma)
            => Task.FromResult<IReadOnlyList<LienKetKhaiNiem>>([]);

        public Task<IReadOnlyList<string>> LayChuoiViSaoAsync(string tu, string den)
        {
            SoLanGoiViSao++;
            return Task.FromResult(Chuoi);
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

    [Fact]
    public async Task ServiceChanLopVuotQuyen()
    {
        var repo = new RepoGia();
        var service = new BanDoService(repo, new NguoiDungGia { Lop = 4 });

        await service.LayDuLieuAsync(12);

        Assert.Equal(4, repo.LopNhan);
    }

    [Fact]
    public async Task KhongCoQuanHeThiDanhDauDeHienLoiGiaiThich()
    {
        var repo = new RepoGia { Nut = [Nut("HINH_CHU_NHAT", 1)] };
        var service = new BanDoService(repo, new NguoiDungGia { Lop = 1 });

        var vm = await service.LayTrangAsync(null, null, null);

        Assert.True(vm.KhongCoQuanHe);
    }

    [Fact]
    public async Task ChonHaiHinhGiongNhauThiKhongGoiTruyVan()
    {
        var repo = new RepoGia { Nut = [Nut("HINH_VUONG", 1)] };
        var service = new BanDoService(repo, new NguoiDungGia());

        var vm = await service.LayTrangAsync(null, "HINH_VUONG", "HINH_VUONG");

        Assert.Equal(0, repo.SoLanGoiViSao);
        Assert.NotNull(vm.ViSao);
        Assert.False(vm.ViSao!.CoDuongDi);
    }

    [Fact]
    public async Task KhongChonHinhThiKhongHoiViSao()
    {
        var repo = new RepoGia { Nut = [Nut("HINH_VUONG", 1)] };
        var service = new BanDoService(repo, new NguoiDungGia());

        var vm = await service.LayTrangAsync(null, null, null);

        Assert.Null(vm.ViSao);
        Assert.Equal(0, repo.SoLanGoiViSao);
    }

    [Fact]
    public async Task ViSaoTraVeChuoiBonHinh()
    {
        var repo = new RepoGia
        {
            Nut = [Nut("HINH_VUONG", 1), Nut("HINH_THANG", 5)],
            Chuoi = ["Hình vuông", "Hình chữ nhật", "Hình bình hành", "Hình thang"]
        };
        var service = new BanDoService(repo, new NguoiDungGia { Lop = 8 });

        var vm = await service.LayTrangAsync(null, "HINH_VUONG", "HINH_THANG");

        Assert.NotNull(vm.ViSao);
        Assert.True(vm.ViSao!.CoDuongDi);
        Assert.Equal(4, vm.ViSao.Chuoi.Count);
    }
}
