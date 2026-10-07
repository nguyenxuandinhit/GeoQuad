namespace GeoQuad.Web.Areas.QuanTri;

/// <summary>
/// Đăng ký dịch vụ của Area QuanTri (phần B).
/// PHẦN 0 tạo khung này và gọi sẵn trong Program.cs — phần B thêm Repository/Service ở đây,
/// không sửa Program.cs.
/// </summary>
public static class QuanTriModule
{
    public static IServiceCollection AddQuanTri(this IServiceCollection services)
    {
        // Phần B: đăng ký Repository và Service của Area QuanTri tại đây.
        return services;
    }
}
