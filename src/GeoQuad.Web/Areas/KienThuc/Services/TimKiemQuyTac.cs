using System.Text;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>
/// Dựng chuỗi truy vấn cho chỉ mục toàn văn Lucene của Neo4j (US-11, FR-12, NFR-14).
/// Logic thuần, không phụ thuộc Neo4j nên unit test được (quy ước 2, điểm 9).
/// </summary>
public static class TimKiemQuyTac
{
    /// <summary>Ký tự có nghĩa đặc biệt với Lucene, phải thoát bằng dấu gạch chéo ngược.</summary>
    private const string KyTuDacBiet = @"+-&|!(){}[]^""~*?:\/";

    public const string ToanTuAnd = "AND";
    public const string ToanTuOr = "OR";

    /// <summary>Giới hạn để một từ khoá quá dài không làm truy vấn phình to.</summary>
    public const int SoTuToiDa = 10;
    public const int DoDaiToiDa = 100;

    /// <summary>
    /// Thoát ký tự đặc biệt Lucene trong một từ. Nhờ vậy gõ <c>(</c>, <c>*</c>, <c>"</c>
    /// không làm truy vấn lỗi cú pháp (AC US-11).
    /// </summary>
    public static string ThoatKyTu(string? tu)
    {
        if (string.IsNullOrEmpty(tu))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(tu.Length + 8);
        foreach (var c in tu)
        {
            if (KyTuDacBiet.Contains(c))
            {
                sb.Append('\\');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>Tách từ khoá thành từng từ, bỏ khoảng trắng thừa và cắt bớt nếu quá dài.</summary>
    public static IReadOnlyList<string> TachTu(string? tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
        {
            return [];
        }

        var cat = tuKhoa.Length > DoDaiToiDa ? tuKhoa[..DoDaiToiDa] : tuKhoa;

        return cat.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                  .Take(SoTuToiDa)
                  .ToList();
    }

    /// <summary>
    /// Dựng chuỗi truy vấn Lucene: thoát ký tự từng từ rồi nối bằng <paramref name="toanTu"/>.
    /// Trả về null khi từ khoá rỗng — khi đó không gửi truy vấn, chỉ nhắc người dùng nhập.
    /// </summary>
    public static string? TaoTruyVan(string? tuKhoa, string toanTu = ToanTuAnd)
    {
        var tu = TachTu(tuKhoa)
            .Select(ThoatKyTu)
            .Where(t => t.Length > 0)
            .ToList();

        return tu.Count == 0 ? null : string.Join($" {toanTu} ", tu);
    }

    /// <summary>Nhóm hiển thị kết quả theo loại (SCR-06).</summary>
    public static string NhomHienThi(IEnumerable<string>? nhan)
    {
        var ds = nhan?.ToList() ?? [];

        if (ds.Contains("KhaiNiem"))
        {
            return "Khái niệm";
        }

        if (ds.Contains("TinhChat") || ds.Contains("DauHieu"))
        {
            return "Tính chất & dấu hiệu";
        }

        return ds.Contains("CongThuc") ? "Công thức" : "Khác";
    }

    /// <summary>Thứ tự nhóm trên trang kết quả.</summary>
    public static readonly string[] ThuTuNhom =
        ["Khái niệm", "Tính chất & dấu hiệu", "Công thức", "Khác"];

    /// <summary>Cắt đoạn trích cho gọn, cắt ở khoảng trắng gần nhất để không đứt giữa từ.</summary>
    public static string CatDoanTrich(string? doanTrich, int doDai = 160)
    {
        if (string.IsNullOrWhiteSpace(doanTrich))
        {
            return string.Empty;
        }

        var s = doanTrich.Trim();
        if (s.Length <= doDai)
        {
            return s;
        }

        var cat = s[..doDai];
        var khoangTrang = cat.LastIndexOf(' ');
        if (khoangTrang > doDai / 2)
        {
            cat = cat[..khoangTrang];
        }

        return cat.TrimEnd() + "…";
    }
}
