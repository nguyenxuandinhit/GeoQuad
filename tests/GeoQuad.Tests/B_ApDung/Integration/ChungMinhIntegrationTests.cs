using System.Net;
using GeoQuad.Tests.B_ApDung.Http;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ChungMinhIntegrationTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task ProofCoCanCuVaDirectUrlFailClosed()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.SeedReviewedForTestsAsync();
        await using var server = await MayChuThu.StartAsync(fixture);
        using var page = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/CM-01");
        var html = WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK,page.StatusCode);
        Assert.Contains("TC_HBH_2",html);
        Assert.Contains("DL_NEN_5",html);
        Assert.Contains("ABCD",html);
        Assert.Contains("/KienThuc/ThuVien/ChiTiet/HINH_BINH_HANH",html);
        using var linkedKnowledge=await server.Client.GetAsync("/KienThuc/ThuVien/ChiTiet/HINH_BINH_HANH");
        Assert.Equal(HttpStatusCode.OK,linkedKnowledge.StatusCode);
        Assert.Contains("Hình bình hành",WebUtility.HtmlDecode(await linkedKnowledge.Content.ReadAsStringAsync()));
        Assert.True(html.IndexOf("buoc-1",StringComparison.Ordinal) < html.IndexOf("buoc-4",StringComparison.Ordinal));
        using var hints = await server.Client.GetAsync("/ApDung/GoiY?nen=HINH_BINH_HANH&dich=HINH_CHU_NHAT");
        Assert.Contains("/ApDung/ChungMinh/Xem/CM-01",await hints.Content.ReadAsStringAsync());
        await fixture.Db.WriteAsync("MATCH (:ChungMinh {ma:'CM-01'})-[:CO_BUOC]->(b:Buoc {ma:'CM-01-B1'})-[r:CAN_CU]->() DELETE r");
        using var incomplete = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/CM-01");
        Assert.Equal(HttpStatusCode.NotFound,incomplete.StatusCode);
        using var noLink = await server.Client.GetAsync("/ApDung/GoiY?nen=HINH_BINH_HANH&dich=HINH_CHU_NHAT");
        Assert.DoesNotContain("/ApDung/ChungMinh/Xem/CM-01",await noLink.Content.ReadAsStringAsync());
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_lop","3"));
        using var grade3 = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/CM-02");
        Assert.Equal(HttpStatusCode.NotFound,grade3.StatusCode);
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_nangcao","1"));
        using var advanced = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/CM-02");
        Assert.Equal(HttpStatusCode.OK,advanced.StatusCode);
        await fixture.Db.WriteAsync("MATCH (cm:ChungMinh {ma:'CM-02'}) SET cm.trangThai='NHAP'");
        using var draft = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/CM-02");
        Assert.Equal(HttpStatusCode.NotFound,draft.StatusCode);
        using var missing = await server.Client.GetAsync("/ApDung/ChungMinh/Xem/UNKNOWN");
        Assert.Equal(HttpStatusCode.NotFound,missing.StatusCode);
    }
}
