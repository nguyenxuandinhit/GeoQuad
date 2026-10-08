namespace GeoQuad.Web.Areas.ApDung.Models;

public sealed record HinhItem(string Ma, string Ten, int Lop);
public sealed record DieuKienItem(string Ma, string NoiDung, int Lop);
public sealed record DauHieuItem(string Ma, string NoiDung, string Nen, string Dich, int Lop,
    IReadOnlyList<DieuKienItem> DieuKien, string? MaChungMinh);
public sealed record GoiYCatalog(IReadOnlyList<HinhItem> Hinh, IReadOnlyList<DieuKienItem> DieuKien,
    IReadOnlyList<DauHieuItem> DauHieu);
public sealed record GoiYRequest(string Nen, string Dich, IReadOnlyList<string> Co);
public sealed record GoiYThe(DauHieuItem DauHieu, IReadOnlyList<DieuKienItem> DaCo, IReadOnlyList<DieuKienItem> ConThieu);
public sealed record GoiYViewModel(GoiYCatalog Catalog, GoiYRequest Request, IReadOnlyList<GoiYThe> The,
    IReadOnlyList<IReadOnlyList<GoiYThe>> Chuoi, string? ThongBao, int Lop, bool NangCao);
