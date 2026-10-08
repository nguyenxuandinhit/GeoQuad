using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using GeoQuad.Tests.B_ApDung.Integration;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.B_ApDung.Http;

/// <summary>Program thật ở Production: cookie, middleware role và CSRF thật.</summary>
public sealed class MayChuThu : IAsyncDisposable
{
    private readonly Process _process;
    private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<string> _errors = [];
    private readonly string _keyDirectory;
    public CookieContainer Cookies { get; } = new();
    public HttpClient Client { get; }

    private MayChuThu(Process process, string keyDirectory)
    {
        _process = process;
        _keyDirectory = keyDirectory;
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data?.Contains("Now listening on: http://127.0.0.1:15080", StringComparison.Ordinal) == true)
                _listening.TrySetResult();
        };
        process.ErrorDataReceived += (_, args) => { if (args.Data != null) lock (_errors) _errors.Add(args.Data); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        Client = new HttpClient(new HttpClientHandler { CookieContainer = Cookies, AllowAutoRedirect = false })
            { BaseAddress = new Uri("http://127.0.0.1:15080"), Timeout = TimeSpan.FromSeconds(10) };
    }

    public static async Task<MayChuThu> StartAsync(Neo4jFixture fixture)
    {
        await fixture.GuardAsync();
        // Isolate dotnet CLI scratch state; authentication still uses the real middleware.
        var keys = Path.Combine(Path.GetTempPath(), "geoquad-b-http-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(keys);
        var info = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            WorkingDirectory = Neo4jFixture.Root, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var arg in new[] { "run", "--no-build", "--no-launch-profile", "--project", "src/GeoQuad.Web",
                     "--", "--urls", "http://127.0.0.1:15080" }) info.ArgumentList.Add(arg);
        info.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        info.Environment["DOTNET_ENVIRONMENT"] = "Production";
        info.Environment["Neo4j__Uri"] = fixture.Uri;
        info.Environment["Neo4j__User"] = fixture.User;
        info.Environment["Neo4j__Password"] = fixture.Password;
        info.Environment["Neo4j__Database"] = "neo4j";
        info.Environment["DOTNET_CLI_HOME"] = keys;
        var server = new MayChuThu(Process.Start(info) ?? throw new InvalidOperationException("Không mở được web test."), keys);
        try
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                if (server._process.HasExited)
                {
                    string error;
                    lock (server._errors) error = string.Join(Environment.NewLine, server._errors);
                    throw new InvalidOperationException("Web test dừng: " + error);
                }
                // Never send credentials to a pre-existing process occupying the test port.
                if (server._listening.Task.IsCompletedSuccessfully)
                {
                    try
                    {
                        using var response = await server.Client.GetAsync("/");
                        if (response.IsSuccessStatusCode) return server;
                    }
                    catch (HttpRequestException) { }
                }
                await Task.Delay(200);
            }
            throw new TimeoutException("Web test không sẵn sàng trong 20 giây.");
        }
        catch { await server.DisposeAsync(); throw; }
    }

    public async Task CreateAccountAsync(Neo4jFixture fixture, string name, string role, int grade)
    {
        await fixture.GuardAsync();
        await new TaiKhoanRepository(fixture.Db).TaoNeuChuaCoAsync(name,
            new AuthHelper().BamMatKhau("Test_B_123!"), name, role, grade);
    }

    public async Task<HttpResponseMessage> LoginAsync(string name)
    {
        using var page = await Client.GetAsync("/TaiKhoan/DangNhap");
        var token = AntiForgeryToken(await page.Content.ReadAsStringAsync());
        return await Client.PostAsync("/TaiKhoan/DangNhap", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["TenDangNhap"] = name, ["MatKhau"] = "Test_B_123!", ["__RequestVerificationToken"] = token }));
    }

    public static string AntiForgeryToken(string html)
    {
        var tag = Regex.Match(html, "<input\\b[^>]*name=\"__RequestVerificationToken\"[^>]*>", RegexOptions.IgnoreCase);
        var value = Regex.Match(tag.Value, "\\bvalue=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        if (!value.Success) throw new InvalidOperationException("Không có CSRF token trên form thật.");
        return WebUtility.HtmlDecode(value.Groups[1].Value);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        if (!_process.HasExited) _process.Kill(entireProcessTree: true);
        await _process.WaitForExitAsync();
        _process.Dispose();
        Directory.Delete(_keyDirectory, recursive: true);
    }
}
