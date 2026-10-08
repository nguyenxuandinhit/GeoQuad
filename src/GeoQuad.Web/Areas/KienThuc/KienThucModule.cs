using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Web.Areas.KienThuc;

/// <summary>
/// Đăng ký dịch vụ của Area KienThuc (phần A).
/// PHẦN 0 tạo khung này và gọi sẵn trong Program.cs — phần A thêm Repository/Service ở đây,
/// không sửa Program.cs.
/// </summary>
public static class KienThucModule
{
    public static IServiceCollection AddKienThuc(this IServiceCollection services)
    {
        // US-09: thư viện kiến thức (SCR-04)
        services.AddScoped<IThuVienRepository, ThuVienRepository>();
        services.AddScoped<IThuVienService, ThuVienService>();

        // US-10: chi tiết khái niệm và "Em đã hiểu" (SCR-05)
        services.AddScoped<IChiTietRepository, ChiTietRepository>();
        services.AddScoped<IChiTietService, ChiTietService>();

        // US-11: tìm kiếm có/không dấu (SCR-06)
        services.AddScoped<ITimKiemRepository, TimKiemRepository>();
        services.AddScoped<ITimKiemService, TimKiemService>();

        // US-12: bản đồ kiến thức (SCR-07)
        services.AddScoped<IBanDoRepository, BanDoRepository>();
        services.AddScoped<IBanDoService, BanDoService>();

        return services;
    }
}
