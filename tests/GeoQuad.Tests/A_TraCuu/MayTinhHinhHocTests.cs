using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-15: máy tính hình học (FR-30, BR-10). README yêu cầu ≥ 8 trường hợp.</summary>
public class MayTinhHinhHocTests
{
    private static Dictionary<string, double> S(params (string Bien, double Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    private static Dictionary<string, double?> Sn(params (string Bien, double? Gia)[] cap)
        => cap.ToDictionary(x => x.Bien, x => x.Gia);

    // ---- AC: hình chữ nhật 5 × 3 ----

    [Fact]
    public void HinhChuNhat5x3ChuViBang16()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_HCN_CV", S(("a", 5), ("b", 3)), "cm");

        Assert.Equal(16, kq.GiaTri);
        Assert.Equal("16", kq.GiaTriHienThi);
        Assert.Equal("cm", kq.DonViKetQua);
    }

    [Fact]
    public void HinhChuNhat5x3DienTichBang15()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_HCN_DT", S(("a", 5), ("b", 3)), "cm");

        Assert.Equal(15, kq.GiaTri);
        Assert.Equal("cm²", kq.DonViKetQua);   // diện tích thì đơn vị bình phương
    }

    [Fact]
    public void HinhChuNhat5x3DuongCheoXapXi583()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_HCN_CHEO", S(("a", 5), ("b", 3)), "cm");

