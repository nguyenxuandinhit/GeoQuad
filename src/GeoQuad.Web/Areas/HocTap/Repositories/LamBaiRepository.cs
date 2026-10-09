using GeoQuad.Web.Areas.HocTap.Models;
using GeoQuad.Web.Areas.HocTap.Services;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

public sealed class LamBaiRepository(IGraphDb db) : ILamBaiRepository
{
    public const string CypherChiTiet = """
        MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND EXISTS { MATCH (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
        OPTIONAL MATCH (b)-[:SU_DUNG]->(:DinhLy)<-[:CHUNG_MINH_CHO]-(cm:ChungMinh {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lc:Lop)
        WHERE lc IS NULL OR lc.so <= $lop
        RETURN b.ma AS ma,b.de AS de,b.loai AS loai,l.so AS lop,b.doKho AS doKho,
               coalesce(b.phuongAn,[]) AS phuongAn,b.dapAnDung AS dapAnDung,
               b.dapAnSo AS dapAnSo,b.saiSo AS saiSo,coalesce(b.donVi,'') AS donVi,
               coalesce(b.giaiThich,'') AS giaiThich,b.loiGiaiMau AS loiGiaiMau,
               cm.ma AS maChungMinh LIMIT 1
        """;

    // FR-51 / UC-11 (US-20): định lý/công thức bài dùng và khái niệm liên quan, chỉ cho trang kết quả.
    // Chỉ nội dung đã rà soát, lớp xuất hiện đầu tiên (min) không vượt lớp hiển thị.
    public const string CypherKienThucLienQuan = """
        MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})
        OPTIONAL MATCH (b)-[:SU_DUNG]->(x {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lx:Lop)
        WHERE x:DinhLy OR x:CongThuc
        WITH b, x, min(lx.so) AS lopX
        WITH b, [i IN collect(CASE WHEN x IS NOT NULL AND lopX <= $lop
                  THEN x {.ma, ten: coalesce(x.ten, x.noiDung), bieuThuc: x.bieuThuc, lop: lopX} END)
                 WHERE i IS NOT NULL] AS kienThuc
        OPTIONAL MATCH (b)-[:LIEN_QUAN_DEN]->(k:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lk:Lop)
        WITH kienThuc, k, min(lk.so) AS lopK
        RETURN kienThuc,
               [i IN collect(CASE WHEN k IS NOT NULL AND lopK <= $lop
                  THEN k {.ma, .ten, lop: lopK} END) WHERE i IS NOT NULL] AS khaiNiem
        """;

