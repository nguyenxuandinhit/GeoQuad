using System.Globalization;
using GeoQuad.Web.Areas.QuanTri.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.QuanTri.Repositories;

public interface IBaiTapQuanTriRepository
{
    Task<QuanTriCatalog> CatalogAsync();
    Task<IReadOnlyList<BaiTapRow>> DanhSachAsync(int? lopLoc,string? loai,bool? hienThi,string? trangThai,string? tuKhoa);
    Task<BaiTapForm?> XemAsync(string ma);
    Task LuuAsync(BaiTapDaKiemTra baiTap,bool sua);
    Task HienThiAsync(string ma,bool hienThi);
}

public sealed class BaiTapQuanTriRepository(IGraphDb db) : IBaiTapQuanTriRepository
{
    public const string CypherKhaiNiem = """
        MATCH (n:KhaiNiem {trangThai:'DA_RA_SOAT'})
        WHERE ($ids IS NULL OR n.ma IN $ids) AND n.ten IS NOT NULL AND
          EXISTS { (n)-[:THUOC_LOP]->(:Lop) }
        RETURN n.ma AS ma,n.ten AS ten ORDER BY ma
        """;
    public const string CypherSuDung = """
        MATCH (n) WHERE (n:DinhLy OR n:CongThuc) AND n.trangThai='DA_RA_SOAT' AND
          ($ids IS NULL OR n.ma IN $ids) AND coalesce(n.noiDung,n.ten) IS NOT NULL AND
          EXISTS { (n)-[:THUOC_LOP]->(:Lop) }
        RETURN n.ma AS ma,coalesce(n.noiDung,n.ten) AS ten ORDER BY ma
        """;
    public const string CypherDanhSach = """
        MATCH (b:BaiTap)
        WHERE ($lopLoc IS NULL OR EXISTS { (b)-[:THUOC_LOP]->(:Lop {so:$lopLoc}) }) AND
          ($loai IS NULL OR b.loai=$loai) AND ($hienThi IS NULL OR coalesce(b.hienThi,false)=$hienThi) AND
          ($trangThai IS NULL OR b.trangThai=$trangThai) AND
          ($tuKhoa IS NULL OR toLower(b.ma) CONTAINS toLower($tuKhoa) OR toLower(b.de) CONTAINS toLower($tuKhoa))
        OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
        WITH b,min(l.so) AS lop
        RETURN b.ma AS ma,coalesce(b.de,'') AS de,b.loai AS loai,lop,coalesce(b.doKho,1) AS doKho,
          coalesce(b.hienThi,false) AS hienThi,coalesce(b.trangThai,'NHAP') AS trangThai,
          COUNT { (:TaiKhoan)-[:DA_LAM]->(b) } AS soLanLam ORDER BY ma
        """;
    public const string CypherXem = """
        MATCH (b:BaiTap {ma:$ma})
        OPTIONAL MATCH (b)-[:THUOC_LOP]->(l:Lop)
        OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
        OPTIONAL MATCH (b)-[:SU_DUNG]->(s)
        RETURN properties(b) AS props,min(l.so) AS lop,collect(DISTINCT k.ma) AS khaiNiem,collect(DISTINCT s.ma) AS suDung
        """;
    public const string CypherTao = "CREATE (b:BaiTap {ma:$ma}) SET b += $props RETURN b.ma AS ma";
    public const string CypherSua = "MATCH (b:BaiTap {ma:$ma}) SET b += $props RETURN b.ma AS ma";
    public const string CypherXoaQuanHe = """
        MATCH (b:BaiTap {ma:$ma}) OPTIONAL MATCH (b)-[r:THUOC_LOP|LIEN_QUAN_DEN|SU_DUNG]->() DELETE r
        """;
    public const string CypherLop = "MATCH (b:BaiTap {ma:$ma}),(l:Lop {so:$lop}) MERGE (b)-[:THUOC_LOP]->(l)";
    public const string CypherGanKhaiNiem = """
        MATCH (b:BaiTap {ma:$ma}) UNWIND $ids AS id MATCH (k:KhaiNiem {ma:id}) MERGE (b)-[:LIEN_QUAN_DEN]->(k)
        """;
    public const string CypherGanSuDung = """
        MATCH (b:BaiTap {ma:$ma}) UNWIND $ids AS id MATCH (n) WHERE n.ma=id AND (n:DinhLy OR n:CongThuc)
        MERGE (b)-[:SU_DUNG]->(n)
        """;
    public const string CypherHienThi = "MATCH (b:BaiTap {ma:$ma}) SET b.hienThi=$hienThi RETURN b.ma AS ma";

