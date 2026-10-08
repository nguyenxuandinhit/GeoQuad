using System.Diagnostics;
using GeoQuad.Tests.B_ApDung.Integration;

namespace GeoQuad.Tests.B_ApDung;

// Flow tests inject a command runner through PATH. They never connect to Docker/Neo4j.
public sealed class BackupFlowTests
{
    [Theory]
    [InlineData("dump-fails", true, true)]
    [InlineData("recovery-unhealthy", true, false)]
    [InlineData("originally-stopped", false, false)]
    public async Task DumpFailureRecoversOriginalStateAndWaitsBeforeWeb(string mode,bool sourceRunning,bool webRecovered)
    {
        var scratch=Path.Combine(Path.GetTempPath(),"geoquad-b-flow-"+Guid.NewGuid().ToString("N"));
        var output=Path.Combine(Neo4jFixture.Root,"backups","test-flow-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        try
        {
            var fake=Path.Combine(scratch,"docker");
            await File.WriteAllTextAsync(fake,FakeDocker);
            await RunAsync("chmod",null,"+x",fake);
            var result=await RunAsync("bash",new Dictionary<string,string> {
                ["PATH"]=scratch+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH"),
                ["GQ_FLOW_DIR"]=scratch,["GQ_FLOW_MODE"]=mode,["GQ_BACKUP_PASSWORD"]="flow-only" },
                Path.Combine(Neo4jFixture.Root,"scripts/backup.sh"),"--project","geoquad-b-tests","--compose",Neo4jFixture.ComposeFile,
                "--output",output,"--quiesced");
            Assert.NotEqual(0,result.ExitCode);
            Assert.DoesNotContain("ARCHIVE=",result.Output);
            var events=await File.ReadAllLinesAsync(Path.Combine(scratch,"events"));
            Assert.Contains("dump",events);
            Assert.True(Array.IndexOf(events,"stop neo") < Array.IndexOf(events,"dump"));
            Assert.Equal(webRecovered,events.Contains("start web"));
            if(sourceRunning) Assert.Contains("start neo",events);
            else Assert.Equal("stop neo",events.Last());
            if(webRecovered) Assert.True(Array.LastIndexOf(events,"healthy") < Array.IndexOf(events,"start web"));
            Assert.DoesNotContain("flow-only",result.Output);
        }
        finally
        {
            Directory.Delete(scratch,true);
            if(Directory.Exists(output)) Directory.Delete(output,true);
        }
    }

    [Fact]
    public async Task InvalidChecksumIsRejectedBeforeAnyDockerCall()
    {
        var directory=Path.Combine(Neo4jFixture.Root,"backups","test-checksum-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var archive=Path.Combine(directory,"neo4j.dump");
            await File.WriteAllTextAsync(archive,"damaged archive");
            await File.WriteAllTextAsync(Path.Combine(directory,"sha256.txt"),new string('0',64));
            await File.WriteAllTextAsync(Path.Combine(directory,"image.txt"),"sha256:"+new string('0',64));
            await File.WriteAllTextAsync(Path.Combine(directory,"source-manifest.txt"),"test only");
            var result=await RunAsync("bash",null,Path.Combine(Neo4jFixture.Root,"scripts/restore.sh"),"--archive",archive,
                "--target-project","geoquad-b-restore-checksum-test");
            Assert.NotEqual(0,result.ExitCode);
            Assert.Contains("Checksum",result.Output);
        }
        finally { Directory.Delete(directory,true); }
    }

    private static async Task<(int ExitCode,string Output)> RunAsync(string command,Dictionary<string,string>? environment,params string[] args)
    {
        var info=new ProcessStartInfo(command) { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true };
        foreach(var arg in args) info.ArgumentList.Add(arg);
        if(environment != null) foreach(var (name,value) in environment) info.Environment[name]=value;
        using var process=Process.Start(info)!;
        var output=process.StandardOutput.ReadToEndAsync(); var error=process.StandardError.ReadToEndAsync();
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch { if(!process.HasExited) process.Kill(true); throw; }
        return (process.ExitCode,await output+await error);
    }

    private const string FakeDocker="""
        #!/usr/bin/env bash
        set -eu
        record() { printf '%s\n' "$1" >> "$GQ_FLOW_DIR/events"; }
        case "$1" in
          compose)
            if [[ "$*" == *'config --services'* ]]; then printf 'neo4j\nweb\n'
            elif [[ "${*: -1}" == neo4j ]]; then echo fake-neo
            else echo fake-web; fi ;;
          inspect)
            if [[ "$*" == *geoquad.backup-run* ]]; then exit 1
            elif [[ "$*" == *com.docker.compose.project* ]]; then echo geoquad-b-tests
            elif [[ "$*" == *geoquad.test-isolation* ]]; then echo B-only
            elif [[ "$*" == *'.Image'* ]]; then printf 'sha256:%064d\n' 0
            elif [[ "$*" == *'.Mounts'* ]]; then echo geoquad-b-tests_b-test-data
            elif [[ "$*" == *'.State.Running'* ]]; then
              if [[ "$GQ_FLOW_MODE" == originally-stopped || -e "$GQ_FLOW_DIR/stopped" ]]; then echo false; else echo true; fi
            elif [[ "$*" == *'.State.Health'* ]]; then
              if [[ "$GQ_FLOW_MODE" == recovery-unhealthy && -e "$GQ_FLOW_DIR/stopped" ]]; then record unhealthy; echo unhealthy
              else record healthy; echo healthy; fi
            else exit 2; fi ;;
          exec) printf 'n\n1\n' ;;
          run)
            if [[ "$*" == *'database dump'* ]]; then record dump; exit 42; fi ;;
          stop)
            if [[ "$2" == fake-neo ]]; then record 'stop neo'; touch "$GQ_FLOW_DIR/stopped"
            else record 'stop web'; fi ;;
          start)
            if [[ "$2" == fake-neo ]]; then record 'start neo'
            else record 'start web'; fi ;;
          *) exit 3 ;;
        esac
        """;
}
