using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.KienThuc.Repositories;

public interface IThuVienRepository
{
    /// <summary>Duyệt nội dung thư viện kiến thức (FR-10, UC-03 — US-09).</summary>
    Task<IReadOnlyList<NoiDungThuVien>> DuyetAsync(
        int lop, string? loai, string? cap, int? lopLoc, string? taiKhoanId);
}

/// <summary>
/// Truy vấn thư viện kiến thức. Câu Cypher là hằng <c>const string</c>, mọi dữ liệu
/// truyền qua tham số (NFR-05).
/// </summary>
public sealed class ThuVienRepository : IThuVienRepository
{
    // FR-10 / UC-03 / SCR-04 (US-09): lấy hình, tính chất, dấu hiệu và công thức
    // đã rà soát, giới hạn theo lớp đang hiển thị, kèm bộ lọc loại / cấp / lớp.
    private const string CypherDuyet = """
        MATCH (n)-[:THUOC_LOP]->(l:Lop)-[:THUOC_CAP]->(cap:CapHoc)
        WHERE ((n:KhaiNiem AND n.loai = 'HINH') OR n:TinhChat OR n:DauHieu OR n:CongThuc)
          AND n.trangThai = 'DA_RA_SOAT' AND l.so <= $lop
          AND ($lopLoc IS NULL OR l.so = $lopLoc)
          AND ($capLoc IS NULL OR cap.ma = $capLoc)
          AND ($loai IS NULL
               OR ($loai = 'HINH' AND n:KhaiNiem) OR ($loai = 'TINH_CHAT' AND n:TinhChat)
               OR ($loai = 'DAU_HIEU' AND n:DauHieu) OR ($loai = 'CONG_THUC' AND n:CongThuc))
        OPTIONAL MATCH (h:KhaiNiem)-[:CO_TINH_CHAT|CO_CONG_THUC]->(n)
        OPTIONAL MATCH (n)-[:KHANG_DINH]->(dich:KhaiNiem)
        RETURN n.ma AS ma, coalesce(n.ten, n.noiDung) AS tieuDe, n.bieuThuc AS bieuThuc,
               [x IN labels(n) WHERE x IN ['KhaiNiem','TinhChat','DauHieu','CongThuc']][0] AS loai,
               l.so AS lop, cap.ma AS cap,
               coalesce(CASE WHEN n:KhaiNiem THEN n.ma END, h.ma, dich.ma) AS maHinh,
               EXISTS { MATCH (:TaiKhoan {id: $tk})-[:DA_HOC]->(n) } AS daHoc
        ORDER BY lop, loai, tieuDe
        """;

    private readonly IGraphDb _db;

    public ThuVienRepository(IGraphDb db) => _db = db;

    public async Task<IReadOnlyList<NoiDungThuVien>> DuyetAsync(
        int lop, string? loai, string? cap, int? lopLoc, string? taiKhoanId)
    {
        var ban = await _db.ReadAsync(CypherDuyet, new Dictionary<string, object?>
        {
            ["lop"] = lop,
            ["loai"] = loai,
            ["capLoc"] = cap,
            ["lopLoc"] = lopLoc,
            // Khách: null nên EXISTS không khớp tài khoản nào, daHoc luôn false (BR-09).
            ["tk"] = taiKhoanId
        });

        return ban.Select(Doc).ToList();
    }

    private static NoiDungThuVien Doc(IRecord r) => new(
        Ma: r["ma"].As<string>(),
        TieuDe: r["tieuDe"].As<string>(),
        BieuThuc: r["bieuThuc"]?.As<string>(),
        // Nhãn phụ TinhChat/DauHieu đứng trước nhãn gốc DinhLy nên danh sách lọc cho đúng loại.
        Loai: r["loai"].As<string>(),
        Lop: r["lop"].As<int>(),
        Cap: r["cap"].As<string>(),
        MaHinh: r["maHinh"]?.As<string>(),
        DaHoc: r["daHoc"].As<bool>());
}
