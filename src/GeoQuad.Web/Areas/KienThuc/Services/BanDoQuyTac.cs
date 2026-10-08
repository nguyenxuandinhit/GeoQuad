using GeoQuad.Web.Areas.KienThuc.Models;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>
/// Quy tắc dựng bản đồ kiến thức, viết thuần C# để unit test được (quy ước 2, điểm 9) — US-12.
/// </summary>
public static class BanDoQuyTac
{
    /// <summary>
    /// Lớp thực sự dùng để truy vấn: lấy lớp người dùng chọn nhưng không bao giờ vượt
    /// quá lớp người đó được xem (BR-04).
    /// </summary>
    public static int LopXem(int? lopLoc, int lopHienThi)
        => lopLoc is null ? lopHienThi : Math.Min(lopLoc.Value, lopHienThi);

    /// <summary>
    /// Dựng danh sách mũi tên từ <c>LaDacBietCua</c> của từng nút.
    /// Chiều: hình đặc biệt → hình tổng quát (BR-11).
    /// Chỉ giữ mũi tên mà cả hai đầu đều có trong danh sách nút đang hiển thị.
    /// </summary>
    public static IReadOnlyList<BanDoCanh> DungCanh(IReadOnlyList<BanDoNut> nut)
    {
        var coTrongBanDo = nut.Select(n => n.Ma).ToHashSet();

        return nut
            .SelectMany(n => n.LaDacBietCua
                .Where(den => !string.IsNullOrEmpty(den) && coTrongBanDo.Contains(den))
                .Select(den => new BanDoCanh(n.Ma, den)))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Màu nút theo cấp học, ba sắc độ trong tông nâu của GeoQuad; phân biệt thêm bằng
    /// độ đậm nhạt và bằng chữ ghi lớp trong nhãn nên không chỉ dùng màu (NFR-11).
    /// </summary>
    public static string MauTheoCap(string? cap) => cap switch
    {
        "CAP_1" => "#a9714a",   // nâu cam
        "CAP_2" => "#74553c",   // nâu
        "CAP_3" => "#3e3027",   // nâu đậm
        _ => "#8c7a68"
    };

    /// <summary>Chú giải màu cho bản đồ.</summary>
    public static IReadOnlyList<(string Cap, string Ten, string Mau)> ChuGiai()
        => ThuVienQuyTac.CacCap
            .Select(c => (c, ThuVienQuyTac.TenCap(c), MauTheoCap(c)))
            .ToList();

    /// <summary>
    /// Câu trả lời cho ô "Vì sao … là …?" dựng từ chuỗi hình.
    /// Ví dụ: Hình vuông → Hình chữ nhật → Hình bình hành → Hình thang.
    /// </summary>
    public static string CauTraLoi(IReadOnlyList<string> chuoi)
    {
        if (chuoi.Count < 2)
        {
            return string.Empty;
        }

        var cau = new List<string>();
        for (var i = 0; i < chuoi.Count - 1; i++)
        {
            cau.Add($"{chuoi[i]} là một trường hợp đặc biệt của {chuoi[i + 1].ToLowerInvariant()}");
        }

        return string.Join("; ", cau) + ".";
    }
}
