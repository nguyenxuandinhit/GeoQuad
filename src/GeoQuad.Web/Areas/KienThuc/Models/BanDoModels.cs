namespace GeoQuad.Web.Areas.KienThuc.Models;

/// <summary>Một nút hình trên bản đồ kiến thức (SCR-07 — US-12).</summary>
public sealed record BanDoNut(
    string Ma,
    string Ten,
    string DinhNghia,
    int Lop,
    string Cap,
    IReadOnlyList<string> LaDacBietCua,
    bool DaHoc);

/// <summary>Một mũi tên: từ hình đặc biệt tới hình tổng quát (BR-11).</summary>
public sealed record BanDoCanh(string Tu, string Den);

/// <summary>Dữ liệu JSON cho Cytoscape.js tại /KienThuc/BanDo/DuLieu?lop=</summary>
public sealed record BanDoDuLieu(
    IReadOnlyList<BanDoNut> Nut,
    IReadOnlyList<BanDoCanh> Canh,
    int LopHienThi,
    int LopHocSinh);

/// <summary>Kết quả ô "Vì sao … là …?" (US-12).</summary>
public sealed record KetQuaViSao(
    string Tu,
    string Den,
    IReadOnlyList<string> Chuoi)
{
    public bool CoDuongDi => Chuoi.Count > 0;
}

/// <summary>Dữ liệu trang /KienThuc/BanDo (SCR-07 — US-12).</summary>
public sealed class BanDoViewModel
{
    public required BanDoDuLieu DuLieu { get; init; }

    /// <summary>Lớp đang lọc (null = tới lớp đang hiển thị của người dùng).</summary>
    public int? LopLoc { get; init; }

    public bool DaDangNhap { get; init; }

    /// <summary>Danh sách hình để chọn trong ô "Vì sao … là …?".</summary>
    public IReadOnlyList<(string Ma, string Ten)> CacHinh
        => DuLieu.Nut.Select(n => (n.Ma, n.Ten)).ToList();

    /// <summary>Chưa có mũi tên nào — cần giải thích cho học sinh (US-12).</summary>
    public bool KhongCoQuanHe => DuLieu.Canh.Count == 0;

    public KetQuaViSao? ViSao { get; init; }

    public bool LaCap1 => DuLieu.LopHocSinh <= 5;
}
