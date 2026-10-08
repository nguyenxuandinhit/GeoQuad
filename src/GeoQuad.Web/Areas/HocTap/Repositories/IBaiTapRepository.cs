using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public interface IBaiTapRepository
{
    Task<IReadOnlyList<BaiTapTomTat>> LocAsync(int lop, BaiTapLoc boLoc, string? taiKhoanId);
    Task<IReadOnlyList<KhaiNiemLoc>> KhaiNiemAsync(int lop);
}
