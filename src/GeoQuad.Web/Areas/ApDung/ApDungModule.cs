using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Web.Areas.ApDung;

/// <summary>
/// Đăng ký dịch vụ của Area ApDung (phần B).
/// PHẦN 0 tạo khung này và gọi sẵn trong Program.cs — phần B thêm Repository/Service ở đây,
/// không sửa Program.cs.
/// </summary>
public static class ApDungModule
{
    public static IServiceCollection AddApDung(this IServiceCollection services)
    {
        services.AddScoped<IGoiYRepository, GoiYRepository>();
        services.AddScoped<GoiYService>();
        services.AddScoped<IChungMinhRepository, ChungMinhRepository>();
        services.AddScoped<ChungMinhService>();
        services.AddScoped<ITinhHuongRepository, TinhHuongRepository>();
        services.AddScoped<TinhHuongService>();
        services.AddScoped<DoDacRepository>();
        return services;
    }
}
