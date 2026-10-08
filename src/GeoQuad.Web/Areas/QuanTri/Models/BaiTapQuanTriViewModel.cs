namespace GeoQuad.Web.Areas.QuanTri.Models;

public sealed class BaiTapForm
{
    public string Ma { get; set; } = "";
    public string De { get; set; } = "";
    public string Loai { get; set; } = "TRAC_NGHIEM";
    public string[] PhuongAn { get; set; } = ["","","",""];
    public string DapAnDung { get; set; } = "";
    public string DapAnSo { get; set; } = "";
    public string SaiSo { get; set; } = "0.01";
    public string DonVi { get; set; } = "";
    public string LoiGiaiMau { get; set; } = "";
    public string GiaiThich { get; set; } = "";
    public int Lop { get; set; } = 8;
    public int DoKho { get; set; } = 1;
    public bool HienThi { get; set; } = true;
    public string Nguon { get; set; } = "Nhóm GeoQuad";
    public string TrangThai { get; set; } = "NHAP";
    public string[] KhaiNiem { get; set; } = [];
    public string[] SuDung { get; set; } = [];
}
public sealed record BaiTapDaKiemTra(string Ma,int Lop,IReadOnlyList<string> KhaiNiem,IReadOnlyList<string> SuDung,
    IReadOnlyDictionary<string,object> ThuocTinh);
public sealed record KiemTraBaiTapKetQua(BaiTapDaKiemTra? BaiTap,IReadOnlyDictionary<string,string> Loi)
{ public bool HopLe => BaiTap != null && Loi.Count == 0; }
public sealed record MucQuanTri(string Ma,string Ten);
public sealed record QuanTriCatalog(IReadOnlyList<MucQuanTri> KhaiNiem,IReadOnlyList<MucQuanTri> SuDung);
public sealed record BaiTapRow(string Ma,string De,string Loai,int? Lop,int DoKho,bool HienThi,string TrangThai,long SoLanLam);
public sealed record BaiTapDanhSach(IReadOnlyList<BaiTapRow> BaiTap,int? Lop,string? Loai,bool? HienThi,string? TrangThai,string? TuKhoa);

public static class BaiTapNhan
{
    public static string Loai(string ma) => ma switch { "TRAC_NGHIEM" => "Trắc nghiệm", "DAP_AN_SO" => "Đáp án số", "CHUNG_MINH" => "Chứng minh", _ => ma };
    public static string TrangThai(string ma) => ma switch { "NHAP" => "Bản nháp", "DA_RA_SOAT" => "Đã rà soát", _ => ma };
}
