namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class HocTapOptions
{
    public decimal NguongCanOn { get; set; } = .6m;

    public static void Bind(HocTapOptions options, IConfiguration configuration)
    {
        var value = configuration["HocTap:NguongCanOn"];
        if (value is null) return;
        if (!decimal.TryParse(value, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var threshold) || threshold is < 0 or > 1)
            throw new InvalidOperationException("HocTap:NguongCanOn phải là số từ 0 đến 1.");
        options.NguongCanOn = threshold;
    }
}
