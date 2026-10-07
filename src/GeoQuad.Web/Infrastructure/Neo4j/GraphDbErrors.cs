using Neo4j.Driver;

namespace GeoQuad.Web.Infrastructure.Neo4j;

/// <summary>Nhận diện lỗi Neo4j mà ứng dụng cần xử lý riêng.</summary>
public static class GraphDbErrors
{
    private const string MaViPhamRangBuoc = "Neo.ClientError.Schema.ConstraintValidationFailed";

    /// <summary>
    /// Đúng khi lỗi là vi phạm ràng buộc duy nhất (ví dụ trùng <c>TaiKhoan.tenDangNhap</c> ở US-05).
    /// </summary>
    public static bool IsUniqueViolation(Exception? ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
        {
            if (e is Neo4jException n &&
                string.Equals(n.Code, MaViPhamRangBuoc, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
