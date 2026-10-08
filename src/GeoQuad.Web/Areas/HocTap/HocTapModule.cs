using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Areas.HocTap.Services;

namespace GeoQuad.Web.Areas.HocTap;

/// <summary>
/// Đăng ký dịch vụ của Area HocTap (phần C).
/// PHẦN 0 tạo khung này và gọi sẵn trong Program.cs — phần C thêm Repository/Service ở đây,
/// không sửa Program.cs.
/// </summary>
public static class HocTapModule
{
    public static IServiceCollection AddHocTap(this IServiceCollection services)
    {
        // US-08: Hồ sơ học sinh
        services.AddScoped<IHoSoRepository, HoSoRepository>();
        services.AddScoped<IHoSoService, HoSoService>();

        // US-19: ngân hàng bài tập.
        services.AddScoped<IBaiTapRepository, BaiTapRepository>();
        services.AddScoped<BaiTapService>();
        services.AddScoped<ILamBaiRepository, LamBaiRepository>();
        services.AddScoped<BaiTapLamService>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<LanLamTokenService>();

        services.AddScoped<ILoTrinhRepository, LoTrinhRepository>();
        services.AddScoped<LoTrinhService>();

        services.AddScoped<ITienDoRepository, TienDoRepository>();
        services.AddOptions<HocTapOptions>()
            .Configure<IConfiguration>(HocTapOptions.Bind)
            .Validate(x => x.NguongCanOn is >= 0 and <= 1, "HocTap:NguongCanOn phải thuộc [0,1].")
            .ValidateOnStart();
        services.AddScoped(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HocTapOptions>>().Value);
        services.AddScoped<TienDoService>();
        services.AddScoped<GoiYService>();

        return services;
    }
}
