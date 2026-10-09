using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Trang;
using GeoQuad.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.HocTap.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần C hoàn thiện ở US-19, US-20, US-21, US-24.</summary>
[Area("HocTap")]
public sealed class BaiTapController : Controller
{
    private readonly BaiTapService _baiTap;
    private readonly BaiTapLamService _lamBai;
    private readonly ICurrentUser _user;
    private readonly GoiYService _goiY;

    public BaiTapController(BaiTapService baiTap, BaiTapLamService lamBai, ICurrentUser user, GoiYService goiY)
        => (_baiTap, _lamBai, _user, _goiY) = (baiTap, lamBai, user, goiY);

    // SCR-13 · /HocTap/BaiTap?cap=&lop=&loai=&doKho=&khaiNiem=
    public async Task<IActionResult> Index(string? cap, int? lop, string? loai, int? doKho, string? khaiNiem)
    {
        var model = await _baiTap.DanhSachAsync(new BaiTapLoc(cap, lop, loai, doKho, khaiNiem));
        // Model binding rejects malformed numbers; do not silently widen the filter.
        if (!ModelState.IsValid)
            model = model with { BaiTap = [], Loi = "Bộ lọc không hợp lệ. Em chọn lại lớp hoặc độ khó nhé." };
        return View(model);
    }

    // SCR-14 · /HocTap/BaiTap/Lam/{ma}
    [HttpGet]
    public async Task<IActionResult> Lam(string id)
    {
        var nonce = _user.DaDangNhap ? null : LayHoacTaoGuestNonce();
        var (bai, token, proof) = await _lamBai.MoAsync(id, nonce);
        if (bai is null || token is null) return NotFound();
        return View(new LamBaiViewModel(bai.Ma, bai.De, bai.Loai, bai.Lop, bai.DoKho,
            bai.PhuongAn, token.Token, bai.DonVi, proof));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lam(string id, BaiTapNopInput form)
    {
        if (!ModelState.IsValid) return await LoiBaiAsync(id, form.Token, "Dữ liệu nộp bài chưa hợp lệ.");
        var result = await _lamBai.NopAsync(id, form, Request.Cookies["gq_lam_nonce"]);
        if (result.XungDot) return Conflict(result.Loi);
        if (!result.HopLe) return await LoiBaiAsync(id, form.Token, result.Loi);

        if (_user.DaDangNhap) return RedirectToAction(nameof(KetQua), new { token = form.Token });
        TempData["Bai"] = id;
        TempData["Token"] = form.Token;
        TempData["DapAn"] = result.DapAnDaChon;
        TempData["Dung"] = result.Dung;
        return RedirectToAction(nameof(KetQua));
    }

    [HttpGet]
    public async Task<IActionResult> KetQua(string? token)
    {
        if (_user.TaiKhoanId is { } account && !string.IsNullOrEmpty(token))
        {
            if (!_lamBai.TryReadToken(token, null, out var attempt)) return NotFound();
            var model = await _lamBai.KetQuaTaiKhoanAsync(attempt.MaLan);
            return model is null ? NotFound() : View("KetQua", model);
        }
        var ma = TempData["Bai"] as string;
        var guestToken = TempData["Token"] as string;
        var answer = TempData["DapAn"] as string ?? "";
        var correct = TempData["Dung"] is bool value && value;
        if (ma is null || guestToken is null) return RedirectToAction(nameof(Index));
        var guest = await _lamBai.KetQuaKhachAsync(ma, guestToken, Request.Cookies["gq_lam_nonce"], answer, correct);
        return guest is null ? NotFound() : View("KetQua", guest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XemLoiGiai(string id)
    {
        var model = await _lamBai.LoiGiaiMauAsync(id);
        return model is null ? NotFound() : View("LoiGiaiMau", model);
    }

    // /HocTap/BaiTap/KeTiep/{ma}
    public async Task<IActionResult> KeTiep(string id)
    {
        if (_user.DaDangNhap)
        {
            var nextForStudent = await _goiY.KeTiepAsync(id);
            return nextForStudent is null ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Lam), new { id = nextForStudent });
        }
        var next = await _lamBai.BaiTiepTheoKhachAsync(id);
        return next is null ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Lam), new { id = next });
    }

    private async Task<IActionResult> LoiBaiAsync(string id, string token, string loi)
    {
        if (!_lamBai.TryReadToken(token, Request.Cookies["gq_lam_nonce"], out _)) return BadRequest(loi);
        var (bai, _, proof) = await _lamBai.MoAsync(id, Request.Cookies["gq_lam_nonce"]);
        return bai is null ? NotFound() : View(nameof(Lam), new LamBaiViewModel(bai.Ma, bai.De,
            bai.Loai, bai.Lop, bai.DoKho, bai.PhuongAn, token, bai.DonVi, proof, loi));
    }

    private string LayHoacTaoGuestNonce()
    {
        if (Request.Cookies.TryGetValue("gq_lam_nonce", out var existing) && !string.IsNullOrWhiteSpace(existing)) return existing;
        var nonce = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        Response.Cookies.Append("gq_lam_nonce", nonce, new Microsoft.AspNetCore.Http.CookieOptions
        {
            HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
            Secure = Request.IsHttps, IsEssential = true, MaxAge = TimeSpan.FromHours(24)
        });
        return nonce;
    }
}
