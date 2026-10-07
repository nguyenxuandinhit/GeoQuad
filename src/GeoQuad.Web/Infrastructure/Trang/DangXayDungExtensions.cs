using Microsoft.AspNetCore.Mvc;

namespace GeoQuad.Web.Infrastructure.Trang;

public static class DangXayDungExtensions
{
    /// <summary>Trả về trang "Đang xây dựng" dùng chung thay cho lỗi 404.</summary>
    public static ViewResult DangXayDung(
        this Controller controller, string tieuDe, string story, string phan, string moTa = "")
        => controller.View("DangXayDung", new DangXayDungViewModel(tieuDe, story, phan, moTa));
}
