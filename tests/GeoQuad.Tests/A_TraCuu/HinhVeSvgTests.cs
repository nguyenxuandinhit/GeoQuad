using System.Text.RegularExpressions;
using System.Xml.Linq;
using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-10: hình SVG minh hoạ cho bảy hình tứ giác (NFR-11 — SVG phải có title).</summary>
public class HinhVeSvgTests
{
    public static IEnumerable<object[]> BayHinh() =>
        new[] { "TU_GIAC", "HINH_THANG", "HINH_THANG_CAN", "HINH_BINH_HANH", "HINH_CHU_NHAT", "HINH_THOI", "HINH_VUONG" }
            .Select(ma => new object[] { ma });

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void BayHinhDeuVeDuoc(string ma) => Assert.NotNull(HinhVeSvg.Ve(ma));

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void SvgLaXmlHopLe(string ma)
    {
        var svg = HinhVeSvg.Ve(ma)!;

        var doc = XDocument.Parse(svg);   // ném ngoại lệ nếu thẻ không đóng đúng

        Assert.Equal("svg", doc.Root!.Name.LocalName);
    }

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void SvgCoTitleVaDesc(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";

        var title = doc.Root!.Element(ns + "title");
        var desc = doc.Root.Element(ns + "desc");

        Assert.NotNull(title);
        Assert.False(string.IsNullOrWhiteSpace(title!.Value));
        Assert.NotNull(desc);
        Assert.False(string.IsNullOrWhiteSpace(desc!.Value));
    }

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void MoiHinhCoDungMotTuGiacBonDinh(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";

        var polygon = Assert.Single(doc.Root!.Elements(ns + "polygon"));
        var soDinh = polygon.Attribute("points")!.Value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        Assert.Equal(4, soDinh);
    }

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void MoiHinhCoDuNhanDinhABCD(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";

        var nhan = doc.Root!.Elements(ns + "text").Select(t => t.Value).ToArray();

        Assert.Equal(new[] { "A", "B", "C", "D" }, nhan);
    }

    [Theory]
    [MemberData(nameof(BayHinh))]
    public void MoiDinhNamTrongKhungNhin(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var khung = doc.Root!.Attribute("viewBox")!.Value.Split(' ').Select(int.Parse).ToArray();

        foreach (var cap in doc.Root.Element(ns + "polygon")!.Attribute("points")!.Value
                     .Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var xy = cap.Split(',').Select(int.Parse).ToArray();
            Assert.InRange(xy[0], khung[0], khung[2]);
            Assert.InRange(xy[1], khung[1], khung[3]);
        }
    }

    [Theory]
    [InlineData("HINH_CHU_NHAT")]
    [InlineData("HINH_VUONG")]
    public void HinhCoGocVuongDuocVeKyHieuGocVuong(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";

        Assert.Single(doc.Root!.Elements(ns + "polyline"));
    }

    [Theory]
    [InlineData("HINH_THOI")]
    [InlineData("HINH_VUONG")]
    [InlineData("HINH_CHU_NHAT")]
    [InlineData("HINH_THANG_CAN")]
    public void HinhCoTinhChatDuongCheoThiVeHaiDuongCheoNetDut(string ma)
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
        XNamespace ns = "http://www.w3.org/2000/svg";

        var cheo = doc.Root!.Elements(ns + "line").ToList();

        Assert.Equal(2, cheo.Count);
        // Nét đứt: phân biệt đường chéo với cạnh mà không chỉ dựa vào màu (NFR-11).
        Assert.All(cheo, c => Assert.NotNull(c.Attribute("stroke-dasharray")));
    }

    [Theory]
    [InlineData("DUONG_CHEO")]
    [InlineData("GOC_VUONG")]
    [InlineData("KHONG_CO_MA_NAY")]
    [InlineData("")]
    [InlineData(null)]
    public void YeuToVaMaLaThiKhongVeHinh(string? ma)
    {
        Assert.Null(HinhVeSvg.Ve(ma));
        Assert.False(HinhVeSvg.CoHinhVe(ma));
    }

    [Fact]
    public void HinhVuongVaHinhChuNhatCoCanhNgangDoc()
    {
        // Hình chữ nhật và hình vuông phải vẽ đúng bốn góc vuông: các cạnh song song với trục.
        foreach (var ma in new[] { "HINH_CHU_NHAT", "HINH_VUONG" })
        {
            var doc = XDocument.Parse(HinhVeSvg.Ve(ma)!);
            XNamespace ns = "http://www.w3.org/2000/svg";
            var d = doc.Root!.Element(ns + "polygon")!.Attribute("points")!.Value
                .Split(' ').Select(p => p.Split(',').Select(int.Parse).ToArray()).ToArray();

            Assert.Equal(d[0][1], d[1][1]);   // A và B cùng tung độ
            Assert.Equal(d[2][1], d[3][1]);   // C và D cùng tung độ
            Assert.Equal(d[1][0], d[2][0]);   // B và C cùng hoành độ
            Assert.Equal(d[3][0], d[0][0]);   // D và A cùng hoành độ
        }
    }

    [Fact]
    public void HinhVuongCoBonCanhBangNhau()
    {
        var doc = XDocument.Parse(HinhVeSvg.Ve("HINH_VUONG")!);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var d = doc.Root!.Element(ns + "polygon")!.Attribute("points")!.Value
            .Split(' ').Select(p => p.Split(',').Select(int.Parse).ToArray()).ToArray();

        var rong = Math.Abs(d[1][0] - d[0][0]);
        var cao = Math.Abs(d[3][1] - d[0][1]);

        Assert.Equal(rong, cao);
    }

    [Fact]
    public void KhongCoKyTuHtmlChuaThoatTrongTitle()
    {
        // Phòng trường hợp sau này thêm hình có dấu < > & trong tên.
        foreach (var ma in new[] { "TU_GIAC", "HINH_THOI" })
        {
            var svg = HinhVeSvg.Ve(ma)!;
            var title = Regex.Match(svg, "<title[^>]*>(.*?)</title>").Groups[1].Value;

            Assert.DoesNotContain('<', title);
            Assert.DoesNotContain('>', title);
        }
    }
}
