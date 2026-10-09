using System.Globalization;

namespace GeoQuad.Web.Areas.HocTap.Services;

public sealed record KetQuaCham(bool HopLe, bool Dung, string? Loi = null);

/// <summary>Logic thuần chấm US-20, không phụ thuộc HTTP hay Neo4j.</summary>
public static class ChamBai
{
    /// <summary>Đáp án đúng dạng chuỗi để hiển thị/so sánh: chữ cái trắc nghiệm hoặc số invariant.</summary>
    public static string DapAnDungDangChuoi(string loai, string? dapAnDung, decimal? dapAnSo) =>
        loai == "TRAC_NGHIEM" ? dapAnDung ?? ""
        : dapAnSo?.ToString(CultureInfo.InvariantCulture) ?? "";

    public static KetQuaCham TracNghiem(string? dapAn, string dapAnDung)
    {
        if (string.IsNullOrWhiteSpace(dapAn)) return new(false, false, "Em hãy chọn một đáp án.");
        var chon = dapAn.Trim().ToUpperInvariant();
        if (chon.Length != 1 || chon[0] is < 'A' or > 'D')
            return new(false, false, "Đáp án trắc nghiệm không hợp lệ.");
        return new(true, chon == dapAnDung.Trim().ToUpperInvariant());
    }

    public static KetQuaCham So(string? dapAn, string? donVi, decimal dapAnDung, decimal saiSo, string donViDung)
    {
        if (string.IsNullOrWhiteSpace(dapAn)) return new(false, false, "Em hãy nhập đáp án.");
        var text = dapAn.Trim();
        if (text.Contains('.') && text.Contains(','))
            return new(false, false, "Em dùng dấu phẩy hoặc dấu chấm thập phân, không dùng dấu tách hàng nghìn.");
        if (text.Contains(',')) text = text.Replace(',', '.');
        if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
            return new(false, false, "Đáp án số không hợp lệ.");
        var donViNhap = (donVi ?? string.Empty).Trim();
        if (!string.Equals(donViNhap, donViDung.Trim(), StringComparison.Ordinal)) return new(true, false);
        return new(true, Math.Abs(value - dapAnDung) <= Math.Max(0m, saiSo));
    }
}
