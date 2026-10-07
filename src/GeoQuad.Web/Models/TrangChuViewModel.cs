namespace GeoQuad.Web.Models;

/// <summary>Dữ liệu trang chủ SCR-01 (US-01).</summary>
public sealed class TrangChuViewModel
{
    public string? BietDanh { get; init; }

    /// <summary>Đúng khi truy vấn Neo4j thành công.</summary>
    public bool KetNoiOk { get; init; }

    /// <summary>Số nút đếm được trong đồ thị, dùng cho dòng "Kết nối Neo4j: OK – N nút".</summary>
    public long SoNut { get; init; }

    /// <summary>Thông báo tiếng Việt khi không kết nối được, kèm cách sửa.</summary>
    public string? LoiKetNoi { get; init; }
}
