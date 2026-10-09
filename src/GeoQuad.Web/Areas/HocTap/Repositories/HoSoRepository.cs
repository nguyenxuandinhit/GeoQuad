using GeoQuad.Web.Infrastructure.Auth;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.HocTap.Repositories;

/// <summary>
/// Thực thi các truy vấn Cypher về hồ sơ học sinh (US-08, FR-04, NFR-06).
/// Chỉ dùng tham số người dùng qua driver, không ghép chuỗi (NFR-05).
/// </summary>
public sealed class HoSoRepository : IHoSoRepository
{
    // FR-04 / UC-14 (US-08): đọc thông tin tài khoản hiện tại kèm quan hệ HOC_LOP.
    private const string CypherTimTheoId = """
        MATCH (tk:TaiKhoan {id: $tk})-[:HOC_LOP]->(l:Lop)
        RETURN tk.id AS id, tk.tenDangNhap AS tenDangNhap, tk.bietDanh AS bietDanh,
               tk.matKhauBam AS matKhauBam, tk.vaiTro AS vaiTro, l.so AS lop,
               tk.ngayTao AS ngayTao
        """;

    // FR-04 / UC-14 (US-08): đổi lớp và biệt danh, xóa quan hệ cũ và MERGE quan hệ HOC_LOP mới.
    private const string CypherCapNhatThongTin = """
        MATCH (tk:TaiKhoan {id: $tk}), (l:Lop {so: $lop})
        OPTIONAL MATCH (tk)-[r:HOC_LOP]->()
        DELETE r
        MERGE (tk)-[:HOC_LOP]->(l)
        SET tk.bietDanh = $bietDanh
        RETURN tk.id AS id, tk.bietDanh AS bietDanh, l.so AS lop
        """;

    // FR-04 / UC-14 (US-08): cập nhật mật khẩu đã băm.
    private const string CypherCapNhatMatKhau = """
        MATCH (tk:TaiKhoan {id: $tk})
        SET tk.matKhauBam = $bam
        RETURN tk.id AS id
        """;

    // NFR-06 / SCR-17 (US-08): xóa tài khoản và ngắt toàn bộ liên kết DA_LAM, DA_HOC, HOC_LOP.
    private const string CypherXoaTaiKhoan = """
        MATCH (tk:TaiKhoan {id: $tk})
        DETACH DELETE tk
        """;

    private readonly IGraphDb _db;

    public HoSoRepository(IGraphDb db) => _db = db;

    public async Task<TaiKhoanHoSoBanGhi?> TimTheoIdAsync(string id)
    {
        var records = await _db.ReadAsync(CypherTimTheoId, new { tk = id });
        if (records.Count == 0)
        {
            return null;
        }

        var r = records[0];
        return new TaiKhoanHoSoBanGhi(
            r["id"].As<string>(),
            r["tenDangNhap"].As<string>(),
            r["bietDanh"].As<string>(),
            r["matKhauBam"].As<string>(),
            r["vaiTro"].As<string>(),
            r["lop"].As<int>(),
            TaiKhoanRepository.DocThoiDiem(r["ngayTao"]));
    }

    public async Task<bool> CapNhatThongTinAsync(string id, string bietDanh, int lop)
    {
        var records = await _db.WriteAsync(CypherCapNhatThongTin, new
        {
            tk = id,
            bietDanh,
            lop
        });
        return records.Count > 0;
    }

    public async Task<bool> CapNhatMatKhauAsync(string id, string matKhauBam)
    {
        var records = await _db.WriteAsync(CypherCapNhatMatKhau, new
        {
            tk = id,
            bam = matKhauBam
        });
        return records.Count > 0;
    }

    public async Task<bool> XoaTaiKhoanAsync(string id)
    {
        await _db.WriteAsync(CypherXoaTaiKhoan, new { tk = id });
        return true;
    }
}
