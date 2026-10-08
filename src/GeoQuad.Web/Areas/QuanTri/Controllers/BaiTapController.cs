using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Areas.QuanTri.Models;
using GeoQuad.Web.Areas.QuanTri.Repositories;
using GeoQuad.Web.Areas.QuanTri.Services;
using GeoQuad.Web.Infrastructure.Neo4j;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.QuanTri.Controllers;

/// <summary>
/// Quản lý bài tập US-25: validation, preview và lưu transaction.
/// Chỉ quản trị viên vào được (US-06).
/// </summary>
[Area("QuanTri")]
[Authorize(Roles = VaiTro.QuanTri)]
public sealed class BaiTapController(IBaiTapQuanTriRepository repo,ILogger<BaiTapController> logger) : Controller
{
    // SCR-18 · /QuanTri/BaiTap
    [HttpGet]
    public async Task<IActionResult> Index(int? lopLoc,string? loai,bool? hienThi,string? trangThai,string? tuKhoa)
    {
        if(!ModelState.IsValid || (lopLoc.HasValue && lopLoc.Value is < 1 or > 12) ||
            (loai != null && !KiemTraBaiTap.Loai.Contains(loai)) || (trangThai != null && !KiemTraBaiTap.TrangThai.Contains(trangThai)) ||
            tuKhoa?.Length > 300) return BadRequest("Bộ lọc không hợp lệ.");
        try { return View(new BaiTapDanhSach(await repo.DanhSachAsync(lopLoc,loai,hienThi,trangThai,tuKhoa),lopLoc,loai,hienThi,trangThai,tuKhoa)); }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }

    // SCR-19 · /QuanTri/BaiTap/Them
    [HttpGet]
    public async Task<IActionResult> Them()
    { try { return await SoanAsync(new(),false); } catch(Neo4jException ex) { return LoiKho(ex); } }

    [HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> Them(BaiTapForm form) => LuuAsync(null,form);

    // SCR-19 · /QuanTri/BaiTap/Sua/{ma}
    [HttpGet]
    public async Task<IActionResult> Sua(string id)
    {
        if(string.IsNullOrWhiteSpace(id) || id.Length > 64) return NotFound();
        try { var form=await repo.XemAsync(id); return form == null ? NotFound() : await SoanAsync(form,true); }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }
    [HttpPost,ValidateAntiForgeryToken]
    public Task<IActionResult> Sua(string id,BaiTapForm form) => LuuAsync(id,form);

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> XemTruoc(BaiTapForm form,string? id)
    {
        Validate(form);
        GiuMaKhiSua(id,form);
        try
        {
            var catalog=await repo.CatalogAsync();
            if(ModelState.IsValid && (!form.KhaiNiem.All(key => catalog.KhaiNiem.Any(k => k.Ma == key)) || !form.SuDung.All(key => catalog.SuDung.Any(k => k.Ma == key))))
                ModelState.AddModelError("","Có kiến thức không tồn tại hoặc chưa rà soát.");
            return await SoanAsync(form,id != null,ModelState.IsValid,catalog);
        }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> HienThi(string id,bool? hienThi)
    {
        if(!ModelState.IsValid || !hienThi.HasValue || string.IsNullOrWhiteSpace(id) || id.Length > 64) return BadRequest("Mã bài/hiển thị không hợp lệ.");
        try { await repo.HienThiAsync(id,hienThi.Value); return RedirectToAction(nameof(Index)); }
        catch(KeyNotFoundException) { return NotFound(); }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }

    private KiemTraBaiTapKetQua Validate(BaiTapForm form)
    {
        var result=KiemTraBaiTap.KiemTra(form);
        foreach(var (key,message) in result.Loi) ModelState.AddModelError(key,message);
        return result;
    }
    private async Task<IActionResult> LuuAsync(string? id,BaiTapForm form)
    {
        var result=Validate(form);
        GiuMaKhiSua(id,form);
        try
        {
            if(ModelState.IsValid)
            {
                try { await repo.LuuAsync(result.BaiTap!,id != null); return RedirectToAction(nameof(Index)); }
                catch(ArgumentException ex) { ModelState.AddModelError("",ex.Message); }
                catch(KeyNotFoundException) { return NotFound(); }
                catch(Neo4jException ex) when(GraphDbErrors.IsUniqueViolation(ex)) { ModelState.AddModelError("Ma","Mã bài đã tồn tại."); }
            }
            return await SoanAsync(form,id != null);
        }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }
    private async Task<IActionResult> SoanAsync(BaiTapForm form,bool sua,bool preview=false,QuanTriCatalog? catalog=null)
    {
        ViewData["Catalog"]=catalog ?? await repo.CatalogAsync();
        ViewData["Sua"]=sua; ViewData["XemTruoc"]=preview;
        return View("Soan",form);
    }
    private void GiuMaKhiSua(string? id,BaiTapForm form)
    {
        if(id == null || id == form.Ma) return;
        ModelState.AddModelError("Ma","Mã bài không được thay đổi khi sửa.");
        form.Ma=id;
        ModelState.SetModelValue("Ma",id,id);
    }
    private ObjectResult LoiKho(Neo4jException ex)
    {
        logger.LogError(ex,"Không truy cập được bài tập quản trị.");
        return StatusCode(503,"Chưa kết nối được kho bài tập. Vui lòng thử lại sau.");
    }
}
