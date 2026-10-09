using System.Net;
using GeoQuad.Tests.B_ApDung.Http;
using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class DoDacIntegrationTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task PostSoDoCsrfCanCuVaKhongGhiHocTap()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        await fixture.SeedReviewedForTestsAsync();
        await using var server=await MayChuThu.StartAsync(fixture);
        var before=await CountsAsync();
        using var page=await server.Client.GetAsync("/ApDung/TinhHuong/Xem/TH-01");
        var token=MayChuThu.AntiForgeryToken(await page.Content.ReadAsStringAsync());
        var fields=new Dictionary<string,string> { ["AB"]="2,00",["BC"]="0,90",["CD"]="2,00",["DA"]="0,90",
            ["AC"]="2,19",["BD"]="2,19",["DonVi"]="m",["SaiSo"]="1",["__RequestVerificationToken"]=token };
        using var result=await server.Client.PostAsync("/ApDung/TinhHuong/DoDac/TH-01",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);
        var html=WebUtility.HtmlDecode(await result.Content.ReadAsStringAsync());
        Assert.Contains("Hình chữ nhật",html); Assert.Contains("DH_HCN_3",html);
        fields.Remove("__RequestVerificationToken");
        using var noToken=await server.Client.PostAsync("/ApDung/TinhHuong/DoDac/TH-01",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.BadRequest,noToken.StatusCode);
        fields["__RequestVerificationToken"]=token; fields["AB"]="NaN";
        using var invalid=await server.Client.PostAsync("/ApDung/TinhHuong/DoDac/TH-01",new FormUrlEncodedContent(fields));
        Assert.Contains("NaN",await invalid.Content.ReadAsStringAsync());
        fields["AB"]="2";
        using var noPractice=await server.Client.PostAsync("/ApDung/TinhHuong/DoDac/TH-02",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.NotFound,noPractice.StatusCode);
        server.Cookies.Add(server.Client.BaseAddress!,new Cookie("gq_lop","3"));
        using var lowGrade=await server.Client.PostAsync("/ApDung/TinhHuong/DoDac/TH-01",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.NotFound,lowGrade.StatusCode);
        Assert.Equal(before,await CountsAsync());
    }
    private async Task<int[]> CountsAsync()
    {
        var r=(await fixture.Db.ReadAsync("RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges")).Single();
        return [r["nodes"].As<int>(),r["edges"].As<int>()];
    }
}
