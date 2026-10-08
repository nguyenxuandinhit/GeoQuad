using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Tests.A_TraCuu;

/// <summary>US-09: chuẩn hoá bộ lọc và nhãn "Nâng cao" (FR-10, BR-04).</summary>
public class ThuVienQuyTacTests
{
    [Theory]
    [InlineData("HINH", "HINH")]
    [InlineData("hinh", "HINH")]
    [InlineData("  Tinh_Chat  ", "TINH_CHAT")]
    [InlineData("DAU_HIEU", "DAU_HIEU")]
    [InlineData("CONG_THUC", "CONG_THUC")]
    public void ChuanHoaLoaiNhanGiaTriChoPhep(string vao, string mong)
        => Assert.Equal(mong, ThuVienQuyTac.ChuanHoaLoai(vao));

    [Theory]
    [InlineData("KHAC")]
    [InlineData("DinhLy")]
    [InlineData("'; MATCH (n) DETACH DELETE n //")]   // giá trị lạ từ URL bị bỏ, không vào truy vấn
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ChuanHoaLoaiBoGiaTriLa(string? vao)
        => Assert.Null(ThuVienQuyTac.ChuanHoaLoai(vao));

    [Theory]
    [InlineData("CAP_1", "CAP_1")]
    [InlineData("cap_3", "CAP_3")]
    public void ChuanHoaCapNhanGiaTriChoPhep(string vao, string mong)
        => Assert.Equal(mong, ThuVienQuyTac.ChuanHoaCap(vao));

    [Theory]
    [InlineData("CAP_4")]
    [InlineData("CAP")]
    [InlineData(null)]
    public void ChuanHoaCapBoGiaTriLa(string? vao)
        => Assert.Null(ThuVienQuyTac.ChuanHoaCap(vao));

    [Theory]
    [InlineData(1, 1)]
    [InlineData(8, 8)]
    [InlineData(12, 12)]
    public void ChuanHoaLopNhanTu1Den12(int vao, int mong)
        => Assert.Equal(mong, ThuVienQuyTac.ChuanHoaLop(vao));

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    [InlineData(null)]
    public void ChuanHoaLopBoGiaTriNgoaiKhoang(int? vao)
        => Assert.Null(ThuVienQuyTac.ChuanHoaLop(vao));

    [Fact]
    public void NoiDungVuotLopHocSinhThiLaNangCao()
    {
        // Học sinh lớp 4 bật xem trước: Hình thang cân (lớp 6) phải mang nhãn "Nâng cao".
        Assert.True(ThuVienQuyTac.LaNangCao(lopNoiDung: 6, lopHocSinh: 4));
        Assert.True(ThuVienQuyTac.LaNangCao(lopNoiDung: 8, lopHocSinh: 4));
    }

    [Fact]
    public void NoiDungBangHoacDuoiLopHocSinhThiKhongPhaiNangCao()
    {
        Assert.False(ThuVienQuyTac.LaNangCao(lopNoiDung: 4, lopHocSinh: 4));
        Assert.False(ThuVienQuyTac.LaNangCao(lopNoiDung: 3, lopHocSinh: 4));
        Assert.False(ThuVienQuyTac.LaNangCao(lopNoiDung: 1, lopHocSinh: 12));
    }

    [Theory]
    [InlineData(1, "CAP_1")]
    [InlineData(5, "CAP_1")]
    [InlineData(6, "CAP_2")]
    [InlineData(9, "CAP_2")]
    [InlineData(10, "CAP_3")]
    [InlineData(12, "CAP_3")]
    public void CapSuyRaTuLop(int lop, string mong)
        => Assert.Equal(mong, ThuVienQuyTac.CapTheoLop(lop));

    [Fact]
    public void BonTabTheoDungThuTuManHinhSCR04()
        => Assert.Equal(new[] { "HINH", "TINH_CHAT", "DAU_HIEU", "CONG_THUC" }, ThuVienQuyTac.CacLoai);

    [Theory]
    [InlineData("HINH", "Hình")]
    [InlineData("TINH_CHAT", "Tính chất")]
    [InlineData("DAU_HIEU", "Dấu hiệu")]
    [InlineData("CONG_THUC", "Công thức")]
    [InlineData("KHONG_BIET", "Khác")]
    [InlineData(null, "Khác")]
    public void TenLoaiTiengViet(string? loai, string mong)
        => Assert.Equal(mong, ThuVienQuyTac.TenLoai(loai));
}
