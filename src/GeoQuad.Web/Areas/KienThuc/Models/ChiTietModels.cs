namespace GeoQuad.Web.Areas.KienThuc.Models;

/// <summary>Khái niệm ở trang chi tiết (SCR-05 — US-10).</summary>
public sealed record KhaiNiemChiTiet(
    string Ma, string Ten, string Loai, string DinhNghia, string? GhiChuTieuHoc);

public sealed record MucTinhChat(string Ma, string NoiDung, int Lop)
{
    public bool NangCao { get; init; }
}

public sealed record MucCongThuc(string Ma, string Ten, string? BieuThuc, int Lop)
{
    public bool NangCao { get; init; }
}

public sealed record MucDauHieu(string Ma, string NoiDung, int Lop)
{
    public bool NangCao { get; init; }
}

/// <summary>Liên kết "tổng quát hơn" / "đặc biệt hơn" (BR-11).</summary>
public sealed record LienKetKhaiNiem(string Ma, string Ten);

public sealed record MucTinhHuong(string Ma, string Ten);

public sealed record MucBaiTap(string Ma, string De, int DoKho, int Lop);

/// <summary>Kết quả truy vấn chính của trang chi tiết.</summary>
public sealed record ChiTietKhaiNiem(
    KhaiNiemChiTiet KhaiNiem,
    int Lop,
    IReadOnlyList<MucTinhChat> TinhChat,
    IReadOnlyList<MucCongThuc> CongThuc,
    IReadOnlyList<MucDauHieu> DauHieu,
    IReadOnlyList<LienKetKhaiNiem> TongQuatHon,
    IReadOnlyList<LienKetKhaiNiem> DacBietHon,
    bool DaHoc);

/// <summary>Kết quả truy vấn tình huống và bài tập liên quan (truy vấn riêng để không nhân dòng).</summary>
public sealed record LienQuanKhaiNiem(
    IReadOnlyList<MucTinhHuong> TinhHuong,
    IReadOnlyList<MucBaiTap> BaiTap);

/// <summary>Dữ liệu trang /KienThuc/ThuVien/ChiTiet/{ma} (SCR-05 — US-10).</summary>
public sealed class ChiTietViewModel
{
    public required ChiTietKhaiNiem ChiTiet { get; init; }
    public required LienQuanKhaiNiem LienQuan { get; init; }

    /// <summary>Đoạn SVG minh hoạ; null với các yếu tố không vẽ được.</summary>
    public string? Svg { get; init; }

    public int LopHocSinh { get; init; }
    public int LopHienThi { get; init; }
    public bool DaDangNhap { get; init; }

    /// <summary>Khách vừa bấm "Em đã hiểu" nên cần mời đăng nhập (BR-09).</summary>
    public bool MoiDangNhap { get; init; }

    public bool LaCap1 => LopHocSinh <= 5;

    /// <summary>Bản thân khái niệm này vượt lớp của học sinh (quy ước 2.7).</summary>
    public bool NangCao { get; init; }

    /// <summary>Ghi chú tiểu học chỉ hiện với học sinh cấp 1 (BR-11).</summary>
    public bool HienGhiChuTieuHoc
        => LaCap1 && !string.IsNullOrWhiteSpace(ChiTiet.KhaiNiem.GhiChuTieuHoc);

    // Tab rỗng thì ẩn (US-10).
    public bool CoTinhChat => ChiTiet.TinhChat.Count > 0;
    public bool CoDauHieu => ChiTiet.DauHieu.Count > 0;
    public bool CoCongThuc => ChiTiet.CongThuc.Count > 0;
    public bool CoTinhHuong => LienQuan.TinhHuong.Count > 0;
    public bool CoBaiTap => LienQuan.BaiTap.Count > 0;
    public bool CoLienKet => ChiTiet.TongQuatHon.Count > 0 || ChiTiet.DacBietHon.Count > 0;
}
