using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;

namespace GeoQuad.Tests.C_HocTap;

/// <summary>
/// Unit tests cho US-08: Hồ sơ học sinh, đổi biệt danh, đổi lớp, đổi mật khẩu và xóa tài khoản.
/// Sử dụng fake repo và fake auth helper, không phụ thuộc CSDL Neo4j thật.
/// </summary>
public class HoSoServiceTests
{
    private sealed class FakeHoSoRepository : IHoSoRepository
    {
        public TaiKhoanHoSoBanGhi? TaiKhoanHienTai { get; set; }
        public bool CapNhatThongTinGoiDuoc { get; private set; }
        public string? BietDanhCapNhat { get; private set; }
        public int? LopCapNhat { get; private set; }

        public bool CapNhatMatKhauGoiDuoc { get; private set; }
        public string? MatKhauBamCapNhat { get; private set; }

        public bool XoaTaiKhoanGoiDuoc { get; private set; }

        public Task<TaiKhoanHoSoBanGhi?> TimTheoIdAsync(string id)
            => Task.FromResult(TaiKhoanHienTai?.Id == id ? TaiKhoanHienTai : null);

        public Task<bool> CapNhatThongTinAsync(string id, string bietDanh, int lop)
        {
            CapNhatThongTinGoiDuoc = true;
            BietDanhCapNhat = bietDanh;
            LopCapNhat = lop;
            return Task.FromResult(true);
        }

        public Task<bool> CapNhatMatKhauAsync(string id, string matKhauBam)
        {
            CapNhatMatKhauGoiDuoc = true;
            MatKhauBamCapNhat = matKhauBam;
            return Task.FromResult(true);
        }

        public Task<bool> XoaTaiKhoanAsync(string id)
        {
            XoaTaiKhoanGoiDuoc = true;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeAuthHelper : IAuthHelper
    {
        public string BamMatKhau(string matKhau) => $"hashed:{matKhau}";

        public bool KiemTraMatKhau(string matKhauBam, string matKhau)
            => matKhauBam == $"hashed:{matKhau}";

        public Task DangNhapAsync(HttpContext http, TaiKhoanAuth tk) => Task.CompletedTask;

        public Task DangXuatAsync(HttpContext http) => Task.CompletedTask;
    }

    private static (HoSoService Service, FakeHoSoRepository Repo, FakeAuthHelper Auth) TaoService(TaiKhoanHoSoBanGhi? tk = null)
    {
        var repo = new FakeHoSoRepository { TaiKhoanHienTai = tk };
        var auth = new FakeAuthHelper();
        var service = new HoSoService(repo, auth);
        return (service, repo, auth);
    }

    [Fact]
    public async Task LayHoSoAsync_TaiKhoanTonTai_TraVeViewModelChinhXac()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, _, _) = TaoService(tk);

        var vm = await service.LayHoSoAsync("uuid-1");

        Assert.NotNull(vm);
        Assert.Equal("uuid-1", vm.Id);
        Assert.Equal("hocsinh8", vm.TenDangNhap);
        Assert.Equal("Học Sinh 8", vm.BietDanh);
        Assert.Equal(8, vm.Lop);
    }

