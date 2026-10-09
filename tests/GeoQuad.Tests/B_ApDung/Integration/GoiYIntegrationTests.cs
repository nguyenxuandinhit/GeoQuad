using System.Net;
using GeoQuad.Tests.B_ApDung.Doubles;
using GeoQuad.Tests.B_ApDung.Http;
using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class GoiYIntegrationTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task BonAcVaQppThucThiTrenNeo4j()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.SeedReviewedForTestsAsync();
        var repo = new GoiYRepository(fixture.Db);
        var service = new GoiYService(repo,new CurrentUserGia());
        var rectangle = await service.TimAsync(new("HINH_BINH_HANH","HINH_CHU_NHAT",["DK_MOT_GOC_VUONG"]));
        Assert.Equal("DH_HCN_2",rectangle.The[0].DauHieu.Ma);
        Assert.Empty(rectangle.The[0].ConThieu);
        var rhombus = await service.TimAsync(new("HINH_BINH_HANH","HINH_THOI",[]));
        Assert.Equal(new[] {"DH_THOI_1","DH_THOI_2","DH_THOI_3","DH_THOI_4"},rhombus.The.Select(c => c.DauHieu.Ma));
        Assert.All(rhombus.The,c => Assert.Single(c.ConThieu));
        var parallelogram = await service.TimAsync(new("TU_GIAC","HINH_BINH_HANH",["DK_CANH_DOI_BANG"]));
        Assert.Equal("DH_HBH_2",parallelogram.The[0].DauHieu.Ma);
        Assert.Empty(parallelogram.The[0].ConThieu);
        var inherited = await service.TimAsync(new("HINH_VUONG","HINH_CHU_NHAT",[]));
        Assert.Empty(inherited.The);
        Assert.Contains("Mọi",inherited.ThongBao);
        var chains = await repo.ChuoiAsync("TU_GIAC","HINH_VUONG",8);
        Assert.InRange(chains.Count,1,3);
        Assert.All(chains,c => Assert.InRange(c.Count,2,3));
        Assert.Equal(chains.Select(c => string.Join("/",c)), (await repo.ChuoiAsync("TU_GIAC","HINH_VUONG",8)).Select(c => string.Join("/",c)));
        Assert.Empty(await repo.ChuoiAsync("TU_GIAC","HINH_VUONG",3));
        await fixture.Db.WriteAsync("MATCH (d:DauHieu {ma:'DH_HCN_1'}) SET d.trangThai='NHAP'");
        Assert.DoesNotContain(await repo.ChuoiAsync("TU_GIAC","HINH_VUONG",8),c => c.Contains("DH_HCN_1"));
        await fixture.Db.WriteAsync("MATCH (d:DieuKien {ma:'DK_BON_CANH_BANG'}) REMOVE d.trangThai");
        var hidden = await service.TimAsync(new("HINH_BINH_HANH","HINH_THOI",[]));
        Assert.DoesNotContain(hidden.The,c => c.DauHieu.Ma == "DH_THOI_1");
    }

    [Fact]
    public async Task HttpCatalogGradeInputVaEncode()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.SeedReviewedForTestsAsync();
        await using var server = await MayChuThu.StartAsync(fixture);
        var uri = "/ApDung/GoiY?nen=HINH_BINH_HANH&dich=HINH_CHU_NHAT&co=DK_MOT_GOC_VUONG";
        using var result = await server.Client.GetAsync(uri);
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);
        Assert.Contains("DH_HCN_2",await result.Content.ReadAsStringAsync());
        using var invalid = await server.Client.GetAsync("/ApDung/GoiY?nen=TU_GIAC&dich=HINH_CHU_NHAT&co=UNKNOWN");
        Assert.Equal(HttpStatusCode.BadRequest,invalid.StatusCode);
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_lop","3"));
        using var grade3 = await server.Client.GetAsync("/ApDung/GoiY?nen=TU_GIAC&dich=HINH_CHU_NHAT");
        Assert.DoesNotContain("DH_HCN_2",await grade3.Content.ReadAsStringAsync());
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_nangcao","1"));
        using var advanced = await server.Client.GetAsync(uri);
        Assert.Contains("DH_HCN_2",await advanced.Content.ReadAsStringAsync());
    }
}
