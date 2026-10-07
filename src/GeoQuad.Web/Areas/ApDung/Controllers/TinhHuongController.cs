using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-17, US-18.</summary>
[Area("ApDung")]
public sealed class TinhHuongController : Controller
{
    // SCR-11 · /ApDung/TinhHuong?boiCanh=
    public IActionResult Index(string? boiCanh)
        => this.DangXayDung("Hình học quanh ta", "US-17", "B", "Tình huống thực tế trong nhà cửa, mảnh vườn và đồ dùng.");

    // SCR-12 · /ApDung/TinhHuong/Xem/{ma}
    public IActionResult Xem(string id)
        => this.DangXayDung("Tình huống thực tế", "US-17", "B", "Mô tả tình huống, kiến thức áp dụng và lời giải.");

    // SCR-12 phần thực hành đo đạc · POST /ApDung/TinhHuong/DoDac/{ma}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DoDac(string id)
        => this.DangXayDung("Thực hành đo đạc", "US-18", "B", "Nhập số đo em đo được, hệ thống kiểm tra kết luận.");
}
