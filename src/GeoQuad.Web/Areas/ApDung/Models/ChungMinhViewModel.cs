namespace GeoQuad.Web.Areas.ApDung.Models;

public sealed record CanCuItem(string Ma,string NoiDung,string? MaHinh);
public sealed record BuocItem(int ThuTu,string NoiDung,IReadOnlyList<CanCuItem> CanCu);
public sealed record ChungMinhViewModel(string Ma,string Ten,string GiaThiet,string KetLuan,string DinhLy,
    IReadOnlyList<BuocItem> Buoc,int Lop,bool NangCao = false);
