using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-14.</summary>
[Area("ApDung")]
public sealed class ChungMinhController : Controller
{
    // SCR-09 · /ApDung/ChungMinh/Xem/{ma}
    public IActionResult Xem(string id)
        => this.DangXayDung("Chứng minh mẫu", "US-14", "B", "Các bước chứng minh kèm căn cứ của từng bước.");
}
