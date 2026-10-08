using System.ComponentModel.DataAnnotations;

namespace GeoQuad.Web.Areas.HocTap.Models;

/// <summary>Chi tiết nội bộ từ repository; không truyền trực tiếp ra view GET.</summary>
public sealed record BaiTapChiTiet(string Ma, string De, string Loai, int Lop, int DoKho,
    IReadOnlyList<string> PhuongAn, string? DapAnDung, decimal? DapAnSo, decimal SaiSo,
    string DonVi, string GiaiThich, string? LoiGiaiMau, string? MaChungMinh);

public sealed record LamBaiViewModel(string Ma, string De, string Loai, int Lop, int DoKho,
    IReadOnlyList<string> PhuongAn, string Token, string DonViYeuCau, bool CoLoiGiaiMau,
    string? Loi = null, string? KetQuaUrl = null);

public sealed class BaiTapNopInput
{
    [Required] public string Token { get; set; } = "";
    [StringLength(128)] public string? DapAn { get; set; }
    [StringLength(32)] public string? DonVi { get; set; }
}

/// <summary>Định lý/công thức bài dùng (SU_DUNG); chỉ hiển thị sau khi nộp.</summary>
public sealed record KienThucLienQuan(string Ma, string Ten, string? BieuThuc, int Lop);
public sealed record KhaiNiemLienQuan(string Ma, string Ten, int Lop);
public sealed record KienThucBaiTap(IReadOnlyList<KienThucLienQuan> KienThuc, IReadOnlyList<KhaiNiemLienQuan> KhaiNiem)
{
    public static readonly KienThucBaiTap Rong = new([], []);
}

public sealed record KetQuaLamBaiViewModel(string Ma, string De, string DapAnDaChon,
    string DapAnDung, string? DonVi, bool Dung, string GiaiThich, string? MaChungMinh,
    string? Loi = null, bool LaKhach = false, int LopHocSinh = 12)
{
    public IReadOnlyList<KienThucLienQuan> KienThuc { get; init; } = [];
    public IReadOnlyList<KhaiNiemLienQuan> KhaiNiem { get; init; } = [];
}

public sealed record KetQuaNopBai(bool HopLe, bool DaNop, bool XungDot, bool Dung,
    string MaLan, string DapAnDaChon, string DapAnDung, string DonVi, string Loi,
    KetQuaLamBaiViewModel? View = null);
