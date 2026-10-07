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
        // Phần B: đăng ký Repository và Service của Area ApDung tại đây.
        return services;
    }
}
