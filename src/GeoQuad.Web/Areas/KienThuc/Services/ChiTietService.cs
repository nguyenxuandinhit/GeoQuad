using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>Kết quả của hành động "Em đã hiểu" (US-10).</summary>
public enum KetQuaDaHieu
{
    /// <summary>Khách bấm — không ghi gì, mời đăng nhập (BR-09).</summary>
    MoiDangNhap,

    /// <summary>Đã ghi nhận (hoặc đã có từ trước).</summary>
    DaGhiNhan,

    /// <summary>Mã khái niệm không tồn tại.</summary>
    KhongTimThay
}

public interface IChiTietService
{
    /// <summary>Dựng dữ liệu SCR-05; null nếu mã không tồn tại → trang 404 (US-10).</summary>
    Task<ChiTietViewModel?> LayAsync(string ma, bool moiDangNhap = false);

    /// <summary>Xử lý nút "Em đã hiểu" (US-10).</summary>
    Task<KetQuaDaHieu> DaHieuAsync(string ma);
}

public sealed class ChiTietService : IChiTietService
{
    private readonly IChiTietRepository _repo;
    private readonly ICurrentUser _nguoiDung;

    public ChiTietService(IChiTietRepository repo, ICurrentUser nguoiDung)
    {
        _repo = repo;
        _nguoiDung = nguoiDung;
    }

    public async Task<ChiTietViewModel?> LayAsync(string ma, bool moiDangNhap = false)
    {
        if (string.IsNullOrWhiteSpace(ma))
        {
            return null;
        }

        var lopHocSinh = _nguoiDung.Lop;
        var chiTiet = await _repo.LayAsync(ma, lopHocSinh, _nguoiDung.TaiKhoanId);
        if (chiTiet is null)
        {
            return null;
        }

        var lienQuan = await _repo.LayLienQuanAsync(ma, lopHocSinh);

        return new ChiTietViewModel
        {
            ChiTiet = chiTiet,
            LienQuan = lienQuan,
            Svg = HinhVeSvg.Ve(chiTiet.KhaiNiem.Ma),
            LopHocSinh = lopHocSinh,
            DaDangNhap = _nguoiDung.DaDangNhap,
            MoiDangNhap = moiDangNhap
        };
    }

    public async Task<KetQuaDaHieu> DaHieuAsync(string ma)
    {
        // Khách không ghi bất kỳ dữ liệu học tập nào (BR-09).
        if (!_nguoiDung.DaDangNhap || _nguoiDung.TaiKhoanId is null)
        {
            return KetQuaDaHieu.MoiDangNhap;
        }

        if (string.IsNullOrWhiteSpace(ma))
        {
            return KetQuaDaHieu.KhongTimThay;
        }

        return await _repo.GhiDaHocAsync(_nguoiDung.TaiKhoanId, ma)
            ? KetQuaDaHieu.DaGhiNhan
            : KetQuaDaHieu.KhongTimThay;
    }
}
