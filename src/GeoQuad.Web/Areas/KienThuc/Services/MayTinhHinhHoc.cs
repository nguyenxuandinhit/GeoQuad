using System.Globalization;

namespace GeoQuad.Web.Areas.KienThuc.Services;

/// <summary>Kết quả tính của máy tính hình học (US-15).</summary>
public sealed record KetQuaTinh(
    double GiaTri,
    string GiaTriHienThi,
    string DonViKetQua,
    string BieuThucLatex,
    IReadOnlyList<string> CacBuoc);

/// <summary>
/// Tính chu vi, diện tích, đường chéo theo mã công thức (US-15, FR-30, BR-10).
/// Lớp thuần, không phụ thuộc Neo4j nên unit test được (quy ước 2, điểm 9).
/// Danh sách đại lượng và công thức hiển thị lấy từ Neo4j; lớp này chỉ nhận
/// <c>ma</c> công thức rồi kiểm tra số đo và tính.
/// </summary>
public static class MayTinhHinhHoc
{
    /// <summary>Số chữ số thập phân khi làm tròn kết quả.</summary>
    public const int SoChuSoThapPhan = 2;

    public const string DaiLuongChuVi = "CHU_VI";
    public const string DaiLuongDienTich = "DIEN_TICH";
    public const string DaiLuongDuongCheo = "DUONG_CHEO";

    /// <summary>Đơn vị độ dài cho phép nhập.</summary>
    public static readonly string[] CacDonVi = ["mm", "cm", "dm", "m"];

    public const string DonViMacDinh = "cm";

    /// <summary>Tên tiếng Việt của từng biến, dùng cho nhãn ô nhập và thông báo lỗi.</summary>
    private static readonly Dictionary<string, string> TenBien = new()
    {
        ["a"] = "cạnh a",
        ["b"] = "cạnh b",
        ["c"] = "cạnh c",
        ["d"] = "cạnh d",
        ["h"] = "chiều cao h",
        ["d1"] = "đường chéo d₁",
        ["d2"] = "đường chéo d₂"
    };

    private sealed record CongThuc(
        string DaiLuong,
        string[] BienSo,
        string BieuThuc,
        Func<Dictionary<string, double>, double> Tinh,
        Func<Dictionary<string, double>, string[]> CacBuoc);

