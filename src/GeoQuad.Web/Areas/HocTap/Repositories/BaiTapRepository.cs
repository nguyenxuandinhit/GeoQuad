using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public sealed class BaiTapRepository(IGraphDb db) : IBaiTapRepository
{
    // FR-50 / US-19: chỉ lấy bài đã duyệt, hiển thị, đúng lớp và có khái niệm.
    // Các bộ lọc được truyền qua tham số, không nối chuỗi vào Cypher.
    public const string CypherLoc = """
        MATCH (bt:BaiTap {hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
        WHERE l.so <= $lop
          AND ($lopLoc IS NULL OR l.so = $lopLoc)
          AND ($capLoc IS NULL OR cap.ma = $capLoc)
          AND ($loai IS NULL OR bt.loai = $loai)
          AND ($doKho IS NULL OR bt.doKho = $doKho)
          AND EXISTS { MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
          AND ($khaiNiem IS NULL OR EXISTS {
              MATCH (bt)-[:LIEN_QUAN_DEN]->(:KhaiNiem {ma:$khaiNiem})
          })
        OPTIONAL MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM]->(bt)
        RETURN bt.ma AS ma, left(bt.de,120) AS de, bt.loai AS loai,
               bt.doKho AS doKho, l.so AS lop, cap.ma AS cap,
               count(DISTINCT d) AS soLan,
               max(CASE WHEN d.dung THEN 1 ELSE 0 END) > 0 AS daDung
        ORDER BY lop, doKho, ma
        """;

    public const string CypherKhaiNiem = """
        MATCH (k:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop
        RETURN DISTINCT k.ma AS ma,k.ten AS ten ORDER BY ten,ma
        """;

    public async Task<IReadOnlyList<BaiTapTomTat>> LocAsync(int lop, BaiTapLoc boLoc, string? taiKhoanId)
    {
        var rows = await db.ReadAsync(CypherLoc, new Dictionary<string, object?>
        {
            ["lop"] = lop,
            ["lopLoc"] = boLoc.Lop,
            ["capLoc"] = boLoc.Cap,
            ["loai"] = boLoc.Loai,
            ["doKho"] = boLoc.DoKho,
            ["khaiNiem"] = boLoc.KhaiNiem,
            ["tk"] = taiKhoanId
        });
        return rows.Select(r => new BaiTapTomTat(
            r["ma"].As<string>(), r["de"].As<string>(), r["loai"].As<string>(),
            r["doKho"].As<int>(), r["lop"].As<int>(), r["cap"].As<string>(),
            r["soLan"].As<long>(), r["daDung"].As<bool>())).ToArray();
    }

    public async Task<IReadOnlyList<KhaiNiemLoc>> KhaiNiemAsync(int lop)
    {
        var rows = await db.ReadAsync(CypherKhaiNiem, new { lop });
        return rows.Select(r => new KhaiNiemLoc(r["ma"].As<string>(), r["ten"].As<string>())).ToArray();
    }
}