    [Fact]
    public async Task LayHoSoAsync_KhongTonTai_TraVeNull()
    {
        var (service, _, _) = TaoService(null);
        var vm = await service.LayHoSoAsync("not-found");
        Assert.Null(vm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task DoiThongTin_BietDanhRong_TraVeLoi(string? bietDanh)
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiThongTinAsync("uuid-1", new DoiThongTinInput { BietDanh = bietDanh!, Lop = 8 });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.CapNhatThongTinGoiDuoc);
        Assert.Contains("biệt danh", kq.Loi?.ToLower() ?? "");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task DoiThongTin_LopNgoaiPhamVi_TraVeLoi(int lop)
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiThongTinAsync("uuid-1", new DoiThongTinInput { BietDanh = "Tên Mới", Lop = lop });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.CapNhatThongTinGoiDuoc);
        Assert.Contains("lớp", kq.Loi?.ToLower() ?? "");
    }

    [Fact]
    public async Task DoiThongTin_HopLe_GoiRepoVaTraVeThanhCong()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 7, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiThongTinAsync("uuid-1", new DoiThongTinInput { BietDanh = "Nguyễn Văn A", Lop = 8 });

        Assert.True(kq.ThanhCong);
        Assert.True(repo.CapNhatThongTinGoiDuoc);
        Assert.Equal("Nguyễn Văn A", repo.BietDanhCapNhat);
        Assert.Equal(8, repo.LopCapNhat);
        Assert.NotNull(kq.TaiKhoanMoi);
        Assert.Equal("Nguyễn Văn A", kq.TaiKhoanMoi.BietDanh);
        Assert.Equal(8, kq.TaiKhoanMoi.Lop);
    }

    [Fact]
    public async Task DoiMatKhau_MatKhauCuSai_TraVeLoi()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiMatKhauAsync("uuid-1", new DoiMatKhauInput
        {
            MatKhauCu = "saimatkhau",
            MatKhauMoi = "matkhau456",
            XacNhanMatKhau = "matkhau456"
        });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.CapNhatMatKhauGoiDuoc);
        Assert.Contains("hiện tại không chính xác", kq.Loi ?? "");
    }

    [Fact]
    public async Task DoiMatKhau_MatKhauMoiNhoHon8KyTu_TraVeLoi()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiMatKhauAsync("uuid-1", new DoiMatKhauInput
        {
            MatKhauCu = "matkhau123",
            MatKhauMoi = "1234567",
            XacNhanMatKhau = "1234567"
        });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.CapNhatMatKhauGoiDuoc);
        Assert.Contains("8 ký tự", kq.Loi ?? "");
    }

    [Fact]
    public async Task DoiMatKhau_XacNhanKhongKhop_TraVeLoi()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiMatKhauAsync("uuid-1", new DoiMatKhauInput
        {
            MatKhauCu = "matkhau123",
            MatKhauMoi = "matkhau456",
            XacNhanMatKhau = "khongkhop456"
        });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.CapNhatMatKhauGoiDuoc);
        Assert.Contains("không khớp", kq.Loi ?? "");
    }

    [Fact]
    public async Task DoiMatKhau_HopLe_BamMatKhauVaGoiRepo()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.DoiMatKhauAsync("uuid-1", new DoiMatKhauInput
        {
            MatKhauCu = "matkhau123",
            MatKhauMoi = "matkhau456",
            XacNhanMatKhau = "matkhau456"
        });

        Assert.True(kq.ThanhCong);
        Assert.True(repo.CapNhatMatKhauGoiDuoc);
        Assert.Equal("hashed:matkhau456", repo.MatKhauBamCapNhat);
    }

    [Fact]
    public async Task XoaTaiKhoan_MatKhauXacNhanSai_TuChoi()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.XoaTaiKhoanAsync("uuid-1", new XoaTaiKhoanInput
        {
            MatKhauXacNhan = "saimatkhau"
        });

        Assert.False(kq.ThanhCong);
        Assert.False(repo.XoaTaiKhoanGoiDuoc);
        Assert.Contains("không đúng", kq.Loi ?? "");
    }

    [Fact]
    public async Task XoaTaiKhoan_HopLe_GoiRepoXoaVaTraVeThanhCong()
    {
        var tk = new TaiKhoanHoSoBanGhi(
            "uuid-1", "hocsinh8", "Học Sinh 8", "hashed:matkhau123", VaiTro.HocSinh, 8, DateTimeOffset.UtcNow);
        var (service, repo, _) = TaoService(tk);

        var kq = await service.XoaTaiKhoanAsync("uuid-1", new XoaTaiKhoanInput
        {
            MatKhauXacNhan = "matkhau123"
        });

        Assert.True(kq.ThanhCong);
        Assert.True(repo.XoaTaiKhoanGoiDuoc);
    }
}
