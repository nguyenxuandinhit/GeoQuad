using System.Globalization;
using System.Xml.Linq;
using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-16: vẽ hình theo số liệu (FR-31, NFR-11).</summary>
public class VeHinhSvgTests
{
    private static readonly XNamespace Ns = "http://www.w3.org/2000/svg";

    private static Dictionary<string, double> S(params (string Bien, double Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    private static double KhoangCach(Dinh p, Dinh q)
        => Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2));

    [Theory]
    [InlineData("HINH_CHU_NHAT")]
    [InlineData("HINH_VUONG")]
    [InlineData("HINH_BINH_HANH")]
    [InlineData("HINH_THOI")]
    [InlineData("HINH_THANG")]
    [InlineData("HINH_THANG_CAN")]
    public void SauHinhDeuVeDuoc(string ma) => Assert.True(VeHinhSvg.VeDuoc(ma));

    [Theory]
    [InlineData("TU_GIAC")]        // máy tính không hỗ trợ tứ giác tổng quát
    [InlineData("DUONG_CHEO")]
    [InlineData(null)]
    public void HinhKhongHoTroThiKhongVe(string? ma)
    {
        Assert.False(VeHinhSvg.VeDuoc(ma));
        Assert.Null(VeHinhSvg.Ve(ma, S(("a", 5))));
    }

