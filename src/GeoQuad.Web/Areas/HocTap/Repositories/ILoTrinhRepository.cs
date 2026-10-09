using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public interface ILoTrinhRepository
{
    Task<int?> LopHienTaiAsync(string taiKhoanId);
    Task<IReadOnlyList<KhaiNiemLoTrinh>> MucTieuHopLeAsync(int lop);
    Task<LoTrinhDoThi?> DoThiAsync(string taiKhoanId, string maMucTieu);
    Task<bool> DanhDauDaHocAsync(string taiKhoanId, string mucTieu, string ma, IReadOnlyCollection<string> maTrongLoTrinh);
}
