using GeoQuad.Web.Areas.HocTap.Services;

namespace GeoQuad.Tests.C_HocTap;

public sealed class LoTrinhThuTuTests
{
    [Fact]
    public void SapXep_DeduplicatesDiamondAndKeepsPrerequisitesFirst()
    {
        var graph = new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["VUONG"] = ["CN", "THOI"],
            ["CN"] = ["BHP"],
            ["THOI"] = ["BHP"],
            ["BHP"] = ["TG"],
            ["TG"] = []
        };

        var ordered = LoTrinhThuTu.SapXep("VUONG", graph);

        Assert.Equal(["TG", "BHP", "CN", "THOI", "VUONG"], ordered);
        Assert.Equal(ordered.Count, ordered.Distinct().Count());
    }

    [Fact]
    public void SapXep_OrdersUnevenChainAndIndependentNodeDeterministically()
    {
        var graph = new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["TARGET"] = ["A", "B"], ["A"] = ["ROOT"], ["B"] = [], ["ROOT"] = []
        };

        var ordered = LoTrinhThuTu.SapXep("TARGET", graph);

        Assert.Equal(["B", "ROOT", "A", "TARGET"], ordered);
    }

    [Fact]
    public void SapXep_RejectsCycleWithoutReturningPartialRoadmap()
    {
        var graph = new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["A"] = ["B"], ["B"] = ["A"]
        };

        Assert.Throws<InvalidOperationException>(() => LoTrinhThuTu.SapXep("A", graph));
    }

    [Fact]
    public void SapXep_RejectsGraphBeyondSafeBound()
    {
        var graph = new Dictionary<string, IReadOnlyCollection<string>>();
        for (var i = 0; i < 13; i++) graph[$"N{i}"] = [$"N{i + 1}"];
        graph["N13"] = [];

        Assert.Throws<InvalidOperationException>(() => LoTrinhThuTu.SapXep("N0", graph));
    }
}
