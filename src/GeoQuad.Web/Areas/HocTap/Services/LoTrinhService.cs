using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class LoTrinhService(ILoTrinhRepository repo, ICurrentUser user)
{
    public async Task<LoTrinhViewModel> XemAsync(string? mucTieu)
    {
        var id = user.TaiKhoanId;
        if (string.IsNullOrWhiteSpace(id)) return Rỗng("Em cần đăng nhập để xem lộ trình.");
        var lop = await repo.LopHienTaiAsync(id);
        if (lop is null) return Rỗng("Không tìm thấy lớp học hiện tại của tài khoản.");
        var targets = await repo.MucTieuHopLeAsync(lop.Value);
        if (targets.Count == 0) return new(targets, null, [], 0, 0, 0, "Chưa có mục tiêu hình học đã rà soát phù hợp với lớp.");
        var target = string.IsNullOrWhiteSpace(mucTieu) ? targets[0].Ma : mucTieu;
        if (!targets.Any(x => x.Ma == target)) return new(targets, null, [], 0, 0, 0, "Mục tiêu không khả dụng trong lớp hiện tại.");

        var graph = await repo.DoThiAsync(id, target);
        if (graph is null || graph.Nodes.Any(x => x.TrangThai != "DA_RA_SOAT" || x.Lop < 1 || x.Lop > lop.Value))
            return new(targets, target, [], 0, 0, 0, "Lộ trình đang thiếu nội dung đã rà soát phù hợp với lớp.");
        try
        {
            var ordered = LoTrinhThuTu.SapXep(target, graph.TienQuyet);
            var byId = graph.Nodes.ToDictionary(x => x.Ma, StringComparer.Ordinal);
            if (ordered.Any(id2 => !byId.ContainsKey(id2)))
                return new(targets, target, [], 0, 0, 0, "Lộ trình đang thiếu khái niệm tiên quyết.");
            var steps = ordered.Select(ma => new BuocLoTrinh(ma, byId[ma].Ten, byId[ma].Lop,
                graph.DaHoc.Contains(ma), ma == target)).ToArray();
            var learned = steps.Count(x => x.DaHoc);
            return new(targets, target, steps, learned, steps.Length,
                steps.Length == 0 ? 0 : (int)Math.Round(learned * 100d / steps.Length, MidpointRounding.AwayFromZero), null);
        }
        catch (InvalidOperationException)
        {
            return new(targets, target, [], 0, 0, 0, "Lộ trình có chu trình hoặc vượt giới hạn an toàn.");
        }
    }

    public async Task<bool> EmDaHieuAsync(string? mucTieu, string? ma)
    {
        var id = user.TaiKhoanId;
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(mucTieu)) return false;
        var model = await XemAsync(mucTieu);
        if (model.Loi is not null || model.MucTieu is null || model.CacBuoc.All(x => x.Ma != ma)) return false;
        return await repo.DanhDauDaHocAsync(id, mucTieu, ma, model.CacBuoc.Select(x => x.Ma).ToArray());
    }

    private static LoTrinhViewModel Rỗng(string loi) => new([], null, [], 0, 0, 0, loi);
}
