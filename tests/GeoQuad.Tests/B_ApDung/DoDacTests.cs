using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Services;

namespace GeoQuad.Tests.B_ApDung;

public sealed class DoDacTests
{
    [Theory]
    [InlineData(2,.9,2,.9,2.19,2.19,"HINH_CHU_NHAT","DH_HBH_2,DH_HCN_3")]
    [InlineData(1,1,1,1,1.414,1.414,"HINH_VUONG","DH_THOI_1,DH_HV_5")]
    [InlineData(1,.995,.991,.986,1.4,1.4,"HINH_VUONG","DH_HBH_2,DH_HCN_3,DH_HV_1")]
    [InlineData(1,1,1,1,1.2,1.6,"HINH_THOI","DH_THOI_1")]
    [InlineData(2,1,2,1,2,2.5,"HINH_BINH_HANH","DH_HBH_2")]
    [InlineData(2,1,1.8,1.1,2,2.2,"KHONG_XAC_DINH","")]
    public void PhanLoaiVaTraceDung(double ab,double bc,double cd,double da,double ac,double bd,string shape,string trace)
    {
        var result = KiemTraHinhDang.KiemTra(new(ab,bc,cd,da,ac,bd,1,"m"));
        Assert.Null(result.Loi);
        Assert.Equal(shape,result.Hinh);
        Assert.Equal(trace,string.Join(",",result.DauHieu));
    }

    [Theory]
    [InlineData(99,100,true)]
    [InlineData(100,99,true)]
    [InlineData(98.99,100,false)]
    [InlineData(100,98.99,false)]
    public void BienSaiSoDoiXung(double a,double b,bool expected) => Assert.Equal(expected,KiemTraHinhDang.Bang(a,b,.01));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void SoDoKhongHopLeKhongKetLuan(double ab) =>
        Assert.NotNull(KiemTraHinhDang.KiemTra(new(ab,1,1,1,1,1,1,"cm")).Loi);

    [Fact]
    public void TamGiacSuyBienBiTuChoi() => Assert.NotNull(KiemTraHinhDang.KiemTra(new(1,1,1,1,2,1,1,"m")).Loi);

    [Theory]
    [InlineData("2,00",true)]
    [InlineData("2.00",true)]
    [InlineData("2,0.0",false)]
    [InlineData("NaN",false)]
    [InlineData("Infinity",false)]
    [InlineData("1 000",false)]
    public void ParserDocLapCulture(string value,bool expected)
    {
        var form = ValidForm(); form.AB=value;
        Assert.Equal(expected,form.TryParse(out var measurements,out _));
        if(expected) Assert.Equal(2,measurements!.AB);
    }

    [Fact]
    public void DonViVaSaiSoBatBuoc()
    {
        var form = ValidForm(); form.DonVi="km"; form.SaiSo="100";
        Assert.False(form.TryParse(out _,out var errors));
        Assert.Contains("DonVi",errors.Keys); Assert.Contains("SaiSo",errors.Keys);
    }

    [Fact]
    public void CapLechLonNhatKhongSoCanhVoiCheo()
    {
        var result=KiemTraHinhDang.KiemTra(new(2,1,1.8,1.1,2,2.2,1,"m"));
        Assert.Equal("AB/BC",result.CapLech); Assert.Equal(.5,result.Lech);
    }

    private static DoDacForm ValidForm() => new() { AB="2",BC=".9",CD="2",DA=".9",AC="2.19",BD="2.19",SaiSo="1",DonVi="m" };
}
