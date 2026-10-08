using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>
/// US-15: đọc số đo từ ô nhập dạng chuỗi — phân biệt "chưa nhập" với "không phải số"
/// để thông báo lỗi nói rõ cách sửa (NFR-11).
/// </summary>
public class MayTinhThoTests
{
    private static Dictionary<string, string?> T(params (string Bien, string? Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    [Theory]
    [InlineData("5", 5.0)]
    [InlineData("5.5", 5.5)]
    [InlineData("5,5", 5.5)]        // học sinh Việt Nam quen dấu phẩy thập phân
    [InlineData("  12  ", 12.0)]
    [InlineData("0.01", 0.01)]
    public void DocDuocCaDauPhayVaDauCham(string tho, double mong)
        => Assert.Equal(mong, MayTinhHinhHoc.DocSo(tho));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("5 cm")]
    [InlineData("--5")]
    [InlineData(null)]
    public void KhongDocDuocThiTraVeNull(string? tho)
        => Assert.Null(MayTinhHinhHoc.DocSo(tho));

    [Fact]
    public void SoDoDungThiKhongLoiVaDocDuDuLieu()
    {
        var (loi, soDo) = MayTinhHinhHoc.KiemTraTho("CT_HCN_CV", T(("a", "5"), ("b", "3")));

        Assert.Empty(loi);
        Assert.Equal(5, soDo["a"]);
        Assert.Equal(3, soDo["b"]);
    }

    [Fact]
    public void NhapChuThiBaoKhongPhaiSoChuKhongBaoChuaNhap()
    {
        var (loi, _) = MayTinhHinhHoc.KiemTraTho("CT_HCN_CV", T(("a", "abc"), ("b", "3")));

        var l = Assert.Single(loi);
        Assert.Contains("chưa đúng", l);
        Assert.Contains("abc", l);
        Assert.Contains("chỉ nhập số", l);
        Assert.DoesNotContain("chưa nhập", l);
    }

    [Fact]
    public void DeTrongThiBaoChuaNhap()
    {
        var (loi, _) = MayTinhHinhHoc.KiemTraTho("CT_HCN_CV", T(("a", "5"), ("b", "")));

        Assert.Contains("Em chưa nhập cạnh b.", loi);
    }

    [Fact]
    public void VuaNhapChuVuaDeTrongThiBaoCaHaiLoi()
    {
        var (loi, _) = MayTinhHinhHoc.KiemTraTho("CT_HT_DT", T(("a", "abc"), ("b", ""), ("h", "4")));

        Assert.Equal(2, loi.Count);
        Assert.Contains(loi, l => l.Contains("chưa đúng"));
        Assert.Contains(loi, l => l.Contains("chưa nhập"));
    }

    [Fact]
    public void DauPhayThapPhanDungDuocDeTinh()
    {
        var (loi, soDo) = MayTinhHinhHoc.KiemTraTho("CT_HCN_DT", T(("a", "2,5"), ("b", "4")));

        Assert.Empty(loi);
        Assert.Equal(10, MayTinhHinhHoc.Tinh("CT_HCN_DT", soDo).GiaTri);
    }

    [Fact]
    public void SoAmVanBiBatTheoBR10()
    {
        var (loi, _) = MayTinhHinhHoc.KiemTraTho("CT_HV_CV", T(("a", "-5")));

        Assert.Contains("lớn hơn 0", Assert.Single(loi));
    }

    [Fact]
    public void MaCongThucLaThiBaoLoi()
    {
        var (loi, soDo) = MayTinhHinhHoc.KiemTraTho("KHONG_CO", T(("a", "5")));

        Assert.NotEmpty(loi);
        Assert.Empty(soDo);
    }
}
