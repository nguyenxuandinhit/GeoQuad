using System.Net;
using GeoQuad.Tests.B_ApDung.Integration;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.B_ApDung.Http;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class MayChuThuTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task ProductionDungCookieRoleVaCsrfThat()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.ResetAsync();
        await using var server = await MayChuThu.StartAsync(fixture);
        using var anonymous = await server.Client.GetAsync("/QuanTri/BaiTap");
        Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
        Assert.StartsWith("http://127.0.0.1:15080/TaiKhoan/DangNhap", anonymous.Headers.Location?.ToString());
        using var noCsrf = await server.Client.PostAsync("/TaiKhoan/DangNhap", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["TenDangNhap"] = "test_admin", ["MatKhau"] = "Test_B_123!" }));
        Assert.Equal(HttpStatusCode.BadRequest, noCsrf.StatusCode);
        await server.CreateAccountAsync(fixture, "test_admin", VaiTro.QuanTri, 8);
        using var login = await server.LoginAsync("test_admin");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        using var admin = await server.Client.GetAsync("/QuanTri/BaiTap");
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);
        var demos = await fixture.Db.ReadAsync("MATCH (t:TaiKhoan) WHERE t.tenDangNhap STARTS WITH 'demo' RETURN t");
        Assert.Empty(demos);
    }
}
