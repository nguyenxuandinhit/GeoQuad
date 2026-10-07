using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-13.</summary>
[Area("ApDung")]
public sealed class GoiYController : Controller
{
    // SCR-08 · /ApDung/GoiY
    public IActionResult Index()
        => this.DangXayDung("Nên dùng định lý nào?", "US-13", "B", "Chọn giả thiết em có, hệ thống gợi ý dấu hiệu nhận biết nên dùng.");
}
