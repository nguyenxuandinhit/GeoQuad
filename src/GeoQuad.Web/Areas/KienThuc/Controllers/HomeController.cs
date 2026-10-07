using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

[Area("KienThuc")]
public sealed class HomeController : Controller
{
    // Vào /KienThuc thì đưa về thư viện kiến thức.
    public IActionResult Index() => RedirectToAction("Index", "ThuVien", new { area = "KienThuc" });
}
