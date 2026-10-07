namespace GeoQuad.Web.Models;

/// <summary>Dữ liệu trang lỗi thân thiện.</summary>
public sealed class LoiViewModel
{
    public int MaTrangThai { get; init; }
    public string TieuDe { get; init; } = "Đã có lỗi xảy ra";
    public string MoTa { get; init; } = "Em thử tải lại trang nhé.";
    public string? MaYeuCau { get; init; }
}
