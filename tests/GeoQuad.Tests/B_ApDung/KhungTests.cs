using GeoQuad.Tests.B_ApDung.Doubles;
using GeoQuad.Tests.B_ApDung.Integration;
using GeoQuad.Web.Areas.ApDung;
using GeoQuad.Web.Areas.QuanTri;
using Microsoft.Extensions.DependencyInjection;

namespace GeoQuad.Tests.B_ApDung;

/// <summary>
/// Test mẫu do PHẦN 0 tạo (US-01) để phần B có chỗ viết test cho Area ApDung.
/// Thay bằng test thật của các story phần B.
/// </summary>
public class KhungTests
{
    [Fact]
    public void ModulesDangKyTrenServiceCollection()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.AddApDung());
        Assert.Same(services, services.AddQuanTri());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    [Theory]
    [InlineData(8, false, 8)]
    [InlineData(3, true, 12)]
    public void GuestKhongCoTaiKhoanVaLopHienThiDung(int lop, bool nangCao, int hienThi)
    {
        var user = new CurrentUserGia(lop, nangCao);
        Assert.False(user.DaDangNhap);
        Assert.Null(user.TaiKhoanId);
        Assert.Equal(hienThi, user.LopHienThi);
    }

    [Theory]
    [InlineData("bolt://127.0.0.1:7687")]
    [InlineData("bolt://example.com:17687")]
    [InlineData("neo4j://127.0.0.1:17687")]
    [InlineData("bolt://127.0.0.1:17687/dev")]
    [InlineData("bolt://user:password@127.0.0.1:17687")]
    public void GuardTuChoiDbKhongCoLap(string uri) =>
        Assert.Throws<InvalidOperationException>(() => Neo4jFixture.ValidateUri(uri));

    [Fact]
    public void GuardChapNhanDungCongTest() => Neo4jFixture.ValidateUri("bolt://127.0.0.1:17687");
}
