using System.Text.RegularExpressions;
using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed class BaiTapService(IBaiTapRepository repo, ICurrentUser user)
{
    private static readonly HashSet<string> CapHopLe = ["CAP_1", "CAP_2", "CAP_3"];
    private static readonly HashSet<string> LoaiHopLe = ["TRAC_NGHIEM", "DAP_AN_SO", "CHUNG_MINH"];
    private static readonly Regex MaKhaiNiem = new("^[A-Z0-9_]{1,64}$", RegexOptions.Compiled);

    public async Task<BaiTapDanhSach> DanhSachAsync(BaiTapLoc boLoc)
    {
        var loc = new BaiTapLoc(
            ChuanHoa(boLoc.Cap), boLoc.Lop, ChuanHoa(boLoc.Loai), boLoc.DoKho,
            ChuanHoa(boLoc.KhaiNiem));
        var khaiNiem = await repo.KhaiNiemAsync(user.LopHienThi);
        var loi = KiemTra(loc, user.LopHienThi);
        var baiTap = loi is null
            ? await repo.LocAsync(user.LopHienThi, loc, user.TaiKhoanId)
            : [];
        return new BaiTapDanhSach(loc, baiTap, khaiNiem, user.Lop,
            user.LopHienThi, user.DaDangNhap, loi);
    }

    private static string? ChuanHoa(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? KiemTra(BaiTapLoc loc, int lopHienThi)
    {
        if (loc.Cap is not null && !CapHopLe.Contains(loc.Cap))
            return "Cấp học không hợp lệ.";
        if (loc.Lop is < 1 or > 12 || loc.Lop > lopHienThi)
            return "Lớp lọc không phù hợp với lớp đang xem.";
        if (loc.Cap is not null && loc.Lop is { } lop && !QuyTacCapLop.LopThuocCap(lop, loc.Cap))
            return $"Lớp {lop} không thuộc {QuyTacCapLop.TenCap(loc.Cap)}. Em chọn lại lớp hoặc cấp học nhé.";
        if (loc.Loai is not null && !LoaiHopLe.Contains(loc.Loai))
            return "Loại bài tập không hợp lệ.";
        if (loc.DoKho is < 1 or > 3)
            return "Độ khó phải từ 1 đến 3.";
        if (loc.KhaiNiem is not null && !MaKhaiNiem.IsMatch(loc.KhaiNiem))
            return "Mã khái niệm không hợp lệ.";
        return null;
    }
}
