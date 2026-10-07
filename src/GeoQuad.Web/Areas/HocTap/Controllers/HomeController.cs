using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

[Area("HocTap")]
public sealed class HomeController : Controller
{
    // Vào /HocTap thì đưa về danh sách bài tập.
    public IActionResult Index() => RedirectToAction("Index", "BaiTap", new { area = "HocTap" });
}
