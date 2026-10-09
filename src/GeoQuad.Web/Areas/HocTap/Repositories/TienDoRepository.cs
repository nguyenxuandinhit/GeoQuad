using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public sealed class TienDoRepository(IGraphDb db) : ITienDoRepository
{
    public const string CypherLop = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
        RETURN l.so AS lop
        """;

    public const string CypherLichSu = """
        MATCH (tk:TaiKhoan {id:$tk})-[r:DA_LAM]->(b:BaiTap)
        OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem)
        WITH r,b,k WHERE k IS NOT NULL
        RETURN coalesce(r.maLan,elementId(r)) AS maLan,b.ma AS maBai,
               coalesce(r.dung,false) AS dung,coalesce(toString(r.luc),'') AS luc,
               k.ma AS maKhaiNiem,k.ten AS tenKhaiNiem
        ORDER BY luc DESC,maLan DESC
        """;

    public const string CypherUngVien = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(lop:Lop)
        MATCH (b:BaiTap {hienThi:true,trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
        MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lk:Lop)
        WHERE lb.so <= lop.so AND lk.so <= lop.so
          AND b.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
          AND NOT EXISTS { MATCH (tk)-[d:DA_LAM]->(b) WHERE d.dung=true }
        RETURN DISTINCT b.ma AS ma,b.de AS de,k.ma AS maKhaiNiem,k.ten AS tenKhaiNiem,lb.so AS lop,b.doKho AS doKho
        ORDER BY maKhaiNiem,doKho,ma
        """;

    public async Task<int?> LopHienTaiAsync(string taiKhoanId)
    {
        var rows = await db.ReadAsync(CypherLop, new { tk = taiKhoanId });
        return rows.Count == 0 ? null : rows[0]["lop"].As<int>();
    }

    public async Task<IReadOnlyList<LuotLamTomTat>> LichSuAsync(string taiKhoanId)
    {
        var rows = await db.ReadAsync(CypherLichSu, new { tk = taiKhoanId });
        return rows.Select(r => new LuotLamTomTat(r["maLan"].As<string>(), r["maBai"].As<string>(),
            r["dung"].As<bool>(), r["luc"].As<string>(), r["maKhaiNiem"].As<string>(),
            r["tenKhaiNiem"].As<string>())).ToArray();
    }

    public async Task<IReadOnlyList<BaiTapGoiY>> UngVienAsync(string taiKhoanId, int lop)
    {
        var rows = await db.ReadAsync(CypherUngVien, new { tk = taiKhoanId, lop });
        return rows.Select(r => new BaiTapGoiY(r["ma"].As<string>(), r["de"].As<string>(),
            r["maKhaiNiem"].As<string>(), r["tenKhaiNiem"].As<string>(), r["lop"].As<int>(), r["doKho"].As<int>())).ToArray();
    }
}
