using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.KienThuc.Repositories;

public interface IChiTietRepository
{
    /// <summary>Chi tiết một khái niệm; null nếu mã không tồn tại (FR-11, UC-03 — US-10).</summary>
    Task<ChiTietKhaiNiem?> LayAsync(string ma, int lop, string? taiKhoanId);

    /// <summary>Tình huống và bài tập liên quan (truy vấn riêng để không nhân dòng).</summary>
    Task<LienQuanKhaiNiem> LayLienQuanAsync(string ma, int lop);

    /// <summary>
    /// Ghi nhận "Em đã hiểu"; trả về false nếu không có tài khoản hoặc khái niệm đó.
    /// MERGE nên bấm hai lần chỉ sinh một quan hệ DA_HOC.
    /// </summary>
    Task<bool> GhiDaHocAsync(string taiKhoanId, string ma);
}

/// <summary>
/// Truy vấn trang chi tiết khái niệm. Câu Cypher là hằng <c>const string</c>,
/// mọi dữ liệu truyền qua tham số (NFR-05).
/// </summary>
public sealed class ChiTietRepository : IChiTietRepository
{
    // FR-11 / UC-03 / SCR-05 (US-10): định nghĩa, tính chất, dấu hiệu, công thức,
    // liên kết tổng quát/đặc biệt hơn; tất cả lọc theo lớp đang hiển thị (BR-04).
    private const string CypherChiTiet = """
        MATCH (k:KhaiNiem {ma: $ma})-[:THUOC_LOP]->(l:Lop)
        OPTIONAL MATCH (k)-[:CO_TINH_CHAT]->(t:TinhChat {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lt:Lop)
          WHERE lt.so <= $lop
        WITH k, l, collect(DISTINCT t {.ma, .noiDung, lop: lt.so}) AS tinhChat
        OPTIONAL MATCH (k)-[:CO_CONG_THUC]->(c:CongThuc {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lc:Lop)
          WHERE lc.so <= $lop
        WITH k, l, tinhChat, collect(DISTINCT c {.ma, .ten, .bieuThuc, lop: lc.so}) AS congThuc
        OPTIONAL MATCH (d:DauHieu {trangThai:'DA_RA_SOAT'})-[:KHANG_DINH]->(k)
        OPTIONAL MATCH (d)-[:THUOC_LOP]->(ld:Lop)
        WITH k, l, tinhChat, congThuc,
             [x IN collect(DISTINCT {ma: d.ma, noiDung: d.noiDung, lop: ld.so})
                WHERE x.ma IS NOT NULL AND x.lop <= $lop] AS dauHieu
        OPTIONAL MATCH (k)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(cha:KhaiNiem)
        OPTIONAL MATCH (con:KhaiNiem)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(k)
        RETURN k {.ma, .ten, .loai, .dinhNghia, .ghiChuTieuHoc} AS khaiNiem, l.so AS lop,
               tinhChat, congThuc, dauHieu,
               collect(DISTINCT cha {.ma, .ten}) AS tongQuatHon,
               collect(DISTINCT con {.ma, .ten}) AS dacBietHon,
               EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(k) } AS daHoc
        """;

    // US-10: tab "Ví dụ thực tế" và "Bài tập" — liên kết sang phần B và phần C.
    private const string CypherLienQuan = """
        MATCH (k:KhaiNiem {ma: $ma})
        OPTIONAL MATCH (th:TinhHuong {trangThai:'DA_RA_SOAT'})-[:LIEN_QUAN_DEN]->(k)
        WITH k, collect(DISTINCT th {.ma, .ten}) AS tinhHuong
        OPTIONAL MATCH (bt:BaiTap {hienThi: true})-[:LIEN_QUAN_DEN]->(k)
        OPTIONAL MATCH (bt)-[:THUOC_LOP]->(lb:Lop)
        WITH tinhHuong, [x IN collect(DISTINCT {ma: bt.ma, de: left(bt.de, 80), doKho: bt.doKho, lop: lb.so})
                         WHERE x.ma IS NOT NULL AND x.lop <= $lop] AS baiTap
        RETURN tinhHuong, baiTap[0..5] AS baiTap
        """;

