using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.ApDung.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.ApDung.Services;

public sealed class GoiYService(IGoiYRepository repo, ICurrentUser user)
{
    public Task<GoiYCatalog> CatalogAsync() => repo.CatalogAsync(user.LopHienThi);

    public async Task<GoiYViewModel> FormAsync(string nen)
    {
        var catalog = await CatalogAsync();
        if (!catalog.Hinh.Any(h => h.Ma == nen)) throw new ArgumentException("Hình không nằm trong danh mục được phép.");
        return new(catalog,new(nen,"",[]),[],[],user.LopHienThi < 8 ?
            "Dấu hiệu nhận biết được học ở lớp 8. Em có thể bật Xem trước kiến thức nâng cao." : null,user.Lop,user.XemNangCao);
    }

    public async Task<GoiYViewModel> TimAsync(GoiYRequest request)
    {
        var catalog = await CatalogAsync();
        if (request.Co.Count > 100 || !catalog.Hinh.Any(h => h.Ma == request.Nen) ||
            !catalog.Hinh.Any(h => h.Ma == request.Dich) ||
            request.Co.Any(ma => !catalog.DieuKien.Any(d => d.Ma == ma)))
            throw new ArgumentException("Hình hoặc điều kiện không nằm trong danh mục được phép.");
        var co = request.Co.ToHashSet(StringComparer.Ordinal);
        request = request with { Co = co.Order(StringComparer.Ordinal).ToArray() };
        GoiYViewModel Result(IReadOnlyList<GoiYThe> cards, IReadOnlyList<IReadOnlyList<GoiYThe>> chains, string? message) =>
            new(catalog,request,cards,chains,message,user.Lop,user.XemNangCao);
        if (user.LopHienThi < 8)
            return Result([],[],"Dấu hiệu nhận biết được học ở lớp 8. Em có thể bật Xem trước kiến thức nâng cao.");
        if (await repo.SuyRaAsync(request.Nen,request.Dich,user.LopHienThi))
            return Result([],[],request.Nen == request.Dich ? "Hình đã biết chính là hình cần chứng minh." :
                $"Mọi {catalog.Hinh.Single(h => h.Ma == request.Nen).Ten} đều là {catalog.Hinh.Single(h => h.Ma == request.Dich).Ten}.");
        var signs = catalog.DauHieu.ToDictionary(d => d.Ma, StringComparer.Ordinal);
        GoiYThe Card(DauHieuItem d) => new(d,d.DieuKien.Where(k => co.Contains(k.Ma)).ToArray(),
            d.DieuKien.Where(k => !co.Contains(k.Ma)).ToArray());
        var direct = (await repo.TrucTiepAsync(request.Nen,request.Dich,user.LopHienThi)).Distinct(StringComparer.Ordinal)
            .Where(signs.ContainsKey).Select(ma => Card(signs[ma])).OrderBy(c => c.ConThieu.Count)
            .ThenBy(c => c.DauHieu.Ma,StringComparer.Ordinal).ToArray();
        if (direct.Length > 0) return Result(direct,[],null);
        var chains = (await repo.ChuoiAsync(request.Nen,request.Dich,user.LopHienThi))
            .Where(c => c.Count is >= 2 and <= 3 && c.Distinct().Count() == c.Count && c.All(signs.ContainsKey))
            .Take(3).Select(c => (IReadOnlyList<GoiYThe>)c.Select(ma => Card(signs[ma])).ToArray()).ToArray();
        return Result([],chains,chains.Length == 0 ? "Chưa có dấu hiệu phù hợp, em thử xem Bản đồ kiến thức." :
            "Đây là hướng gợi ý trung gian. Mỗi bước vẫn cần các điều kiện được đánh dấu còn thiếu.");
    }
}