    private static readonly Dictionary<string, CongThuc> Bang = new()
    {
        ["CT_TG_CHUVI"] = new(DaiLuongChuVi, ["a", "b", "c", "d"], @"P = a + b + c + d",
            s => s["a"] + s["b"] + s["c"] + s["d"],
            s =>
            [
                $"Thay số: P = {So(s["a"])} + {So(s["b"])} + {So(s["c"])} + {So(s["d"])}",
                $"Tính: P = {So(s["a"] + s["b"] + s["c"] + s["d"])}"
            ]),

        ["CT_HCN_CV"] = new(DaiLuongChuVi, ["a", "b"], @"P = 2(a + b)",
            s => 2 * (s["a"] + s["b"]),
            s =>
            [
                $"Thay số: P = 2 × ({So(s["a"])} + {So(s["b"])})",
                $"Cộng trong ngoặc: P = 2 × {So(s["a"] + s["b"])}",
                $"Tính: P = {So(2 * (s["a"] + s["b"]))}"
            ]),

        ["CT_HCN_DT"] = new(DaiLuongDienTich, ["a", "b"], @"S = a \cdot b",
            s => s["a"] * s["b"],
            s =>
            [
                $"Thay số: S = {So(s["a"])} × {So(s["b"])}",
                $"Tính: S = {So(s["a"] * s["b"])}"
            ]),

        ["CT_HV_CV"] = new(DaiLuongChuVi, ["a"], @"P = 4a",
            s => 4 * s["a"],
            s =>
            [
                $"Thay số: P = 4 × {So(s["a"])}",
                $"Tính: P = {So(4 * s["a"])}"
            ]),

        ["CT_HV_DT"] = new(DaiLuongDienTich, ["a"], @"S = a^2",
            s => s["a"] * s["a"],
            s =>
            [
                $"Thay số: S = {So(s["a"])} × {So(s["a"])}",
                $"Tính: S = {So(s["a"] * s["a"])}"
            ]),

        ["CT_HBH_CV"] = new(DaiLuongChuVi, ["a", "b"], @"P = 2(a + b)",
            s => 2 * (s["a"] + s["b"]),
            s =>
            [
                $"Thay số: P = 2 × ({So(s["a"])} + {So(s["b"])})",
                $"Cộng trong ngoặc: P = 2 × {So(s["a"] + s["b"])}",
                $"Tính: P = {So(2 * (s["a"] + s["b"]))}"
            ]),

        ["CT_HBH_DT"] = new(DaiLuongDienTich, ["a", "h"], @"S = a \cdot h",
            s => s["a"] * s["h"],
            s =>
            [
                $"Thay số: S = {So(s["a"])} × {So(s["h"])}  (đáy nhân chiều cao, không nhân cạnh bên)",
                $"Tính: S = {So(s["a"] * s["h"])}"
            ]),

        ["CT_THOI_CV"] = new(DaiLuongChuVi, ["a"], @"P = 4a",
            s => 4 * s["a"],
            s =>
            [
                $"Hình thoi có bốn cạnh bằng nhau nên P = 4 × {So(s["a"])}",
                $"Tính: P = {So(4 * s["a"])}"
            ]),

        ["CT_THOI_DT"] = new(DaiLuongDienTich, ["d1", "d2"], @"S = \frac{d_1 \cdot d_2}{2}",
            s => s["d1"] * s["d2"] / 2,
            s =>
            [
                $"Thay số: S = ({So(s["d1"])} × {So(s["d2"])}) : 2",
                $"Nhân hai đường chéo: S = {So(s["d1"] * s["d2"])} : 2",
                $"Tính: S = {So(s["d1"] * s["d2"] / 2)}"
            ]),

        ["CT_HT_DT"] = new(DaiLuongDienTich, ["a", "b", "h"], @"S = \frac{(a + b) \cdot h}{2}",
            s => (s["a"] + s["b"]) * s["h"] / 2,
            s =>
            [
                $"Thay số: S = ({So(s["a"])} + {So(s["b"])}) × {So(s["h"])} : 2",
                $"Cộng hai đáy: S = {So(s["a"] + s["b"])} × {So(s["h"])} : 2",
                $"Nhân với chiều cao: S = {So((s["a"] + s["b"]) * s["h"])} : 2",
                $"Tính: S = {So((s["a"] + s["b"]) * s["h"] / 2)}"
            ]),

        ["CT_HCN_CHEO"] = new(DaiLuongDuongCheo, ["a", "b"], @"d = \sqrt{a^2 + b^2}",
            s => Math.Sqrt(s["a"] * s["a"] + s["b"] * s["b"]),
            s =>
            [
                $"Theo định lý Pythagore: d² = {So(s["a"])}² + {So(s["b"])}²",
                $"Bình phương: d² = {So(s["a"] * s["a"])} + {So(s["b"] * s["b"])} = {So(s["a"] * s["a"] + s["b"] * s["b"])}",
                $"Lấy căn bậc hai: d = {So(Math.Sqrt(s["a"] * s["a"] + s["b"] * s["b"]))}"
            ]),

        ["CT_HV_CHEO"] = new(DaiLuongDuongCheo, ["a"], @"d = a\sqrt{2}",
            s => s["a"] * Math.Sqrt(2),
            s =>
            [
                $"Thay số: d = {So(s["a"])} × √2",
                $"Tính: d = {So(s["a"] * Math.Sqrt(2))}"
            ])
    };

    public static bool CoCongThuc(string? ma) => ma is not null && Bang.ContainsKey(ma);

    public static IReadOnlyList<string> BienSo(string ma)
        => Bang.TryGetValue(ma, out var ct) ? ct.BienSo : [];