    public async Task<QuanTriCatalog> CatalogAsync()
    {
        var kn=await db.ReadAsync(CypherKhaiNiem,new { ids=(string[]?)null });
        var use=await db.ReadAsync(CypherSuDung,new { ids=(string[]?)null });
        return new(kn.Select(MapCatalog).ToArray(),use.Select(MapCatalog).ToArray());
    }
    private static MucQuanTri MapCatalog(IRecord r) => new(r["ma"].As<string>(),r["ten"].As<string>());
    public async Task<IReadOnlyList<BaiTapRow>> DanhSachAsync(int? lopLoc,string? loai,bool? hienThi,string? trangThai,string? tuKhoa) =>
        (await db.ReadAsync(CypherDanhSach,new { lopLoc,loai,hienThi,trangThai,tuKhoa })).Select(r => new BaiTapRow(
            r["ma"].As<string>(),r["de"].As<string>(),r["loai"].As<string>(),r["lop"]?.As<int>(),r["doKho"].As<int>(),
            r["hienThi"].As<bool>(),r["trangThai"].As<string>(),r["soLanLam"].As<long>())).ToArray();
    public async Task<BaiTapForm?> XemAsync(string ma)
    {
        var rows=await db.ReadAsync(CypherXem,new { ma });
        if(rows.Count == 0) return null;
        var r=rows[0]; var props=r["props"].As<Dictionary<string,object>>();
        string Text(string name) => props.GetValueOrDefault(name)?.As<string>() ?? "";
        string Number(string name) => props.TryGetValue(name,out var n) ? n.As<double>().ToString("R",CultureInfo.InvariantCulture) : "";
        return new() { Ma=ma,De=Text("de"),Loai=Text("loai"),GiaiThich=Text("giaiThich"),Nguon=Text("nguon"),TrangThai=Text("trangThai"),
            Lop=r["lop"]?.As<int>() ?? 8,DoKho=props.GetValueOrDefault("doKho")?.As<int>() ?? 1,
            HienThi=props.GetValueOrDefault("hienThi")?.As<bool>() ?? false,
            PhuongAn=props.GetValueOrDefault("phuongAn")?.As<List<string>>().ToArray() ?? ["","","",""],DapAnDung=Text("dapAnDung"),
            DapAnSo=Number("dapAnSo"),SaiSo=Number("saiSo"),DonVi=Text("donVi"),LoiGiaiMau=Text("loiGiaiMau"),
            KhaiNiem=r["khaiNiem"].As<List<string>>().ToArray(),SuDung=r["suDung"].As<List<string>>().ToArray() };
    }
    public async Task LuuAsync(BaiTapDaKiemTra b,bool sua)
    {
        await db.WriteTransactionAsync(async tx =>
        {
            var grade=await (await tx.RunAsync("MATCH (l:Lop {so:$lop}) RETURN l.so AS so",new { lop=b.Lop })).ToListAsync();
            if(grade.Count != 1) throw new ArgumentException("Lớp không tồn tại.");
            async Task CheckIds(string query,IReadOnlyList<string> ids)
            {
                var rows=await (await tx.RunAsync(query,new { ids })).ToListAsync();
                if(rows.Count != ids.Count || !rows.Select(r => r["ma"].As<string>()).ToHashSet().SetEquals(ids))
                    throw new ArgumentException("Có khái niệm/định lý/công thức không tồn tại, sai nhãn hoặc chưa rà soát.");
            }
            await CheckIds(CypherKhaiNiem,b.KhaiNiem);
            await CheckIds(CypherSuDung,b.SuDung);
            var mutation=await (await tx.RunAsync(sua ? CypherSua : CypherTao,new { ma=b.Ma,props=new Dictionary<string,object>(b.ThuocTinh) })).ToListAsync();
            if(mutation.Count != 1) throw new KeyNotFoundException("Bài tập không tồn tại.");
            if(sua) await (await tx.RunAsync(CypherXoaQuanHe,new { ma=b.Ma })).ConsumeAsync();
            await (await tx.RunAsync(CypherLop,new { ma=b.Ma,lop=b.Lop })).ConsumeAsync();
            await (await tx.RunAsync(CypherGanKhaiNiem,new { ma=b.Ma,ids=b.KhaiNiem })).ConsumeAsync();
            await (await tx.RunAsync(CypherGanSuDung,new { ma=b.Ma,ids=b.SuDung })).ConsumeAsync();
        });
    }
    public async Task HienThiAsync(string ma,bool hienThi)
    {
        if((await db.WriteAsync(CypherHienThi,new { ma,hienThi })).Count != 1) throw new KeyNotFoundException("Bài tập không tồn tại.");
    }
}
