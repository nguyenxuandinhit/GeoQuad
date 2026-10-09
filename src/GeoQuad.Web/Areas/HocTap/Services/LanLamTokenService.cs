using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed record LanLamClaim(string MaLan, string BaiTap, string ChuThe, DateTimeOffset MoLuc);
public sealed record LanLamToken(string Token, string GuestNonce, string MaLan, DateTimeOffset MoLuc);

/// <summary>Bind lượt làm vào bài và chủ thể, ký bằng ASP.NET Data Protection.</summary>
public sealed class LanLamTokenService(IDataProtectionProvider provider, TimeProvider clock)
{
    private readonly IDataProtector _protector = provider.CreateProtector("GeoQuad.HocTap.LamBai.v1");

    public LanLamToken Tao(string baiTap, string? taiKhoanId, string? guestNonce = null)
    {
        var owner = taiKhoanId is not null ? "tk:" + taiKhoanId : "guest:" + (guestNonce ??= Convert.ToHexString(RandomNumberGenerator.GetBytes(24)));
        var now = clock.GetUtcNow();
        var claim = new LanLamClaim(Guid.NewGuid().ToString("N"), baiTap, owner, now);
        return new LanLamToken(_protector.Protect(JsonSerializer.Serialize(claim)), guestNonce ?? "", claim.MaLan, now);
    }

    public bool ThuGiaiMa(string token, string baiTap, string owner, out LanLamClaim claim)
    {
        return TryRead(token, owner, out claim) && claim.BaiTap == baiTap;
    }

    public bool TryRead(string token, string owner, out LanLamClaim claim)
    {
        claim = default!;
        try
        {
            var data = JsonSerializer.Deserialize<LanLamClaim>(_protector.Unprotect(token));
            if (data is null || data.ChuThe != owner || data.MoLuc > clock.GetUtcNow() ||
                clock.GetUtcNow() - data.MoLuc > TimeSpan.FromHours(24)) return false;
            claim = data;
            return true;
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or ArgumentException)
        {
            return false;
        }
    }

    public static string Hash(string? dapAn, string? donVi)
    {
        var bytes = Encoding.UTF8.GetBytes((dapAn ?? "").Trim() + "\0" + (donVi ?? "").Trim());
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
