using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Repositories;

public interface IChungMinhRepository { Task<ChungMinhViewModel?> XemAsync(string ma,int lop); }

public sealed class ChungMinhRepository(IGraphDb db) : IChungMinhRepository
{
    // Shared by proof detail and hint links so incomplete proofs are never advertised.
    public const string SanSang = """
        cm.trangThai='DA_RA_SOAT' AND cm.giaThiet IS NOT NULL AND trim(cm.giaThiet)<>'' AND
        cm.ketLuan IS NOT NULL AND trim(cm.ketLuan)<>'' AND
        EXISTS { (cm)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
        EXISTS { (cm)-[:CHUNG_MINH_CHO]->(dh:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(h:KhaiNiem {trangThai:'DA_RA_SOAT'})
          WHERE dh.noiDung IS NOT NULL AND EXISTS { (dh)-[:THUOC_LOP]->(dl:Lop) WHERE dl.so <= $lop }
          AND EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop } } AND
        COUNT { (cm)-[:CO_BUOC]->(:Buoc) } >= 3 AND COUNT { (cm)-[:CO_BUOC]->(:Buoc) } <= 6 AND
        all(i IN range(1,COUNT { (cm)-[:CO_BUOC]->(:Buoc) }) WHERE
          COUNT { (cm)-[r:CO_BUOC]->(:Buoc) WHERE r.thuTu=i }=1) AND
        NOT EXISTS { (cm)-[:CO_BUOC]->(b:Buoc)
          WHERE b.trangThai IS NULL OR b.trangThai <> 'DA_RA_SOAT' OR b.noiDung IS NULL OR trim(b.noiDung)='' OR
          COUNT { (b)-[:CAN_CU]->(:DinhLy) }=0 OR
          EXISTS { (b)-[:CAN_CU]->(cc:DinhLy)
            WHERE cc.trangThai IS NULL OR cc.trangThai <> 'DA_RA_SOAT' OR cc.noiDung IS NULL OR trim(cc.noiDung)='' OR
            NOT EXISTS { (cc)-[:THUOC_LOP]->(cl:Lop) WHERE cl.so <= $lop } OR
            (cc:TinhChat AND NOT EXISTS { (owner:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:CO_TINH_CHAT]->(cc)
              WHERE EXISTS { (owner)-[:THUOC_LOP]->(ol:Lop) WHERE ol.so <= $lop } }) } }
        """;
    public const string CypherCatalog = "MATCH (cm:ChungMinh)-[:CHUNG_MINH_CHO]->(dh:DauHieu) WHERE " + SanSang +
        " RETURN cm.ma AS ma,dh.ma AS dauHieu ORDER BY ma";
    public const string CypherXem = "MATCH (cm:ChungMinh {ma:$ma}) WHERE " + SanSang + """
        MATCH (cm)-[:CHUNG_MINH_CHO]->(dh:DauHieu), (cm)-[:THUOC_LOP]->(l:Lop), (cm)-[r:CO_BUOC]->(b:Buoc)
        MATCH (b)-[:CAN_CU]->(cc:DinhLy)
        OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT]->(cc)
        WITH cm,dh,l,r,b,collect(DISTINCT {ma:cc.ma,noiDung:cc.noiDung,maHinh:h.ma}) AS canCu
        ORDER BY r.thuTu
        RETURN cm.ma AS ma,cm.ten AS ten,cm.giaThiet AS giaThiet,cm.ketLuan AS ketLuan,
          dh.noiDung AS dinhLy,l.so AS lop,collect({thuTu:r.thuTu,noiDung:b.noiDung,canCu:canCu}) AS buoc
        """;

    public async Task<ChungMinhViewModel?> XemAsync(string ma,int lop)
    {
        var rows = await db.ReadAsync(CypherXem,new { ma,lop });
        if (rows.Count != 1) return null;
        var r = rows[0];
        var steps = r["buoc"].As<List<Dictionary<string,object>>>().Select(b => new BuocItem(b["thuTu"].As<int>(),
            b["noiDung"].As<string>(), b["canCu"].As<List<Dictionary<string,object>>>().Select(c =>
                new CanCuItem(c["ma"].As<string>(),c["noiDung"].As<string>(),c.GetValueOrDefault("maHinh")?.As<string>()))
                .OrderBy(c => c.Ma,StringComparer.Ordinal).ToArray())).ToArray();
        return new(r["ma"].As<string>(),r["ten"].As<string>(),r["giaThiet"].As<string>(),r["ketLuan"].As<string>(),
            r["dinhLy"].As<string>(),steps,r["lop"].As<int>());
    }
}
