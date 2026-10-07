using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.QuanTri.Controllers;

[Area("QuanTri")]
public sealed class HomeController : Controller
{
    // Vào /QuanTri thì đưa về danh sách bài tập.
    public IActionResult Index() => RedirectToAction("Index", "BaiTap", new { area = "QuanTri" });
}
