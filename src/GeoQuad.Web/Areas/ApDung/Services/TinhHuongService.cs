using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.ApDung.Services;

public sealed class TinhHuongService(ITinhHuongRepository repo,ICurrentUser user)
{
    public async Task<TinhHuongDanhSach> DanhSachAsync(string? boiCanh)
    {
        var contexts = await repo.BoiCanhAsync();
        if (boiCanh != null && !contexts.Any(c => c.Ma == boiCanh))
            throw new ArgumentException("Bối cảnh không nằm trong danh mục.");
        var scenarios = await repo.DocAsync(user.Lop);
        return new(contexts,scenarios,boiCanh,user.Lop);
    }
    public async Task<TinhHuongViewModel?> XemAsync(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 64) return null;
        var item = (await repo.DocAsync(user.Lop,ma)).SingleOrDefault();
        return item == null ? null : new(item);
    }
}
