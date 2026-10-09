using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Services;

public static class TienDoQuyTac
{
    // Lịch sử có một dòng cho mỗi cặp lượt–khái niệm; gộp về một dòng mỗi lượt.
    public static IReadOnlyList<LuotLamTomTat> KhuTrung(IEnumerable<LuotLamTomTat> source) =>
        source.GroupBy(x => x.MaLan, StringComparer.Ordinal).Select(g => g.First()).ToArray();

    // BR-06 cần ≥ 3 lượt mới kết luận; chưa khái niệm nào đủ thì không coi là "không cần ôn".
    public static LoaiGoiY PhanLoai(int soLuot, IReadOnlyList<TienDoKhaiNiem> concepts) =>
        soLuot == 0 ? LoaiGoiY.ChuaCoLichSu
        : concepts.Any(x => x.CanOn) ? LoaiGoiY.CanOn
        : concepts.Any(x => x.SoLuot >= 3) ? LoaiGoiY.KhongCanOn
        : LoaiGoiY.ChuaDuDuLieu;

    public static IReadOnlyList<TienDoKhaiNiem> PhanTich(IEnumerable<LuotLamTomTat> source, decimal nguong)
    {
        if (nguong is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(nguong));
        return source.GroupBy(x => (x.MaKhaiNiem, x.TenKhaiNiem))
            .Select(group =>
            {
                var events = group.GroupBy(x => x.MaLan, StringComparer.Ordinal).Select(g => g.First())
                    .OrderByDescending(x => x.Luc, StringComparer.Ordinal)
                    .ThenByDescending(x => x.MaLan, StringComparer.Ordinal).ToArray();
                var last = events.Take(5).ToArray();
                var allCorrect = events.Count(x => x.Dung);
                var recentCorrect = last.Count(x => x.Dung);
                var allRate = events.Length == 0 ? 0 : (decimal)allCorrect / events.Length;
                var recentRate = last.Length == 0 ? 0 : (decimal)recentCorrect / last.Length;
                return new TienDoKhaiNiem(group.Key.MaKhaiNiem, group.Key.TenKhaiNiem,
                    events.Length, allCorrect, recentCorrect, allRate, recentRate,
                    events.Length >= 3 && recentRate < nguong);
            }).OrderBy(x => x.Ma, StringComparer.Ordinal).ToArray();
    }
}
