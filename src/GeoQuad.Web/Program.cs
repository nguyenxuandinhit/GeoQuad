using GeoQuad.Web.Areas.ApDung;
using GeoQuad.Web.Areas.HocTap;
using GeoQuad.Web.Areas.KienThuc;
using GeoQuad.Web.Areas.QuanTri;
using GeoQuad.Web.Infrastructure.Neo4j;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ---- Neo4j (US-01) ----
builder.Services.Configure<Neo4jOptions>(builder.Configuration.GetSection(Neo4jOptions.Section));
builder.Services.AddSingleton<IDriver>(sp =>
{
    var o = sp.GetRequiredService<IOptions<Neo4jOptions>>().Value;
    return GraphDatabase.Driver(o.Uri, AuthTokens.Basic(o.User, o.Password));
});
builder.Services.AddSingleton<IGraphDb, GraphDb>();

// ---- Các Area của A, B, C: PHẦN 0 gọi sẵn, A/B/C không sửa Program.cs ----
builder.Services.AddKienThuc();   // Phần A – Tra cứu & trực quan
builder.Services.AddApDung();     // Phần B – Áp dụng kiến thức
builder.Services.AddQuanTri();    // Phần B – Quản trị bài tập
builder.Services.AddHocTap();     // Phần C – Học tập cá nhân

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Loi");
}

app.UseStatusCodePagesWithReExecute("/Home/Loi", "?maTrangThai={0}");
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
