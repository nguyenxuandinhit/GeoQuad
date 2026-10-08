using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.KienThuc.Repositories;

public interface IBanDoRepository
{
    /// <summary>Các hình và quan hệ đặc biệt hoá, giới hạn theo lớp (FR-13, UC-05 — US-12).</summary>
    Task<IReadOnlyList<BanDoNut>> LayNutAsync(int lop, string? taiKhoanId);

    /// <summary>Mọi hình tổng quát hơn, đi nhiều bước (US-12).</summary>
    Task<IReadOnlyList<LienKetKhaiNiem>> LayTongQuatHonAsync(string ma);

    /// <summary>Chuỗi "Vì sao … là …?" theo đường đi ngắn nhất; rỗng nếu không có đường (US-12).</summary>
    Task<IReadOnlyList<string>> LayChuoiViSaoAsync(string tu, string den);
}

/// <summary>
/// Truy vấn bản đồ kiến thức. Câu Cypher là hằng <c>const string</c>,
/// mọi dữ liệu truyền qua tham số (NFR-05).
/// </summary>
public sealed class BanDoRepository : IBanDoRepository
{
    // FR-13 / UC-05 / SCR-07 (US-12): dữ liệu cho Cytoscape.js.
    // Quan hệ chỉ được giữ khi CẢ hình đích cũng nằm trong lớp đang xem, nhờ vậy
    // lọc theo lớp không để lại mũi tên trỏ vào nút không hiển thị.
    private const string CypherNut = """
        MATCH (a:KhaiNiem {loai:'HINH', trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(la:Lop)-[:THUOC_CAP]->(cap:CapHoc)
        WHERE la.so <= $lop
        OPTIONAL MATCH (a)-[:LA_TRUONG_HOP_DAC_BIET_CUA]->(b:KhaiNiem {trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(lb:Lop)
          WHERE lb.so <= $lop
        RETURN a.ma AS ma, a.ten AS ten, a.dinhNghia AS dinhNghia, la.so AS lop, cap.ma AS cap,
               collect(b.ma) AS laDacBietCua,
               EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(a) } AS daHoc
        ORDER BY lop, ten
        """;

    // US-12: mọi hình tổng quát hơn, đi từ 1 bước trở lên.
    private const string CypherTongQuatHon = """
        MATCH (:KhaiNiem {ma: $ma})-[:LA_TRUONG_HOP_DAC_BIET_CUA*1..]->(t:KhaiNiem)
        RETURN DISTINCT t.ma AS ma, t.ten AS ten
        """;

    // US-12: "Vì sao Hình vuông là Hình thang?" — đường đi ngắn nhất giữa hai hình.
    private const string CypherViSao = """
        MATCH p = shortestPath((a:KhaiNiem {ma: $tu})-[:LA_TRUONG_HOP_DAC_BIET_CUA*]->(b:KhaiNiem {ma: $den}))
        RETURN [n IN nodes(p) | n.ten] AS chuoi
        """;

    private readonly IGraphDb _db;

    public BanDoRepository(IGraphDb db) => _db = db;

    public async Task<IReadOnlyList<BanDoNut>> LayNutAsync(int lop, string? taiKhoanId)
    {
        var ban = await _db.ReadAsync(CypherNut, new Dictionary<string, object?>
        {
            ["lop"] = lop,
            ["tk"] = taiKhoanId
        });

        return ban.Select(r => new BanDoNut(
            Ma: r["ma"].As<string>(),
            Ten: r["ten"].As<string>(),
            DinhNghia: r["dinhNghia"].As<string>(),
            Lop: r["lop"].As<int>(),
            Cap: r["cap"].As<string>(),
            LaDacBietCua: r["laDacBietCua"].As<List<string>>(),
            DaHoc: r["daHoc"].As<bool>())).ToList();
    }

    public async Task<IReadOnlyList<LienKetKhaiNiem>> LayTongQuatHonAsync(string ma)
    {
        var ban = await _db.ReadAsync(CypherTongQuatHon, new { ma });

        return ban.Select(r => new LienKetKhaiNiem(r["ma"].As<string>(), r["ten"].As<string>()))
                  .ToList();
    }

    public async Task<IReadOnlyList<string>> LayChuoiViSaoAsync(string tu, string den)
    {
        var ban = await _db.ReadAsync(CypherViSao, new { tu, den });

        return ban.Count == 0 ? [] : ban[0]["chuoi"].As<List<string>>();
    }
}
