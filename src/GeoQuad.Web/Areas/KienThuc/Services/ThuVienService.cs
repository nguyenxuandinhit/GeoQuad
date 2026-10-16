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

        var lopHocSinh = _nguoiDung.Lop;
        var danhSach = await _repo.DuyetAsync(
            lopHocSinh, loaiLoc, capLoc, lopLoc, _nguoiDung.TaiKhoanId);

        return new ThuVienViewModel
        {
            DanhSach = danhSach,
            Loai = loaiLoc,
            Cap = capLoc,
            Lop = lopLoc,
            LopHocSinh = lopHocSinh,
            DaDangNhap = _nguoiDung.DaDangNhap
        };
    }
}
