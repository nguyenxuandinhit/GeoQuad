using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Tests.Chung;

/// <summary>Test mẫu của PHẦN 0 (US-01): nhận diện lỗi vi phạm ràng buộc duy nhất.</summary>
public class GraphDbErrorsTests
{
    [Fact]
    public void NhanDienLoiTrungKhoa()
    {
        var ex = new ClientException("Neo.ClientError.Schema.ConstraintValidationFailed", "trùng khóa");

        Assert.True(GraphDbErrors.IsUniqueViolation(ex));
    }

    [Fact]
    public void NhanDienLoiTrungKhoaBenTrongInnerException()
    {
        var ex = new InvalidOperationException(
            "bọc ngoài",
            new ClientException("Neo.ClientError.Schema.ConstraintValidationFailed", "trùng khóa"));

        Assert.True(GraphDbErrors.IsUniqueViolation(ex));
    }

    [Fact]
    public void LoiKhacKhongPhaiTrungKhoa()
    {
        Assert.False(GraphDbErrors.IsUniqueViolation(new ClientException("Neo.ClientError.Statement.SyntaxError", "sai cú pháp")));
        Assert.False(GraphDbErrors.IsUniqueViolation(new Exception("lỗi khác")));
        Assert.False(GraphDbErrors.IsUniqueViolation(null));
    }
}
