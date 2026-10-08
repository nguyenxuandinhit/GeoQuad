using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Areas.KienThuc.Services;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-10: chi tiết khái niệm và "Em đã hiểu" (FR-11, BR-09, BR-11).</summary>
public class ChiTietServiceTests
{
    private sealed class RepoGia : IChiTietRepository
    {
        public ChiTietKhaiNiem? KetQua { get; init; }
        public LienQuanKhaiNiem LienQuan { get; init; } = new([], []);
        public bool GhiThanhCong { get; init; } = true;

        public int LopNhan { get; private set; }
        public string? TaiKhoanIdNhan { get; private set; }
        public int SoLanGhi { get; private set; }
        public string? MaGhi { get; private set; }

        public Task<ChiTietKhaiNiem?> LayAsync(string ma, int lop, string? taiKhoanId)
        {
            LopNhan = lop;
            TaiKhoanIdNhan = taiKhoanId;
            return Task.FromResult(KetQua is null || KetQua.KhaiNiem.Ma != ma ? null : KetQua);
        }

        public Task<LienQuanKhaiNiem> LayLienQuanAsync(string ma, int lop)
            => Task.FromResult(LienQuan);

        public Task<bool> GhiDaHocAsync(string taiKhoanId, string ma)
        {
            SoLanGhi++;
            MaGhi = ma;
            return Task.FromResult(GhiThanhCong);
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

    private static ChiTietKhaiNiem HinhThoi(
        IReadOnlyList<MucTinhChat>? tinhChat = null,
        IReadOnlyList<MucCongThuc>? congThuc = null,
        IReadOnlyList<MucDauHieu>? dauHieu = null,
        string? ghiChuTieuHoc = null,
        bool daHoc = false)
        => new(
            new KhaiNiemChiTiet("HINH_THOI", "Hình thoi", "HINH",
                "Tứ giác có bốn cạnh bằng nhau.", ghiChuTieuHoc),
            Lop: 4,
            TinhChat: tinhChat ?? [],
            CongThuc: congThuc ?? [],
            DauHieu: dauHieu ?? [],
            TongQuatHon: [new LienKetKhaiNiem("HINH_BINH_HANH", "Hình bình hành")],
            DacBietHon: [new LienKetKhaiNiem("HINH_VUONG", "Hình vuông")],
            DaHoc: daHoc);

    [Fact]
    public async Task MaKhongTonTaiThiTraVeNull()
    {
        var service = new ChiTietService(new RepoGia { KetQua = HinhThoi() }, new NguoiDungGia());

        Assert.Null(await service.LayAsync("KHONG_CO"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MaRongThiTraVeNull(string ma)
    {
        var service = new ChiTietService(new RepoGia { KetQua = HinhThoi() }, new NguoiDungGia());

        Assert.Null(await service.LayAsync(ma));
    }

    [Fact]
    public async Task TruyenLopHienThiXuongTruyVan()
    {
        var repo = new RepoGia { KetQua = HinhThoi() };
        var service = new ChiTietService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        await service.LayAsync("HINH_THOI");

        Assert.Equal(12, repo.LopNhan);
    }

    [Fact]
    public async Task GanNhanNangCaoChoTungMucVuotLopHocSinh()
    {
        // Học sinh lớp 4 bật xem trước: tính chất lớp 8 của hình thoi phải mang nhãn "Nâng cao".
        var repo = new RepoGia
        {
            KetQua = HinhThoi(
                tinhChat:
                [
                    new MucTinhChat("TC_THOI_1", "Hai đường chéo vuông góc với nhau.", 8)
                ],
                congThuc:
                [
                    new MucCongThuc("CT_THOI_CV", "Chu vi hình thoi", "P = 4a", 4),
                    new MucCongThuc("CT_HCN_CHEO", "Đường chéo", "d = a", 8)
                ])
        };
        var service = new ChiTietService(repo, new NguoiDungGia { Lop = 4, XemNangCao = true });

        var vm = (await service.LayAsync("HINH_THOI"))!;

        Assert.True(vm.ChiTiet.TinhChat[0].NangCao);
        Assert.False(vm.ChiTiet.CongThuc.Single(c => c.Ma == "CT_THOI_CV").NangCao);
        Assert.True(vm.ChiTiet.CongThuc.Single(c => c.Ma == "CT_HCN_CHEO").NangCao);
    }

    [Fact]
    public async Task TabRongThiDuocDanhDauDeAn()
    {
        var service = new ChiTietService(new RepoGia { KetQua = HinhThoi() }, new NguoiDungGia());

        var vm = (await service.LayAsync("HINH_THOI"))!;

        Assert.False(vm.CoTinhChat);
        Assert.False(vm.CoDauHieu);
        Assert.False(vm.CoCongThuc);
        Assert.False(vm.CoTinhHuong);
        Assert.False(vm.CoBaiTap);
        Assert.True(vm.CoLienKet);   // hình thoi luôn có liên kết tổng quát/đặc biệt hơn
    }

    [Fact]
    public async Task TabCoNoiDungThiDuocDanhDauDeHien()
    {
        var repo = new RepoGia
        {
            KetQua = HinhThoi(tinhChat: [new MucTinhChat("TC_THOI_1", "…", 8)]),
            LienQuan = new LienQuanKhaiNiem(
                [new MucTinhHuong("TH-03", "Lát sân bằng gạch hình thoi")],
                [new MucBaiTap("BT-009", "Một hình thoi…", 2, 4)])
        };
        var service = new ChiTietService(repo, new NguoiDungGia { Lop = 8 });

        var vm = (await service.LayAsync("HINH_THOI"))!;

        Assert.True(vm.CoTinhChat);
        Assert.True(vm.CoTinhHuong);
        Assert.True(vm.CoBaiTap);
    }

    [Fact]
    public async Task GhiChuTieuHocChiHienVoiHocSinhCap1()
    {
        // BR-11: ghi chú tiểu học chỉ dành cho học sinh cấp 1.
        var repo = new RepoGia { KetQua = HinhThoi(ghiChuTieuHoc: "Ở tiểu học: …") };

        var cap1 = await new ChiTietService(repo, new NguoiDungGia { Lop = 4 }).LayAsync("HINH_THOI");
        var cap2 = await new ChiTietService(repo, new NguoiDungGia { Lop = 8 }).LayAsync("HINH_THOI");

        Assert.True(cap1!.HienGhiChuTieuHoc);
        Assert.False(cap2!.HienGhiChuTieuHoc);
    }

    [Fact]
    public async Task KhongCoGhiChuTieuHocThiKhongHienDuLaCap1()
    {
        var repo = new RepoGia { KetQua = HinhThoi(ghiChuTieuHoc: null) };
        var service = new ChiTietService(repo, new NguoiDungGia { Lop = 3 });

        var vm = (await service.LayAsync("HINH_THOI"))!;

        Assert.True(vm.LaCap1);
        Assert.False(vm.HienGhiChuTieuHoc);
    }

    [Fact]
    public async Task BayHinhCoSvgMinhHoa()
    {
        var repo = new RepoGia { KetQua = HinhThoi() };
        var service = new ChiTietService(repo, new NguoiDungGia());

        var vm = (await service.LayAsync("HINH_THOI"))!;

        Assert.NotNull(vm.Svg);
        Assert.Contains("<title", vm.Svg);
    }

    // ---- "Em đã hiểu" ----

    [Fact]
    public async Task KhachBamEmDaHieuThiDuocMoiDangNhapVaKhongGhiGi()
    {
        // BR-09: khách không ghi bất kỳ dữ liệu học tập nào.
        var repo = new RepoGia();
        var service = new ChiTietService(repo, new NguoiDungGia { DaDangNhap = false });

        var ketQua = await service.DaHieuAsync("HINH_THOI");

        Assert.Equal(KetQuaDaHieu.MoiDangNhap, ketQua);
        Assert.Equal(0, repo.SoLanGhi);
    }

    [Fact]
    public async Task HocSinhBamEmDaHieuThiGhiNhan()
    {
        var repo = new RepoGia();
        var service = new ChiTietService(
            repo, new NguoiDungGia { DaDangNhap = true, TaiKhoanId = "tk-1" });

        var ketQua = await service.DaHieuAsync("HINH_THOI");

        Assert.Equal(KetQuaDaHieu.DaGhiNhan, ketQua);
        Assert.Equal(1, repo.SoLanGhi);
        Assert.Equal("HINH_THOI", repo.MaGhi);
    }

    [Fact]
    public async Task BamHaiLanVanBaoDaGhiNhan()
    {
        // MERGE trong Cypher đảm bảo chỉ một quan hệ DA_HOC; service gọi lại vẫn hợp lệ.
        var repo = new RepoGia();
        var service = new ChiTietService(
            repo, new NguoiDungGia { DaDangNhap = true, TaiKhoanId = "tk-1" });

        Assert.Equal(KetQuaDaHieu.DaGhiNhan, await service.DaHieuAsync("HINH_THOI"));
        Assert.Equal(KetQuaDaHieu.DaGhiNhan, await service.DaHieuAsync("HINH_THOI"));
    }

    [Fact]
    public async Task MaKhongTonTaiThiBaoKhongTimThay()
    {
        var repo = new RepoGia { GhiThanhCong = false };
        var service = new ChiTietService(
            repo, new NguoiDungGia { DaDangNhap = true, TaiKhoanId = "tk-1" });

        Assert.Equal(KetQuaDaHieu.KhongTimThay, await service.DaHieuAsync("KHONG_CO"));
    }

    [Fact]
    public async Task KhachKhongGuiTaiKhoanIdKhiXemChiTiet()
    {
        var repo = new RepoGia { KetQua = HinhThoi() };
        var service = new ChiTietService(repo, new NguoiDungGia { DaDangNhap = false });

        await service.LayAsync("HINH_THOI");

        Assert.Null(repo.TaiKhoanIdNhan);
    }
}
