using System.ComponentModel.DataAnnotations;

namespace GeoQuad.Web.Models;

/// <summary>Biểu mẫu đăng nhập SCR-03 (US-06).</summary>
public sealed class DangNhapViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Em nhập tên đăng nhập nhé.")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Em nhập mật khẩu nhé.")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>Trang quay lại sau khi đăng nhập (chỉ nhận đường dẫn nội bộ).</summary>
    public string? TiepTuc { get; set; }
}
