using System.Globalization;
using System.Net;
using System.Text;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>
/// Sinh hình SVG minh hoạ cho bảy hình tứ giác (US-10).
/// Logic thuần, không phụ thuộc Neo4j nên unit test được (quy ước 2, điểm 9).
/// Mọi hình đều có &lt;title&gt; và &lt;desc&gt; để đọc được bằng trình đọc màn hình (NFR-11).
/// </summary>
public static class HinhVeSvg
{
    private const int RongKhung = 220;
    private const int CaoKhung = 150;

    /// <summary>Toạ độ bốn đỉnh (theo thứ tự A, B, C, D) và lời mô tả của từng hình.</summary>
    private static readonly Dictionary<string, (string Ten, (int X, int Y)[] Dinh, string MoTa)> BangHinh = new()
    {
        ["TU_GIAC"] = ("Tứ giác",
            [(25, 35), (180, 20), (195, 120), (55, 132)],
            "Bốn đoạn thẳng nối tiếp khép kín, các cạnh không bằng nhau và không có cặp cạnh nào song song."),

        ["HINH_THANG"] = ("Hình thang",
            [(60, 30), (160, 30), (195, 120), (20, 120)],
            "Tứ giác có cạnh AB song song với cạnh DC; hai cạnh bên AD và BC không song song."),

        ["HINH_THANG_CAN"] = ("Hình thang cân",
            [(65, 30), (155, 30), (190, 120), (30, 120)],
            "Hình thang có AB song song DC và hai cạnh bên AD, BC bằng nhau; hai góc kề đáy DC bằng nhau."),

        ["HINH_BINH_HANH"] = ("Hình bình hành",
            [(60, 30), (195, 30), (160, 120), (25, 120)],
            "Tứ giác có AB song song và bằng DC, AD song song và bằng BC."),

        ["HINH_CHU_NHAT"] = ("Hình chữ nhật",
            [(35, 30), (185, 30), (185, 120), (35, 120)],
            "Tứ giác có bốn góc vuông; hai cạnh đối bằng nhau."),

        ["HINH_THOI"] = ("Hình thoi",
            [(110, 20), (195, 75), (110, 130), (25, 75)],
            "Tứ giác có bốn cạnh bằng nhau; hai đường chéo vuông góc với nhau."),

        ["HINH_VUONG"] = ("Hình vuông",
            [(65, 30), (155, 30), (155, 120), (65, 120)],
            "Tứ giác có bốn góc vuông và bốn cạnh bằng nhau.")
    };

    /// <summary>Những hình có đường chéo vẽ kèm để thấy tính chất đường chéo.</summary>
    private static readonly HashSet<string> CoDuongCheo =
        ["HINH_THOI", "HINH_VUONG", "HINH_CHU_NHAT", "HINH_THANG_CAN"];

    /// <summary>Những hình cần vẽ ký hiệu góc vuông ở đỉnh A.</summary>
    private static readonly HashSet<string> CoGocVuong = ["HINH_CHU_NHAT", "HINH_VUONG"];

    public static bool CoHinhVe(string? ma) => ma is not null && BangHinh.ContainsKey(ma);

    /// <summary>
    /// Trả về đoạn SVG cho mã hình, hoặc null nếu không phải một trong bảy hình
    /// (ví dụ các yếu tố như "Đường chéo" thì không vẽ).
    /// </summary>
    public static string? Ve(string? ma)
    {
        if (ma is null || !BangHinh.TryGetValue(ma, out var h))
        {
            return null;
        }

        var ten = WebUtility.HtmlEncode(h.Ten);
        var moTa = WebUtility.HtmlEncode(h.MoTa);
        var diem = string.Join(" ", h.Dinh.Select(d => $"{d.X},{d.Y}"));

        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {RongKhung} {CaoKhung}" role="img" aria-labelledby="gq-svg-t-{ma} gq-svg-d-{ma}" class="gq-hinh-tu-giac">""");
        sb.Append(CultureInfo.InvariantCulture, $"""<title id="gq-svg-t-{ma}">{ten}</title>""");
        sb.Append(CultureInfo.InvariantCulture, $"""<desc id="gq-svg-d-{ma}">{moTa}</desc>""");

        // Đường chéo vẽ nét đứt để phân biệt với cạnh mà không cần dựa vào màu (NFR-11).
        if (CoDuongCheo.Contains(ma))
        {
            var (ax, ay) = h.Dinh[0];
            var (bx, by) = h.Dinh[1];
            var (cx, cy) = h.Dinh[2];
            var (dx, dy) = h.Dinh[3];
            sb.Append(CultureInfo.InvariantCulture,
                $"""<line x1="{ax}" y1="{ay}" x2="{cx}" y2="{cy}" class="gq-cheo" stroke-dasharray="5 4" />""");
            sb.Append(CultureInfo.InvariantCulture,
                $"""<line x1="{bx}" y1="{by}" x2="{dx}" y2="{dy}" class="gq-cheo" stroke-dasharray="5 4" />""");
        }

        sb.Append(CultureInfo.InvariantCulture, $"""<polygon points="{diem}" class="gq-canh" />""");

        // Ký hiệu góc vuông ở đỉnh A.
        if (CoGocVuong.Contains(ma))
        {
            var (ax, ay) = h.Dinh[0];
            sb.Append(CultureInfo.InvariantCulture,
                $"""<polyline points="{ax + 14},{ay} {ax + 14},{ay + 14} {ax},{ay + 14}" class="gq-goc-vuong" />""");
        }

        // Nhãn đỉnh A, B, C, D — truyền thông tin bằng chữ, không chỉ bằng màu.
        var nhan = new[] { "A", "B", "C", "D" };
        for (var i = 0; i < h.Dinh.Length; i++)
        {
            var (x, y) = h.Dinh[i];
            // Đẩy nhãn ra ngoài hình theo hướng từ tâm hình tới đỉnh.
            var tamX = h.Dinh.Average(d => d.X);
            var tamY = h.Dinh.Average(d => d.Y);
            var lechX = x - tamX >= 0 ? 10 : -12;
            var lechY = y - tamY >= 0 ? 16 : -6;
            sb.Append(CultureInfo.InvariantCulture,
                $"""<text x="{x + lechX}" y="{y + lechY}" class="gq-nhan-dinh">{nhan[i]}</text>""");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}
