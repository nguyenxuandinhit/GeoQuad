using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.ApDung.Services;

public sealed class ChungMinhService(IChungMinhRepository repo,ICurrentUser user)
{
    public async Task<ChungMinhViewModel?> XemAsync(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 64) return null;
        var proof = await repo.XemAsync(ma,user.LopHienThi);
        if (proof == null || proof.Buoc.Count is < 3 or > 6) return null;
        var steps = proof.Buoc.OrderBy(b => b.ThuTu).ToArray();
        if (!steps.Select(b => b.ThuTu).SequenceEqual(Enumerable.Range(1,steps.Length)) ||
            steps.Any(b => string.IsNullOrWhiteSpace(b.NoiDung) || b.CanCu.Count == 0)) return null;
        return proof with { Buoc=steps, NangCao=proof.Lop > user.Lop };
    }
}
