using GeoQuad.Web.Areas.HocTap.Services;
using Microsoft.Extensions.Configuration;

namespace GeoQuad.Tests.C_HocTap;

public sealed class HocTapOptionsTests
{
    [Fact]
    public void Bind_UsesTheDefaultThresholdWhenUnset()
    {
        var options = new HocTapOptions();
        var config = new ConfigurationBuilder().Build();

        HocTapOptions.Bind(options, config);

        Assert.Equal(.6m, options.NguongCanOn);
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("1.01")]
    [InlineData("not-a-number")]
    public void Bind_RejectsInvalidThreshold(string value)
    {
        var options = new HocTapOptions();
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["HocTap:NguongCanOn"] = value }).Build();

        Assert.Throws<InvalidOperationException>(() => HocTapOptions.Bind(options, config));
    }
}
