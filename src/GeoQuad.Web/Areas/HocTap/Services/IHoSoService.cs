using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Services;

/// <summary>Nghiệp vụ cập nhật hồ sơ, đổi mật khẩu và xóa tài khoản (US-08, FR-04, NFR-06).</summary>
public interface IHoSoService
{
    /// <summary>Lấy thông tin hiển thị trang hồ sơ của tài khoản đang đăng nhập.</summary>
    Task<HoSoViewModel?> LayHoSoAsync(string taiKhoanId);

    /// <summary>Đổi biệt danh và lớp học (cập nhật Cookie Auth nếu thành công).</summary>
    Task<KetQuaHoSo> DoiThongTinAsync(string taiKhoanId, DoiThongTinInput input);

    /// <summary>Đổi mật khẩu (xác thực mật khẩu cũ trước khi đổi).</summary>
    Task<KetQuaHoSo> DoiMatKhauAsync(string taiKhoanId, DoiMatKhauInput input);

    /// <summary>Xóa tài khoản (yêu cầu xác thực mật khẩu).</summary>
    Task<KetQuaHoSo> XoaTaiKhoanAsync(string taiKhoanId, XoaTaiKhoanInput input);
}
