using GeoQuad.Web.Areas.ApDung;
using GeoQuad.Web.Areas.HocTap;
using GeoQuad.Web.Areas.KienThuc;
using GeoQuad.Web.Areas.QuanTri;
using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Infrastructure.Neo4j;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ---- Neo4j (US-01) ----
builder.Services.Configure<Neo4jOptions>(builder.Configuration.GetSection(Neo4jOptions.Section));
builder.Services.PostConfigure<Neo4jOptions>(options =>
{
    var uri = Environment.GetEnvironmentVariable("NEO4J_URI")
           ?? Environment.GetEnvironmentVariable("Neo4j_Uri")
           ?? Environment.GetEnvironmentVariable("NEO4J_URL");
    if (!string.IsNullOrWhiteSpace(uri)) options.Uri = uri;

    var user = Environment.GetEnvironmentVariable("NEO4J_USER")
            ?? Environment.GetEnvironmentVariable("Neo4j_User")
            ?? Environment.GetEnvironmentVariable("NEO4J_USERNAME");
    if (!string.IsNullOrWhiteSpace(user)) options.User = user;

    var pass = Environment.GetEnvironmentVariable("NEO4J_PASSWORD")
            ?? Environment.GetEnvironmentVariable("Neo4j_Password");
    if (!string.IsNullOrWhiteSpace(pass)) options.Password = pass;

    var db = Environment.GetEnvironmentVariable("NEO4J_DATABASE")
          ?? Environment.GetEnvironmentVariable("Neo4j_Database")
          ?? Environment.GetEnvironmentVariable("NEO4J_DB");
    if (!string.IsNullOrWhiteSpace(db)) options.Database = db;
});
builder.Services.AddSingleton<IDriver>(sp =>
{
    var o = sp.GetRequiredService<IOptions<Neo4jOptions>>().Value;
    Console.WriteLine($"[Neo4j] Đang kết nối tới: {o.Uri} | User: {o.User} | Database: {o.Database}");
    return GraphDatabase.Driver(o.Uri, AuthTokens.Basic(o.User, o.Password));
});
builder.Services.AddSingleton<IGraphDb, GraphDb>();

// ---- Xác thực bằng cookie (US-05, US-06) ----
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<ITaiKhoanRepository, TaiKhoanRepository>();
builder.Services.AddScoped<DemoAccountSeeder>();

// ---- Người đang dùng: học sinh đã đăng nhập hoặc khách (US-07) ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "gq_auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.IsEssential = true;
        o.LoginPath = "/TaiKhoan/DangNhap";
        o.LogoutPath = "/TaiKhoan/DangXuat";
        o.AccessDeniedPath = "/Home/TuChoi";   // PathString không nhận chuỗi truy vấn
        o.ExpireTimeSpan = TimeSpan.FromDays(30);
        o.SlidingExpiration = true;
    });

// ---- Các Area của A, B, C: PHẦN 0 gọi sẵn, A/B/C không sửa Program.cs ----
builder.Services.AddKienThuc();   // Phần A – Tra cứu & trực quan
builder.Services.AddApDung();     // Phần B – Áp dụng kiến thức
builder.Services.AddQuanTri();    // Phần B – Quản trị bài tập
builder.Services.AddHocTap();     // Phần C – Học tập cá nhân

var app = builder.Build();

// ---- Tài khoản demo, chỉ ở Development (US-06) ----
if (app.Environment.IsDevelopment())
{
    using var pham = app.Services.CreateScope();
    var seeder = pham.ServiceProvider.GetRequiredService<DemoAccountSeeder>();
    try
    {
        await seeder.ChayAsync();
    }
    catch (Exception ex)
    {
        // Chưa có Neo4j hoặc chưa chạy seed khung thì vẫn cho ứng dụng khởi động.
        app.Logger.LogWarning(ex,
            "Chưa tạo được tài khoản demo. Em chạy \"docker compose up -d neo4j\" và "
            + "\"scripts/seed\" rồi khởi động lại.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Loi");
}

app.UseStatusCodePagesWithReExecute("/Home/Loi", "?maTrangThai={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
