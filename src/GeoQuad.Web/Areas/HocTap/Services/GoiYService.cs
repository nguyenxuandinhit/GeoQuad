using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class GoiYService(ITienDoRepository repo, ICurrentUser user, HocTapOptions options)
{
    public async Task<GoiYViewModel?> XemAsync()
    {
        if (string.IsNullOrWhiteSpace(user.TaiKhoanId)) return null;
        var lop = await repo.LopHienTaiAsync(user.TaiKhoanId);
        if (lop is null) return null;
        var events = await repo.LichSuAsync(user.TaiKhoanId);
        var allAttempts = TienDoQuyTac.KhuTrung(events);
        var concepts = TienDoQuyTac.PhanTich(events, options.NguongCanOn);
        var weak = concepts.Where(x => x.CanOn).ToArray();
        var loai = TienDoQuyTac.PhanLoai(allAttempts.Count, concepts);
        var candidates = await repo.UngVienAsync(user.TaiKhoanId, lop.Value);

        IOrderedEnumerable<BaiTapGoiY> ordered = loai switch
        {
            LoaiGoiY.CanOn => candidates.Where(x => weak.Any(w => w.Ma == x.MaKhaiNiem))
                .OrderBy(x => x.DoKho).ThenBy(x => x.MaKhaiNiem, StringComparer.Ordinal),
            // UC-13 luồng 3a: gợi ý bài khó hơn trước.
            LoaiGoiY.KhongCanOn => candidates.OrderByDescending(x => x.DoKho),
            _ => candidates.OrderBy(x => x.DoKho)
        };
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var output = ordered.ThenBy(x => x.Ma, StringComparer.Ordinal)
            .Where(x => seen.Add(x.Ma)).Take(5).ToArray();

        var message = loai switch
        {
            LoaiGoiY.ChuaCoLichSu => "Hãy làm bài đầu tiên để bắt đầu theo dõi tiến độ.",
            LoaiGoiY.ChuaDuDuLieu => "Em làm thêm vài bài nữa để GeoQuad biết em cần ôn phần nào nhé.",
            LoaiGoiY.CanOn when output.Length > 0 => "Đây là bài đã rà soát gắn với khái niệm em nên ôn thêm.",
            LoaiGoiY.CanOn => "Chưa có bài phù hợp để ôn các khái niệm này. Em có thể xem lộ trình học.",
            _ => "Chúc mừng em! Không có khái niệm nào cần ôn. Thử các bài khó hơn hoặc xem lộ trình học."
        };
        return new(output, weak, allAttempts.Count > 0, message, loai);
    }

    public async Task<string?> KeTiepAsync(string hienTai)
    {
        var model = await XemAsync();
        return model?.BaiTap.FirstOrDefault(x => x.Ma != hienTai)?.Ma;
    }
}
