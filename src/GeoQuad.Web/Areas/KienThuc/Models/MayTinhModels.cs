using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Web.Areas.KienThuc.Models;

/// <summary>Một đại lượng tính được cho hình đang chọn, kèm công thức lấy từ Neo4j (US-15).</summary>
public sealed record DaiLuongTinh(
    string DaiLuong,
    string MaCongThuc,
    string Ten,
    string BieuThuc,
    IReadOnlyList<string> BienSo,
    string TuHinh)
{
    public string TenDaiLuong => DaiLuongTinh.TenCua(DaiLuong);

    public static string TenCua(string? daiLuong) => daiLuong switch
    {
        MayTinhHinhHoc.DaiLuongChuVi => "Chu vi",
        MayTinhHinhHoc.DaiLuongDienTich => "Diện tích",
        MayTinhHinhHoc.DaiLuongDuongCheo => "Đường chéo",
        _ => "Khác"
    };
}

/// <summary>
/// Dữ liệu trang /KienThuc/MayTinh (SCR-10 — US-15, US-16).
/// Là <c>record</c> để service dựng dần bằng biểu thức <c>with</c>.
/// <c>TuHinh</c> là tên hình sở hữu công thức — có thể là hình tổng quát hơn.
/// </summary>
public sealed record MayTinhViewModel
{
    /// <summary>Các hình chọn được (lấy từ Neo4j, giới hạn theo lớp).</summary>
    public IReadOnlyList<LienKetKhaiNiem> CacHinh { get; init; } = [];

    public string? Hinh { get; init; }
    public string? TenHinh { get; init; }

    /// <summary>Các đại lượng tính được cho hình đang chọn.</summary>
    public IReadOnlyList<DaiLuongTinh> CacDaiLuong { get; init; } = [];

    public string? DaiLuong { get; init; }
    public DaiLuongTinh? CongThuc { get; init; }

    /// <summary>Số đo người dùng đã nhập, giữ lại để không phải nhập lại khi có lỗi.</summary>
    public IReadOnlyDictionary<string, string?> SoDoNhap { get; init; }
        = new Dictionary<string, string?>();

    public string DonVi { get; init; } = MayTinhHinhHoc.DonViMacDinh;

    public IReadOnlyList<string> Loi { get; init; } = [];

    public KetQuaTinh? KetQua { get; init; }

    /// <summary>Hình vẽ theo số đo vừa nhập (US-16).</summary>
    public string? Svg { get; init; }

    public int LopHienThi { get; init; }
    public int LopHocSinh { get; init; }
    public bool LaCap1 => LopHocSinh <= 5;

    public bool CoKetQua => KetQua is not null;
    public bool CoLoi => Loi.Count > 0;
}
