namespace GeoQuad.Web.Areas.HocTap.Models;

public sealed record BaiTapLoc(
    string? Cap = null,
    int? Lop = null,
    string? Loai = null,
    int? DoKho = null,
    string? KhaiNiem = null);

public sealed record BaiTapTomTat(
    string Ma,
    string DeRutGon,
    string Loai,
    int DoKho,
    int Lop,
    string Cap,
    long SoLan,
    bool DaDung);

public sealed record KhaiNiemLoc(string Ma, string Ten);

public sealed record BaiTapDanhSach(
    BaiTapLoc BoLoc,
    IReadOnlyList<BaiTapTomTat> BaiTap,
    IReadOnlyList<KhaiNiemLoc> KhaiNiem,
    int LopNguoiDung,
    int LopHienThi,
    bool DaDangNhap,
    string? Loi = null)
{
    /// <summary>Các lớp hiện trong ô "Lớp": thuộc cấp đang chọn và không vượt lớp hiển thị.</summary>
    public IReadOnlyList<int> LopChon => Services.QuyTacCapLop.CacLop(BoLoc.Cap, LopHienThi);
}
