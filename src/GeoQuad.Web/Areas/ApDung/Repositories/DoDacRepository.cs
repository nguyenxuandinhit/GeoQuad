using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Repositories;

public sealed class DoDacRepository(IGraphDb db)
{
    public const string Cypher = """
        MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(h:KhaiNiem {trangThai:'DA_RA_SOAT'})
        WHERE d.ma IN $mas AND d.noiDung IS NOT NULL AND
          EXISTS { (d)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
          EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop }
        RETURN DISTINCT d.ma AS ma,d.noiDung AS noiDung,h.ma AS hinh ORDER BY ma
        """;
    public async Task<IReadOnlyList<KienThucItem>> DauHieuAsync(IReadOnlyList<string> mas,int lop) =>
        (await db.ReadAsync(Cypher,new { mas,lop })).Select(r => new KienThucItem(r["ma"].As<string>(),
            r["noiDung"].As<string>(),null,r["hinh"].As<string>())).ToArray();
}
