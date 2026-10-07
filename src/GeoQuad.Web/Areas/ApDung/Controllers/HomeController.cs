using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

[Area("ApDung")]
public sealed class HomeController : Controller
{
    // Vào /ApDung thì đưa về trang "Nên dùng định lý nào?".
    public IActionResult Index() => RedirectToAction("Index", "GoiY", new { area = "ApDung" });
}
