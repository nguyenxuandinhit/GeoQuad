using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Areas.ApDung.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-17, US-18.</summary>
[Area("ApDung")]
public sealed class TinhHuongController(TinhHuongService service,DoDacRepository measurements,
    ICurrentUser user,ILogger<TinhHuongController> logger) : Controller
{
    // SCR-11 · /ApDung/TinhHuong?boiCanh=
    [HttpGet]
    public async Task<IActionResult> Index(string? boiCanh)
    {
        try { return View(await service.DanhSachAsync(boiCanh)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Neo4jException ex) { return LoiKho(ex); }
    }

    // SCR-12 · /ApDung/TinhHuong/Xem/{ma}
    [HttpGet]
    public async Task<IActionResult> Xem(string id)
    {
        try { var model = await service.XemAsync(id); return model == null ? NotFound() : View(model); }
        catch (Neo4jException ex) { return LoiKho(ex); }
    }

    private ObjectResult LoiKho(Neo4jException ex)
    {
        logger.LogError(ex,"Không đọc được tình huống.");
        return StatusCode(503,"Chưa kết nối được kho kiến thức. Em thử lại sau nhé.");
    }

    // SCR-12 phần thực hành đo đạc · POST /ApDung/TinhHuong/DoDac/{ma}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoDac(string id,DoDacForm form)
    {
        try
        {
            var model=await service.XemAsync(id);
            if(model == null || !model.TinhHuong.ThucHanh) return NotFound();
            model=model with { DoDac=form };
            if(!form.TryParse(out var input,out var errors))
            {
                foreach(var (key,error) in errors) ModelState.AddModelError(key,error);
                return View("Xem",model);
            }
            var result=KiemTraHinhDang.KiemTra(input!);
            if(result.Loi != null) { ModelState.AddModelError("",result.Loi); return View("Xem",model); }
            var grounds=await measurements.DauHieuAsync(result.DauHieu,user.LopHienThi);
            if(!grounds.Select(g => g.Ma).ToHashSet().SetEquals(result.DauHieu))
            {
                ModelState.AddModelError("","Chưa có đủ căn cứ đã rà soát cho kết luận này. Em thử lại sau nhé.");
                Response.StatusCode=503;
                return View("Xem",model);
            }
            return View("Xem",model with { KetQua=result,CanCu=grounds });
        }
        catch(Neo4jException ex) { return LoiKho(ex); }
    }
}
