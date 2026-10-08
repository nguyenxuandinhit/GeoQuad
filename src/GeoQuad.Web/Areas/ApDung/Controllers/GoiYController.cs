using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Services;
using Neo4j.Driver;
using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Gợi ý định lý US-13 với catalog theo lớp và trạng thái rà soát.</summary>
[Area("ApDung")]
public sealed class GoiYController(GoiYService service, ILogger<GoiYController> logger) : Controller
{
    // SCR-08 · /ApDung/GoiY
    [HttpGet]
    public async Task<IActionResult> Index(string nen = "TU_GIAC", string? dich = null, string[]? co = null)
    {
        try
        {
            if (dich is null)
            {
                return View(await service.FormAsync(nen));
            }
            return View(await service.TimAsync(new(nen,dich,co ?? [])));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Neo4jException ex)
        {
            logger.LogError(ex,"Không đọc được gợi ý.");
            return StatusCode(503,"Chưa kết nối được kho kiến thức. Em thử lại sau nhé.");
        }
    }
}
