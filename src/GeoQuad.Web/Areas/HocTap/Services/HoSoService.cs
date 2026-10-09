using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

/// <summary>
/// Xử lý logic nghiệp vụ cho màn hình Hồ sơ (US-08, FR-04, NFR-06).
/// </summary>
public sealed class HoSoService : IHoSoService
{
    private readonly IHoSoRepository _repo;
    private readonly IAuthHelper _authHelper;

    public HoSoService(IHoSoRepository repo, IAuthHelper authHelper)
    {
        _repo = repo;
        _authHelper = authHelper;
    }

    public async Task<HoSoViewModel?> LayHoSoAsync(string taiKhoanId)
    {
        var tk = await _repo.TimTheoIdAsync(taiKhoanId);
        if (tk is null)
        {
            return null;
        }

        return new HoSoViewModel
        {
            Id = tk.Id,
            TenDangNhap = tk.TenDangNhap,
            BietDanh = tk.BietDanh,
            Lop = tk.Lop,
            VaiTro = tk.VaiTro,
            NgayTao = tk.NgayTao,
            DoiThongTin = new DoiThongTinInput
            {
                BietDanh = tk.BietDanh,
                Lop = tk.Lop
            }
        };
    }

    public async Task<KetQuaHoSo> DoiThongTinAsync(string taiKhoanId, DoiThongTinInput input)
    {
        var bietDanh = (input.BietDanh ?? string.Empty).Trim();
        if (!QuyTacTaiKhoan.BietDanhHopLe(bietDanh))
        {
            return new KetQuaHoSo(false, "Biệt danh không được để trống và tối đa 50 ký tự.");
        }

        if (!QuyTacTaiKhoan.LopHopLe(input.Lop))
        {
            return new KetQuaHoSo(false, "Lớp học phải từ lớp 1 đến lớp 12.");
        }

        var tk = await _repo.TimTheoIdAsync(taiKhoanId);
        if (tk is null)
        {
            return new KetQuaHoSo(false, "Không tìm thấy thông tin tài khoản.");
        }

        var thanhCong = await _repo.CapNhatThongTinAsync(taiKhoanId, bietDanh, input.Lop);
        if (!thanhCong)
        {
            return new KetQuaHoSo(false, "Không thể cập nhật thông tin trong CSDL.");
        }

        var tkMoi = new TaiKhoanAuth(tk.Id, tk.TenDangNhap, bietDanh, tk.VaiTro, input.Lop);
        return new KetQuaHoSo(true, TaiKhoanMoi: tkMoi);
    }

    public async Task<KetQuaHoSo> DoiMatKhauAsync(string taiKhoanId, DoiMatKhauInput input)
    {
        if (string.IsNullOrWhiteSpace(input.MatKhauCu))
        {
            return new KetQuaHoSo(false, "Vui lòng nhập mật khẩu hiện tại.");
        }

        var tk = await _repo.TimTheoIdAsync(taiKhoanId);
        if (tk is null)
        {
            return new KetQuaHoSo(false, "Không tìm thấy thông tin tài khoản.");
        }

        if (!_authHelper.KiemTraMatKhau(tk.MatKhauBam, input.MatKhauCu))
        {
            return new KetQuaHoSo(false, "Mật khẩu hiện tại không chính xác.");
        }

        if (!QuyTacTaiKhoan.MatKhauHopLe(input.MatKhauMoi))
        {
            return new KetQuaHoSo(false, "Mật khẩu mới phải có ít nhất 8 ký tự.");
        }

        if (input.MatKhauMoi != input.XacNhanMatKhau)
        {
            return new KetQuaHoSo(false, "Xác nhận mật khẩu mới không khớp.");
        }

        var bam = _authHelper.BamMatKhau(input.MatKhauMoi);
        var thanhCong = await _repo.CapNhatMatKhauAsync(taiKhoanId, bam);
        if (!thanhCong)
        {
            return new KetQuaHoSo(false, "Không thể cập nhật mật khẩu.");
        }

        return new KetQuaHoSo(true);
    }

    public async Task<KetQuaHoSo> XoaTaiKhoanAsync(string taiKhoanId, XoaTaiKhoanInput input)
    {
        if (string.IsNullOrWhiteSpace(input.MatKhauXacNhan))
        {
            return new KetQuaHoSo(false, "Vui lòng nhập mật khẩu để xác nhận xóa.");
        }

        var tk = await _repo.TimTheoIdAsync(taiKhoanId);
        if (tk is null)
        {
            return new KetQuaHoSo(false, "Không tìm thấy thông tin tài khoản.");
        }

        if (!_authHelper.KiemTraMatKhau(tk.MatKhauBam, input.MatKhauXacNhan))
        {
            return new KetQuaHoSo(false, "Mật khẩu xác nhận không đúng. Không thể xóa tài khoản.");
        }

        var thanhCong = await _repo.XoaTaiKhoanAsync(taiKhoanId);
        if (!thanhCong)
        {
            return new KetQuaHoSo(false, "Không thể xóa tài khoản khỏi CSDL.");
        }

        return new KetQuaHoSo(true);
    }
}
