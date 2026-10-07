using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần A hoàn thiện ở US-12.</summary>
[Area("KienThuc")]
public sealed class BanDoController : Controller
{
    // SCR-07 · /KienThuc/BanDo
    public IActionResult Index()
        => this.DangXayDung("Bản đồ kiến thức", "US-12", "A", "Sơ đồ hình nào là trường hợp đặc biệt của hình nào.");

    // /KienThuc/BanDo/DuLieu?lop=
    public IActionResult DuLieu(int? lop)
        => this.DangXayDung("Dữ liệu bản đồ kiến thức", "US-12", "A");
}
