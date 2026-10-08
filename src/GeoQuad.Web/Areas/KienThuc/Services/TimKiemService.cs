using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.KienThuc.Services;

public interface ITimKiemService
{
    /// <summary>Tìm kiếm kiến thức cho SCR-06 (US-11).</summary>
    Task<TimKiemViewModel> TimAsync(string? tuKhoa);
}

public sealed class TimKiemService : ITimKiemService
{
    private readonly ITimKiemRepository _repo;
    private readonly ICurrentUser _nguoiDung;

    public TimKiemService(ITimKiemRepository repo, ICurrentUser nguoiDung)
    {
        _repo = repo;
        _nguoiDung = nguoiDung;
    }

    public async Task<TimKiemViewModel> TimAsync(string? tuKhoa)
    {
        var lopHienThi = _nguoiDung.LopHienThi;
        var lopHocSinh = _nguoiDung.Lop;

        // Từ khoá rỗng: không gửi truy vấn, chỉ nhắc người dùng nhập.
        var truyVanAnd = TimKiemQuyTac.TaoTruyVan(tuKhoa, TimKiemQuyTac.ToanTuAnd);
        if (truyVanAnd is null)
        {
            return new TimKiemViewModel
            {
                TuKhoa = tuKhoa,
                ChuaNhapTuKhoa = true,
                LopHienThi = lopHienThi,
                LopHocSinh = lopHocSinh
            };
        }

        var ketQua = await _repo.TimAsync(truyVanAnd, lopHienThi);
        var daNoiLong = false;

        // Không có kết quả khi nối bằng AND thì nới sang OR.
        if (ketQua.Count == 0)
        {
            var truyVanOr = TimKiemQuyTac.TaoTruyVan(tuKhoa, TimKiemQuyTac.ToanTuOr);
            if (truyVanOr is not null && truyVanOr != truyVanAnd)
            {
                ketQua = await _repo.TimAsync(truyVanOr, lopHienThi);
                daNoiLong = ketQua.Count > 0;
            }
        }

        var coNhan = ketQua
            .Select(k => k with { NangCao = ThuVienQuyTac.LaNangCao(k.Lop, lopHocSinh) })
            .ToList();

        var theoNhom = TimKiemQuyTac.ThuTuNhom
            .Select(nhom => (Nhom: nhom,
                             Muc: (IReadOnlyList<KetQuaTimKiem>)coNhan.Where(k => k.Nhom == nhom).ToList()))
            .Where(x => x.Muc.Count > 0)
            .ToList();

        return new TimKiemViewModel
        {
            TuKhoa = tuKhoa,
            TheoNhom = theoNhom,
            TongSo = coNhan.Count,
            DaNoiLong = daNoiLong,
            LopHienThi = lopHienThi,
            LopHocSinh = lopHocSinh
        };
    }
}
