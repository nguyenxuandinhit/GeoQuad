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
        // Phần C: đăng ký Repository và Service của Area HocTap tại đây.
        return services;
    }
}
