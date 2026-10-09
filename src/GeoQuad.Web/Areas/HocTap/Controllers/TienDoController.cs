using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

[Area("HocTap")]
[Authorize]
public sealed class TienDoController(TienDoService service, GoiYService goiY, ICurrentUser user) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrWhiteSpace(user.TaiKhoanId)) return Challenge();
        var model = await service.XemAsync();
        return model is null ? Challenge() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GoiY()
    {
        if (string.IsNullOrWhiteSpace(user.TaiKhoanId)) return Challenge();
        var model = await goiY.XemAsync();
        return model is null ? Challenge() : View(model);
    }
}