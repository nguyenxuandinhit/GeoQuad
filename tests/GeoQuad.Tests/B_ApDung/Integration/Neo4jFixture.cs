using System.Diagnostics;
using System.Text.Json;
using GeoQuad.Web.Infrastructure.Neo4j;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class Neo4jCollection : ICollectionFixture<Neo4jFixture>
{
    public const string Name = "Neo4j B isolated";
}

public sealed class Neo4jFixture : IAsyncLifetime
{
    public const string Project = "geoquad-b-tests";
    public static string Root { get; } = FindRoot();
    public static string ComposeFile => Path.Combine(Root,
        "tests/GeoQuad.Tests/B_ApDung/Integration/compose.neo4j.yml");
    private IDriver? _driver;
    public IGraphDb Db { get; private set; } = null!;
    public string Uri { get; private set; } = "";
    public string User { get; private set; } = "";
    public string Password { get; private set; } = "";

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("GQ_B_INTEGRATION") != "1")
            throw new InvalidOperationException("DB/HTTP B cần GQ_B_INTEGRATION=1 và Compose test riêng; không chạy trên DB dev.");
        Uri = Required("GQ_B_TEST_URI");
        User = Required("GQ_B_TEST_USER");
        Password = Required("GQ_B_TEST_PASSWORD");
        ValidateUri(Uri);
        await GuardAsync();
        _driver = GraphDatabase.Driver(Uri, AuthTokens.Basic(User, Password));
        await _driver.VerifyConnectivityAsync();
        Db = new GraphDb(_driver, Options.Create(new Neo4jOptions
        { Uri = Uri, User = User, Password = Password, Database = "neo4j" }));
        var components = await Db.ReadAsync("CALL dbms.components() YIELD versions RETURN versions[0] AS version");
        var version = Version.Parse(components.Single()["version"].As<string>());
        if (version < new Version(5, 9))
            throw new InvalidOperationException("Test B yêu cầu Neo4j >= 5.9.");
    }

    public static void ValidateUri(string value)
    {
        if (!System.Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != "bolt" || uri.Host != "127.0.0.1" || uri.Port != 17687 ||
            !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath is not ("" or "/") ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidOperationException("Chỉ cho phép bolt://127.0.0.1:17687 của container test B.");
    }

    public async Task GuardAsync()
    {
        var id = (await ComposeAsync("ps", "-q", "neo4j")).Trim();
        if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("Chưa có container test B.");
        using var json = JsonDocument.Parse(await RunAsync("docker", "inspect", id));
        var container = json.RootElement[0];
        var labels = container.GetProperty("Config").GetProperty("Labels");
        if (labels.GetProperty("com.docker.compose.project").GetString() != Project ||
            labels.GetProperty("geoquad.test-isolation").GetString() != "B-only")
            throw new InvalidOperationException("Container không mang marker isolation B.");
        var port = container.GetProperty("NetworkSettings").GetProperty("Ports").GetProperty("7687/tcp");
        if (port.GetArrayLength() != 1 || port[0].GetProperty("HostIp").GetString() != "127.0.0.1" ||
            port[0].GetProperty("HostPort").GetString() != "17687")
            throw new InvalidOperationException("Sai mapping cổng test B.");
        var data = container.GetProperty("Mounts").EnumerateArray().Single(m => m.GetProperty("Destination").GetString() == "/data");
        if (data.GetProperty("Type").GetString() != "volume" ||
            data.GetProperty("Name").GetString() != Project + "_b-test-data")
            throw new InvalidOperationException("Không được mutation volume dev.");
        var seed = container.GetProperty("Mounts").EnumerateArray().Single(m => m.GetProperty("Destination").GetString() == "/seed");
        var expectedSeed = Path.Combine(Root, "neo4j/seed");
        var mountedSeed = seed.GetProperty("Source").GetString();
        // Docker Desktop reports macOS bind mounts through the VM's /host_mnt prefix.
        if (seed.GetProperty("RW").GetBoolean() ||
            (mountedSeed != expectedSeed && mountedSeed != "/host_mnt" + expectedSeed))
            throw new InvalidOperationException("Container phải mount đúng seed checkout ở chế độ read-only.");
        using var volume = JsonDocument.Parse(await RunAsync("docker", "volume", "inspect", Project + "_b-test-data"));
        var volumeLabels = volume.RootElement[0].GetProperty("Labels");
        if (volumeLabels.GetProperty("com.docker.compose.project").GetString() != Project ||
            volumeLabels.GetProperty("geoquad.test-isolation").GetString() != "B-only")
            throw new InvalidOperationException("Volume không mang marker isolation B.");
    }

    public async Task ResetAsync()
    {
        await GuardAsync();
        await Db.WriteAsync("MATCH (n) DETACH DELETE n");
        await SeedAsync("00-schema.cypher", "01-khung.cypher");
    }

    public async Task SeedReviewedForTestsAsync()
    {
        await ResetAsync();
        await SeedAsync("10-kienthuc-A.cypher", "11-baitap-A.cypher", "12-tinhhuong-A.cypher", "20-dinhly-B.cypher", "21-chungminh-B.cypher");
        // Synthetic test state only. Repository seed remains NHAP awaiting the human C review.
        await Db.WriteAsync("""
            MATCH (n) WHERE n:DieuKien OR n:DinhLy OR n:ChungMinh OR n:Buoc
            SET n.trangThai='DA_RA_SOAT',n.reviewTestOnly=true
            """);
    }

    public async Task SeedAsync(params string[] files)
    {
        await GuardAsync();
        foreach (var file in files)
        {
            if (Path.GetFileName(file) != file) throw new ArgumentException("Seed phải là tên file.");
            var path = Path.Combine(Root, "neo4j/seed", file);
            if (!File.Exists(path)) throw new FileNotFoundException("Thiếu seed B: " + file, path);
            await ComposeAsync("exec", "-T", "neo4j", "cypher-shell", "-u", User, "-p", Password,
                "--fail-fast", "-f", "/seed/" + file);
        }
    }

    private static string Required(string name) => Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException("Thiếu cấu hình test " + name);

    public static Task<string> ComposeAsync(params string[] args) =>
        RunAsync("docker", new[] { "compose", "-p", Project, "-f", ComposeFile }.Concat(args).ToArray());

    public static async Task<string> RunAsync(string executable, params string[] args)
    {
        var info = new ProcessStartInfo(executable) { WorkingDirectory = Root,
            RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var arg in args) info.ArgumentList.Add(arg);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("Không chạy được " + executable);
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch { if (!process.HasExited) process.Kill(true); throw; }
        var text = await output;
        var stderr = await error;
        if (process.ExitCode != 0)
            throw new InvalidOperationException(executable + " thất bại: " + stderr);
        return text;
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GeoQuad.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Không tìm thấy GeoQuad.sln.");
    }

    public async Task DisposeAsync() { if (_driver != null) await _driver.DisposeAsync(); }
}
