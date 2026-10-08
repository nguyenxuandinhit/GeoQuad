using GeoQuad.Web.Areas.ApDung.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.ApDung.Repositories;

public interface IGoiYRepository
{
    Task<GoiYCatalog> CatalogAsync(int lop);
    Task<bool> SuyRaAsync(string nen, string dich, int lop);
    Task<IReadOnlyList<string>> TrucTiepAsync(string nen, string dich, int lop);
    Task<IReadOnlyList<IReadOnlyList<string>>> ChuoiAsync(string nen, string dich, int lop);
}

public sealed class GoiYRepository(IGraphDb db) : IGoiYRepository
{
    public const string CypherHinh = """
        MATCH (h:KhaiNiem {loai:'HINH',trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND h.ten IS NOT NULL
        RETURN h.ma AS ma,h.ten AS ten,min(l.so) AS lop ORDER BY ma
        """;
    public const string CypherDieuKien = """
        MATCH (d:DieuKien {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND d.noiDung IS NOT NULL
        RETURN d.ma AS ma,d.noiDung AS noiDung,min(l.so) AS lop ORDER BY ma
        """;
    public const string CypherDauHieu = """
        MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:YEU_CAU_LA]->(nen:KhaiNiem {trangThai:'DA_RA_SOAT'}),
          (d)-[:KHANG_DINH]->(dich:KhaiNiem {trangThai:'DA_RA_SOAT'}), (d)-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND d.noiDung IS NOT NULL
          AND all(h IN [nen,dich] WHERE EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop })
          AND COUNT { (d)-[:YEU_CAU_CO]->(:DieuKien) } > 0
          AND NOT EXISTS { (d)-[:YEU_CAU_CO]->(bad:DieuKien)
            WHERE bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR bad.noiDung IS NULL
              OR NOT EXISTS { (bad)-[:THUOC_LOP]->(bl:Lop) WHERE bl.so <= $lop } }
        MATCH (d)-[:YEU_CAU_CO]->(dk:DieuKien)-[:THUOC_LOP]->(kl:Lop)
        WHERE kl.so <= $lop
        WITH d,nen,dich,l,dk,min(kl.so) AS dkLop ORDER BY dk.ma
        RETURN d.ma AS ma,d.noiDung AS noiDung,nen.ma AS nen,dich.ma AS dich,l.so AS lop,
          collect(DISTINCT {ma:dk.ma,noiDung:dk.noiDung,lop:dkLop}) AS dks
        ORDER BY ma
        """;
    public const string CypherSuyRa = """
        RETURN EXISTS {
          MATCH p=(nen:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(dich:KhaiNiem {ma:$dich})
          WHERE all(h IN nodes(p) WHERE h.trangThai='DA_RA_SOAT' AND
            EXISTS { (h)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop })
        } AS yes
        """;
    public const string CypherTrucTiep = """
        MATCH p=(input:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(nen:KhaiNiem)
        MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:YEU_CAU_LA]->(nen),
          (d)-[:KHANG_DINH]->(:KhaiNiem {ma:$dich}), (d)-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND all(h IN nodes(p) WHERE h.trangThai='DA_RA_SOAT' AND
          EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop })
        RETURN DISTINCT d.ma AS ma ORDER BY ma
        """;
    public const string CypherChuoi = """
        MATCH inheritance=(input:KhaiNiem {ma:$nen})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..6]->(start:KhaiNiem)
        MATCH p=(start)
          ((:KhaiNiem)<-[:YEU_CAU_LA]-(d:DauHieu)-[:KHANG_DINH]->(:KhaiNiem)){2,3}
          (:KhaiNiem {ma:$dich})
        WHERE all(x IN d WHERE x.trangThai='DA_RA_SOAT' AND x.noiDung IS NOT NULL AND
          EXISTS { (x)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop } AND
          COUNT { (x)-[:YEU_CAU_CO]->(:DieuKien) } > 0 AND
          NOT EXISTS { (x)-[:YEU_CAU_CO]->(bad:DieuKien)
            WHERE bad.trangThai IS NULL OR bad.trangThai <> 'DA_RA_SOAT' OR bad.noiDung IS NULL OR
            NOT EXISTS { (bad)-[:THUOC_LOP]->(bl:Lop) WHERE bl.so <= $lop } })
          AND all(h IN nodes(p)+nodes(inheritance) WHERE NOT h:KhaiNiem OR
            (h.trangThai='DA_RA_SOAT' AND EXISTS { (h)-[:THUOC_LOP]->(hl:Lop) WHERE hl.so <= $lop }))
          AND all(n IN nodes(p) WHERE size([other IN nodes(p) WHERE other=n])=1)
        WITH DISTINCT [x IN d | x.ma] AS codes
        RETURN codes ORDER BY size(codes),codes LIMIT 3
        """;

    public async Task<GoiYCatalog> CatalogAsync(int lop)
    {
        var hinh = await db.ReadAsync(CypherHinh, new { lop });
        var dk = await db.ReadAsync(CypherDieuKien, new { lop });
        var dh = await db.ReadAsync(CypherDauHieu, new { lop });
        var proofs = (await db.ReadAsync(ChungMinhRepository.CypherCatalog,new { lop }))
            .GroupBy(r => r["dauHieu"].As<string>()).ToDictionary(g => g.Key,g => g.First()["ma"].As<string>());
        return new(hinh.Select(r => new HinhItem(r["ma"].As<string>(),r["ten"].As<string>(),r["lop"].As<int>())).ToArray(),
            dk.Select(r => new DieuKienItem(r["ma"].As<string>(),r["noiDung"].As<string>(),r["lop"].As<int>())).ToArray(),
            dh.Select(r => new DauHieuItem(r["ma"].As<string>(),r["noiDung"].As<string>(),r["nen"].As<string>(),
                r["dich"].As<string>(),r["lop"].As<int>(), r["dks"].As<List<Dictionary<string,object>>>()
                    .Select(d => new DieuKienItem(d["ma"].As<string>(),d["noiDung"].As<string>(),d["lop"].As<int>())).ToArray(),
                    proofs.GetValueOrDefault(r["ma"].As<string>()))).ToArray());
    }
    public async Task<bool> SuyRaAsync(string nen,string dich,int lop) =>
        (await db.ReadAsync(CypherSuyRa,new { nen,dich,lop })).Single()["yes"].As<bool>();
    public async Task<IReadOnlyList<string>> TrucTiepAsync(string nen,string dich,int lop) =>
        (await db.ReadAsync(CypherTrucTiep,new { nen,dich,lop })).Select(r => r["ma"].As<string>()).ToArray();
    public async Task<IReadOnlyList<IReadOnlyList<string>>> ChuoiAsync(string nen,string dich,int lop) =>
        (await db.ReadAsync(CypherChuoi,new { nen,dich,lop })).Select(r => (IReadOnlyList<string>)r["codes"].As<List<string>>()).ToArray();
}
