using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.KienThuc.Services;

public interface IBanDoService
{
    /// <summary>Dữ liệu bản đồ cho /KienThuc/BanDo/DuLieu?lop= (US-12).</summary>
    Task<BanDoDuLieu> LayDuLieuAsync(int? lopLoc);

    /// <summary>Dữ liệu trang /KienThuc/BanDo, kèm kết quả ô "Vì sao … là …?" nếu có (US-12).</summary>
    Task<BanDoViewModel> LayTrangAsync(int? lopLoc, string? tu, string? den);
}

public sealed class BanDoService : IBanDoService
{
    private readonly IBanDoRepository _repo;
    private readonly ICurrentUser _nguoiDung;

    public BanDoService(IBanDoRepository repo, ICurrentUser nguoiDung)
    {
        _repo = repo;
        _nguoiDung = nguoiDung;
    }

    public async Task<BanDoDuLieu> LayDuLieuAsync(int? lopLoc)
    {
        // Lọc theo lớp nhưng không bao giờ vượt quá lớp người dùng được xem (BR-04).
        var lop = BanDoQuyTac.LopXem(ThuVienQuyTac.ChuanHoaLop(lopLoc), _nguoiDung.LopHienThi);

        var nut = await _repo.LayNutAsync(lop, _nguoiDung.TaiKhoanId);
        var canh = BanDoQuyTac.DungCanh(nut);

        return new BanDoDuLieu(nut, canh, lop, _nguoiDung.Lop);
    }

    public async Task<BanDoViewModel> LayTrangAsync(int? lopLoc, string? tu, string? den)
    {
        var duLieu = await LayDuLieuAsync(lopLoc);

        KetQuaViSao? viSao = null;
        if (!string.IsNullOrWhiteSpace(tu) && !string.IsNullOrWhiteSpace(den))
        {
            var chuoi = tu == den ? [] : await _repo.LayChuoiViSaoAsync(tu, den);
            viSao = new KetQuaViSao(
                Tu: TenHinh(duLieu, tu),
                Den: TenHinh(duLieu, den),
                Chuoi: chuoi);
        }

        return new BanDoViewModel
        {
            DuLieu = duLieu,
            LopLoc = ThuVienQuyTac.ChuanHoaLop(lopLoc),
            DaDangNhap = _nguoiDung.DaDangNhap,
            ViSao = viSao
        };
    }

    private static string TenHinh(BanDoDuLieu duLieu, string ma)
        => duLieu.Nut.FirstOrDefault(n => n.Ma == ma)?.Ten ?? ma;
}
