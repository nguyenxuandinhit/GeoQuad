using GeoQuad.Web.Areas.HocTap.Services;

namespace GeoQuad.Tests.C_HocTap;

public sealed class ChamBaiTests
{
    [Theory]
    [InlineData("B", true)]
    [InlineData(" b ", true)]
    [InlineData("A", false)]
    public void ChamTracNghiem_KhongPhanBietHoaThuong(string answer, bool expected)
    {
        var result = ChamBai.TracNghiem(answer, "B");
        Assert.True(result.HopLe);
        Assert.Equal(expected, result.Dung);
    }

    [Fact]
    public void ChamTracNghiem_TuChoiLuaChonKhongTonTai()
    {
        Assert.False(ChamBai.TracNghiem("E", "B").HopLe);
    }

    [Theory]
    [InlineData("96,0", "cm²", true)]
    [InlineData("96.0", "cm²", true)]
    [InlineData("96,01", "cm²", true)]
    [InlineData("95.99", "cm²", true)]
    [InlineData("96.02", "cm²", false)]
    [InlineData("95.98", "cm²", false)]
    [InlineData("96", "cm", false)]
    public void ChamSo_ApDungSaiSoInclusiveVaDonVi(string answer, string unit, bool expected)
    {
        var result = ChamBai.So(answer, unit, 96m, .01m, "cm²");
        Assert.True(result.HopLe);
        Assert.Equal(expected, result.Dung);
    }

    [Fact]
    public void ChamSo_ChoPhepBaiKhongCoDonVi()
    {
        var result = ChamBai.So("12", "", 12m, .01m, "");
        Assert.True(result.HopLe);
        Assert.True(result.Dung);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1,234.5")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1e2")]
    public void ChamSo_TuChoiDauVaoKhongHopLe(string answer)
    {
        Assert.False(ChamBai.So(answer, "", 100m, .01m, "").HopLe);
    }
}