    // US-10: nút "Em đã hiểu". MERGE nên bấm nhiều lần chỉ có một quan hệ DA_HOC.
    private const string CypherDaHoc = """
        MATCH (tk:TaiKhoan {id: $tk}), (k:KhaiNiem {ma: $ma})
        MERGE (tk)-[h:DA_HOC]->(k) ON CREATE SET h.luc = datetime()
        RETURN k.ma AS ma
        """;

    private readonly IGraphDb _db;

    public ChiTietRepository(IGraphDb db) => _db = db;

    public async Task<ChiTietKhaiNiem?> LayAsync(string ma, int lop, string? taiKhoanId)
    {
        var ban = await _db.ReadAsync(CypherChiTiet, new Dictionary<string, object?>
        {
            ["ma"] = ma,
            ["lop"] = lop,
            ["tk"] = taiKhoanId
        });

        if (ban.Count == 0)
        {
            return null;
        }

        var r = ban[0];
        var kn = Map(r["khaiNiem"]);

        return new ChiTietKhaiNiem(
            KhaiNiem: new KhaiNiemChiTiet(
                Chuoi(kn, "ma"), Chuoi(kn, "ten"), Chuoi(kn, "loai"),
                Chuoi(kn, "dinhNghia"), ChuoiHoacNull(kn, "ghiChuTieuHoc")),
            Lop: r["lop"].As<int>(),
            TinhChat: DanhSach(r["tinhChat"])
                .Select(m => new MucTinhChat(Chuoi(m, "ma"), Chuoi(m, "noiDung"), So(m, "lop")))
                .ToList(),
            CongThuc: DanhSach(r["congThuc"])
                .Select(m => new MucCongThuc(
                    Chuoi(m, "ma"), Chuoi(m, "ten"), ChuoiHoacNull(m, "bieuThuc"), So(m, "lop")))
                .ToList(),
            DauHieu: DanhSach(r["dauHieu"])
                .Select(m => new MucDauHieu(Chuoi(m, "ma"), Chuoi(m, "noiDung"), So(m, "lop")))
                .ToList(),
            TongQuatHon: DanhSach(r["tongQuatHon"])
                .Select(m => new LienKetKhaiNiem(Chuoi(m, "ma"), Chuoi(m, "ten")))
                .ToList(),
            DacBietHon: DanhSach(r["dacBietHon"])
                .Select(m => new LienKetKhaiNiem(Chuoi(m, "ma"), Chuoi(m, "ten")))
                .ToList(),
            DaHoc: r["daHoc"].As<bool>());
    }

    public async Task<LienQuanKhaiNiem> LayLienQuanAsync(string ma, int lop)
    {
        var ban = await _db.ReadAsync(CypherLienQuan, new { ma, lop });

        if (ban.Count == 0)
        {
            return new LienQuanKhaiNiem([], []);
        }

        var r = ban[0];
        return new LienQuanKhaiNiem(
            TinhHuong: DanhSach(r["tinhHuong"])
                .Select(m => new MucTinhHuong(Chuoi(m, "ma"), Chuoi(m, "ten")))
                .ToList(),
            BaiTap: DanhSach(r["baiTap"])
                .Select(m => new MucBaiTap(
                    Chuoi(m, "ma"), Chuoi(m, "de"), So(m, "doKho"), So(m, "lop")))
                .ToList());
    }

    public async Task<bool> GhiDaHocAsync(string taiKhoanId, string ma)
    {
        var ban = await _db.WriteAsync(CypherDaHoc, new { tk = taiKhoanId, ma });
        return ban.Count > 0;
    }

    // ---- Đọc map và danh sách map mà driver trả về ----

    private static IDictionary<string, object> Map(object? gia)
        => gia as IDictionary<string, object> ?? new Dictionary<string, object>();

    private static IEnumerable<IDictionary<string, object>> DanhSach(object? gia)
        => gia is IEnumerable<object> ds
            ? ds.OfType<IDictionary<string, object>>()
            : [];

    private static string Chuoi(IDictionary<string, object> m, string khoa)
        => m.TryGetValue(khoa, out var v) && v is not null ? v.ToString() ?? string.Empty : string.Empty;

    private static string? ChuoiHoacNull(IDictionary<string, object> m, string khoa)
        => m.TryGetValue(khoa, out var v) && v is not null ? v.ToString() : null;

    private static int So(IDictionary<string, object> m, string khoa)
        => m.TryGetValue(khoa, out var v) && v is not null ? Convert.ToInt32(v) : 0;
}