    public static string BieuThuc(string ma)
        => Bang.TryGetValue(ma, out var ct) ? ct.BieuThuc : string.Empty;

    public static string DaiLuong(string ma)
        => Bang.TryGetValue(ma, out var ct) ? ct.DaiLuong : string.Empty;

    public static string TenBienTiengViet(string bien)
        => TenBien.TryGetValue(bien, out var ten) ? ten : bien;

    public static string ChuanHoaDonVi(string? donVi)
    {
        var d = (donVi ?? string.Empty).Trim().ToLowerInvariant();
        return CacDonVi.Contains(d) ? d : DonViMacDinh;
    }

    /// <summary>Đơn vị của kết quả: diện tích thì bình phương, còn lại giữ nguyên.</summary>
    public static string DonViKetQua(string ma, string donVi)
        => DaiLuong(ma) == DaiLuongDienTich ? donVi + "²" : donVi;

    /// <summary>
    /// Đọc số từ ô nhập; nhận cả dấu phẩy và dấu chấm thập phân.
    /// Trả về null khi để trống hoặc không phải số.
    /// </summary>
    public static double? DocSo(string? tho)
    {
        if (string.IsNullOrWhiteSpace(tho))
        {
            return null;
        }

        var s = tho.Trim().Replace(',', '.');

        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var gia)
            ? gia
            : null;
    }

    /// <summary>
    /// Kiểm tra số đo từ ô nhập dạng chuỗi: phân biệt "chưa nhập" với "nhập không phải số",
    /// rồi kiểm tra tiếp theo BR-10. Trả về danh sách lỗi và các giá trị đã đọc được.
    /// </summary>
    public static (IReadOnlyList<string> Loi, IReadOnlyDictionary<string, double> SoDo) KiemTraTho(
        string ma, IReadOnlyDictionary<string, string?> soDoTho)
    {
        if (!Bang.TryGetValue(ma, out var ct))
        {
            return (["Không có công thức này. Em chọn lại hình và đại lượng nhé."],
                    new Dictionary<string, double>());
        }

        var loi = new List<string>();
        var daDoc = new Dictionary<string, double?>();
        var daSaiKieu = new HashSet<string>();

        foreach (var bien in ct.BienSo)
        {
            var tho = soDoTho.TryGetValue(bien, out var v) ? v : null;

            if (string.IsNullOrWhiteSpace(tho))
            {
                daDoc[bien] = null;
                continue;
            }

            var gia = DocSo(tho);
            if (gia is null)
            {
                // Báo đúng lý do "không phải số"; KiemTra sẽ bỏ qua ô này để không báo trùng.
                loi.Add(
                    $"Số đo {TenBienTiengViet(bien)} chưa đúng: em đang nhập \"{tho.Trim()}\". "
                    + "Em chỉ nhập số, ví dụ 5 hoặc 5,5.");
                daSaiKieu.Add(bien);
                continue;
            }

            daDoc[bien] = gia;
        }

        // Còn lại để KiemTra lo: chưa nhập, ≤ 0, và quy tắc bốn cạnh tứ giác.
        loi.AddRange(KiemTra(ma, daDoc, daSaiKieu));

        var soDo = daDoc
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value!.Value);

        return (loi, soDo);
    }

    /// <summary>
    /// Kiểm tra số đo theo BR-10. Trả về danh sách lỗi tiếng Việt nói rõ phải sửa gì;
    /// danh sách rỗng nghĩa là hợp lệ.
    /// </summary>
    public static IReadOnlyList<string> KiemTra(string ma, IReadOnlyDictionary<string, double?> soDo)
        => KiemTra(ma, soDo, boQua: null);

    /// <param name="boQua">
    /// Các biến đã được báo lỗi ở bước đọc số (ví dụ nhập chữ). Bỏ qua để không báo
    /// hai lỗi cho cùng một ô nhập.
    /// </param>
    private static IReadOnlyList<string> KiemTra(
        string ma, IReadOnlyDictionary<string, double?> soDo, ISet<string>? boQua)
    {
        if (!Bang.TryGetValue(ma, out var ct))
        {
            return ["Không có công thức này. Em chọn lại hình và đại lượng nhé."];
        }

        var loi = new List<string>();

        foreach (var bien in ct.BienSo)
        {
            if (boQua is not null && boQua.Contains(bien))
            {
                continue;
            }

            if (!soDo.TryGetValue(bien, out var gia) || gia is null)
            {
                loi.Add($"Em chưa nhập {TenBienTiengViet(bien)}.");
                continue;
            }

            if (double.IsNaN(gia.Value) || double.IsInfinity(gia.Value))
            {
                loi.Add($"Số đo {TenBienTiengViet(bien)} không phải là một số. Em nhập lại nhé.");
                continue;
            }

            // BR-10: độ dài phải lớn hơn 0.
            if (gia.Value <= 0)
            {
                loi.Add($"Số đo {TenBienTiengViet(bien)} phải lớn hơn 0 (em đang nhập {So(gia.Value)}).");
            }
        }

        if (loi.Count > 0)
        {
            return loi;
        }

        // BR-10: bốn cạnh của một tứ giác — mỗi cạnh phải nhỏ hơn tổng ba cạnh còn lại.
        // Chỉ xét khi đã đọc đủ bốn số đo.
        if (ma == "CT_TG_CHUVI" && ct.BienSo.All(b => soDo.TryGetValue(b, out var g) && g is not null))
        {
            var canh = ct.BienSo.Select(b => (Bien: b, Gia: soDo[b]!.Value)).ToList();
            var tong = canh.Sum(x => x.Gia);

            foreach (var (bien, gia) in canh)
            {
                if (gia >= tong - gia)
                {
                    loi.Add(
                        $"Bốn số đo này không ghép được thành tứ giác: {TenBienTiengViet(bien)} = {So(gia)} "
                        + $"mà tổng ba cạnh còn lại chỉ là {So(tong - gia)}. "
                        + "Mỗi cạnh phải nhỏ hơn tổng ba cạnh kia.");
                }
            }
        }

        // Chiều cao của hình bình hành không thể lớn hơn cạnh bên... nhưng ở đây chỉ nhập
        // đáy và chiều cao nên không kiểm tra thêm được; hình thang cũng vậy.
        return loi;
    }

    /// <summary>
    /// Tính kết quả, làm tròn 2 chữ số, kèm công thức LaTeX và các bước thay số.
    /// Gọi sau khi <see cref="KiemTra"/> cho danh sách rỗng.
    /// </summary>
    public static KetQuaTinh Tinh(string ma, IReadOnlyDictionary<string, double> soDo, string? donVi = null)
    {
        var ct = Bang[ma];
        var s = ct.BienSo.ToDictionary(b => b, b => soDo[b]);

        var tho = ct.Tinh(s);
        var giaTri = Math.Round(tho, SoChuSoThapPhan, MidpointRounding.AwayFromZero);
        var dv = ChuanHoaDonVi(donVi);
        var dvKetQua = DonViKetQua(ma, dv);

        var buoc = new List<string> { $"Công thức: {ct.BieuThuc}" };
        buoc.AddRange(ct.CacBuoc(s));
        buoc.Add($"Kết quả: {So(giaTri)} {dvKetQua}");

        return new KetQuaTinh(giaTri, So(giaTri), dvKetQua, ct.BieuThuc, buoc);
    }

    /// <summary>
    /// Định dạng số theo kiểu Việt Nam (dấu phẩy thập phân), bỏ số 0 vô nghĩa ở cuối.
    /// Ví dụ 16 → "16", 5.8309 → "5,83".
    /// </summary>
    public static string So(double gia)
    {
        var lamTron = Math.Round(gia, SoChuSoThapPhan, MidpointRounding.AwayFromZero);
        var vi = CultureInfo.GetCultureInfo("vi-VN");

        return lamTron == Math.Floor(lamTron)
            ? lamTron.ToString("0", vi)
            : lamTron.ToString("0.##", vi);
    }
}
