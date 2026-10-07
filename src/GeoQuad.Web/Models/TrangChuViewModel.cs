namespace GeoQuad.Web.Models;

/// <summary>Dữ liệu trang chủ SCR-01 (US-01).</summary>
public sealed class TrangChuViewModel
{
    public string? BietDanh { get; init; }

    /// <summary>Khách thì được mời đăng nhập để lưu tiến độ (US-07).</summary>
    public bool DaDangNhap { get; init; }

    /// <summary>Lớp đang dùng để lọc nội dung (BR-04).</summary>
    public int Lop { get; init; }

    /// <summary>Đúng khi truy vấn Neo4j thành công.</summary>
    public bool KetNoiOk { get; init; }

    /// <summary>Số nút đếm được trong đồ thị, dùng cho dòng "Kết nối Neo4j: OK – N nút".</summary>
    public long SoNut { get; init; }

    /// <summary>Thông báo tiếng Việt khi không kết nối được, kèm cách sửa.</summary>
    public string? LoiKetNoi { get; init; }
}
