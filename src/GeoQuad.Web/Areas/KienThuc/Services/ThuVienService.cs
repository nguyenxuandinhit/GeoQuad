using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.KienThuc.Services;

public interface IThuVienService
{
    /// <summary>Dựng dữ liệu cho SCR-04 theo lớp của người đang dùng (US-09).</summary>
    Task<ThuVienViewModel> DuyetAsync(string? loai, string? cap, int? lop);
}

public sealed class ThuVienService : IThuVienService
{
    private readonly IThuVienRepository _repo;
    private readonly ICurrentUser _nguoiDung;

    public ThuVienService(IThuVienRepository repo, ICurrentUser nguoiDung)
    {
        _repo = repo;
        _nguoiDung = nguoiDung;
    }

    public async Task<ThuVienViewModel> DuyetAsync(string? loai, string? cap, int? lop)
    {
        // Chỉ nhận giá trị trong danh sách cho phép; giá trị lạ coi như không lọc.
        var loaiLoc = ThuVienQuyTac.ChuanHoaLoai(loai);
        var capLoc = ThuVienQuyTac.ChuanHoaCap(cap);
        var lopLoc = ThuVienQuyTac.ChuanHoaLop(lop);

        var danhSach = await _repo.DuyetAsync(
            _nguoiDung.LopHienThi, loaiLoc, capLoc, lopLoc, _nguoiDung.TaiKhoanId);

        // Gắn nhãn "Nâng cao" cho nội dung vượt lớp thật của học sinh (quy ước 2.7).
        var lopHocSinh = _nguoiDung.Lop;
        var coNhan = danhSach
            .Select(n => n with { NangCao = ThuVienQuyTac.LaNangCao(n.Lop, lopHocSinh) })
            .ToList();

        return new ThuVienViewModel
        {
            DanhSach = coNhan,
            Loai = loaiLoc,
            Cap = capLoc,
            Lop = lopLoc,
            LopHienThi = _nguoiDung.LopHienThi,
            LopHocSinh = lopHocSinh,
            XemNangCao = _nguoiDung.XemNangCao,
            DaDangNhap = _nguoiDung.DaDangNhap
        };
    }
}
