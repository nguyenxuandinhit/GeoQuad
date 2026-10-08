using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class TienDoService(ITienDoRepository repo, ICurrentUser user, HocTapOptions options)
{
    public async Task<TienDoViewModel?> XemAsync()
    {
        if (string.IsNullOrWhiteSpace(user.TaiKhoanId)) return null;
        var lop = await repo.LopHienTaiAsync(user.TaiKhoanId);
        if (lop is null) return null;
        var history = await repo.LichSuAsync(user.TaiKhoanId);
        var concepts = TienDoQuyTac.PhanTich(history, options.NguongCanOn);
        var attempts = TienDoQuyTac.KhuTrung(history);
        var exerciseCount = attempts.Select(x => x.MaBai).Distinct(StringComparer.Ordinal).Count();
        var correct = attempts.Count(x => x.Dung);
        return new(lop.Value, exerciseCount, attempts.Count,
            attempts.Count == 0 ? 0 : (decimal)correct / attempts.Count, concepts, attempts.Count > 0,
            TienDoQuyTac.PhanLoai(attempts.Count, concepts));
    }
}
