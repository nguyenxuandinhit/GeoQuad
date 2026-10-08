using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public interface ITienDoRepository
{
    Task<int?> LopHienTaiAsync(string taiKhoanId);
    Task<IReadOnlyList<LuotLamTomTat>> LichSuAsync(string taiKhoanId);
    Task<IReadOnlyList<BaiTapGoiY>> UngVienAsync(string taiKhoanId, int lop);
}
