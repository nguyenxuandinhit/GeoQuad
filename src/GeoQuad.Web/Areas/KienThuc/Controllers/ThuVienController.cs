using GeoQuad.Web.Areas.KienThuc.Services;
using GeoQuad.Web.Infrastructure.Trang;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.KienThuc.Controllers;

/// <summary>Thư viện kiến thức (US-09), chi tiết khái niệm (US-10), tìm kiếm (US-11).</summary>
[Area("KienThuc")]
public sealed class ThuVienController : Controller
{
    private readonly IThuVienService _thuVien;
    private readonly IChiTietService _chiTiet;

    public ThuVienController(IThuVienService thuVien, IChiTietService chiTiet)
    {
        _thuVien = thuVien;
        _chiTiet = chiTiet;
    }

    // SCR-04 · /KienThuc/ThuVien?loai=&cap=&lop=
    public async Task<IActionResult> Index(string? loai, string? cap, int? lop)
        => View(await _thuVien.DuyetAsync(loai, cap, lop));

    // SCR-05 · /KienThuc/ThuVien/ChiTiet/{ma}
    public async Task<IActionResult> ChiTiet(string id, bool moi = false)
    {
        var vm = await _chiTiet.LayAsync(id, moiDangNhap: moi);

        // Mã không tồn tại → trang 404 thân thiện của PHẦN 0.
        return vm is null ? NotFound() : View(vm);
    }

    // "Em đã hiểu" · POST /KienThuc/ThuVien/DaHieu/{ma}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DaHieu(string id)
    {
        var ketQua = await _chiTiet.DaHieuAsync(id);

        return ketQua switch
        {
            // Khách: quay lại trang chi tiết và hiện khối mời đăng nhập (BR-09).
            KetQuaDaHieu.MoiDangNhap => RedirectToAction(nameof(ChiTiet), new { id, moi = true }),
            KetQuaDaHieu.DaGhiNhan => RedirectToAction(nameof(ChiTiet), new { id }),
            _ => NotFound()
        };
    }

    // SCR-06 · /KienThuc/ThuVien/TimKiem?q=
    public IActionResult TimKiem(string? q)
        => this.DangXayDung("Kết quả tìm kiếm", "US-11", "A", "Tìm kiếm kiến thức, gõ có dấu hay không dấu đều được.");
}
