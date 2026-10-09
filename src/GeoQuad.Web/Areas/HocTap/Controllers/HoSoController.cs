using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>
/// Hồ sơ học sinh: cập nhật biệt danh/lớp, đổi mật khẩu và xóa tài khoản (US-08, FR-04, SCR-17, NFR-06).
/// </summary>
[Area("HocTap")]
[Authorize]
public sealed class HoSoController : Controller
{
    private readonly IHoSoService _hoSoService;
    private readonly ICurrentUser _currentUser;
    private readonly IAuthHelper _authHelper;

    public HoSoController(
        IHoSoService hoSoService,
        ICurrentUser currentUser,
        IAuthHelper authHelper)
    {
        _hoSoService = hoSoService;
        _currentUser = currentUser;
        _authHelper = authHelper;
    }

    // SCR-17 · GET /HocTap/HoSo
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var id = _currentUser.TaiKhoanId;
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }

        var vm = await _hoSoService.LayHoSoAsync(id);
        if (vm is null)
        {
            await _authHelper.DangXuatAsync(HttpContext);
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }

        if (TempData["ThongBao"] is string tb)
        {
            vm.ThongBaoThanhCong = tb;
        }

        if (TempData["Loi"] is string l)
        {
            vm.ThongBaoLoi = l;
        }

        return View(vm);
    }

    // POST /HocTap/HoSo/DoiThongTin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiThongTin([Bind(Prefix = "DoiThongTin")] DoiThongTinInput form)
    {
        var id = _currentUser.TaiKhoanId;
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }

        if (!ModelState.IsValid)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.DoiThongTin = form;
                return View(nameof(Index), vm);
            }
            return RedirectToAction(nameof(Index));
        }

        var kq = await _hoSoService.DoiThongTinAsync(id, form);
        if (!kq.ThanhCong)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.DoiThongTin = form;
                vm.ThongBaoLoi = kq.Loi;
                return View(nameof(Index), vm);
            }
            TempData["Loi"] = kq.Loi;
            return RedirectToAction(nameof(Index));
        }

        // Cập nhật lại Cookie Authentication với lớp và biệt danh mới (US-08)
        if (kq.TaiKhoanMoi != null)
        {
            await _authHelper.DangNhapAsync(HttpContext, kq.TaiKhoanMoi);
        }

        TempData["ThongBao"] = "Cập nhật biệt danh và lớp học thành công!";
        return RedirectToAction(nameof(Index));
    }

    // POST /HocTap/HoSo/DoiMatKhau
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau([Bind(Prefix = "DoiMatKhau")] DoiMatKhauInput form)
    {
        var id = _currentUser.TaiKhoanId;
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }

        if (!ModelState.IsValid)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.DoiMatKhau = form;
                return View(nameof(Index), vm);
            }
            return RedirectToAction(nameof(Index));
        }

        var kq = await _hoSoService.DoiMatKhauAsync(id, form);
        if (!kq.ThanhCong)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.DoiMatKhau = form;
                vm.ThongBaoLoi = kq.Loi;
                return View(nameof(Index), vm);
            }
            TempData["Loi"] = kq.Loi;
            return RedirectToAction(nameof(Index));
        }

        TempData["ThongBao"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Index));
    }

    // POST /HocTap/HoSo/XoaTaiKhoan
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaTaiKhoan([Bind(Prefix = "XoaTaiKhoan")] XoaTaiKhoanInput form)
    {
        var id = _currentUser.TaiKhoanId;
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("DangNhap", "TaiKhoan", new { area = "" });
        }

        if (!ModelState.IsValid)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.XoaTaiKhoan = form;
                return View(nameof(Index), vm);
            }
            return RedirectToAction(nameof(Index));
        }

        var kq = await _hoSoService.XoaTaiKhoanAsync(id, form);
        if (!kq.ThanhCong)
        {
            var vm = await _hoSoService.LayHoSoAsync(id);
            if (vm != null)
            {
                vm.XoaTaiKhoan = form;
                vm.ThongBaoLoi = kq.Loi;
                return View(nameof(Index), vm);
            }
            TempData["Loi"] = kq.Loi;
            return RedirectToAction(nameof(Index));
        }

        // Xóa xong đăng xuất và về trang chủ
        await _authHelper.DangXuatAsync(HttpContext);
        TempData["ThongBao"] = "Tài khoản của em đã được xóa hoàn toàn khỏi hệ thống.";
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}
