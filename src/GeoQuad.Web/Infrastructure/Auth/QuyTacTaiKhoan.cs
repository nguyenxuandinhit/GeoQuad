using System.Text.RegularExpressions;

namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>
/// Quy tắc kiểm tra tài khoản, viết thuần C# (không phụ thuộc Neo4j) để unit test được
/// (BR-01, BR-02, BR-03 — US-05).
/// </summary>
public static partial class QuyTacTaiKhoan
{
    public const int TenToiThieu = 4;
    public const int TenToiDa = 20;
    public const int MatKhauToiThieu = 8;

    /// <summary>Số lần sai liên tiếp thì khóa tài khoản (US-06).</summary>
    public const int SoLanSaiToiDa = 5;

    /// <summary>Thời gian khóa sau khi sai quá số lần cho phép (US-06).</summary>
    public static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(5);

    [GeneratedRegex("^[a-z0-9_]+$", RegexOptions.CultureInvariant)]
    private static partial Regex KyTuChoPhep();

    /// <summary>Đưa tên đăng nhập về chữ thường, bỏ khoảng trắng hai đầu (BR-01).</summary>
    public static string ChuanHoaTen(string? ten)
        => (ten ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>Tên đăng nhập: 4–20 ký tự, chỉ gồm chữ thường, số và dấu gạch dưới.</summary>
    public static bool TenHopLe(string? ten)
    {
        var t = ChuanHoaTen(ten);
        return t.Length is >= TenToiThieu and <= TenToiDa && KyTuChoPhep().IsMatch(t);
    }

    /// <summary>Mật khẩu tối thiểu 8 ký tự (BR-02).</summary>
    public static bool MatKhauHopLe(string? matKhau)
        => !string.IsNullOrEmpty(matKhau) && matKhau.Length >= MatKhauToiThieu;

    /// <summary>Lớp từ 1 đến 12 (BR-03).</summary>
    public static bool LopHopLe(int lop)
        => lop is >= QuyUoc.LopNhoNhat and <= QuyUoc.LopLonNhat;

    /// <summary>Tài khoản đang bị khóa tại thời điểm <paramref name="bayGio"/> hay không (US-06).</summary>
    public static bool DangBiKhoa(DateTimeOffset? khoaDen, DateTimeOffset bayGio)
        => khoaDen.HasValue && khoaDen.Value > bayGio;

    /// <summary>Thời gian còn phải chờ, làm tròn lên phút để báo cho học sinh (US-06).</summary>
    public static int SoPhutConPhaiCho(DateTimeOffset? khoaDen, DateTimeOffset bayGio)
    {
        if (!DangBiKhoa(khoaDen, bayGio))
        {
            return 0;
        }

        return (int)Math.Ceiling((khoaDen!.Value - bayGio).TotalMinutes);
    }

    /// <summary>
    /// Thời điểm hết khóa sau lần sai thứ <paramref name="soLanSaiMoi"/>;
    /// null nghĩa là chưa tới ngưỡng nên không khóa (US-06).
    /// </summary>
    public static DateTimeOffset? TinhKhoaDen(int soLanSaiMoi, DateTimeOffset bayGio)
        => soLanSaiMoi >= SoLanSaiToiDa ? bayGio + ThoiGianKhoa : null;

    /// <summary>Biệt danh: bắt buộc, tối đa 50 ký tự.</summary>
    public static bool BietDanhHopLe(string? bietDanh)
    {
        var b = (bietDanh ?? string.Empty).Trim();
        return b.Length is > 0 and <= 50;
    }
}
