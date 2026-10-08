using System.Globalization;
using System.Text.RegularExpressions;

namespace GeoQuad.Web.Areas.ApDung.Models;

public sealed record SoDo(double AB,double BC,double CD,double DA,double AC,double BD,double SaiSoPhanTram,string DonVi);
public sealed record DoDacKetQua(string Hinh,IReadOnlyList<string> DauHieu,string CapLech,double Lech,string? Loi)
{
    public string TenHinh => Hinh switch { "HINH_VUONG" => "Hình vuông", "HINH_THOI" => "Hình thoi",
        "HINH_CHU_NHAT" => "Hình chữ nhật", "HINH_BINH_HANH" => "Hình bình hành", _ => "Chưa xác định được hình đặc biệt" };
}
public sealed class DoDacForm
{
    public string AB { get; set; } = "";
    public string BC { get; set; } = "";
    public string CD { get; set; } = "";
    public string DA { get; set; } = "";
    public string AC { get; set; } = "";
    public string BD { get; set; } = "";
    public string DonVi { get; set; } = "cm";
    public string SaiSo { get; set; } = "1";

    public static bool DocSo(string? value,out double number)
    {
        number=0;
        var text=value?.Trim();
        return text != null && text.Length <= 128 &&
            Regex.IsMatch(text,@"^[+-]?(?:\d+(?:[.,]\d+)?|[.,]\d+)$",RegexOptions.CultureInvariant) &&
            double.TryParse(text.Replace(',','.'),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,out number) && double.IsFinite(number);
    }
    public bool TryParse(out SoDo? measurements,out IReadOnlyDictionary<string,string> errors)
    {
        var invalid=new Dictionary<string,string>();
        var names=new[] {nameof(AB),nameof(BC),nameof(CD),nameof(DA),nameof(AC),nameof(BD)};
        var inputs=new[] {AB,BC,CD,DA,AC,BD};
        var values=new double[6];
        for(var i=0;i<6;i++)
            if(!DocSo(inputs[i],out values[i]) || values[i] <= 0) invalid[names[i]]="Nhập số đo hữu hạn lớn hơn 0, dùng dấu phẩy hoặc dấu chấm thập phân.";
        if(!DocSo(SaiSo,out var tolerance) || tolerance <= 0 || tolerance >= 100)
            invalid[nameof(SaiSo)]="Sai số phải lớn hơn 0 và nhỏ hơn 100%.";
        if(DonVi is not ("cm" or "m")) invalid[nameof(DonVi)]="Chọn đơn vị cm hoặc m cho toàn bộ số đo.";
        errors=invalid;
        measurements=invalid.Count > 0 ? null : new(values[0],values[1],values[2],values[3],values[4],values[5],tolerance,DonVi);
        return measurements != null;
    }
}
