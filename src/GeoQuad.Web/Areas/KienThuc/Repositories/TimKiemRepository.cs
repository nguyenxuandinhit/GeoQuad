using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.KienThuc.Repositories;

public interface ITimKiemRepository
{
    /// <summary>Tìm kiếm toàn văn kiến thức (FR-12, UC-04 — US-11).</summary>
    Task<IReadOnlyList<KetQuaTimKiem>> TimAsync(string truyVanLucene, int lop);
}

/// <summary>
/// Tìm kiếm qua chỉ mục toàn văn <c>kienThucTimKiem</c> (analyzer <c>standard-folding</c>
/// nên gõ có dấu hay không dấu đều ra, kể cả chữ "đ").
/// </summary>
public sealed class TimKiemRepository : ITimKiemRepository
{
    // FR-12 / UC-04 / SCR-06 (US-11). Chuỗi truy vấn Lucene vào qua tham số $q,
    // đã thoát ký tự đặc biệt ở TimKiemQuyTac (NFR-05).
    private const string CypherTim = """
        CALL db.index.fulltext.queryNodes('kienThucTimKiem', $q) YIELD node, score
        WHERE node.trangThai = 'DA_RA_SOAT'
        MATCH (node)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
        OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(node)
        OPTIONAL MATCH (node)-[:KHANG_DINH]->(dich:KhaiNiem)
        RETURN labels(node) AS nhan, node.ma AS ma, coalesce(node.ten, node.noiDung) AS tieuDe,
               coalesce(node.dinhNghia, node.noiDung, node.bieuThuc) AS doanTrich,
               l.so AS lop, score,
               coalesce(CASE WHEN node:KhaiNiem THEN node.ma END, h.ma, dich.ma) AS maHinh
        ORDER BY score DESC LIMIT 30
        """;

    private readonly IGraphDb _db;

    public TimKiemRepository(IGraphDb db) => _db = db;

    public async Task<IReadOnlyList<KetQuaTimKiem>> TimAsync(string truyVanLucene, int lop)
    {
        var ban = await _db.ReadAsync(CypherTim, new { q = truyVanLucene, lop });

        return ban.Select(Doc).ToList();
    }

    private static KetQuaTimKiem Doc(IRecord r) => new(
        Ma: r["ma"].As<string>(),
        TieuDe: r["tieuDe"].As<string>(),
        DoanTrich: r["doanTrich"]?.As<string>(),
        Nhan: r["nhan"].As<List<string>>(),
        Lop: r["lop"].As<int>(),
        Diem: r["score"].As<double>(),
        MaHinh: r["maHinh"]?.As<string>());
}
