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

    // SCR-03 · GET /TaiKhoan/DangNhap
    [HttpGet]
    public IActionResult DangNhap(string? tiepTuc, string? returnUrl)
        => View(new DangNhapViewModel { TiepTuc = tiepTuc ?? returnUrl });

    // SCR-03 · POST /TaiKhoan/DangNhap
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var ten = QuyTacTaiKhoan.ChuanHoaTen(form.TenDangNhap);
        var tk = await _repo.TimTheoTenAsync(ten);
        var bayGio = DateTimeOffset.Now;

        // Đang bị khóa: báo thời gian chờ, không thử mật khẩu.
        if (tk is not null && QuyTacTaiKhoan.DangBiKhoa(tk.KhoaDen, bayGio))
        {
            var phut = QuyTacTaiKhoan.SoPhutConPhaiCho(tk.KhoaDen, bayGio);
            ModelState.AddModelError(string.Empty,
                $"Tài khoản đang tạm khóa vì đăng nhập sai {QuyTacTaiKhoan.SoLanSaiToiDa} lần liên tiếp. "
                + $"Em thử lại sau {phut} phút nhé.");
            return View(form);
        }

        // Sai tên hoặc sai mật khẩu đều báo chung một thông báo (không tiết lộ tên nào có thật).
        if (tk is null || !_auth.KiemTraMatKhau(tk.MatKhauBam, form.MatKhau))
        {
            if (tk is not null)
            {
                var (soLanSai, khoaDen) = await _repo.GhiNhanSaiAsync(ten);

                // Lấy lại thời điểm hiện tại: khoaDen do Neo4j tính sau khi ghi, nếu so với
                // `bayGio` ở trên thì thời gian chờ bị làm tròn lên thừa một phút.
                var sauKhiGhi = DateTimeOffset.Now;
                if (QuyTacTaiKhoan.DangBiKhoa(khoaDen, sauKhiGhi))
                {
                    var phut = QuyTacTaiKhoan.SoPhutConPhaiCho(khoaDen, sauKhiGhi);
                    ModelState.AddModelError(string.Empty,
                        $"Em đã nhập sai {soLanSai} lần liên tiếp nên tài khoản bị tạm khóa. "
                        + $"Em thử lại sau {phut} phút nhé.");
                    return View(form);
                }
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(form);
        }

        await _repo.DatLaiSoLanSaiAsync(ten);
        await _auth.DangNhapAsync(
            HttpContext, new TaiKhoanAuth(tk.Id, tk.TenDangNhap, tk.BietDanh, tk.VaiTro, tk.Lop));

        if (!string.IsNullOrWhiteSpace(form.TiepTuc) && Url.IsLocalUrl(form.TiepTuc))
        {
            return Redirect(form.TiepTuc);
        }

        // Quản trị viên về trang quản trị bài tập, học sinh về trang chủ.
        return tk.VaiTro == VaiTro.QuanTri
            ? RedirectToAction("Index", "BaiTap", new { area = "QuanTri" })
            : RedirectToAction("Index", "Home");
    }

    // POST /TaiKhoan/DangXuat
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await _auth.DangXuatAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }

    // GET /TaiKhoan/DangXuat — mở bằng liên kết thì hiện nút xác nhận (đăng xuất phải là POST).
    [HttpGet]
    [ActionName("DangXuat")]
    public IActionResult DangXuatXacNhan() => View("DangXuat");
}
