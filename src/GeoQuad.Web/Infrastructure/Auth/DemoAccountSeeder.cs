namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Tạo tài khoản demo khi khởi động ở môi trường Development (US-06).
/// Chỉ tạo nếu chưa có, không ghi đè mật khẩu tài khoản đang dùng.
/// </summary>
public sealed class DemoAccountSeeder
{
    private static readonly (string Ten, string MatKhau, string BietDanh, string VaiTro, int Lop)[] DanhSach =
    [
        ("admin",     "Admin@123",    "Quản trị viên", VaiTro.QuanTri, 12),
        ("hocsinh8",  "Hocsinh@123",  "Bạn lớp 8",     VaiTro.HocSinh,  8),
        ("hocsinh4",  "Hocsinh@123",  "Bạn lớp 4",     VaiTro.HocSinh,  4)
    ];

    private readonly ITaiKhoanRepository _repo;
    private readonly IAuthHelper _auth;
    private readonly ILogger<DemoAccountSeeder> _logger;

    public DemoAccountSeeder(
        ITaiKhoanRepository repo, IAuthHelper auth, ILogger<DemoAccountSeeder> logger)
    {
        _repo = repo;
        _auth = auth;
        _logger = logger;
    }

    public async Task ChayAsync()
    {
        foreach (var tk in DanhSach)
        {
            await _repo.TaoNeuChuaCoAsync(
                tk.Ten, _auth.BamMatKhau(tk.MatKhau), tk.BietDanh, tk.VaiTro, tk.Lop);
        }

        _logger.LogInformation(
            "Đã kiểm tra {So} tài khoản demo: admin, hocsinh8, hocsinh4", DanhSach.Length);
    }
}
