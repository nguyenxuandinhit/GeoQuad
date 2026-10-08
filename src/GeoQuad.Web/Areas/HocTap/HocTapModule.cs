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

        return services;
    }
}
