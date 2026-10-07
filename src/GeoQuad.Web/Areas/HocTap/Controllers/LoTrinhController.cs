using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần C hoàn thiện ở US-22.</summary>
[Area("HocTap")]
public sealed class LoTrinhController : Controller
{
    // SCR-15 · /HocTap/LoTrinh?muc={ma}
    public IActionResult Index(string? muc)
        => this.DangXayDung("Lộ trình học", "US-22", "C", "Thứ tự học các khái niệm cần biết trước để đạt mục tiêu.");
}
