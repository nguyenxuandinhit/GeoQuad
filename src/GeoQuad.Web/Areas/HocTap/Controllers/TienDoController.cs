using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần C hoàn thiện ở US-23, US-24.</summary>
[Area("HocTap")]
public sealed class TienDoController : Controller
{
    // SCR-16 · /HocTap/TienDo
    public IActionResult Index()
        => this.DangXayDung("Tiến độ của em", "US-23", "C", "Số bài đã làm, tỉ lệ đúng và khái niệm cần ôn.");

    // /HocTap/TienDo/GoiY
    public IActionResult GoiY()
        => this.DangXayDung("Bài tập gợi ý", "US-24", "C", "Bài tập nên làm tiếp dựa trên tiến độ của em.");
}
