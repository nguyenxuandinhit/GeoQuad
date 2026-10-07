using System.Diagnostics;
using GeoQuad.Web.Infrastructure;
using GeoQuad.Web.Infrastructure.Neo4j;
using GeoQuad.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace GeoQuad.Web.Controllers;

public sealed class HomeController : Controller
{
    // US-01: dòng "Kết nối Neo4j: OK – N nút" trên trang chủ.
    private const string DemSoNut = "MATCH (n) RETURN count(n) AS soNut";

    private readonly IGraphDb _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IGraphDb db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // SCR-01 · /
    public async Task<IActionResult> Index()
    {
        long soNut = 0;
        var ok = true;
        string? loi = null;

        try
        {
            var ban = await _db.ReadAsync(DemSoNut);
            soNut = ban.Count > 0 ? ban[0]["soNut"].As<long>() : 0;
        }
        catch (Exception ex)
        {
            ok = false;
            loi = "Chưa kết nối được Neo4j. Em mở terminal ở thư mục dự án rồi chạy "
                  + "\"docker compose up -d neo4j\", đợi khoảng 30 giây và tải lại trang.";
            _logger.LogWarning(ex, "Không kết nối được Neo4j khi mở trang chủ");
        }

        return View(new TrangChuViewModel
        {
            KetNoiOk = ok,
            SoNut = soNut,
            LoiKetNoi = loi
        });
    }

    // Bộ chọn lớp của khách · POST /Home/ChonLop
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChonLop(int lop, string? tiepTuc)
    {
        if (lop is >= QuyUoc.LopNhoNhat and <= QuyUoc.LopLonNhat)
        {
            Response.Cookies.Append(GqCookie.Lop, lop.ToString(), CookieBen());
        }

        return RedirectVeTrangTruoc(tiepTuc);
    }

    // Công tắc "Xem trước kiến thức nâng cao" · POST /Home/NangCao
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NangCao(bool bat, string? tiepTuc)
    {
        if (bat)
        {
            Response.Cookies.Append(GqCookie.NangCao, "1", CookieBen());
        }
        else
        {
            Response.Cookies.Delete(GqCookie.NangCao);
        }

        return RedirectVeTrangTruoc(tiepTuc);
    }

    // Trang lỗi thân thiện, dùng cho cả 404 và lỗi chưa xử lý.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Loi(int? maTrangThai)
    {
        var ma = maTrangThai ?? 500;
        var (tieuDe, moTa) = ma switch
        {
            404 => ("Không tìm thấy trang này",
                    "Có thể đường dẫn bị sai hoặc nội dung đã đổi. Em thử tìm kiếm hoặc về trang chủ nhé."),
            403 => ("Em không có quyền mở trang này",
                    "Trang này chỉ dành cho quản trị viên."),
            _ => ("Đã có lỗi xảy ra",
                    "Em thử tải lại trang. Nếu vẫn lỗi, kiểm tra Neo4j đã chạy chưa bằng \"docker compose ps\".")
        };

        Response.StatusCode = ma;
        return View(new LoiViewModel
        {
            MaTrangThai = ma,
            TieuDe = tieuDe,
            MoTa = moTa,
            MaYeuCau = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private static CookieOptions CookieBen() => new()
    {
        Expires = DateTimeOffset.UtcNow.AddDays(180),
        IsEssential = true,
        HttpOnly = false,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };

    /// <summary>Quay lại trang người dùng đang xem; chỉ nhận đường dẫn nội bộ (chống open redirect).</summary>
    private IActionResult RedirectVeTrangTruoc(string? tiepTuc)
        => !string.IsNullOrWhiteSpace(tiepTuc) && Url.IsLocalUrl(tiepTuc)
            ? Redirect(tiepTuc)
            : RedirectToAction(nameof(Index));
}
