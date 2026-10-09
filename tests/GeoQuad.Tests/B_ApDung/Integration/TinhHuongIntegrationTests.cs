using System.Net;
using GeoQuad.Tests.B_ApDung.Http;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class TinhHuongIntegrationTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task TinhHuongLocToanBoKienThucVaDirectUrl()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.SeedReviewedForTestsAsync();
        await using var server = await MayChuThu.StartAsync(fixture);
        using var detail = await server.Client.GetAsync("/ApDung/TinhHuong/Xem/TH-01");
        var html = await detail.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK,detail.StatusCode);
        Assert.Contains("DH_HBH_2",html);
        Assert.Contains("DH_HCN_3",html);
        using var list = await server.Client.GetAsync("/ApDung/TinhHuong");
        var catalog = await list.Content.ReadAsStringAsync();
        foreach (var id in new[] {"BC_NHA_CUA","BC_MANH_VUON","BC_DO_DUNG"}) Assert.Contains(id,catalog);
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_lop","3"));
        using var lowList = await server.Client.GetAsync("/ApDung/TinhHuong");
        Assert.DoesNotContain("/ApDung/TinhHuong/Xem/TH-01",await lowList.Content.ReadAsStringAsync());
        using var lowDetail = await server.Client.GetAsync("/ApDung/TinhHuong/Xem/TH-01");
        Assert.Equal(HttpStatusCode.NotFound,lowDetail.StatusCode);
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_nangcao","1"));
        using var advanced = await server.Client.GetAsync("/ApDung/TinhHuong/Xem/TH-01");
        Assert.Contains("DH_HBH_2",await advanced.Content.ReadAsStringAsync());
        await fixture.Db.WriteAsync("MATCH (d:DinhLy {ma:'DH_HCN_3'})-[r:THUOC_LOP]->() DELETE r");
        using var incomplete = await server.Client.GetAsync("/ApDung/TinhHuong/Xem/TH-01");
        Assert.Equal(HttpStatusCode.NotFound,incomplete.StatusCode);
        using var unknown = await server.Client.GetAsync("/ApDung/TinhHuong?boiCanh=UNKNOWN");
        Assert.Equal(HttpStatusCode.BadRequest,unknown.StatusCode);
    }
}
