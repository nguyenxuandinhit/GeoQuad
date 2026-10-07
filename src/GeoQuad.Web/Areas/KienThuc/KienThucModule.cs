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
        // Phần A: đăng ký Repository và Service của Area KienThuc tại đây.
        return services;
    }
}
