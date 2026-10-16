using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Web.Areas.KienThuc.Models;

/// <summary>Một thẻ nội dung trên thư viện kiến thức (SCR-04 — US-09).</summary>
public sealed record NoiDungThuVien(
    string Ma,
    string TieuDe,
    string? BieuThuc,
    string Loai,
    int Lop,
    string Cap,
    string? MaHinh,
    bool DaHoc)
{
    public string TenLoai => ThuVienQuyTac.TenLoai(Loai);

    /// <summary>
    /// Thẻ liên kết tới chi tiết của hình liên quan (US-10).
    /// Null khi không suy ra được hình nào — khi đó thẻ không có liên kết.
    /// </summary>
    public string? DuongDanChiTiet
        => string.IsNullOrEmpty(MaHinh) ? null : $"/KienThuc/ThuVien/ChiTiet/{Uri.EscapeDataString(MaHinh)}";
}

/// <summary>Dữ liệu trang /KienThuc/ThuVien (SCR-04 — US-09).</summary>
public sealed class ThuVienViewModel
{
    public IReadOnlyList<NoiDungThuVien> DanhSach { get; init; } = [];

    // Bộ lọc đang áp dụng (null = không lọc).
    public string? Loai { get; init; }
    public string? Cap { get; init; }
    public int? Lop { get; init; }

    /// <summary>Lớp của học sinh — cũng là lớp cao nhất được hiện nội dung (BR-04).</summary>
    public int LopHocSinh { get; init; }

    public bool DaDangNhap { get; init; }

    /// <summary>Học sinh cấp 1 cần nút bấm lớn hơn (NFR-04).</summary>
    public bool LaCap1 => LopHocSinh <= 5;

    public bool CoLoc => Loai is not null || Cap is not null || Lop is not null;
}
