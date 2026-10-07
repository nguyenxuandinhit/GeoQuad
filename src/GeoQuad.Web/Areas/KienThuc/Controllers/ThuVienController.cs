using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần A hoàn thiện ở US-09, US-10, US-11.</summary>
[Area("KienThuc")]
public sealed class ThuVienController : Controller
{
    // SCR-04 · /KienThuc/ThuVien?loai=&cap=&lop=
    public IActionResult Index()
        => this.DangXayDung("Thư viện kiến thức", "US-09", "A", "Duyệt hình, tính chất, dấu hiệu và công thức theo cấp và lớp.");

    // SCR-05 · /KienThuc/ThuVien/ChiTiet/{ma}
    public IActionResult ChiTiet(string id)
        => this.DangXayDung("Chi tiết khái niệm", "US-10", "A", "Định nghĩa, tính chất, dấu hiệu nhận biết, công thức và ví dụ thực tế.");

    // "Em đã hiểu" · POST /KienThuc/ThuVien/DaHieu/{ma}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DaHieu(string id)
        => this.DangXayDung("Em đã hiểu", "US-10", "A", "Ghi nhận khái niệm em đã học.");

    // SCR-06 · /KienThuc/ThuVien/TimKiem?q=
    public IActionResult TimKiem(string? q)
        => this.DangXayDung("Kết quả tìm kiếm", "US-11", "A", "Tìm kiếm kiến thức, gõ có dấu hay không dấu đều được.");
}
