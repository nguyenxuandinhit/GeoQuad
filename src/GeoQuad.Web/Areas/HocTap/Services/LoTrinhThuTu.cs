namespace GeoQuad.Web.Areas.HocTap.Services;

public static class LoTrinhThuTu
{
    public const int GioiHan = 12;

    // Edges point from concept to prerequisite; Kahn therefore runs on the reversed graph.
    public static IReadOnlyList<string> SapXep(string mucTieu,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> tienQuyet)
    {
        if (!tienQuyet.ContainsKey(mucTieu)) throw new InvalidOperationException("Thiếu mục tiêu lộ trình.");
        if (tienQuyet.Count > GioiHan + 1) throw new InvalidOperationException("Lộ trình vượt giới hạn an toàn.");
        if (tienQuyet.Any(kv => kv.Value.Any(p => !tienQuyet.ContainsKey(p))))
            throw new InvalidOperationException("Lộ trình thiếu khái niệm tiên quyết.");

        var dependents = tienQuyet.Keys.ToDictionary(k => k, _ => new List<string>(), StringComparer.Ordinal);
        var remaining = tienQuyet.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct(StringComparer.Ordinal).Count(), StringComparer.Ordinal);
        foreach (var (dependent, prerequisites) in tienQuyet)
            foreach (var prerequisite in prerequisites.Distinct(StringComparer.Ordinal)) dependents[prerequisite].Add(dependent);

        var ready = new SortedSet<string>(remaining.Where(kv => kv.Value == 0).Select(kv => kv.Key), StringComparer.Ordinal);
        var result = new List<string>(remaining.Count);
        while (ready.Count > 0)
        {
            var next = ready.Min!;
            ready.Remove(next);
            result.Add(next);
            foreach (var dependent in dependents[next].Order(StringComparer.Ordinal))
                if (--remaining[dependent] == 0) ready.Add(dependent);
        }
        if (result.Count != remaining.Count) throw new InvalidOperationException("Phát hiện chu trình trong lộ trình.");
        return result;
    }
}
