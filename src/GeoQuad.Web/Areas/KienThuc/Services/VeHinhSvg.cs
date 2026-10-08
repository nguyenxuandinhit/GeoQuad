using System.Globalization;
using System.Net;
using System.Text;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>Một đỉnh của hình vẽ, toạ độ trong khung nhìn SVG.</summary>
public sealed record Dinh(string Nhan, double X, double Y);

/// <summary>Hình vẽ theo số liệu: bốn đỉnh và đoạn SVG tương ứng (US-16).</summary>
public sealed record HinhTheoSoDo(IReadOnlyList<Dinh> CacDinh, string Svg);

/// <summary>
/// Sinh SVG tỉ lệ theo số đo người dùng vừa nhập ở máy tính hình học
/// (US-16, FR-31). Lớp thuần, không phụ thuộc Neo4j nên unit test được.
/// Khung nhìn 320×240, hình được co giãn vừa khung mà giữ đúng tỉ lệ.
/// </summary>
public static class VeHinhSvg
{
    public const int RongKhung = 320;
    public const int CaoKhung = 240;

    /// <summary>Lề để còn chỗ cho nhãn đỉnh và nhãn cạnh.</summary>
    private const double Le = 34;

    /// <summary>Góc mặc định của hình bình hành khi không có chiều cao h (độ).</summary>
    public const double GocMacDinhHinhBinhHanh = 60;

    private static readonly string[] NhanDinh = ["A", "B", "C", "D"];

    public static bool VeDuoc(string? maHinh) => maHinh switch
    {
        "HINH_CHU_NHAT" or "HINH_VUONG" or "HINH_BINH_HANH"
            or "HINH_THOI" or "HINH_THANG" or "HINH_THANG_CAN" => true,
        _ => false
    };

    /// <summary>
    /// Dựng hình theo số đo. Trả về null khi không vẽ được hình này hoặc thiếu số đo cần thiết.
    /// </summary>
    /// <param name="veDuongCheo">Vẽ hai đường chéo nét đứt (khi đang tính đường chéo).</param>
    public static HinhTheoSoDo? Ve(
        string? maHinh,
        IReadOnlyDictionary<string, double> soDo,
        string donVi = "cm",
        bool veDuongCheo = false)
    {
        if (!VeDuoc(maHinh))
        {
            return null;
        }

        var diem = maHinh switch
        {
            "HINH_CHU_NHAT" => ChuNhat(soDo),
            "HINH_VUONG" => Vuong(soDo),
            "HINH_BINH_HANH" => BinhHanh(soDo),
            "HINH_THOI" => Thoi(soDo),
            "HINH_THANG" => Thang(soDo, can: false),
            "HINH_THANG_CAN" => Thang(soDo, can: true),
            _ => null
        };

        if (diem is null || diem.Count != 4)
        {
            return null;
        }

        var dinh = CoGian(diem);
        var svg = DungSvg(maHinh!, dinh, soDo, donVi, veDuongCheo);

        return new HinhTheoSoDo(dinh, svg);
    }

    // ---- Toạ độ "thật" theo số đo, trục y hướng lên; CoGian sẽ lật và co lại ----

    private static List<(double X, double Y)>? ChuNhat(IReadOnlyDictionary<string, double> s)
        => Lay(s, "a", "b") is var (a, b) && a > 0 && b > 0
            ? [(0, b), (a, b), (a, 0), (0, 0)]
            : null;

    private static List<(double X, double Y)>? Vuong(IReadOnlyDictionary<string, double> s)
        => s.TryGetValue("a", out var a) && a > 0
            ? [(0, a), (a, a), (a, 0), (0, 0)]
            : null;

    private static List<(double X, double Y)>? BinhHanh(IReadOnlyDictionary<string, double> s)
    {
        if (!s.TryGetValue("a", out var a) || a <= 0)
        {
            return null;
        }

        // Có chiều cao h thì dựng đúng chiều cao; cạnh bên b dùng để tính độ nghiêng.
        if (s.TryGetValue("h", out var h) && h > 0)
        {
            var canhBen = s.TryGetValue("b", out var b) && b >= h ? b : h / Math.Sin(Math.PI / 3);
            var lech = Math.Sqrt(Math.Max(canhBen * canhBen - h * h, 0));
            return [(lech, h), (lech + a, h), (a, 0), (0, 0)];
        }

        // Không có h: dùng góc mặc định 60° với cạnh bên b (hoặc a nếu không có b).
        var canh = s.TryGetValue("b", out var b2) && b2 > 0 ? b2 : a;
        var goc = GocMacDinhHinhBinhHanh * Math.PI / 180;
        var dx = canh * Math.Cos(goc);
        var dy = canh * Math.Sin(goc);
        return [(dx, dy), (dx + a, dy), (a, 0), (0, 0)];
    }

