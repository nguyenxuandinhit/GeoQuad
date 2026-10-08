using GeoQuad.Web.Areas.KienThuc.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Máy tính hình học (US-15) và vẽ hình theo số liệu (US-16).</summary>
[Area("KienThuc")]
public sealed class MayTinhController : Controller
{
    /// <summary>Mọi biến có thể xuất hiện trong công thức (xem <see cref="MayTinhHinhHoc"/>).</summary>
    private static readonly string[] CacBien = ["a", "b", "c", "d", "h", "d1", "d2"];

    private readonly IMayTinhService _mayTinh;

    public MayTinhController(IMayTinhService mayTinh) => _mayTinh = mayTinh;

    // SCR-10 · /KienThuc/MayTinh?hinh=&daiLuong=&a=&b=&h=&d1=&d2=&donVi=
    public async Task<IActionResult> Index(string? hinh, string? daiLuong, string? donVi)
    {
        // Đọc số đo từ query string. Dùng GET để em có thể lưu/chia sẻ lại phép tính.
        var soDo = CacBien.ToDictionary(
            b => b,
            b => Request.Query.TryGetValue(b, out var v) ? v.ToString() : null);

        return View(await _mayTinh.LayAsync(hinh, daiLuong, soDo, donVi));
    }
}
