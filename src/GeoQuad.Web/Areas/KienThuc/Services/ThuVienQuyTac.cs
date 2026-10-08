using GeoQuad.Web.Infrastructure;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>
/// Quy tắc của thư viện kiến thức, viết thuần C# (không phụ thuộc Neo4j) để unit test được
/// (README quy ước 2, điểm 9) — US-09.
/// </summary>
public static class ThuVienQuyTac
{
    public const string LoaiHinh = "HINH";
    public const string LoaiTinhChat = "TINH_CHAT";
    public const string LoaiDauHieu = "DAU_HIEU";
    public const string LoaiCongThuc = "CONG_THUC";

    /// <summary>Bốn tab loại nội dung của SCR-04, theo thứ tự hiển thị.</summary>
    public static readonly string[] CacLoai = [LoaiHinh, LoaiTinhChat, LoaiDauHieu, LoaiCongThuc];

    /// <summary>Ba cấp học (BR-03).</summary>
    public static readonly string[] CacCap = ["CAP_1", "CAP_2", "CAP_3"];

    private static readonly Dictionary<string, string> TenLoaiTiengViet = new()
    {
        [LoaiHinh] = "Hình",
        [LoaiTinhChat] = "Tính chất",
        [LoaiDauHieu] = "Dấu hiệu",
        [LoaiCongThuc] = "Công thức"
    };

    private static readonly Dictionary<string, string> TenCapTiengViet = new()
    {
        ["CAP_1"] = "Cấp 1 (lớp 1–5)",
        ["CAP_2"] = "Cấp 2 (lớp 6–9)",
        ["CAP_3"] = "Cấp 3 (lớp 10–12)"
    };

    /// <summary>
    /// Chỉ nhận bốn giá trị loại cho phép; giá trị lạ trả về null nghĩa là "không lọc".
    /// Nhờ vậy tham số từ URL không thể đưa giá trị tuỳ ý vào truy vấn.
    /// </summary>
    public static string? ChuanHoaLoai(string? loai)
    {
        if (string.IsNullOrWhiteSpace(loai))
        {
            return null;
        }

        var t = loai.Trim().ToUpperInvariant();
        return CacLoai.Contains(t) ? t : null;
    }

    /// <summary>Chỉ nhận CAP_1, CAP_2, CAP_3; giá trị lạ trả về null.</summary>
    public static string? ChuanHoaCap(string? cap)
    {
        if (string.IsNullOrWhiteSpace(cap))
        {
            return null;
        }

        var c = cap.Trim().ToUpperInvariant();
        return CacCap.Contains(c) ? c : null;
    }

    /// <summary>Chỉ nhận lớp 1–12; ngoài khoảng đó trả về null.</summary>
    public static int? ChuanHoaLop(int? lop)
        => lop is >= QuyUoc.LopNhoNhat and <= QuyUoc.LopLonNhat ? lop : null;

    /// <summary>
    /// Nội dung được gắn nhãn "Nâng cao" khi lớp của nó lớn hơn lớp thật của học sinh
    /// (README quy ước 2, điểm 7 — BR-04).
    /// </summary>
    public static bool LaNangCao(int lopNoiDung, int lopHocSinh) => lopNoiDung > lopHocSinh;

    public static string TenLoai(string? loai)
        => loai is not null && TenLoaiTiengViet.TryGetValue(loai, out var ten) ? ten : "Khác";

    public static string TenCap(string? cap)
        => cap is not null && TenCapTiengViet.TryGetValue(cap, out var ten) ? ten : "Khác";

    /// <summary>Cấp học suy ra từ số lớp (BR-03), dùng cho nhãn trên thẻ nội dung.</summary>
    public static string CapTheoLop(int lop) => lop switch
    {
        <= 5 => "CAP_1",
        <= 9 => "CAP_2",
        _ => "CAP_3"
    };
}
