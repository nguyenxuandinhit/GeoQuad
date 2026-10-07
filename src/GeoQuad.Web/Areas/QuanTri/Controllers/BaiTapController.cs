using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.QuanTri.Controllers;

/// <summary>
/// Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-25.
/// Chỉ quản trị viên vào được (US-06).
/// </summary>
[Area("QuanTri")]
[Authorize(Roles = VaiTro.QuanTri)]
public sealed class BaiTapController : Controller
{
    // SCR-18 · /QuanTri/BaiTap
    public IActionResult Index()
        => this.DangXayDung("Quản trị bài tập", "US-25", "B", "Danh sách bài tập, thêm, sửa và ẩn bài.");

    // SCR-19 · /QuanTri/BaiTap/Them
    public IActionResult Them()
        => this.DangXayDung("Thêm bài tập", "US-25", "B");

    // SCR-19 · /QuanTri/BaiTap/Sua/{ma}
    public IActionResult Sua(string id)
        => this.DangXayDung("Sửa bài tập", "US-25", "B");
}