    private static List<(double X, double Y)>? Thoi(IReadOnlyDictionary<string, double> s)
    {
        // Dựng từ hai đường chéo nếu có.
        // d1 là đường chéo AC (dọc), d2 là đường chéo BD (ngang) — khớp với nhãn ở NhanCanh.
        if (s.TryGetValue("d1", out var d1) && s.TryGetValue("d2", out var d2) && d1 > 0 && d2 > 0)
        {
            return [(d2 / 2, d1), (d2, d1 / 2), (d2 / 2, 0), (0, d1 / 2)];
        }

        // Chỉ có cạnh a: dựng hình thoi góc 60°.
        if (s.TryGetValue("a", out var a) && a > 0)
        {
            var goc = 60 * Math.PI / 180;
            var dx = a * Math.Cos(goc);
            var dy = a * Math.Sin(goc);
            return [(dx, dy), (dx + a, dy), (a, 0), (0, 0)];
        }

        return null;
    }

    private static List<(double X, double Y)>? Thang(IReadOnlyDictionary<string, double> s, bool can)
    {
        if (!s.TryGetValue("a", out var a) || !s.TryGetValue("b", out var b) || a <= 0 || b <= 0)
        {
            return null;
        }

        // Thiếu chiều cao thì lấy một nửa đáy lớn cho hình cân đối.
        var h = s.TryGetValue("h", out var hh) && hh > 0 ? hh : Math.Max(a, b) / 2;

        var dayDuoi = Math.Max(a, b);
        var dayTren = Math.Min(a, b);

        // Hình thang cân: đáy nhỏ đặt cân giữa. Hình thang thường: lệch sang phải một chút.
        var lech = can ? (dayDuoi - dayTren) / 2 : (dayDuoi - dayTren) * 0.3;

        return [(lech, h), (lech + dayTren, h), (dayDuoi, 0), (0, 0)];
    }

    private static (double, double) Lay(IReadOnlyDictionary<string, double> s, string k1, string k2)
        => (s.TryGetValue(k1, out var v1) ? v1 : 0, s.TryGetValue(k2, out var v2) ? v2 : 0);

    /// <summary>
    /// Co giãn đều hai trục để hình vừa khung 320×240, lật trục y cho đúng hướng màn hình,
    /// rồi đặt vào giữa khung. Vì cùng một hệ số nên **tỉ lệ cạnh được giữ nguyên**.
    /// </summary>
    private static List<Dinh> CoGian(List<(double X, double Y)> diem)
    {
        var minX = diem.Min(p => p.X);
        var maxX = diem.Max(p => p.X);
        var minY = diem.Min(p => p.Y);
        var maxY = diem.Max(p => p.Y);

        var rong = Math.Max(maxX - minX, 1e-9);
        var cao = Math.Max(maxY - minY, 1e-9);

        var heSo = Math.Min((RongKhung - 2 * Le) / rong, (CaoKhung - 2 * Le) / cao);

        var veRong = rong * heSo;
        var veCao = cao * heSo;
        var batDauX = (RongKhung - veRong) / 2;
        var batDauY = (CaoKhung - veCao) / 2;

        return diem.Select((p, i) => new Dinh(
            NhanDinh[i],
            Lam2(batDauX + (p.X - minX) * heSo),
            // Lật trục y: toạ độ toán học hướng lên, SVG hướng xuống.
            Lam2(batDauY + (maxY - p.Y) * heSo))).ToList();
    }

    private static double Lam2(double x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);

    // ---- Dựng chuỗi SVG ----

