using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.Chung;

/// <summary>US-05: quy tắc tên đăng nhập, mật khẩu, lớp (BR-01, BR-02, BR-03).</summary>
public class QuyTacTaiKhoanTests
{
    [Theory]
    [InlineData("HocSinh8", "hocsinh8")]
    [InlineData("  Minh_An1 ", "minh_an1")]
    [InlineData(null, "")]
    public void ChuanHoaTenVeChuThuong(string? vao, string mong)
        => Assert.Equal(mong, QuyTacTaiKhoan.ChuanHoaTen(vao));

    [Theory]
    [InlineData("hocsinh8")]
    [InlineData("abcd")]                   // đúng 4 ký tự
    [InlineData("a1234567890123456789")]   // đúng 20 ký tự
    [InlineData("HOCSINH8")]               // sẽ được đưa về chữ thường
    [InlineData("minh_an_1")]
    public void TenHopLe(string ten) => Assert.True(QuyTacTaiKhoan.TenHopLe(ten));

    [Theory]
    [InlineData("abc")]                     // ngắn quá
    [InlineData("a12345678901234567890")]   // 21 ký tự
    [InlineData("minh an")]                 // có khoảng trắng
    [InlineData("minh-an")]                 // có dấu gạch ngang
    [InlineData("minhán")]                  // có dấu
    [InlineData("")]
    [InlineData(null)]
    public void TenKhongHopLe(string? ten) => Assert.False(QuyTacTaiKhoan.TenHopLe(ten));

    [Theory]
    [InlineData("12345678", true)]
    [InlineData("Hocsinh@123", true)]
    [InlineData("1234567", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void MatKhauToiThieu8KyTu(string? mk, bool mong)
        => Assert.Equal(mong, QuyTacTaiKhoan.MatKhauHopLe(mk));

    [Theory]
    [InlineData(1, true)]
    [InlineData(8, true)]
    [InlineData(12, true)]
    [InlineData(0, false)]
    [InlineData(13, false)]
    public void LopTu1Den12(int lop, bool mong)
        => Assert.Equal(mong, QuyTacTaiKhoan.LopHopLe(lop));

    [Theory]
    [InlineData("Minh An", true)]
    [InlineData("  ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void BietDanhBatBuoc(string? bd, bool mong)
        => Assert.Equal(mong, QuyTacTaiKhoan.BietDanhHopLe(bd));
}
