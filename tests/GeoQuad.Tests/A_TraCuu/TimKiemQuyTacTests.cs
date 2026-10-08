using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-11: thoát ký tự Lucene và dựng chuỗi truy vấn (FR-12, NFR-14).</summary>
public class TimKiemQuyTacTests
{
    [Theory]
    [InlineData("hinh", "hinh")]
    [InlineData("thoi", "thoi")]
    [InlineData("hình", "hình")]          // chữ có dấu không phải ký tự đặc biệt của Lucene
    [InlineData("a1_2", "a1_2")]
    public void TuBinhThuongKhongBiDoi(string vao, string mong)
        => Assert.Equal(mong, TimKiemQuyTac.ThoatKyTu(vao));

    [Theory]
    [InlineData("(", @"\(")]
    [InlineData(")", @"\)")]
    [InlineData("*", @"\*")]
    [InlineData("?", @"\?")]
    [InlineData("\"", "\\\"")]
    [InlineData("+", @"\+")]
    [InlineData("-", @"\-")]
    [InlineData("!", @"\!")]
    [InlineData("{", @"\{")]
    [InlineData("}", @"\}")]
    [InlineData("[", @"\[")]
    [InlineData("]", @"\]")]
    [InlineData("^", @"\^")]
    [InlineData("~", @"\~")]
    [InlineData(":", @"\:")]
    [InlineData("/", @"\/")]
    [InlineData(@"\", @"\\")]
    [InlineData("&", @"\&")]
    [InlineData("|", @"\|")]
    public void MoiKyTuDacBietDeuDuocThoat(string vao, string mong)
        => Assert.Equal(mong, TimKiemQuyTac.ThoatKyTu(vao));

    [Fact]
    public void ThoatCaChuoiLanLon()
    {
        // AC US-11: ký tự ( * " không gây lỗi cú pháp.
        Assert.Equal(@"hinh\(thoi\)", TimKiemQuyTac.ThoatKyTu("hinh(thoi)"));
        Assert.Equal(@"\*\*\*", TimKiemQuyTac.ThoatKyTu("***"));
        Assert.Equal("\\\"abc\\\"", TimKiemQuyTac.ThoatKyTu("\"abc\""));
        Assert.Equal(@"a\&\&b", TimKiemQuyTac.ThoatKyTu("a&&b"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ThoatChuoiRong(string? vao)
        => Assert.Equal(string.Empty, TimKiemQuyTac.ThoatKyTu(vao));

    [Fact]
    public void TachTuBoKhoangTrangThua()
    {
        Assert.Equal(new[] { "hinh", "thoi" }, TimKiemQuyTac.TachTu("  hinh   thoi  "));
        Assert.Equal(new[] { "a", "b", "c" }, TimKiemQuyTac.TachTu("a\tb\nc"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(null)]
    public void TuKhoaRongThiKhongCoTuNao(string? vao)
        => Assert.Empty(TimKiemQuyTac.TachTu(vao));

    [Fact]
    public void GioiHanSoTuVaDoDai()
    {
        var nhieuTu = string.Join(' ', Enumerable.Range(1, 50).Select(i => $"tu{i}"));

        Assert.Equal(TimKiemQuyTac.SoTuToiDa, TimKiemQuyTac.TachTu(nhieuTu).Count);
        Assert.True(TimKiemQuyTac.TachTu(new string('a', 500)).All(t => t.Length <= TimKiemQuyTac.DoDaiToiDa));
    }

    [Fact]
    public void NoiTuBangAndMacDinh()
        => Assert.Equal("hinh AND thoi", TimKiemQuyTac.TaoTruyVan("hinh thoi"));

    [Fact]
    public void NoiTuBangOrKhiNoiLong()
        => Assert.Equal("hinh OR thoi", TimKiemQuyTac.TaoTruyVan("hinh thoi", TimKiemQuyTac.ToanTuOr));

    [Fact]
    public void MotTuThiKhongCoToanTu()
    {
        Assert.Equal("thoi", TimKiemQuyTac.TaoTruyVan("thoi"));
        Assert.Equal("thoi", TimKiemQuyTac.TaoTruyVan("thoi", TimKiemQuyTac.ToanTuOr));
    }

    [Fact]
    public void TruyVanDaThoatKyTuTungTu()
    {
        Assert.Equal(@"hinh\(1\) AND thoi\*", TimKiemQuyTac.TaoTruyVan("hinh(1) thoi*"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TuKhoaRongThiTruyVanLaNull(string? vao)
        => Assert.Null(TimKiemQuyTac.TaoTruyVan(vao));

    [Fact]
    public void NhomHienThiTheoNhan()
    {
        Assert.Equal("Khái niệm", TimKiemQuyTac.NhomHienThi(["KhaiNiem"]));
        Assert.Equal("Tính chất & dấu hiệu", TimKiemQuyTac.NhomHienThi(["DinhLy", "TinhChat"]));
        Assert.Equal("Tính chất & dấu hiệu", TimKiemQuyTac.NhomHienThi(["DinhLy", "DauHieu"]));
        Assert.Equal("Công thức", TimKiemQuyTac.NhomHienThi(["CongThuc"]));
        Assert.Equal("Khác", TimKiemQuyTac.NhomHienThi(["DinhLy"]));
        Assert.Equal("Khác", TimKiemQuyTac.NhomHienThi(null));
    }

    [Fact]
    public void BaNhomDauTheoDungThuTuSCR06()
        => Assert.Equal(
            new[] { "Khái niệm", "Tính chất & dấu hiệu", "Công thức" },
            TimKiemQuyTac.ThuTuNhom[..3]);

    [Fact]
    public void CatDoanTrichGiuNguyenNeuNgan()
        => Assert.Equal("Tứ giác có bốn góc vuông.", TimKiemQuyTac.CatDoanTrich("Tứ giác có bốn góc vuông."));

    [Fact]
    public void CatDoanTrichKhongDutGiuaTu()
    {
        var dai = string.Join(' ', Enumerable.Repeat("canh", 80));

        var ngan = TimKiemQuyTac.CatDoanTrich(dai, 40);

        Assert.EndsWith("…", ngan);
        Assert.True(ngan.Length <= 41);
        Assert.DoesNotContain("can…", ngan);   // không cắt giữa một từ
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CatDoanTrichRong(string? vao)
        => Assert.Equal(string.Empty, TimKiemQuyTac.CatDoanTrich(vao));
}
