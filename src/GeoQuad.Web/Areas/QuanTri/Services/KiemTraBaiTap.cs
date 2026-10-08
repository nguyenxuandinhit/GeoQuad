using System.Text.RegularExpressions;
using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Areas.QuanTri.Models;

namespace GeoQuad.Web.Areas.QuanTri.Services;

public static class KiemTraBaiTap
{
    public static readonly IReadOnlyList<string> Loai = Array.AsReadOnly(new[] {"TRAC_NGHIEM","DAP_AN_SO","CHUNG_MINH"});
    public static readonly IReadOnlyList<string> TrangThai = Array.AsReadOnly(new[] {"NHAP","DA_RA_SOAT"});
    public static KiemTraBaiTapKetQua KiemTra(BaiTapForm f)
    {
        var errors=new Dictionary<string,string>();
        bool Text(string? s,int max) => !string.IsNullOrWhiteSpace(s) && s.Length <= max;
        bool Id(string? id) => Text(id,64) && Regex.IsMatch(id!,@"^[A-Z0-9][A-Z0-9_-]*$",RegexOptions.CultureInvariant);
        if(!Id(f.Ma)) errors["Ma"]="Mã gồm chữ hoa, số, dấu - hoặc _, tối đa 64 ký tự.";
        if(!Text(f.De,10000)) errors["De"]="Nhập đề bài, tối đa 10.000 ký tự.";
        if(!Loai.Contains(f.Loai)) errors["Loai"]="Chọn một trong ba loại bài.";
        if(f.Lop is < 1 or > 12) errors["Lop"]="Lớp từ 1 đến 12.";
        if(f.DoKho is < 1 or > 3) errors["DoKho"]="Độ khó từ 1 đến 3.";
        if(!TrangThai.Contains(f.TrangThai)) errors["TrangThai"]="Trạng thái là NHAP hoặc DA_RA_SOAT.";
        if(!Text(f.Nguon,500)) errors["Nguon"]="Nhập nguồn nội dung, tối đa 500 ký tự.";
        if(!Text(f.GiaiThich,10000)) errors["GiaiThich"]="Nhập giải thích, tối đa 10.000 ký tự.";
        bool Ids(string[]? ids,bool required) => ids != null && ids.Length <= 100 && (!required || ids.Length > 0) &&
            ids.All(Id);
        if(!Ids(f.KhaiNiem,true)) errors["KhaiNiem"]="Chọn ít nhất một khái niệm hợp lệ.";
        if(!Ids(f.SuDung,false)) errors["SuDung"]="Danh sách định lý/công thức không hợp lệ.";
        var props=new Dictionary<string,object> { ["loai"]=f.Loai,["de"]=f.De?.Trim()!,["giaiThich"]=f.GiaiThich?.Trim()!,
            ["doKho"]=f.DoKho,["hienThi"]=f.HienThi,["nguon"]=f.Nguon?.Trim()!,["trangThai"]=f.TrangThai,
            ["phuongAn"]=null!,["dapAnDung"]=null!,["dapAnSo"]=null!,["saiSo"]=null!,["donVi"]=null!,["loiGiaiMau"]=null! };
        if(f.Loai == "TRAC_NGHIEM")
        {
            if(f.PhuongAn == null || f.PhuongAn.Length != 4 || f.PhuongAn.Any(s => !Text(s,2000))) errors["PhuongAn"]="Trắc nghiệm cần đúng 4 phương án không trống.";
            if(f.DapAnDung is not ("A" or "B" or "C" or "D")) errors["DapAnDung"]="Chọn đúng một đáp án A–D.";
            props["phuongAn"]=f.PhuongAn?.Select(p => p?.Trim() ?? "").ToArray()!; props["dapAnDung"]=f.DapAnDung!;
        }
        if(f.Loai == "DAP_AN_SO")
        {
            if(!DoDacForm.DocSo(f.DapAnSo,out var answer)) errors["DapAnSo"]="Nhập đáp án số hữu hạn; 0 là đáp án hợp lệ.";
            var toleranceText=string.IsNullOrWhiteSpace(f.SaiSo) ? "0.01" : f.SaiSo;
            if(!DoDacForm.DocSo(toleranceText,out var tolerance) || tolerance <= 0) errors["SaiSo"]="Sai số phải hữu hạn và lớn hơn 0.";
            if(!Text(f.DonVi,64)) errors["DonVi"]="Nhập đơn vị đáp án số.";
            props["dapAnSo"]=answer; props["saiSo"]=tolerance; props["donVi"]=f.DonVi?.Trim()!;
        }
        if(f.Loai == "CHUNG_MINH")
        {
            if(!Text(f.LoiGiaiMau,20000)) errors["LoiGiaiMau"]="Bài chứng minh cần lời giải mẫu, tối đa 20.000 ký tự.";
            props["loiGiaiMau"]=f.LoiGiaiMau?.Trim()!;
        }
        if(errors.Count > 0) return new(null,errors);
        return new(new(f.Ma,f.Lop,f.KhaiNiem.Distinct(StringComparer.Ordinal).ToArray(),
            f.SuDung.Distinct(StringComparer.Ordinal).ToArray(),props),errors);
    }
}