        Assert.Equal(5.83, kq.GiaTri);
        Assert.Equal("5,83", kq.GiaTriHienThi);   // dấu phẩy thập phân kiểu Việt Nam
        Assert.Equal("cm", kq.DonViKetQua);
    }

    // ---- AC: hình thoi hai đường chéo 12 và 16 ----

    [Fact]
    public void HinhThoiHaiDuongCheo12Va16DienTichBang96()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_THOI_DT", S(("d1", 12), ("d2", 16)), "cm");

        Assert.Equal(96, kq.GiaTri);
        Assert.Equal("cm²", kq.DonViKetQua);
    }

    // ---- Các công thức còn lại ----

    [Theory]
    [InlineData("CT_HV_CV", 7, 28)]
    [InlineData("CT_HV_DT", 6, 36)]
    [InlineData("CT_THOI_CV", 6, 24)]
    public void CongThucMotBien(string ma, double a, double mong)
        => Assert.Equal(mong, MayTinhHinhHoc.Tinh(ma, S(("a", a))).GiaTri);

    [Fact]
    public void HinhVuongDuongCheoCanh5()
        // 5√2 ≈ 7,07
        => Assert.Equal(7.07, MayTinhHinhHoc.Tinh("CT_HV_CHEO", S(("a", 5))).GiaTri);

    [Fact]
    public void HinhBinhHanhDienTichDungDayNhanChieuCao()
        => Assert.Equal(60, MayTinhHinhHoc.Tinh("CT_HBH_DT", S(("a", 12), ("h", 5))).GiaTri);

    [Fact]
    public void HinhBinhHanhChuVi()
        => Assert.Equal(34, MayTinhHinhHoc.Tinh("CT_HBH_CV", S(("a", 12), ("b", 5))).GiaTri);

    [Fact]
    public void HinhThangDienTich()
        // (6 + 10) × 4 : 2 = 32
        => Assert.Equal(32, MayTinhHinhHoc.Tinh("CT_HT_DT", S(("a", 6), ("b", 10), ("h", 4))).GiaTri);

    [Fact]
    public void TuGiacChuVi()
        => Assert.Equal(20, MayTinhHinhHoc.Tinh("CT_TG_CHUVI", S(("a", 5), ("b", 5), ("c", 5), ("d", 5))).GiaTri);

    // ---- Làm tròn ----

    [Fact]
    public void LamTronHaiChuSoThapPhan()
    {
        // √(1² + 1²) = 1,41421… → 1,41
        Assert.Equal(1.41, MayTinhHinhHoc.Tinh("CT_HCN_CHEO", S(("a", 1), ("b", 1))).GiaTri);
        // 2,5 × 2,5 = 6,25
        Assert.Equal(6.25, MayTinhHinhHoc.Tinh("CT_HV_DT", S(("a", 2.5))).GiaTri);
    }

    [Theory]
    [InlineData(16, "16")]
    [InlineData(5.8309, "5,83")]
    [InlineData(0.5, "0,5")]
    [InlineData(0.125, "0,13")]     // điểm giữa biểu diễn chính xác được: làm tròn ra xa 0
    [InlineData(96.0, "96")]
    public void DinhDangSoKieuVietNam(double gia, string mong)
        => Assert.Equal(mong, MayTinhHinhHoc.So(gia));

    // ---- Các bước thay số ----

    [Fact]
    public void CoCongThucVaCacBuocThaySo()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_HCN_CV", S(("a", 5), ("b", 3)), "cm");

        Assert.StartsWith("Công thức:", kq.CacBuoc[0]);
        Assert.Contains(kq.CacBuoc, b => b.Contains("Thay số") && b.Contains('5') && b.Contains('3'));
        Assert.EndsWith("Kết quả: 16 cm", kq.CacBuoc[^1]);
        Assert.True(kq.CacBuoc.Count >= 3);
    }

    [Fact]
    public void DuongCheoGiaiThichTheoPythagore()
    {
        var kq = MayTinhHinhHoc.Tinh("CT_HCN_CHEO", S(("a", 5), ("b", 3)), "cm");

        Assert.Contains(kq.CacBuoc, b => b.Contains("Pythagore"));
        Assert.Contains(kq.CacBuoc, b => b.Contains("34"));   // 25 + 9
    }

    // ---- BR-10: kiểm tra số đo ----

    [Fact]
    public void SoDoHopLeThiKhongCoLoi()
        => Assert.Empty(MayTinhHinhHoc.KiemTra("CT_HCN_CV", Sn(("a", 5), ("b", 3))));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.5)]
    public void DoDaiPhaiLonHon0(double gia)
    {
        var loi = MayTinhHinhHoc.KiemTra("CT_HCN_CV", Sn(("a", gia), ("b", 3)));

        var l = Assert.Single(loi);
        Assert.Contains("cạnh a", l);
        Assert.Contains("lớn hơn 0", l);   // nói rõ lý do (BR-10)
    }

    [Fact]
    public void ChuaNhapThiBaoChuaNhapDungTen()
    {
        var loi = MayTinhHinhHoc.KiemTra("CT_HT_DT", Sn(("a", 6), ("b", null), ("h", null)));

        Assert.Equal(2, loi.Count);
        Assert.Contains(loi, l => l.Contains("cạnh b"));
        Assert.Contains(loi, l => l.Contains("chiều cao h"));
    }

    [Fact]
    public void BonCanhKhongGhepDuocThanhTuGiac()
    {
        // 20 ≥ 1 + 1 + 1 nên không thể là tứ giác (BR-10).
        var loi = MayTinhHinhHoc.KiemTra("CT_TG_CHUVI", Sn(("a", 20), ("b", 1), ("c", 1), ("d", 1)));

        var l = Assert.Single(loi);
        Assert.Contains("không ghép được thành tứ giác", l);
        Assert.Contains("cạnh a", l);
        Assert.Contains("nhỏ hơn tổng ba cạnh kia", l);
    }

    [Fact]
    public void BonCanhVuaDuBangNhauCungKhongGhepDuoc()
    {
        // Trường hợp biên: 3 = 1 + 1 + 1 → suy biến, vẫn phải báo lỗi.
        var loi = MayTinhHinhHoc.KiemTra("CT_TG_CHUVI", Sn(("a", 3), ("b", 1), ("c", 1), ("d", 1)));

        Assert.NotEmpty(loi);
    }

    [Fact]
    public void BonCanhHopLeThiKhongBaoLoi()
        => Assert.Empty(MayTinhHinhHoc.KiemTra("CT_TG_CHUVI", Sn(("a", 5), ("b", 4), ("c", 3), ("d", 3))));

    [Fact]
    public void SoKhongHopLeNhuNaNThiBaoLoi()
    {
        var loi = MayTinhHinhHoc.KiemTra("CT_HV_CV", Sn(("a", double.NaN)));

        Assert.Contains("không phải là một số", Assert.Single(loi));
    }

    [Fact]
    public void MaCongThucLaThiBaoLoi()
        => Assert.NotEmpty(MayTinhHinhHoc.KiemTra("KHONG_CO", Sn(("a", 5))));

    // ---- Siêu dữ liệu công thức ----

    [Theory]
    [InlineData("CT_TG_CHUVI", new[] { "a", "b", "c", "d" })]
    [InlineData("CT_HCN_CV", new[] { "a", "b" })]
    [InlineData("CT_HBH_DT", new[] { "a", "h" })]
    [InlineData("CT_THOI_DT", new[] { "d1", "d2" })]
    [InlineData("CT_HT_DT", new[] { "a", "b", "h" })]
    [InlineData("CT_HV_CHEO", new[] { "a" })]
    public void BienSoKhopVoiSeedTrongNeo4j(string ma, string[] mong)
        // Phải khớp với bienSo đã seed ở 10-kienthuc-A.cypher.
        => Assert.Equal(mong, MayTinhHinhHoc.BienSo(ma));

    [Theory]
    [InlineData("CT_HCN_CV", "CHU_VI")]
    [InlineData("CT_HCN_DT", "DIEN_TICH")]
    [InlineData("CT_HCN_CHEO", "DUONG_CHEO")]
    public void DaiLuongKhopVoiSeed(string ma, string mong)
        => Assert.Equal(mong, MayTinhHinhHoc.DaiLuong(ma));

    [Fact]
    public void CoDuMuoiHaiCongThucNhuSeed()
    {
        string[] tatCa =
        [
            "CT_TG_CHUVI", "CT_HCN_CV", "CT_HCN_DT", "CT_HV_CV", "CT_HV_DT", "CT_HBH_CV",
            "CT_HBH_DT", "CT_THOI_CV", "CT_THOI_DT", "CT_HT_DT", "CT_HCN_CHEO", "CT_HV_CHEO"
        ];

        Assert.All(tatCa, ma => Assert.True(MayTinhHinhHoc.CoCongThuc(ma), ma));
        Assert.False(MayTinhHinhHoc.CoCongThuc("KHONG_CO"));
        Assert.False(MayTinhHinhHoc.CoCongThuc(null));
    }

    [Theory]
    [InlineData("cm", "cm")]
    [InlineData("M", "m")]
    [InlineData("  mm ", "mm")]
    [InlineData("inch", "cm")]     // đơn vị lạ → mặc định
    [InlineData(null, "cm")]
    public void ChuanHoaDonVi(string? vao, string mong)
        => Assert.Equal(mong, MayTinhHinhHoc.ChuanHoaDonVi(vao));

    [Fact]
    public void DonViKetQuaBinhPhuongChoDienTich()
    {
        Assert.Equal("m²", MayTinhHinhHoc.DonViKetQua("CT_HCN_DT", "m"));
        Assert.Equal("m", MayTinhHinhHoc.DonViKetQua("CT_HCN_CV", "m"));
        Assert.Equal("m", MayTinhHinhHoc.DonViKetQua("CT_HCN_CHEO", "m"));
    }
}
