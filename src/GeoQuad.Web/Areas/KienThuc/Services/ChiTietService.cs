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

        var lopHienThi = _nguoiDung.LopHienThi;
        var chiTiet = await _repo.LayAsync(ma, lopHienThi, _nguoiDung.TaiKhoanId);
        if (chiTiet is null)
        {
            return null;
        }

        var lienQuan = await _repo.LayLienQuanAsync(ma, lopHienThi);
        var lopHocSinh = _nguoiDung.Lop;

        // Gắn nhãn "Nâng cao" cho từng mục vượt lớp thật của học sinh (quy ước 2.7).
        var coNhan = chiTiet with
        {
            TinhChat = chiTiet.TinhChat
                .Select(x => x with { NangCao = ThuVienQuyTac.LaNangCao(x.Lop, lopHocSinh) }).ToList(),
            CongThuc = chiTiet.CongThuc
                .Select(x => x with { NangCao = ThuVienQuyTac.LaNangCao(x.Lop, lopHocSinh) }).ToList(),
            DauHieu = chiTiet.DauHieu
                .Select(x => x with { NangCao = ThuVienQuyTac.LaNangCao(x.Lop, lopHocSinh) }).ToList()
        };

        return new ChiTietViewModel
        {
            ChiTiet = coNhan,
            LienQuan = lienQuan,
            Svg = HinhVeSvg.Ve(coNhan.KhaiNiem.Ma),
            LopHocSinh = lopHocSinh,
            LopHienThi = lopHienThi,
            DaDangNhap = _nguoiDung.DaDangNhap,
            MoiDangNhap = moiDangNhap,
            NangCao = ThuVienQuyTac.LaNangCao(coNhan.Lop, lopHocSinh)
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
