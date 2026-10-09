namespace GeoQuad.Web.Areas.HocTap.Models;

public sealed record KhaiNiemLoTrinh(string Ma, string Ten, int Lop, string TrangThai);
public sealed record BuocLoTrinh(string Ma, string Ten, int Lop, bool DaHoc, bool LaMucTieu);

public sealed record LoTrinhViewModel(
    IReadOnlyList<KhaiNiemLoTrinh> MucTieuHopLe,
    string? MucTieu,
    IReadOnlyList<BuocLoTrinh> CacBuoc,
    int SoDaHoc,
    int TongSo,
    int PhanTram,
    string? Loi)
{
    /// <summary>Mục tiêu không có tiên quyết: lộ trình chỉ gồm chính nó (US-22).</summary>
    public bool CoTheHocNgay => Loi is null && CacBuoc.Count == 1;
}

public sealed record LoTrinhDoThi(
    IReadOnlyList<KhaiNiemLoTrinh> Nodes,
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> TienQuyet,
    IReadOnlySet<string> DaHoc);
