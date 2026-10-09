using GeoQuad.Web.Areas.HocTap.Models;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public sealed record BanGhiLuotLam(string MaLan, string PayloadHash, string DapAn,
    string DapAnDung, string DonVi, bool Dung, int ThoiGianGiay, bool XungDot, string MaBaiTap = "");

public interface ILamBaiRepository
{
    Task<BaiTapChiTiet?> ChiTietAsync(string ma, int lopHienThi);
    Task<BanGhiLuotLam?> GhiNhanAsync(string taiKhoanId, int lopThuc, BaiTapChiTiet bai,
        string maLan, string payloadHash, string dapAn, string donViDaChon, bool dung, int seconds);
    Task<bool> LaChungMinhCongKhaiAsync(string ma, int lopHienThi);
    Task<BanGhiLuotLam?> KetQuaAsync(string taiKhoanId, string maLan);
    Task<string?> BaiTiepTheoKhachAsync(string maHienTai, int lopHienThi);
    Task<KienThucBaiTap> KienThucLienQuanAsync(string ma, int lopHienThi);
}
