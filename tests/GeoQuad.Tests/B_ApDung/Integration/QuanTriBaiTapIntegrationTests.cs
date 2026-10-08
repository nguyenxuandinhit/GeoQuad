using System.Net;
using GeoQuad.Tests.B_ApDung.Http;
using GeoQuad.Web.Areas.QuanTri.Repositories;
using GeoQuad.Web.Areas.QuanTri.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class QuanTriBaiTapIntegrationTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task AtomicEditRollbackTypeChangeAndHistory()
    {
        await fixture.SeedReviewedForTestsAsync();
        var repo=new BaiTapQuanTriRepository(fixture.Db);
        var form=QuanTriBaiTapTests.ValidForm();
        await repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,false);
        Assert.Equal(form.Ma,(await repo.XemAsync(form.Ma))!.Ma);
        await new TaiKhoanRepository(fixture.Db).TaoNeuChuaCoAsync("history_b",new AuthHelper().BamMatKhau("History_Test_123!"),"B",VaiTro.HocSinh,8);
        await fixture.Db.WriteAsync("MATCH (t:TaiKhoan {tenDangNhap:'history_b'}),(b:BaiTap {ma:'BT-TEST'}) UNWIND range(1,7) AS i CREATE (t)-[:DA_LAM {luc:datetime('2026-10-08T01:00:00Z')+duration({seconds:i}),dung:true,dapAnDaChon:'A',thoiGianGiay:i}]->(b)");
        var before=await SnapshotAsync();
        form.De="Không được commit";
        await Assert.ThrowsAsync<InvalidOperationException>(() => new BaiTapQuanTriRepository(new GraphDbGayLoi(fixture.Db))
            .LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,true));
        Assert.Equal(before,await SnapshotAsync());
        form.KhaiNiem=["HINH_CHU_NHAT","UNKNOWN"];
        await Assert.ThrowsAsync<ArgumentException>(() => repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,true));
        Assert.Equal(before,await SnapshotAsync());
        form.KhaiNiem=["HINH_CHU_NHAT"]; form.SuDung=["HINH_THOI"];
        await Assert.ThrowsAsync<ArgumentException>(() => repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,true));
        form.SuDung=["CT_HCN_CV"]; form.Loai="DAP_AN_SO"; form.DapAnSo="0"; form.DonVi="cm";
        await repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,true);
        var row=(await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-TEST'}) RETURN b.phuongAn AS old,b.dapAnDung AS answer,b.dapAnSo AS number")).Single();
        Assert.Null(row["old"]); Assert.Null(row["answer"]); Assert.Equal(0d,row["number"].As<double>());
        form.Loai="CHUNG_MINH"; form.LoiGiaiMau="Chứng minh mẫu";
        await repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,true);
        row=(await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-TEST'}) RETURN b.dapAnSo AS old,b.loiGiaiMau AS proof")).Single();
        Assert.Null(row["old"]); Assert.Equal(form.LoiGiaiMau,row["proof"].As<string>());
        await repo.HienThiAsync(form.Ma,false); await repo.HienThiAsync(form.Ma,true);
        var history=await fixture.Db.ReadAsync("MATCH (:TaiKhoan {tenDangNhap:'history_b'})-[r:DA_LAM]->(:BaiTap {ma:'BT-TEST'}) RETURN r.dung AS correct,r.dapAnDaChon AS answer,r.thoiGianGiay AS seconds,r.luc AS time ORDER BY seconds");
        Assert.Equal(7,history.Count);
        Assert.All(history,h => { Assert.True(h["correct"].As<bool>()); Assert.Equal("A",h["answer"].As<string>()); Assert.NotNull(h["time"]); });
        Assert.Equal(Enumerable.Range(1,7),history.Select(h => h["seconds"].As<int>()));
        Assert.Equal(7,(await repo.DanhSachAsync(null,null,null,null,"BT-TEST")).Single().SoLanLam);
        var tasks=Enumerable.Range(0,2).Select(async _ => { try { await repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,false); return true; } catch(ClientException) { return false; } }).ToArray();
        Assert.All(await Task.WhenAll(tasks),success => Assert.False(success));
        form.Ma="BT-RACE";
        tasks=Enumerable.Range(0,2).Select(async _ => { try { await repo.LuuAsync(KiemTraBaiTap.KiemTra(form).BaiTap!,false); return true; } catch(ClientException) { return false; } }).ToArray();
        Assert.Single((await Task.WhenAll(tasks)).Where(success => success));
    }

    [Fact]
    public async Task HttpRolesCsrfImmutableIdPreviewVaTatCaLop()
    {
        await fixture.SeedReviewedForTestsAsync();
        await using var server=await MayChuThu.StartAsync(fixture);
        using var guest=await server.Client.GetAsync("/QuanTri/BaiTap/Them");
        Assert.Equal(HttpStatusCode.Redirect,guest.StatusCode);
        async Task AssertAllAdminRoutesDeniedAsync()
        {
            foreach(var path in new[]{"/QuanTri/BaiTap","/QuanTri/BaiTap/Them","/QuanTri/BaiTap/Sua/BT-001"})
            {
                using var response=await server.Client.GetAsync(path);
                Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
            }
            foreach(var path in new[]{"/QuanTri/BaiTap/Them","/QuanTri/BaiTap/Sua/BT-001","/QuanTri/BaiTap/XemTruoc","/QuanTri/BaiTap/HienThi/BT-001"})
            {
                using var response=await server.Client.PostAsync(path,new FormUrlEncodedContent(new Dictionary<string,string> { ["Ma"]="BT-DENIED",["hienThi"]="false" }));
                Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
            }
            Assert.Empty(await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-DENIED'}) RETURN b"));
            Assert.True((await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-001'}) RETURN b.hienThi AS visible")).Single()["visible"].As<bool>());
        }
        await AssertAllAdminRoutesDeniedAsync();
        await server.CreateAccountAsync(fixture,"b_student",VaiTro.HocSinh,8);
        using var studentLogin=await server.LoginAsync("b_student");
        using var student=await server.Client.GetAsync("/QuanTri/BaiTap");
        Assert.Equal(HttpStatusCode.Redirect,student.StatusCode);
        await AssertAllAdminRoutesDeniedAsync();
        await server.CreateAccountAsync(fixture,"b_admin",VaiTro.QuanTri,8);
        using var adminLogin=await server.LoginAsync("b_admin");
        using var formPage=await server.Client.GetAsync("/QuanTri/BaiTap/Them");
        var token=MayChuThu.AntiForgeryToken(await formPage.Content.ReadAsStringAsync());
        var fields=new Dictionary<string,string> { ["Ma"]="BT-HTTP",["De"]="<script>alert(1)</script>",["Loai"]="DAP_AN_SO",
            ["DapAnSo"]="0",["SaiSo"]="0.01",["DonVi"]="cm",["GiaiThich"]="Giải thích",["Lop"]="12",["DoKho"]="1",
            ["Nguon"]="Nhóm test",["TrangThai"]="NHAP",["KhaiNiem"]="HINH_CHU_NHAT",["SuDung"]="CT_HCN_CV",
            ["__RequestVerificationToken"]=token,["hienThi"]="false",["evil"]="overpost" };
        using var preview=await server.Client.PostAsync("/QuanTri/BaiTap/XemTruoc",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK,preview.StatusCode);
        var previewHtml=await preview.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<script>alert(1)</script>",previewHtml); Assert.Contains("&lt;script&gt;",previewHtml);
        Assert.Empty(await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-HTTP'}) RETURN b"));
        using var created=await server.Client.PostAsync("/QuanTri/BaiTap/Them",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect,created.StatusCode);
        using var edit=await server.Client.GetAsync("/QuanTri/BaiTap/Sua/BT-HTTP");
        Assert.Equal(HttpStatusCode.OK,edit.StatusCode);
        using var editPreview=await server.Client.PostAsync("/QuanTri/BaiTap/XemTruoc/BT-HTTP",new FormUrlEncodedContent(fields));
        Assert.Contains("/QuanTri/BaiTap/Sua/BT-HTTP",await editPreview.Content.ReadAsStringAsync());
        using var list=await server.Client.GetAsync("/QuanTri/BaiTap?lopLoc=12&hienThi=false&trangThai=NHAP");
        Assert.Contains("BT-HTTP",await list.Content.ReadAsStringAsync());
        Assert.Null((await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-HTTP'}) RETURN b.evil AS value")).Single()["value"]);
        fields["Ma"]="BT-HIJACK";
        using var renamed=await server.Client.PostAsync("/QuanTri/BaiTap/Sua/BT-HTTP",new FormUrlEncodedContent(fields));
        Assert.NotEqual(HttpStatusCode.Redirect,renamed.StatusCode);
        Assert.Contains("/QuanTri/BaiTap/Sua/BT-HTTP",await renamed.Content.ReadAsStringAsync());
        Assert.Empty(await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-HIJACK'}) RETURN b"));
        fields.Remove("__RequestVerificationToken");
        using var noToken=await server.Client.PostAsync("/QuanTri/BaiTap/Them",new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.BadRequest,noToken.StatusCode);
    }
    private async Task<string[]> SnapshotAsync()
    {
        var rows=await fixture.Db.ReadAsync("MATCH (b:BaiTap {ma:'BT-TEST'}) OPTIONAL MATCH (b)-[r]->(x) RETURN properties(b) AS props,type(r) AS edge,coalesce(x.ma,toString(x.so)) AS target ORDER BY edge,target");
        return rows.Select(r => System.Text.Json.JsonSerializer.Serialize(r["props"].As<Dictionary<string,object>>().OrderBy(p => p.Key,StringComparer.Ordinal))+r["edge"]+r["target"]).ToArray();
    }
}
