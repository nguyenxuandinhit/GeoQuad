using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Tests.Chung;

/// <summary>US-06: sai 5 lần liên tiếp thì khóa 5 phút, báo thời gian chờ.</summary>
public class KhoaTaiKhoanTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 10, 7, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void NguongLaNamLanSai()
    {
        Assert.Equal(5, QuyTacTaiKhoan.SoLanSaiToiDa);
        Assert.Equal(TimeSpan.FromMinutes(5), QuyTacTaiKhoan.ThoiGianKhoa);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void ChuaDuNamLanThiChuaKhoa(int soLanSai)
        => Assert.Null(QuyTacTaiKhoan.TinhKhoaDen(soLanSai, BayGio));

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public void DuNamLanThiKhoaNamPhut(int soLanSai)
    {
        var khoaDen = QuyTacTaiKhoan.TinhKhoaDen(soLanSai, BayGio);

        Assert.Equal(BayGio.AddMinutes(5), khoaDen);
    }

    [Fact]
    public void DangBiKhoaKhiChuaToiThoiDiemHetKhoa()
    {
        var khoaDen = BayGio.AddMinutes(5);

        Assert.True(QuyTacTaiKhoan.DangBiKhoa(khoaDen, BayGio));
        Assert.True(QuyTacTaiKhoan.DangBiKhoa(khoaDen, BayGio.AddMinutes(4.9)));
    }

    [Fact]
    public void HetKhoaThiVaoDuocLai()
    {
        var khoaDen = BayGio.AddMinutes(5);

        Assert.False(QuyTacTaiKhoan.DangBiKhoa(khoaDen, khoaDen));
        Assert.False(QuyTacTaiKhoan.DangBiKhoa(khoaDen, BayGio.AddMinutes(6)));
        Assert.False(QuyTacTaiKhoan.DangBiKhoa(null, BayGio));
    }

    [Theory]
    [InlineData(5.0, 5)]
    [InlineData(4.1, 5)]   // làm tròn lên để không báo thiếu thời gian chờ
    [InlineData(0.5, 1)]
    public void BaoSoPhutConPhaiCho(double phutConLai, int mong)
    {
        var khoaDen = BayGio.AddMinutes(phutConLai);

        Assert.Equal(mong, QuyTacTaiKhoan.SoPhutConPhaiCho(khoaDen, BayGio));
    }

    [Fact]
    public void KhongBiKhoaThiKhongPhaiCho()
    {
        Assert.Equal(0, QuyTacTaiKhoan.SoPhutConPhaiCho(null, BayGio));
        Assert.Equal(0, QuyTacTaiKhoan.SoPhutConPhaiCho(BayGio.AddMinutes(-1), BayGio));
    }
}
