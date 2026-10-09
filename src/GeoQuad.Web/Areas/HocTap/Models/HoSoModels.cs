using System.ComponentModel.DataAnnotations;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Models;

/// <summary>
/// Dữ liệu hiển thị màn hình Hồ sơ học sinh (SCR-17, US-08, FR-04).
/// </summary>
public sealed class HoSoViewModel
{
    public string Id { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string BietDanh { get; set; } = string.Empty;
    public int Lop { get; set; }
    public string VaiTro { get; set; } = string.Empty;
    public DateTimeOffset? NgayTao { get; set; }

    public DoiThongTinInput DoiThongTin { get; set; } = new();
    public DoiMatKhauInput DoiMatKhau { get; set; } = new();
    public XoaTaiKhoanInput XoaTaiKhoan { get; set; } = new();

    public string? ThongBaoThanhCong { get; set; }
    public string? ThongBaoLoi { get; set; }
}

/// <summary>Dữ liệu gửi lên khi đổi biệt danh và lớp học.</summary>
public sealed class DoiThongTinInput
{
    [Required(ErrorMessage = "Vui lòng nhập biệt danh.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Biệt danh tối đa 50 ký tự.")]
    public string BietDanh { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Lớp học phải từ lớp 1 đến lớp 12.")]
    public int Lop { get; set; } = 8;
}

/// <summary>Dữ liệu gửi lên khi đổi mật khẩu.</summary>
public sealed class DoiMatKhauInput
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    public string MatKhauCu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu mới không khớp.")]
    [DataType(DataType.Password)]
    public string XacNhanMatKhau { get; set; } = string.Empty;
}

/// <summary>Dữ liệu gửi lên khi xác nhận xóa tài khoản (NFR-06).</summary>
public sealed class XoaTaiKhoanInput
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại để xác nhận xóa.")]
    [DataType(DataType.Password)]
    public string MatKhauXacNhan { get; set; } = string.Empty;
}

/// <summary>Kết quả thực hiện tác vụ hồ sơ.</summary>
public sealed record KetQuaHoSo(
    bool ThanhCong,
    string? Loi = null,
    TaiKhoanAuth? TaiKhoanMoi = null);
