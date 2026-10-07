using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Infrastructure.Neo4j;
using GeoQuad.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Controllers;

/// <summary>Đăng ký (US-05), đăng nhập và đăng xuất (US-06) — SCR-02, SCR-03.</summary>
public sealed class TaiKhoanController : Controller
{
    private readonly ITaiKhoanRepository _repo;
    private readonly IAuthHelper _auth;
    private readonly ILogger<TaiKhoanController> _logger;

    public TaiKhoanController(
        ITaiKhoanRepository repo, IAuthHelper auth, ILogger<TaiKhoanController> logger)
    {
        _repo = repo;
        _auth = auth;
        _logger = logger;
    }

    // SCR-02 · GET /TaiKhoan/DangKy
    [HttpGet]
    public IActionResult DangKy() => View(new DangKyViewModel());

    // SCR-02 · POST /TaiKhoan/DangKy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(DangKyViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var ten = QuyTacTaiKhoan.ChuanHoaTen(form.TenDangNhap);
        if (!QuyTacTaiKhoan.TenHopLe(ten))
        {
            ModelState.AddModelError(nameof(form.TenDangNhap),
                "Tên đăng nhập chỉ gồm chữ không dấu, số và dấu gạch dưới (_), dài 4–20 ký tự.");
            return View(form);
        }

        var bam = _auth.BamMatKhau(form.MatKhau);

        string? id;
        try
        {
            id = await _repo.TaoAsync(ten, bam, form.BietDanh.Trim(), form.Lop);
        }
        catch (Exception ex) when (GraphDbErrors.IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.TenDangNhap),
                "Tên đăng nhập đã tồn tại. Em chọn tên khác nhé.");
            return View(form);
        }

        if (id is null)
        {
            // Không tìm thấy nút Lop → dữ liệu khung chưa được nạp.
            ModelState.AddModelError(string.Empty,
                "Chưa có dữ liệu lớp trong Neo4j. Em chạy \"scripts/seed\" rồi đăng ký lại nhé.");
            _logger.LogWarning("Đăng ký thất bại: không có nút Lop {Lop}", form.Lop);
            return View(form);
        }

        // Đăng ký xong tự đăng nhập, về trang chủ.
        await _auth.DangNhapAsync(
            HttpContext,
            new TaiKhoanAuth(id, ten, form.BietDanh.Trim(), VaiTro.HocSinh, form.Lop));

        return RedirectToAction("Index", "Home");
    }
}
