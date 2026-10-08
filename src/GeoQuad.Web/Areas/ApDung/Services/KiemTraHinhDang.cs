using GeoQuad.Web.Areas.ApDung.Models;

namespace GeoQuad.Web.Areas.ApDung.Services;

/// <summary>Phân loại từ số đo/dung sai, với giả định ABCD lồi; không phải chứng minh chính xác.</summary>
public static class KiemTraHinhDang
{
    public static double Lech(double a,double b) => Math.Abs(a-b)/Math.Max(a,b);
    public static bool Bang(double a,double b,double epsilon) =>
        double.IsFinite(a) && double.IsFinite(b) && a > 0 && b > 0 && Lech(a,b) <= epsilon;

    private static bool TamGiac(double a,double b,double c)
    {
        var max=Math.Max(a,Math.Max(b,c));
        var sides=new[] {a/max,b/max,c/max};
        Array.Sort(sides);
        return sides[0]+sides[1] > sides[2];
    }
    public static DoDacKetQua KiemTra(SoDo s)
    {
        var sides=new[] {s.AB,s.BC,s.CD,s.DA,s.AC,s.BD};
        if(sides.Any(x => !double.IsFinite(x) || x <= 0) || !double.IsFinite(s.SaiSoPhanTram) ||
            s.SaiSoPhanTram is <= 0 or >= 100)
            return new("KHONG_XAC_DINH",[],"",0,"Mọi số đo phải hữu hạn và lớn hơn 0; sai số trong khoảng (0,100)%.");
        if(!TamGiac(s.AB,s.BC,s.AC) || !TamGiac(s.AC,s.CD,s.DA) ||
            !TamGiac(s.AB,s.BD,s.DA) || !TamGiac(s.BC,s.CD,s.BD))
            return new("KHONG_XAC_DINH",[],"",0,"Số đo không thỏa bất đẳng thức tam giác ABC, ACD, ABD hoặc BCD. Em đo lại nhé.");
        var pairs=new[] {("AB/BC",s.AB,s.BC),("AB/CD",s.AB,s.CD),("AB/DA",s.AB,s.DA),
            ("BC/CD",s.BC,s.CD),("BC/DA",s.BC,s.DA),("CD/DA",s.CD,s.DA),("AC/BD",s.AC,s.BD)};
        var worst=pairs.Select(p => (Name:p.Item1,Delta:Lech(p.Item2,p.Item3)))
            .OrderByDescending(p => p.Delta).ThenBy(p => p.Name,StringComparer.Ordinal).First();
        var eps=s.SaiSoPhanTram/100;
        var edges=sides.Take(4).ToArray();
        var trace=new List<string>();
        var shape="KHONG_XAC_DINH";
        if(Lech(edges.Min(),edges.Max()) <= eps)
        {
            shape="HINH_THOI"; trace.Add("DH_THOI_1");
            if(Bang(s.AC,s.BD,eps)) { shape="HINH_VUONG"; trace.Add("DH_HV_5"); }
        }
        else if(Bang(s.AB,s.CD,eps) && Bang(s.BC,s.DA,eps))
        {
            shape="HINH_BINH_HANH"; trace.Add("DH_HBH_2");
            if(Bang(s.AC,s.BD,eps))
            {
                shape="HINH_CHU_NHAT"; trace.Add("DH_HCN_3");
                if(Bang(s.AB,s.BC,eps)) { shape="HINH_VUONG"; trace.Add("DH_HV_1"); }
            }
        }
        return new(shape,trace.ToArray(),worst.Name,worst.Delta,null);
    }
}