    private static string DungSvg(
        string maHinh,
        IReadOnlyList<Dinh> dinh,
        IReadOnlyDictionary<string, double> soDo,
        string donVi,
        bool veDuongCheo)
    {
        var inv = CultureInfo.InvariantCulture;
        var ten = WebUtility.HtmlEncode(TenHinh(maHinh));
        var moTa = WebUtility.HtmlEncode(MoTa(maHinh, soDo, donVi));
        var id = maHinh.ToLowerInvariant();

        var sb = new StringBuilder();
        sb.Append(inv,
            $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {RongKhung} {CaoKhung}" role="img" aria-labelledby="gq-vh-t-{id} gq-vh-d-{id}" class="gq-hinh-so-do">""");
        sb.Append(inv, $"""<title id="gq-vh-t-{id}">{ten}</title>""");
        sb.Append(inv, $"""<desc id="gq-vh-d-{id}">{moTa}</desc>""");

        if (veDuongCheo)
        {
            sb.Append(inv,
                $"""<line x1="{dinh[0].X}" y1="{dinh[0].Y}" x2="{dinh[2].X}" y2="{dinh[2].Y}" class="gq-cheo" stroke-dasharray="5 4" />""");
            sb.Append(inv,
                $"""<line x1="{dinh[1].X}" y1="{dinh[1].Y}" x2="{dinh[3].X}" y2="{dinh[3].Y}" class="gq-cheo" stroke-dasharray="5 4" />""");
        }

        var diem = string.Join(" ", dinh.Select(d => $"{d.X.ToString(inv)},{d.Y.ToString(inv)}"));
        sb.Append(inv, $"""<polygon points="{diem}" class="gq-canh" />""");

        // Ký hiệu góc vuông ở đỉnh D với hình có góc vuông.
        if (maHinh is "HINH_CHU_NHAT" or "HINH_VUONG")
        {
            var d = dinh[3];
            sb.Append(inv,
                $"""<polyline points="{d.X + 14},{d.Y} {d.X + 14},{d.Y - 14} {d.X},{d.Y - 14}" class="gq-goc-vuong" />""");
        }

        // Nhãn độ dài cạnh: đặt ở trung điểm từng cạnh có số đo.
        foreach (var (i, j, nhan) in NhanCanh(maHinh, soDo, donVi))
        {
            var giua = ((dinh[i].X + dinh[j].X) / 2, (dinh[i].Y + dinh[j].Y) / 2);
            var raNgoai = RaNgoai(dinh, giua);
            sb.Append(inv,
                $"""<text x="{Lam2(giua.Item1 + raNgoai.X).ToString(inv)}" y="{Lam2(giua.Item2 + raNgoai.Y).ToString(inv)}" class="gq-nhan-canh">{WebUtility.HtmlEncode(nhan)}</text>""");
        }

        // Nhãn đỉnh A, B, C, D.
        foreach (var d in dinh)
        {
            var ra = RaNgoai(dinh, (d.X, d.Y));
            sb.Append(inv,
                $"""<text x="{Lam2(d.X + ra.X * 1.2).ToString(inv)}" y="{Lam2(d.Y + ra.Y * 1.2).ToString(inv)}" class="gq-nhan-dinh">{d.Nhan}</text>""");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>Hướng đẩy nhãn ra ngoài hình, tính từ tâm hình.</summary>
    private static (double X, double Y) RaNgoai(IReadOnlyList<Dinh> dinh, (double X, double Y) diem)
    {
        var tamX = dinh.Average(d => d.X);
        var tamY = dinh.Average(d => d.Y);
        var dx = diem.X - tamX;
        var dy = diem.Y - tamY;
        var dai = Math.Sqrt(dx * dx + dy * dy);

        return dai < 1e-6 ? (0, -12) : (dx / dai * 16, dy / dai * 16 + 4);
    }

    private static IEnumerable<(int I, int J, string Nhan)> NhanCanh(
        string maHinh, IReadOnlyDictionary<string, double> soDo, string donVi)
    {
        string Nh(string bien) => $"{bien} = {MayTinhHinhHoc.So(soDo[bien])} {donVi}";

        switch (maHinh)
        {
            case "HINH_CHU_NHAT":
                if (soDo.ContainsKey("a")) yield return (0, 1, Nh("a"));
                if (soDo.ContainsKey("b")) yield return (1, 2, Nh("b"));
                break;

            case "HINH_VUONG":
                if (soDo.ContainsKey("a")) yield return (0, 1, Nh("a"));
                break;

            case "HINH_BINH_HANH":
                if (soDo.ContainsKey("a")) yield return (3, 2, Nh("a"));
                if (soDo.ContainsKey("b")) yield return (0, 3, Nh("b"));
                if (soDo.ContainsKey("h")) yield return (0, 1, Nh("h"));
                break;

            case "HINH_THOI":
                if (soDo.ContainsKey("a")) yield return (3, 2, Nh("a"));
                if (soDo.ContainsKey("d1")) yield return (0, 2, Nh("d1"));
                if (soDo.ContainsKey("d2")) yield return (1, 3, Nh("d2"));
                break;

            case "HINH_THANG":
            case "HINH_THANG_CAN":
                if (soDo.ContainsKey("a")) yield return (0, 1, Nh("a"));
                if (soDo.ContainsKey("b")) yield return (3, 2, Nh("b"));
                if (soDo.ContainsKey("h")) yield return (0, 3, Nh("h"));
                break;
        }
    }

    private static string TenHinh(string maHinh) => maHinh switch
    {
        "HINH_CHU_NHAT" => "Hình chữ nhật theo số đo em nhập",
        "HINH_VUONG" => "Hình vuông theo số đo em nhập",
        "HINH_BINH_HANH" => "Hình bình hành theo số đo em nhập",
        "HINH_THOI" => "Hình thoi theo số đo em nhập",
        "HINH_THANG" => "Hình thang theo số đo em nhập",
        "HINH_THANG_CAN" => "Hình thang cân theo số đo em nhập",
        _ => "Hình tứ giác"
    };

    private static string MoTa(string maHinh, IReadOnlyDictionary<string, double> soDo, string donVi)
    {
        var so = string.Join(", ", soDo
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key} = {MayTinhHinhHoc.So(x.Value)} {donVi}"));

        return $"{TenHinh(maHinh)} với bốn đỉnh A, B, C, D và số đo {so}. "
             + "Hình vẽ giữ đúng tỉ lệ giữa các số đo.";
    }
}
