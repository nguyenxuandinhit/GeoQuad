using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Areas.KienThuc.Repositories;
using GeoQuad.Web.Infrastructure.Auth;

namespace GeoQuad.Web.Areas.KienThuc.Services;

public interface IMayTinhService
{
    /// <summary>Dựng dữ liệu SCR-10; tính kết quả khi có đủ số đo (US-15, US-16).</summary>
    Task<MayTinhViewModel> LayAsync(
        string? hinh, string? daiLuong, IReadOnlyDictionary<string, string?> soDo, string? donVi);
}

public sealed class MayTinhService : IMayTinhService
{
    /// <summary>Sáu hình mà máy tính hình học hỗ trợ (US-15).</summary>
    public static readonly string[] HinhHoTro =
    [
        "HINH_CHU_NHAT", "HINH_VUONG", "HINH_BINH_HANH",
        "HINH_THOI", "HINH_THANG", "HINH_THANG_CAN"
    ];

    private readonly IMayTinhRepository _repo;
    private readonly ICurrentUser _nguoiDung;

    public MayTinhService(IMayTinhRepository repo, ICurrentUser nguoiDung)
    {
        _repo = repo;
        _nguoiDung = nguoiDung;
    }

    public async Task<MayTinhViewModel> LayAsync(
        string? hinh, string? daiLuong, IReadOnlyDictionary<string, string?> soDo, string? donVi)
    {
        var lopHienThi = _nguoiDung.LopHienThi;
        var cacHinh = await _repo.LayCacHinhAsync(lopHienThi, HinhHoTro);

        // Chỉ nhận mã hình nằm trong danh sách lấy từ Neo4j.
        var hinhChon = cacHinh.FirstOrDefault(h => h.Ma == hinh);
        var dv = MayTinhHinhHoc.ChuanHoaDonVi(donVi);

        var khung = new MayTinhViewModel
        {
            CacHinh = cacHinh,
            DonVi = dv,
            SoDoNhap = soDo,
            LopHienThi = lopHienThi,
            LopHocSinh = _nguoiDung.Lop
        };

        if (hinhChon is null)
        {
            return khung;
        }

        // Học sinh cấp 1 chỉ thấy đại lượng có công thức lớp ≤ 5 — truy vấn đã lọc theo $lop.
        var cacDaiLuong = await _repo.LayDaiLuongAsync(hinhChon.Ma, lopHienThi);
        var congThuc = cacDaiLuong.FirstOrDefault(d => d.DaiLuong == daiLuong)
                       ?? (daiLuong is null ? cacDaiLuong.FirstOrDefault() : null);

        khung = khung with
        {
            Hinh = hinhChon.Ma,
            TenHinh = hinhChon.Ten,
            CacDaiLuong = cacDaiLuong,
            DaiLuong = congThuc?.DaiLuong,
            CongThuc = congThuc
        };

        if (congThuc is null || !MayTinhHinhHoc.CoCongThuc(congThuc.MaCongThuc))
        {
            return khung;
        }

        // Chưa nhập số đo nào thì chỉ hiện biểu mẫu, không báo lỗi.
        var bien = MayTinhHinhHoc.BienSo(congThuc.MaCongThuc);
        if (bien.All(b => string.IsNullOrWhiteSpace(LayChuoi(soDo, b))))
        {
            return khung;
        }

        var (loi, soDay) = MayTinhHinhHoc.KiemTraTho(congThuc.MaCongThuc, soDo);
        if (loi.Count > 0)
        {
            return khung with { Loi = loi };
        }

        var ketQua = MayTinhHinhHoc.Tinh(congThuc.MaCongThuc, soDay, dv);

        // US-16: vẽ hình theo số đo vừa nhập; vẽ đường chéo nét đứt khi đang tính đường chéo.
        var veCheo = congThuc.DaiLuong == MayTinhHinhHoc.DaiLuongDuongCheo;
        var hinhVe = VeHinhSvg.Ve(hinhChon.Ma, soDay, dv, veCheo);

        return khung with { KetQua = ketQua, Svg = hinhVe?.Svg };
    }

    private static string? LayChuoi(IReadOnlyDictionary<string, string?> soDo, string bien)
        => soDo.TryGetValue(bien, out var v) ? v : null;

}
