using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Repositories;

public interface ITinhHuongRepository
{
    Task<IReadOnlyList<BoiCanhItem>> BoiCanhAsync();
    Task<IReadOnlyList<TinhHuongItem>> DocAsync(int lop,string? ma = null);
}

public sealed class TinhHuongRepository(IGraphDb db) : ITinhHuongRepository
{
    public const string CypherBoiCanh = "MATCH (b:BoiCanh) RETURN b.ma AS ma,b.ten AS ten ORDER BY ma";
    public const string CypherDoc = """
        MATCH (th:TinhHuong {trangThai:'DA_RA_SOAT'})-[:TRONG_BOI_CANH]->(bc:BoiCanh)
        WHERE ($ma IS NULL OR th.ma=$ma) AND th.ten IS NOT NULL AND th.moTa IS NOT NULL AND
          th.loiGiai IS NOT NULL AND trim(th.loiGiai) <> '' AND
          COUNT { (th)-[:AP_DUNG]->() } > 0 AND COUNT { (th)-[:LIEN_QUAN_DEN]->(:KhaiNiem) } > 0 AND
          NOT EXISTS { (th)-[:AP_DUNG]->(bad) WHERE NOT (bad:DinhLy OR bad:CongThuc) OR
            bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR
            coalesce(bad.noiDung,bad.ten) IS NULL OR trim(coalesce(bad.noiDung,bad.ten))='' OR
            NOT EXISTS { (bad)-[:THUOC_LOP]->(:Lop) } OR
            EXISTS { (bad)-[:THUOC_LOP]->(high:Lop) WHERE high.so > $lop } } AND
          NOT EXISTS { (th)-[:LIEN_QUAN_DEN]->(h) WHERE NOT h:KhaiNiem OR h.trangThai IS NULL OR
            h.trangThai <> 'DA_RA_SOAT' OR NOT EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop } }
        MATCH (th)-[:AP_DUNG]->(x)-[:THUOC_LOP]->(l:Lop)
        OPTIONAL MATCH (owner:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(x)
        OPTIONAL MATCH (x)-[:KHANG_DINH]->(dich:KhaiNiem)
        WITH th,bc,max(l.so) AS lopCan,collect(DISTINCT {ma:x.ma,noiDung:coalesce(x.noiDung,x.ten),
          bieuThuc:x.bieuThuc,maHinh:coalesce(owner.ma,dich.ma)}) AS knowledge
        WHERE lopCan <= $lop
        RETURN th.ma AS ma,th.ten AS ten,bc.ma AS bc,bc.ten AS boiCanh,th.moTa AS moTa,th.loiGiai AS loiGiai,
          coalesce(th.thucHanh,false) AS thucHanh,lopCan AS lop,knowledge ORDER BY ma
        """;
    public async Task<IReadOnlyList<BoiCanhItem>> BoiCanhAsync() =>
        (await db.ReadAsync(CypherBoiCanh)).Select(r => new BoiCanhItem(r["ma"].As<string>(),r["ten"].As<string>())).ToArray();
    public async Task<IReadOnlyList<TinhHuongItem>> DocAsync(int lop,string? ma = null) =>
        (await db.ReadAsync(CypherDoc,new { lop,ma })).Select(r => new TinhHuongItem(r["ma"].As<string>(),r["ten"].As<string>(),
            r["bc"].As<string>(),r["boiCanh"].As<string>(),r["moTa"].As<string>(),r["loiGiai"].As<string>(),
            r["thucHanh"].As<bool>(),r["lop"].As<int>(),r["knowledge"].As<List<Dictionary<string,object>>>()
                .Select(k => new KienThucItem(k["ma"].As<string>(),k["noiDung"].As<string>(),
                    k.GetValueOrDefault("bieuThuc")?.As<string>(),k.GetValueOrDefault("maHinh")?.As<string>()))
                .GroupBy(k => k.Ma).Select(g => g.First()).OrderBy(k => k.Ma,StringComparer.Ordinal).ToArray())).ToArray();
}
