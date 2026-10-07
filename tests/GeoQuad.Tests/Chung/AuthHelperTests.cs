using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.Chung;

/// <summary>US-05: mật khẩu phải được lưu dạng băm, không lưu bản rõ (BR-12).</summary>
public class AuthHelperTests
{
    private readonly AuthHelper _auth = new();

    [Fact]
    public void BamKhongChuaMatKhauGoc()
    {
        const string matKhau = "Hocsinh@123";

        var bam = _auth.BamMatKhau(matKhau);

        Assert.NotEqual(matKhau, bam);
        Assert.DoesNotContain(matKhau, bam, StringComparison.Ordinal);
        Assert.True(bam.Length > 40);
    }

    [Fact]
    public void HaiLanBamCungMatKhauChoKetQuaKhacNhau()
    {
        // PasswordHasher dùng muối ngẫu nhiên nên hai chuỗi băm phải khác nhau.
        Assert.NotEqual(_auth.BamMatKhau("Hocsinh@123"), _auth.BamMatKhau("Hocsinh@123"));
    }

    [Fact]
    public void KiemTraDungMatKhau()
    {
        var bam = _auth.BamMatKhau("Hocsinh@123");

        Assert.True(_auth.KiemTraMatKhau(bam, "Hocsinh@123"));
    }

    [Theory]
    [InlineData("hocsinh@123")]   // khác chữ hoa/thường
    [InlineData("Hocsinh@124")]
    [InlineData("")]
    public void KiemTraSaiMatKhau(string thu)
    {
        var bam = _auth.BamMatKhau("Hocsinh@123");

        Assert.False(_auth.KiemTraMatKhau(bam, thu));
    }

    [Fact]
    public void ChuoiBamHongKhongLamUngDungLoi()
    {
        Assert.False(_auth.KiemTraMatKhau("khong-phai-chuoi-bam", "Hocsinh@123"));
        Assert.False(_auth.KiemTraMatKhau("", "Hocsinh@123"));
    }
}
