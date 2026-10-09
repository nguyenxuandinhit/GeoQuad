namespace GeoQuad.Web.Areas.HocTap.Models;

public sealed record LuotLamTomTat(string MaLan, string MaBai, bool Dung, string Luc, string MaKhaiNiem, string TenKhaiNiem);
public sealed record TienDoKhaiNiem(string Ma, string Ten, int SoLuot, int SoDungAllTime, int SoDungGanNhat, decimal TiLeAllTime, decimal TiLeGanNhat, bool CanOn);
public sealed record TienDoViewModel(int Lop, int SoBaiDaLam, int SoLuot, decimal TiLeDung, IReadOnlyList<TienDoKhaiNiem> KhaiNiem, bool CoLichSu,
    LoaiGoiY Loai = LoaiGoiY.ChuaCoLichSu)
{
    public IReadOnlyList<TienDoKhaiNiem> CanOnLai => KhaiNiem.Where(x => x.CanOn).OrderBy(x => x.TiLeGanNhat).ToArray();
}
public sealed record BaiTapGoiY(string Ma, string De, string MaKhaiNiem, string TenKhaiNiem, int Lop, int DoKho);
/// <summary>Nhánh gợi ý US-24 theo UC-13: 1a chưa làm bài, đủ/chưa đủ dữ liệu, 3a không cần ôn.</summary>
public enum LoaiGoiY { ChuaCoLichSu, ChuaDuDuLieu, CanOn, KhongCanOn }
public sealed record GoiYViewModel(IReadOnlyList<BaiTapGoiY> BaiTap, IReadOnlyList<TienDoKhaiNiem> KhaiNiemYeu, bool CoLichSu, string ThongBao,
    LoaiGoiY Loai = LoaiGoiY.ChuaCoLichSu);
