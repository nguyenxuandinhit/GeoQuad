namespace GeoQuad.Web.Areas.ApDung.Models;

public sealed record BoiCanhItem(string Ma,string Ten);
public sealed record KienThucItem(string Ma,string NoiDung,string? BieuThuc,string? MaHinh);
public sealed record TinhHuongItem(string Ma,string Ten,string MaBoiCanh,string BoiCanh,string MoTa,string LoiGiai,
    bool ThucHanh,int Lop,IReadOnlyList<KienThucItem> KienThuc);
public sealed record TinhHuongDanhSach(IReadOnlyList<BoiCanhItem> BoiCanh,IReadOnlyList<TinhHuongItem> TinhHuong,
    string? Loc,int Lop);
public sealed record TinhHuongViewModel(TinhHuongItem TinhHuong,bool NangCao,
    DoDacForm? DoDac = null,DoDacKetQua? KetQua = null,IReadOnlyList<KienThucItem>? CanCu = null);
