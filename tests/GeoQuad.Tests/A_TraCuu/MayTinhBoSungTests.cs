using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>
/// Hai công thức bổ sung ngoài Phụ lục B: chu vi hình thang và đường chéo còn lại
/// của hình thoi (US-15).
/// </summary>
public class MayTinhBoSungTests
{
    private static Dictionary<string, double> S(params (string Bien, double Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    private static Dictionary<string, double?> Sn(params (string Bien, double? Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    [Fact]
    public void HaiCongThucMoiDeuCoTrongBang()
    {
        Assert.True(MayTinhHinhHoc.CoCongThuc("CT_HT_CV"));
        Assert.True(MayTinhHinhHoc.CoCongThuc("CT_THOI_CHEO"));
    }

    [Theory]
    [InlineData("CT_HT_CV", new[] { "a", "b", "c", "d" })]
    [InlineData("CT_THOI_CHEO", new[] { "a", "d1" })]
    public void BienSoKhopVoiSeed(string ma, string[] mong)
        => Assert.Equal(mong, MayTinhHinhHoc.BienSo(ma));

    [Fact]
    public void ChuViHinhThang()
        // Hai đáy 8 và 14, hai cạnh bên 5 và 6 → 33
        => Assert.Equal(33, MayTinhHinhHoc.Tinh("CT_HT_CV", S(("a", 8), ("b", 14), ("c", 5), ("d", 6))).GiaTri);

    [Fact]
    public void KhongDuaDuongTrungBinhVaoMayTinh()
    {
        // Máy tính mượn công thức của hình tổng quát gần nhất, nên nếu hình thang có
        // công thức đường trung bình thì hình thoi và hình chữ nhật mượn luôn và hỏi
        // học sinh "hai đáy" — nhập chiều dài với chiều rộng là ra kết quả sai.
        // Kiến thức này giữ ở dạng tính chất TC_HT_2 trong thư viện.
        Assert.False(MayTinhHinhHoc.CoCongThuc("CT_HT_TB"));
    }

    [Fact]
    public void DuongCheoConLaiCuaHinhThoi()
    {
        // Khớp với bài BT-023 của phần C: cạnh 13, một đường chéo 10 → đường chéo kia 24.
        var kq = MayTinhHinhHoc.Tinh("CT_THOI_CHEO", S(("a", 13), ("d1", 10)), "cm");

        Assert.Equal(24, kq.GiaTri);
        Assert.Equal("cm", kq.DonViKetQua);
    }

    [Theory]
    [InlineData(5, 6, 8)]        // nửa chéo 3, cạnh 5 → nửa chéo kia 4 → 8
    [InlineData(10, 12, 16)]     // nửa chéo 6, cạnh 10 → nửa chéo kia 8 → 16
    public void DuongCheoHinhThoiTheoBoBaPythagore(double a, double d1, double mong)
        => Assert.Equal(mong, MayTinhHinhHoc.Tinh("CT_THOI_CHEO", S(("a", a), ("d1", d1))).GiaTri);

    [Fact]
    public void HinhVuongLaTruongHopRiengCuaHinhThoi()
    {
        // Hình vuông cạnh a có hai đường chéo bằng nhau, đều bằng a√2.
        var kq = MayTinhHinhHoc.Tinh("CT_THOI_CHEO", S(("a", 5), ("d1", 5 * Math.Sqrt(2))));

        Assert.Equal(7.07, kq.GiaTri);
    }

    [Fact]
    public void CacBuocDuongCheoThoiCoNhacPythagore()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_THOI_CHEO", S(("a", 13), ("d1", 10)), "cm");

        Assert.Contains(kq.CacBuoc, b => b.Contains("Pythagore"));
        Assert.Contains(kq.CacBuoc, b => b.Contains("144"));   // 169 − 25
    }

    // ---- BR-10 ----

    [Fact]
    public void DuongCheoPhaiNhoHonHaiLanCanh()
    {
        // d₁ = 30 với cạnh 13 thì bốn cạnh không khép lại thành hình thoi.
        var loi = MayTinhHinhHoc.KiemTra("CT_THOI_CHEO", Sn(("a", 13), ("d1", 30)));

        var l = Assert.Single(loi);
        Assert.Contains("nhỏ hơn hai lần cạnh", l);
        Assert.Contains("không khép lại thành hình thoi", l);
    }

    [Fact]
    public void TruongHopBienDuongCheoBangHaiLanCanhCungBiChan()
        // d₁ = 2a thì hình thoi suy biến thành một đoạn thẳng.
        => Assert.NotEmpty(MayTinhHinhHoc.KiemTra("CT_THOI_CHEO", Sn(("a", 5), ("d1", 10))));

    [Fact]
    public void DuongCheoHopLeThiKhongBaoLoi()
        => Assert.Empty(MayTinhHinhHoc.KiemTra("CT_THOI_CHEO", Sn(("a", 13), ("d1", 10))));

    [Fact]
    public void ChuViHinhThangCungApDungQuyTacBonCanh()
    {
        // 20 ≥ 1 + 1 + 1 nên bốn số đo này không ghép được thành hình thang.
        var loi = MayTinhHinhHoc.KiemTra("CT_HT_CV", Sn(("a", 20), ("b", 1), ("c", 1), ("d", 1)));

        Assert.Contains("không ghép được thành tứ giác", Assert.Single(loi));
    }

    [Fact]
    public void ChuViHinhThangHopLeThiKhongBaoLoi()
        => Assert.Empty(MayTinhHinhHoc.KiemTra("CT_HT_CV", Sn(("a", 8), ("b", 14), ("c", 5), ("d", 6))));
}
