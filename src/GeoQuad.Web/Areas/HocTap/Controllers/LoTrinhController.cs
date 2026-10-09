using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

[Area("HocTap")]
[Authorize]
public sealed class LoTrinhController : Controller
{
    private readonly LoTrinhService _service;
    private readonly ICurrentUser _user;

    public LoTrinhController(LoTrinhService service, ICurrentUser user) => (_service, _user) = (service, user);

    [HttpGet]
    public async Task<IActionResult> Index(string? muc)
    {
        if (string.IsNullOrWhiteSpace(_user.TaiKhoanId)) return Challenge();
        return View(await _service.XemAsync(muc));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmDaHieu(string? muc, string? ma)
    {
        if (string.IsNullOrWhiteSpace(_user.TaiKhoanId)) return Challenge();
        await _service.EmDaHieuAsync(muc, ma);
        return RedirectToAction(nameof(Index), new { muc });
    }
}