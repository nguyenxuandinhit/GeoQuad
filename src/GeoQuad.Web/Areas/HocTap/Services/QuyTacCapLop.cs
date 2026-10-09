namespace GeoQuad.Web.Areas.HocTap.Services;

/// <summary>BR-03: cấp 1 = lớp 1–5, cấp 2 = lớp 6–9, cấp 3 = lớp 10–12. Logic thuần, không phụ thuộc Neo4j.</summary>
public static class QuyTacCapLop
{
    private static readonly Dictionary<string, (int Tu, int Den, string Ten)> Cap = new()
    {
        ["CAP_1"] = (1, 5, "Cấp 1"),
        ["CAP_2"] = (6, 9, "Cấp 2"),
        ["CAP_3"] = (10, 12, "Cấp 3"),
    };

    /// <summary>Lớp thuộc cấp (hoặc mọi lớp khi chưa chọn cấp), không vượt lớp hiển thị.</summary>
    public static IReadOnlyList<int> CacLop(string? cap, int lopHienThi)
    {
        var (tu, den) = cap is not null && Cap.TryGetValue(cap, out var c) ? (c.Tu, c.Den) : (1, 12);
        return Enumerable.Range(tu, Math.Max(0, Math.Min(den, lopHienThi) - tu + 1)).ToArray();
    }

    public static bool LopThuocCap(int lop, string cap) =>
        Cap.TryGetValue(cap, out var c) && lop >= c.Tu && lop <= c.Den;

    public static string TenCap(string cap) => Cap.TryGetValue(cap, out var c) ? c.Ten : cap;
}
