using GeoQuad.Web.Areas.QuanTri.Models;
using GeoQuad.Web.Areas.QuanTri.Services;

namespace GeoQuad.Tests.B_ApDung;

public sealed class QuanTriBaiTapTests
{
    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("A,B")]
    [InlineData("E")]
    public void TracNghiemChiMotDapAn(string answer)
    { var form=ValidForm(); form.DapAnDung=answer; Assert.False(KiemTraBaiTap.KiemTra(form).HopLe); }
    [Fact]
    public void BonPhuongAnVaConceptBatBuoc()
    {
        var form=ValidForm(); form.PhuongAn=["A","B","C"]; form.KhaiNiem=[];
        var result=KiemTraBaiTap.KiemTra(form);
        Assert.Contains("PhuongAn",result.Loi.Keys); Assert.Contains("KhaiNiem",result.Loi.Keys);
    }
    [Fact]
    public void DapAnSoKhongNhapVaKhongHuuHanBiTuChoiConKhongHopLe()
    {
        var form=ValidForm(); form.Loai="DAP_AN_SO"; form.DapAnSo="0"; form.SaiSo="0,01"; form.DonVi="cm";
        var valid=KiemTraBaiTap.KiemTra(form); Assert.True(valid.HopLe); Assert.Equal(0d,valid.BaiTap!.ThuocTinh["dapAnSo"]);
        form.DapAnSo=""; Assert.False(KiemTraBaiTap.KiemTra(form).HopLe);
        form.DapAnSo="NaN"; Assert.False(KiemTraBaiTap.KiemTra(form).HopLe);
    }
    [Fact]
    public void ChuyenLoaiXoaThuocTinhCuVaKhongOverpost()
    {
        var form=ValidForm(); form.Loai="CHUNG_MINH"; form.LoiGiaiMau="Lời giải từng bước";
        var result=KiemTraBaiTap.KiemTra(form); Assert.True(result.HopLe);
        Assert.Null(result.BaiTap!.ThuocTinh["phuongAn"]); Assert.Null(result.BaiTap.ThuocTinh["dapAnDung"]);
        Assert.Null(result.BaiTap.ThuocTinh["dapAnSo"]); Assert.DoesNotContain("ma",result.BaiTap.ThuocTinh.Keys);
        form.LoiGiaiMau=""; Assert.False(KiemTraBaiTap.KiemTra(form).HopLe);
    }
    [Fact]
    public void MaLopDoKhoVaStatusWhitelist()
    {
        var form=ValidForm(); form.Ma="x/../"; form.Lop=13; form.DoKho=4; form.TrangThai="PUBLIC";
        Assert.Equal(4,KiemTraBaiTap.KiemTra(form).Loi.Count);
    }
    public static BaiTapForm ValidForm() => new() { Ma="BT-TEST",Loai="TRAC_NGHIEM",De="Tính chu vi?",
        PhuongAn=["1","2","3","4"],DapAnDung="B",GiaiThich="Giải thích",Lop=8,DoKho=1,
        KhaiNiem=["HINH_CHU_NHAT"],SuDung=["CT_HCN_CV"],Nguon="Nhóm GeoQuad",TrangThai="NHAP" };
}
