using System.ComponentModel.DataAnnotations;
using GeoQuad.Web.Infrastructure;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Models;

/// <summary>Biểu mẫu đăng ký SCR-02 (US-05). Không hỏi email, số điện thoại (NFR-06).</summary>
public sealed class DangKyViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Em nhập tên đăng nhập nhé.")]
    [StringLength(QuyTacTaiKhoan.TenToiDa, MinimumLength = QuyTacTaiKhoan.TenToiThieu,
        ErrorMessage = "Tên đăng nhập phải có từ {2} đến {1} ký tự.")]
    [RegularExpression("^[a-zA-Z0-9_]+$",
        ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số và dấu gạch dưới (_), ví dụ: hocsinh_8a.")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Em nhập mật khẩu nhé.")]
    [StringLength(100, MinimumLength = QuyTacTaiKhoan.MatKhauToiThieu,
        ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    [Display(Name = "Nhập lại mật khẩu")]
    [Required(ErrorMessage = "Em nhập lại mật khẩu nhé.")]
    [Compare(nameof(MatKhau), ErrorMessage = "Hai lần nhập mật khẩu chưa giống nhau.")]
    [DataType(DataType.Password)]
    public string NhapLaiMatKhau { get; set; } = string.Empty;

    [Display(Name = "Biệt danh")]
    [Required(ErrorMessage = "Em nhập biệt danh để hệ thống gọi tên em nhé.")]
    [StringLength(50, ErrorMessage = "Biệt danh dài tối đa {1} ký tự.")]
    public string BietDanh { get; set; } = string.Empty;

    [Display(Name = "Lớp")]
    [Range(QuyUoc.LopNhoNhat, QuyUoc.LopLonNhat, ErrorMessage = "Em chọn lớp từ {1} đến {2}.")]
    public int Lop { get; set; } = QuyUoc.LopMacDinh;

    [Display(Name = "Xác nhận quyền riêng tư")]
    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Em cần tích vào ô xác nhận đã đọc thông báo quyền riêng tư và được cha mẹ/người giám hộ đồng ý.")]
    public bool DongYQuyenRiengTu { get; set; }
}
