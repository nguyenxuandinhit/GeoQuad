using GeoQuad.Web.Areas.KienThuc.Services;

namespace GeoQuad.Web.Areas.KienThuc.Models;

/// <summary>Một dòng kết quả tìm kiếm (SCR-06 — US-11).</summary>
public sealed record KetQuaTimKiem(
    string Ma,
    string TieuDe,
    string? DoanTrich,
    IReadOnlyList<string> Nhan,
    int Lop,
    double Diem,
    string? MaHinh)
{
    public bool NangCao { get; init; }

    public string Nhom => TimKiemQuyTac.NhomHienThi(Nhan);

    public string DoanTrichNgan => TimKiemQuyTac.CatDoanTrich(DoanTrich);

    /// <summary>
    /// Với công thức thì đoạn trích chính là <c>bieuThuc</c> dạng LaTeX,
    /// nên view bọc $…$ để KaTeX dựng thành công thức thật (NFR-12).
    /// </summary>
    public bool DoanTrichLaCongThuc => Nhan.Contains("CongThuc");

    public string? DuongDanChiTiet
        => string.IsNullOrEmpty(MaHinh) ? null : $"/KienThuc/ThuVien/ChiTiet/{Uri.EscapeDataString(MaHinh)}";
}

/// <summary>Dữ liệu trang /KienThuc/ThuVien/TimKiem?q= (SCR-06 — US-11).</summary>
public sealed class TimKiemViewModel
{
    public string? TuKhoa { get; init; }

    /// <summary>Kết quả đã gom theo nhóm, giữ thứ tự nhóm của SCR-06.</summary>
    public IReadOnlyList<(string Nhom, IReadOnlyList<KetQuaTimKiem> Muc)> TheoNhom { get; init; } = [];

    public int TongSo { get; init; }

    /// <summary>Không có kết quả với AND nên đã nới sang OR.</summary>
    public bool DaNoiLong { get; init; }

    /// <summary>Từ khoá rỗng: không gửi truy vấn, chỉ nhắc nhập.</summary>
    public bool ChuaNhapTuKhoa { get; init; }

    public int LopHienThi { get; init; }
    public int LopHocSinh { get; init; }
    public bool LaCap1 => LopHocSinh <= 5;

    public bool KhongCoKetQua => !ChuaNhapTuKhoa && TongSo == 0;
}
