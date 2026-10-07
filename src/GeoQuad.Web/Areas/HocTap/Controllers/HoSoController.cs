using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần C hoàn thiện ở US-08.</summary>
[Area("HocTap")]
public sealed class HoSoController : Controller
{
    // SCR-17 · /HocTap/HoSo
    public IActionResult Index()
        => this.DangXayDung("Hồ sơ của em", "US-08", "C", "Đổi biệt danh, đổi lớp, đổi mật khẩu và xóa tài khoản.");
}
