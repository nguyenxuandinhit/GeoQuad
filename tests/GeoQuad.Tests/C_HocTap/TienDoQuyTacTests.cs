using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Services;

namespace GeoQuad.Tests.C_HocTap;

public sealed class TienDoQuyTacTests
{
    private static LuotLamTomTat[] Events(params bool[] correct) => correct.Select((ok, i) =>
        new LuotLamTomTat($"A{i}", $"BT-{i:000}", ok, $"2026-10-08T00:{i:00}:00Z", "HINH_THOI", "Hình thoi")).ToArray();

    [Fact]
    public void PhanTich_UsesAllHistoryForProgressAndLastFiveForWeakness()
    {
        var events = Events(true, true, true, true, true, false, false, false, false, false);

        var result = TienDoQuyTac.PhanTich(events, .6m);

        var concept = Assert.Single(result);
        Assert.Equal(10, concept.SoLuot);
        Assert.Equal(5, concept.SoDungAllTime);
        Assert.Equal(0, concept.SoDungGanNhat);
        Assert.True(concept.CanOn);
    }

    [Fact]
    public void PhanTich_ThresholdIsInclusiveAndDuplicateRowsPerConceptDoNotInflate()
    {
        var attempts = Events(true, true, true, false, false);
        var duplicated = attempts.Concat(attempts.Select(x => x with { MaKhaiNiem = "HINH_THOI" })).ToArray();

        var result = Assert.Single(TienDoQuyTac.PhanTich(duplicated, .6m));

        Assert.Equal(5, result.SoLuot);
        Assert.Equal(3, result.SoDungGanNhat);
        Assert.False(result.CanOn);
    }

    [Fact]
    public void PhanTich_RequiresThreeDistinctAttemptsToMarkWeak()
    {
        var result = Assert.Single(TienDoQuyTac.PhanTich(Events(false, false), .6m));

        Assert.False(result.CanOn);
        Assert.Equal(2, result.SoLuot);
    }
}
