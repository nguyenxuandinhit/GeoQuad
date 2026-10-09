using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public sealed class LoTrinhRepository(IGraphDb db) : ILoTrinhRepository
{
    public const string CypherLop = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
        RETURN l.so AS lop
        """;

    public const string CypherMucTieu = """
        MATCH (k:KhaiNiem {loai:'HINH',trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WITH k,min(l.so) AS lop
        WHERE lop <= $lop
        RETURN k.ma AS ma,k.ten AS ten,lop,k.trangThai AS trangThai
        ORDER BY lop DESC,ma
        """;

    public const string CypherDoThi = """
        MATCH (target:KhaiNiem {ma:$ma,trangThai:'DA_RA_SOAT'})
        OPTIONAL MATCH (target)-[:CAN_BIET_TRUOC*0..12]->(n:KhaiNiem)
        WITH n WHERE n IS NOT NULL
        OPTIONAL MATCH (n)-[:THUOC_LOP]->(l:Lop)
        WITH n,min(l.so) AS lop
        OPTIONAL MATCH (n)-[:CAN_BIET_TRUOC]->(p:KhaiNiem)
        RETURN n.ma AS ma,n.ten AS ten,coalesce(n.trangThai,'') AS trangThai,coalesce(lop,-1) AS lop,
               collect(DISTINCT p.ma) AS tienQuyet
        ORDER BY ma
        """;

    public const string CypherDaHoc = """
        MATCH (tk:TaiKhoan {id:$tk})-[r:DA_HOC]->(k:KhaiNiem)
        WHERE k.ma IN $ma
        RETURN DISTINCT k.ma AS ma
        """;

    private const string CypherKhoaTaiKhoan = """
        MATCH (tk:TaiKhoan {id:$tk})
        SET tk._gqRoadmapLock=$lock
        REMOVE tk._gqRoadmapLock
        RETURN tk.id AS id
        """;

    private const string CypherGhiDaHoc = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
        MATCH (target:KhaiNiem {ma:$muc,trangThai:'DA_RA_SOAT'})
        MATCH (target)-[:CAN_BIET_TRUOC*0..12]->(k:KhaiNiem {ma:$ma,trangThai:'DA_RA_SOAT'})
        MATCH (k)-[:THUOC_LOP]->(kl:Lop)
        WHERE k.ma IN $maLoTrinh AND kl.so <= l.so
        MERGE (tk)-[r:DA_HOC]->(k)
        ON CREATE SET r.luc=datetime()
        RETURN count(r)>0 AS ok
        """;

    public async Task<int?> LopHienTaiAsync(string taiKhoanId)
    {
        var rows = await db.ReadAsync(CypherLop, new { tk = taiKhoanId });
        return rows.Count == 0 ? null : rows[0]["lop"].As<int>();
    }

    public async Task<IReadOnlyList<KhaiNiemLoTrinh>> MucTieuHopLeAsync(int lop)
    {
        var rows = await db.ReadAsync(CypherMucTieu, new { lop });
        return rows.Select(r => new KhaiNiemLoTrinh(r["ma"].As<string>(), r["ten"].As<string>(),
            r["lop"].As<int>(), r["trangThai"].As<string>())).ToArray();
    }

    public async Task<LoTrinhDoThi?> DoThiAsync(string taiKhoanId, string maMucTieu)
    {
        var rows = await db.ReadAsync(CypherDoThi, new { ma = maMucTieu });
        if (rows.Count == 0) return null;
        var nodes = rows.Select(r => new KhaiNiemLoTrinh(r["ma"].As<string>(), r["ten"].As<string>(),
            r["lop"].As<int>(), r["trangThai"].As<string>())).ToArray();
        var edges = rows.ToDictionary(r => r["ma"].As<string>(),
            r => (IReadOnlyCollection<string>)r["tienQuyet"].As<List<string>>(), StringComparer.Ordinal);
        var learnedRows = await db.ReadAsync(CypherDaHoc, new { tk = taiKhoanId, ma = nodes.Select(n => n.Ma).ToArray() });
        var learned = learnedRows.Select(r => r["ma"].As<string>()).ToHashSet(StringComparer.Ordinal);
        return new LoTrinhDoThi(nodes, edges, learned);
    }

    public async Task<bool> DanhDauDaHocAsync(string taiKhoanId, string mucTieu, string ma,
        IReadOnlyCollection<string> maTrongLoTrinh)
    {
        var marked = false;
        await db.WriteTransactionAsync(async tx =>
        {
            var locked = await tx.RunAsync(CypherKhoaTaiKhoan, new { tk = taiKhoanId, @lock = Guid.NewGuid().ToString("N") });
            if (!(await locked.ToListAsync()).Any()) return;
            var result = await tx.RunAsync(CypherGhiDaHoc,
                new { tk = taiKhoanId, ma, muc = mucTieu, maLoTrinh = maTrongLoTrinh.ToArray() });
            var rows = await result.ToListAsync();
            marked = rows.Count > 0 && rows[0]["ok"].As<bool>();
        });
        return marked;
    }
}
