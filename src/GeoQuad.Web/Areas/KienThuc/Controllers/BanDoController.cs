using GeoQuad.Web.Areas.KienThuc.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Bản đồ kiến thức (US-12).</summary>
[Area("KienThuc")]
public sealed class BanDoController : Controller
{
    private readonly IBanDoService _banDo;

    public BanDoController(IBanDoService banDo) => _banDo = banDo;

    // SCR-07 · /KienThuc/BanDo?lop=&tu=&den=
    public async Task<IActionResult> Index(int? lop, string? tu, string? den)
        => View(await _banDo.LayTrangAsync(lop, tu, den));

    // /KienThuc/BanDo/DuLieu?lop= — dữ liệu JSON cho Cytoscape.js
    public async Task<IActionResult> DuLieu(int? lop)
        => Json(await _banDo.LayDuLieuAsync(lop));
}