    private const string CypherLockAccount = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(l:Lop)
        SET tk._hocTapLock = $nonce
        REMOVE tk._hocTapLock
        RETURN l.so AS lop
        """;

    private const string CypherGhiLan = """
        MATCH (tk:TaiKhoan {id:$tk})-[:HOC_LOP]->(la:Lop)
        MATCH (b:BaiTap {ma:$ma, hienThi:true, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
        WHERE la.so=$lopThuc AND lb.so <= la.so AND b.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
          AND EXISTS { MATCH (b)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
        MERGE (tk)-[d:DA_LAM {maLan:$maLan}]->(b)
        ON CREATE SET d.payloadHash=$hash, d.dapAnDaChon=$dapAn, d.donViDaChon=$donVi,
                      d.dung=$dung,
                      d.luc=datetime(), d.thoiGianGiay=$seconds
        RETURN d.maLan AS maLan,b.ma AS maBaiTap,d.payloadHash AS payloadHash,d.dapAnDaChon AS dapAn,
               d.dung AS dung,d.thoiGianGiay AS seconds
        """;

    private const string CypherProofPublic = """
        MATCH (cm:ChungMinh {ma:$ma, trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND EXISTS { MATCH (cm)-[:CHUNG_MINH_CHO]->(:DinhLy {trangThai:'DA_RA_SOAT'}) }
        RETURN count(cm)>0 AS ok
        """;

    private const string CypherKetQua = """
        MATCH (:TaiKhoan {id:$tk})-[d:DA_LAM {maLan:$maLan}]->(b:BaiTap {hienThi:true,trangThai:'DA_RA_SOAT'})
        WHERE b.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
        RETURN d.maLan AS maLan,b.ma AS maBaiTap,d.payloadHash AS payloadHash,d.dapAnDaChon AS dapAn,
               d.dung AS dung,d.thoiGianGiay AS seconds,b.loai AS loai,b.dapAnDung AS keyChoice,
               b.dapAnSo AS keyNumber,coalesce(b.donVi,'') AS donVi
        """;

    private const string CypherBaiTiepTheo = """
        MATCH (b:BaiTap {ma:$ma})-[:THUOC_LOP]->(lb:Lop)
        MATCH (n:BaiTap {hienThi:true,trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE lb.so <= $lop AND l.so=lb.so AND n.ma>$ma AND n.loai IN ['TRAC_NGHIEM','DAP_AN_SO']
          AND EXISTS { MATCH (n)-[:LIEN_QUAN_DEN]->(:KhaiNiem) }
        RETURN n.ma AS ma ORDER BY n.ma LIMIT 1
        """;

    public async Task<KienThucBaiTap> KienThucLienQuanAsync(string ma, int lopHienThi)
    {
        var rows = await db.ReadAsync(CypherKienThucLienQuan, new { ma, lop = lopHienThi });
        if (rows.Count == 0) return KienThucBaiTap.Rong;
        var kienThuc = rows[0]["kienThuc"].As<List<IDictionary<string, object>>>()
            .Select(x => new KienThucLienQuan(x["ma"].As<string>(), x["ten"].As<string>(),
                x.TryGetValue("bieuThuc", out var bt) ? bt?.As<string>() : null, x["lop"].As<int>()))
            .OrderBy(x => x.Lop).ThenBy(x => x.Ma, StringComparer.Ordinal).ToArray();
        var khaiNiem = rows[0]["khaiNiem"].As<List<IDictionary<string, object>>>()
            .Select(x => new KhaiNiemLienQuan(x["ma"].As<string>(), x["ten"].As<string>(), x["lop"].As<int>()))
            .OrderBy(x => x.Lop).ThenBy(x => x.Ma, StringComparer.Ordinal).ToArray();
        return new KienThucBaiTap(kienThuc, khaiNiem);
    }

    public async Task<BaiTapChiTiet?> ChiTietAsync(string ma, int lopHienThi)
    {
        var rows = await db.ReadAsync(CypherChiTiet, new { ma, lop = lopHienThi });
        if (rows.Count == 0) return null;
        var r = rows[0];
        return new BaiTapChiTiet(r["ma"].As<string>(), r["de"].As<string>(), r["loai"].As<string>(),
            r["lop"].As<int>(), r["doKho"].As<int>(), r["phuongAn"].As<List<string>>(),
            Optional<string>(r, "dapAnDung"), OptionalDecimal(r, "dapAnSo"),
            OptionalDecimal(r, "saiSo") ?? .01m, r["donVi"].As<string>(), r["giaiThich"].As<string>(),
            Optional<string>(r, "loiGiaiMau"), Optional<string>(r, "maChungMinh"));
    }

    public async Task<BanGhiLuotLam?> GhiNhanAsync(string taiKhoanId, int lopThuc, BaiTapChiTiet bai,
        string maLan, string payloadHash, string dapAn, string donViDaChon, bool dung, int seconds)
    {
        BanGhiLuotLam? saved = null;
        await db.WriteTransactionAsync(async tx =>
        {
            var lockRows = await (await tx.RunAsync(CypherLockAccount,
                new { tk = taiKhoanId, nonce = maLan })).ToListAsync();
            if (lockRows.Count == 0 || lockRows[0]["lop"].As<int>() != lopThuc) return;

            var rows = await (await tx.RunAsync(CypherGhiLan, new
            {
                tk = taiKhoanId, lopThuc, ma = bai.Ma, maLan, hash = payloadHash,
                dapAn, donVi = donViDaChon, dung, seconds
            })).ToListAsync();
            if (rows.Count == 0) return;
            var r = rows[0];
            saved = new BanGhiLuotLam(r["maLan"].As<string>(), r["payloadHash"].As<string>(),
                r["dapAn"].As<string>(), ChamBai.DapAnDungDangChuoi(bai.Loai, bai.DapAnDung, bai.DapAnSo),
                bai.DonVi, r["dung"].As<bool>(), r["seconds"].As<int>(), r["payloadHash"].As<string>() != payloadHash,
                r["maBaiTap"].As<string>());
        });
        return saved;
    }

    public async Task<bool> LaChungMinhCongKhaiAsync(string ma, int lopHienThi)
    {
        var rows = await db.ReadAsync(CypherProofPublic, new { ma, lop = lopHienThi });
        return rows.Count > 0 && rows[0]["ok"].As<bool>();
    }

    public async Task<BanGhiLuotLam?> KetQuaAsync(string taiKhoanId, string maLan)
    {
        var rows = await db.ReadAsync(CypherKetQua, new { tk = taiKhoanId, maLan });
        if (rows.Count == 0) return null;
        var r = rows[0];
        var key = ChamBai.DapAnDungDangChuoi(r["loai"].As<string>(), Optional<string>(r, "keyChoice"),
            OptionalDecimal(r, "keyNumber"));
        return new BanGhiLuotLam(r["maLan"].As<string>(), r["payloadHash"].As<string>(),
            r["dapAn"].As<string>(), key, r["donVi"].As<string>(), r["dung"].As<bool>(),
            r["seconds"].As<int>(), false, r["maBaiTap"].As<string>());
    }

    public async Task<string?> BaiTiepTheoKhachAsync(string maHienTai, int lopHienThi)
    {
        var rows = await db.ReadAsync(CypherBaiTiepTheo, new { ma = maHienTai, lop = lopHienThi });
        return rows.Count == 0 ? null : rows[0]["ma"].As<string>();
    }

    private static T? Optional<T>(IRecord r, string key) => r[key] is null ? default : r[key] is T value ? value : (T)Convert.ChangeType(r[key], typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    private static decimal? OptionalDecimal(IRecord r, string key) => r[key] is null ? null : Convert.ToDecimal(r[key], System.Globalization.CultureInfo.InvariantCulture);
}
