using GeoQuad.Web.Areas.ApDung.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Controllers;

/// <summary>Khung rỗng do PHẦN 0 tạo (US-01). Phần B hoàn thiện ở US-14.</summary>
[Area("ApDung")]
public sealed class ChungMinhController(ChungMinhService service,ILogger<ChungMinhController> logger) : Controller
{
    // SCR-09 · /ApDung/ChungMinh/Xem/{ma}
    [HttpGet]
    public async Task<IActionResult> Xem(string id)
    {
        try
        {
            var model = await service.XemAsync(id);
            return model == null ? NotFound() : View(model);
        }
        catch (Neo4jException ex)
        {
            logger.LogError(ex,"Không đọc được chứng minh.");
            return StatusCode(503,"Chưa kết nối được kho kiến thức. Em thử lại sau nhé.");
        }
    }
}
