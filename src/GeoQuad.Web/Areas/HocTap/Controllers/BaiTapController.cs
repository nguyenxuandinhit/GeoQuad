using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần C hoàn thiện ở US-19, US-20, US-21, US-24.</summary>
[Area("HocTap")]
public sealed class BaiTapController : Controller
{
    // SCR-13 · /HocTap/BaiTap?cap=&lop=&loai=&doKho=&khaiNiem=
    public IActionResult Index(string? cap, int? lop, string? loai, int? doKho, string? khaiNiem)
        => this.DangXayDung("Bài tập", "US-19", "C", "Ngân hàng bài tập, lọc theo cấp, lớp, loại, độ khó và khái niệm.");

    // SCR-14 · /HocTap/BaiTap/Lam/{ma}
    public IActionResult Lam(string id)
        => this.DangXayDung("Làm bài tập", "US-20", "C", "Làm bài và được chấm tự động kèm giải thích.");

    // /HocTap/BaiTap/KeTiep/{ma}
    public IActionResult KeTiep(string id)
        => this.DangXayDung("Bài tập kế tiếp", "US-24", "C", "Gợi ý bài nên làm tiếp theo.");
}
