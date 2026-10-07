using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần A hoàn thiện ở US-15, US-16.</summary>
[Area("KienThuc")]
public sealed class MayTinhController : Controller
{
    // SCR-10 · /KienThuc/MayTinh?hinh=&daiLuong=
    public IActionResult Index(string? hinh, string? daiLuong)
        => this.DangXayDung("Máy tính hình học", "US-15", "A", "Tính chu vi, diện tích, đường chéo và vẽ hình theo số liệu em nhập.");
}
