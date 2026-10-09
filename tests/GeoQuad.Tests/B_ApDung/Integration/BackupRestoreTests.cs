using System.Diagnostics;
using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Infrastructure.Neo4j;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category","Integration")]
public sealed class BackupRestoreTests(Neo4jFixture fixture)
{
    /// <summary>
    /// Lý do bỏ qua bài diễn tập dump/restore thật trên Windows: <c>backup.sh</c> gọi
    /// <c>docker run -v "$output:/backup"</c>, mà msys đổi <c>/c/…/ID:/backup</c> thành
    /// <c>C:\…\ID;C:\backup</c> trước khi tới docker.exe nên bind mount sai. Chạy bài này
    /// trên Linux/macOS hoặc trong WSL (ba bài backup còn lại không bind mount nên chạy được).
    /// </summary>
    public const string LyDoBoQuaDienTap =
        "BỎ QUA diễn tập dump/restore: scripts/backup.sh bind mount kiểu POSIX, "
        + "msys trên Windows đổi sai đường dẫn. Chạy trên Linux/macOS hoặc WSL.";

    [Fact]
    public async Task OfflineDumpRestoreIntoFreshVolumePreservesGraphAndHistory()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        if (Neo4jFixture.BashPosix is null) return;   // xem Neo4jFixture.LyDoThieuBash
        if (OperatingSystem.IsWindows()) return;      // xem LyDoBoQuaDienTap
        Assert.True(File.Exists(Path.Combine(Neo4jFixture.Root,"scripts/backup.sh")),"Thiếu backup.sh");
        Assert.True(File.Exists(Path.Combine(Neo4jFixture.Root,"scripts/restore.sh")),"Thiếu restore.sh");
        await fixture.SeedReviewedForTestsAsync();
        await new TaiKhoanRepository(fixture.Db).TaoNeuChuaCoAsync("b_restore",new AuthHelper().BamMatKhau("Restore_Test_123!"),"B",VaiTro.HocSinh,8);
        await fixture.Db.WriteAsync("MATCH (t:TaiKhoan {tenDangNhap:'b_restore'}),(b:BaiTap {ma:'BT-001'}) UNWIND range(1,3) AS i CREATE (t)-[:DA_LAM {luc:datetime('2026-10-08T01:00:00Z')+duration({seconds:i}),dung:true,dapAnDaChon:'A',thoiGianGiay:i}]->(b)");
        var before=(await fixture.Db.ReadAsync("RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges")).Single();
        var backup=await ScriptAsync("backup.sh","--project",Neo4jFixture.Project,"--compose",Neo4jFixture.ComposeFile,"--quiesced");
        var archive=Neo4jFixture.DangWindows(backup.Split('\n').Single(line => line.StartsWith("ARCHIVE=",StringComparison.Ordinal))[8..].Trim());
        Assert.True(File.Exists(archive));
        await fixture.GuardAsync();
        var target="geoquad-b-restore-"+Guid.NewGuid().ToString("N")[..12];
        try
        {
            await ScriptAsync("restore.sh","--archive",archive,"--target-project",target);
            await using var driver=GraphDatabase.Driver("bolt://127.0.0.1:27687",AuthTokens.Basic("neo4j",fixture.Password));
            var restored=new GraphDb(driver,Options.Create(new Neo4jOptions { Database="neo4j" }));
            var actual=(await restored.ReadAsync("RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges")).Single();
            Assert.Equal(before["nodes"].As<int>(),actual["nodes"].As<int>());
            Assert.Equal(before["edges"].As<int>(),actual["edges"].As<int>());
            var history=await restored.ReadAsync("MATCH (:TaiKhoan {tenDangNhap:'b_restore'})-[r:DA_LAM]->(:BaiTap {ma:'BT-001'}) RETURN r.dung AS correct,r.dapAnDaChon AS answer,r.thoiGianGiay AS seconds,r.luc AS time ORDER BY seconds");
            Assert.Equal(3,history.Count);
            Assert.All(history,h => { Assert.True(h["correct"].As<bool>()); Assert.Equal("A",h["answer"].As<string>()); Assert.NotNull(h["time"]); });
            Assert.Equal(Enumerable.Range(1,3),history.Select(h => h["seconds"].As<int>()));
            var account=await new TaiKhoanRepository(restored).TimTheoTenAsync("b_restore");
            Assert.True(new AuthHelper().KiemTraMatKhau(account!.MatKhauBam,"Restore_Test_123!"));
            var source=(await fixture.Db.ReadAsync("RETURN COUNT { () } AS nodes,COUNT { ()-[]->() } AS edges")).Single();
            Assert.Equal(before["nodes"].As<int>(),source["nodes"].As<int>());
            Assert.Equal(before["edges"].As<int>(),source["edges"].As<int>());
            var report=new { archive=Path.GetRelativePath(Neo4jFixture.Root,archive),target,nodes=actual["nodes"].As<int>(),
                edges=actual["edges"].As<int>(),historyPreserved=true,applicationPasswordVerified=true,systemDatabaseRestored=false };
            File.WriteAllText(Path.Combine(Neo4jFixture.Root,"plans/261008-0132-quan-b-ap-dung-tdd/reports/backup-drill-B.json"),
                System.Text.Json.JsonSerializer.Serialize(report,new System.Text.Json.JsonSerializerOptions { WriteIndented=true }));
        }
        finally
        {
            var containers=(await Neo4jFixture.RunAsync("docker","ps","-a","--filter","label=geoquad.restore-run="+target,"--format","{{.Names}}")).Split('\n');
            if(containers.Contains(target+"-neo4j")) await Neo4jFixture.RunAsync("docker","rm","-f",target+"-neo4j");
            var volumes=(await Neo4jFixture.RunAsync("docker","volume","ls","--filter","label=geoquad.restore-run="+target,"--format","{{.Name}}")).Split('\n');
            if(volumes.Contains(target+"_data")) await Neo4jFixture.RunAsync("docker","volume","rm",target+"_data");
        }
    }

    [Fact]
    public async Task MissingArchiveAndDevTargetAreRejectedBeforeMutation()
    {
        if (!fixture.DaBat) return;   // chưa bật GQ_B_INTEGRATION — xem Neo4jFixture.LyDoBoQua
        if (Neo4jFixture.BashPosix is null) return;   // xem Neo4jFixture.LyDoThieuBash
        Assert.True(File.Exists(Path.Combine(Neo4jFixture.Root,"scripts/restore.sh")),"Thiếu restore.sh");
        await Assert.ThrowsAsync<InvalidOperationException>(() => ScriptAsync("restore.sh","--archive","/tmp/missing.dump","--target-project","geoquad"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ScriptAsync("backup.sh","--project",Neo4jFixture.Project,"--compose",Neo4jFixture.ComposeFile));
        await fixture.GuardAsync();
    }

    // Script là bash POSIX: gọi bash đã dò được (trên Windows là bash của Git for Windows,
    // vì `bash` trong PATH là bash của WSL) và đổi mọi đường dẫn sang dạng /c/… cho script hiểu.
    private async Task<string> ScriptAsync(string file,params string[] args)
    {
        var info=new ProcessStartInfo(Neo4jFixture.BashPosix!) { WorkingDirectory=Neo4jFixture.Root,UseShellExecute=false,
            RedirectStandardOutput=true,RedirectStandardError=true };
        info.ArgumentList.Add(Neo4jFixture.DangPosix(Path.Combine(Neo4jFixture.Root,"scripts",file)));
        foreach(var arg in args) info.ArgumentList.Add(Neo4jFixture.DangPosix(arg));
        info.Environment["GQ_BACKUP_USER"]=fixture.User; info.Environment["GQ_BACKUP_PASSWORD"]=fixture.Password;
        info.Environment["GQ_RESTORE_PASSWORD"]=fixture.Password;
        using var process=Process.Start(info)!;
        var stdout=process.StandardOutput.ReadToEndAsync(); var stderr=process.StandardError.ReadToEndAsync();
        using var timeout=new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch { if(!process.HasExited) process.Kill(true); throw; }
        var output=await stdout; var error=await stderr;
        if(process.ExitCode != 0) throw new InvalidOperationException(file+" thất bại: "+error);
        return output;
    }
}