    [Fact]
    public void ThieuSoDoThiKhongVe()
    {
        Assert.Null(VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5))));          // thiếu b
        Assert.Null(VeHinhSvg.Ve("HINH_THANG", S(("a", 6))));             // thiếu b
        Assert.Null(VeHinhSvg.Ve("HINH_VUONG", S(("a", 0))));             // số đo không hợp lệ
    }

    // ---- AC: hình chữ nhật 5 × 3 có tỉ lệ cạnh đúng 5:3 ----

    [Fact]
    public void HinhChuNhat5x3CoTiLeCanhDung5Tren3()
    {
        var h = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)))!;
        var d = h.CacDinh;

        var canhA = KhoangCach(d[0], d[1]);   // AB ứng với a
        var canhB = KhoangCach(d[1], d[2]);   // BC ứng với b

        Assert.Equal(5.0 / 3.0, canhA / canhB, precision: 2);
    }

    [Fact]
    public void HinhChuNhatCoBonGocVuong()
    {
        var d = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)))!.CacDinh;

        Assert.Equal(d[0].Y, d[1].Y, precision: 2);   // AB nằm ngang
        Assert.Equal(d[2].Y, d[3].Y, precision: 2);   // DC nằm ngang
        Assert.Equal(d[1].X, d[2].X, precision: 2);   // BC thẳng đứng
        Assert.Equal(d[3].X, d[0].X, precision: 2);   // AD thẳng đứng
    }

    [Fact]
    public void HinhVuongCoBonCanhBangNhau()
    {
        var d = VeHinhSvg.Ve("HINH_VUONG", S(("a", 6)))!.CacDinh;

        var canh = new[]
        {
            KhoangCach(d[0], d[1]), KhoangCach(d[1], d[2]),
            KhoangCach(d[2], d[3]), KhoangCach(d[3], d[0])
        };

        Assert.All(canh, c => Assert.Equal(canh[0], c, precision: 2));
    }

    [Fact]
    public void HinhThoiDungTuHaiDuongCheoCoTiLeDung()
    {
        // d1 = 12, d2 = 16 → tỉ lệ hai đường chéo phải là 12:16.
        var d = VeHinhSvg.Ve("HINH_THOI", S(("d1", 12), ("d2", 16)))!.CacDinh;

        var cheo1 = KhoangCach(d[0], d[2]);
        var cheo2 = KhoangCach(d[1], d[3]);

        Assert.Equal(12.0 / 16.0, cheo1 / cheo2, precision: 2);
    }

    [Fact]
    public void HinhThoiHaiDuongCheoVuongGocVaCatNhauTaiTrungDiem()
    {
        var d = VeHinhSvg.Ve("HINH_THOI", S(("d1", 12), ("d2", 16)))!.CacDinh;

        // Trung điểm hai đường chéo trùng nhau.
        Assert.Equal((d[0].X + d[2].X) / 2, (d[1].X + d[3].X) / 2, precision: 1);
        Assert.Equal((d[0].Y + d[2].Y) / 2, (d[1].Y + d[3].Y) / 2, precision: 1);

        // Tích vô hướng của hai vectơ đường chéo bằng 0 → vuông góc.
        var v1 = (X: d[2].X - d[0].X, Y: d[2].Y - d[0].Y);
        var v2 = (X: d[3].X - d[1].X, Y: d[3].Y - d[1].Y);
        Assert.Equal(0, v1.X * v2.X + v1.Y * v2.Y, precision: 0);
    }

    [Fact]
    public void HinhThangCanDatCanGiua()
    {
        var d = VeHinhSvg.Ve("HINH_THANG_CAN", S(("a", 6), ("b", 10), ("h", 4)))!.CacDinh;

        // Đáy nhỏ AB cân giữa đáy lớn DC: hai cạnh bên dài bằng nhau.
        Assert.Equal(KhoangCach(d[0], d[3]), KhoangCach(d[1], d[2]), precision: 1);
    }

    [Fact]
    public void HinhThangThuongKhongCanGiua()
    {
        var d = VeHinhSvg.Ve("HINH_THANG", S(("a", 6), ("b", 10), ("h", 4)))!.CacDinh;

        Assert.NotEqual(KhoangCach(d[0], d[3]), KhoangCach(d[1], d[2]), precision: 1);
    }

    [Fact]
    public void HinhBinhHanhDungChieuCaoKhiCoH()
    {
        var d = VeHinhSvg.Ve("HINH_BINH_HANH", S(("a", 12), ("h", 5)))!.CacDinh;

        // Hai cạnh đối song song và bằng nhau.
        Assert.Equal(KhoangCach(d[0], d[1]), KhoangCach(d[3], d[2]), precision: 1);
        Assert.Equal(KhoangCach(d[1], d[2]), KhoangCach(d[0], d[3]), precision: 1);
    }

    [Fact]
    public void HinhBinhHanhKhongCoHThiDungGocMacDinh60Do()
    {
        var h = VeHinhSvg.Ve("HINH_BINH_HANH", S(("a", 10), ("b", 6)));

        Assert.NotNull(h);
        Assert.Equal(60, VeHinhSvg.GocMacDinhHinhBinhHanh);
    }

    // ---- Khung nhìn và tỉ lệ ----

    [Theory]
    [InlineData(5, 3)]
    [InlineData(100, 2)]
    [InlineData(1, 50)]
    public void MoiDinhNamTrongKhung320x240(double a, double b)
    {
        var d = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", a), ("b", b)))!.CacDinh;

        Assert.All(d, p =>
        {
            Assert.InRange(p.X, 0, VeHinhSvg.RongKhung);
            Assert.InRange(p.Y, 0, VeHinhSvg.CaoKhung);
        });
    }

    [Fact]
    public void HinhRatDaiVanGiuDungTiLe()
    {
        var d = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 100), ("b", 2)))!.CacDinh;

        var tiLe = KhoangCach(d[0], d[1]) / KhoangCach(d[1], d[2]);

        Assert.Equal(50, tiLe, precision: 1);
    }

    // ---- SVG ----

    [Fact]
    public void SvgLaXmlHopLeCoTitleVaDesc()
    {
        var svg = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)))!.Svg;

        var doc = XDocument.Parse(svg);

        Assert.Equal("svg", doc.Root!.Name.LocalName);
        Assert.False(string.IsNullOrWhiteSpace(doc.Root.Element(Ns + "title")!.Value));
        Assert.False(string.IsNullOrWhiteSpace(doc.Root.Element(Ns + "desc")!.Value));
    }

    [Fact]
    public void MoTaThayTheCoSoDoDeHocSinhDungTrinhDocManHinh()
    {
        var svg = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)), "cm")!.Svg;
        var desc = XDocument.Parse(svg).Root!.Element(Ns + "desc")!.Value;

        Assert.Contains("a = 5 cm", desc);
        Assert.Contains("b = 3 cm", desc);
        Assert.Contains("A, B, C, D", desc);
    }

    [Fact]
    public void SvgCoNhanDinhABCD()
    {
        var svg = VeHinhSvg.Ve("HINH_VUONG", S(("a", 5)))!.Svg;
        var chu = XDocument.Parse(svg).Root!.Elements(Ns + "text").Select(t => t.Value).ToList();

        Assert.Contains("A", chu);
        Assert.Contains("B", chu);
        Assert.Contains("C", chu);
        Assert.Contains("D", chu);
    }

    [Fact]
    public void SvgCoNhanDoDaiCanh()
    {
        var svg = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)), "cm")!.Svg;
        var chu = XDocument.Parse(svg).Root!.Elements(Ns + "text").Select(t => t.Value).ToList();

        Assert.Contains("a = 5 cm", chu);
        Assert.Contains("b = 3 cm", chu);
    }

    [Fact]
    public void HinhCoGocVuongDuocVeKyHieuGocVuong()
    {
        foreach (var ma in new[] { "HINH_CHU_NHAT", "HINH_VUONG" })
        {
            var soDo = ma == "HINH_VUONG" ? S(("a", 5)) : S(("a", 5), ("b", 3));
            var doc = XDocument.Parse(VeHinhSvg.Ve(ma, soDo)!.Svg);

            Assert.Single(doc.Root!.Elements(Ns + "polyline"));
        }
    }

    [Fact]
    public void VeDuongCheoNetDutKhiDangTinhDuongCheo()
    {
        var co = XDocument.Parse(
            VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)), veDuongCheo: true)!.Svg);
        var khong = XDocument.Parse(
            VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)), veDuongCheo: false)!.Svg);

        var cheo = co.Root!.Elements(Ns + "line").ToList();
        Assert.Equal(2, cheo.Count);
        Assert.All(cheo, c => Assert.NotNull(c.Attribute("stroke-dasharray")));
        Assert.Empty(khong.Root!.Elements(Ns + "line"));
    }

    [Fact]
    public void ToaDoTrongSvgDungDauChamThapPhanKhongPhuThuocNgonNgu()
    {
        // Nếu dùng dấu phẩy thập phân thì thuộc tính points của SVG sẽ sai.
        var cu = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("vi-VN");
            var svg = VeHinhSvg.Ve("HINH_THANG", S(("a", 6), ("b", 10), ("h", 4)))!.Svg;

            var doc = XDocument.Parse(svg);
            var points = doc.Root!.Element(Ns + "polygon")!.Attribute("points")!.Value;

            foreach (var cap in points.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var xy = cap.Split(',');
                Assert.Equal(2, xy.Length);
                Assert.True(double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out _));
                Assert.True(double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out _));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = cu;
        }
    }

    [Fact]
    public void BonDinhTheoDungThuTuABCD()
    {
        var d = VeHinhSvg.Ve("HINH_CHU_NHAT", S(("a", 5), ("b", 3)))!.CacDinh;

        Assert.Equal(["A", "B", "C", "D"], d.Select(x => x.Nhan).ToArray());
    }
}
