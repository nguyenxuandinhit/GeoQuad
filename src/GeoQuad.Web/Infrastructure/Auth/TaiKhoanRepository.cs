using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Infrastructure.Auth;

/// <summary>Bản ghi tài khoản đọc từ Neo4j (dùng cho đăng nhập — US-06).</summary>
public sealed record TaiKhoanBanGhi(
    string Id,
    string TenDangNhap,
    string MatKhauBam,
    string BietDanh,
    string VaiTro,
    int Lop,
    int SoLanSai,
    DateTimeOffset? KhoaDen);

public interface ITaiKhoanRepository
{
    /// <summary>Tạo tài khoản học sinh mới (FR-01, UC-01).</summary>
    Task<string?> TaoAsync(string tenDangNhap, string matKhauBam, string bietDanh, int lop);

    /// <summary>Tìm tài khoản theo tên đăng nhập; null nếu không có (FR-02).</summary>
    Task<TaiKhoanBanGhi?> TimTheoTenAsync(string tenDangNhap);

    /// <summary>
    /// Ghi nhận một lần đăng nhập sai; tới ngưỡng thì đặt <c>khoaDen</c> (US-06).
    /// Trả về số lần sai mới và thời điểm hết khóa.
    /// </summary>
    Task<(int SoLanSai, DateTimeOffset? KhoaDen)> GhiNhanSaiAsync(string tenDangNhap);

    /// <summary>Đặt lại <c>soLanSai = 0</c> và bỏ khóa sau khi đăng nhập đúng (US-06).</summary>
    Task DatLaiSoLanSaiAsync(string tenDangNhap);

    /// <summary>Tạo tài khoản demo nếu chưa có, không ghi đè tài khoản đang có (US-06).</summary>
    Task TaoNeuChuaCoAsync(string tenDangNhap, string matKhauBam, string bietDanh, string vaiTro, int lop);
}

/// <summary>
/// Truy cập nút <c>TaiKhoan</c>. PHẦN 0 sở hữu; A, B, C gọi lại qua giao diện.
/// Mọi câu Cypher là hằng <c>const string</c> và chỉ truyền dữ liệu qua tham số (NFR-05).
/// </summary>
public sealed class TaiKhoanRepository : ITaiKhoanRepository
{
    // FR-01 / UC-01 (US-05): tạo tài khoản và gắn đúng một quan hệ HOC_LOP (BR-03).
    private const string CypherTao = """
        MATCH (l:Lop {so: $lop})
        CREATE (tk:TaiKhoan {id: randomUUID(), tenDangNhap: $ten, matKhauBam: $bam,
                             bietDanh: $bietDanh, vaiTro: 'HOC_SINH', ngayTao: datetime(), soLanSai: 0})
        CREATE (tk)-[:HOC_LOP]->(l)
        RETURN tk.id AS id
        """;

    // FR-02 / UC-02 (US-06): đọc tài khoản để đối chiếu mật khẩu và trạng thái khóa.
    private const string CypherTimTheoTen = """
        MATCH (tk:TaiKhoan {tenDangNhap: $ten})-[:HOC_LOP]->(l:Lop)
        RETURN tk.id AS id, tk.tenDangNhap AS tenDangNhap, tk.matKhauBam AS matKhauBam,
               tk.bietDanh AS bietDanh, tk.vaiTro AS vaiTro, l.so AS lop,
               coalesce(tk.soLanSai, 0) AS soLanSai, tk.khoaDen AS khoaDen
        """;

    // US-06: đếm số lần sai liên tiếp và khóa tạm thời. Nếu lần khóa trước đã hết hạn
    // thì đếm lại từ đầu.
    private const string CypherGhiNhanSai = """
        MATCH (tk:TaiKhoan {tenDangNhap: $ten})
        WITH tk, CASE
                   WHEN tk.khoaDen IS NOT NULL AND tk.khoaDen <= datetime() THEN 0
                   ELSE coalesce(tk.soLanSai, 0)
                 END AS truoc
        SET tk.soLanSai = truoc + 1,
            tk.khoaDen  = CASE
                            WHEN truoc + 1 >= $soLanToiDa
                              THEN datetime() + duration({minutes: $phutKhoa})
                            ELSE null
                          END
        RETURN tk.soLanSai AS soLanSai, tk.khoaDen AS khoaDen
        """;

    // US-06: đăng nhập đúng thì xóa bộ đếm sai.
    private const string CypherDatLaiSoLanSai = """
        MATCH (tk:TaiKhoan {tenDangNhap: $ten})
        SET tk.soLanSai = 0, tk.khoaDen = null
        """;

    // US-06: tài khoản demo cho môi trường Development; ON CREATE nên chạy lại không ghi đè.
    private const string CypherTaoNeuChuaCo = """
        MATCH (l:Lop {so: $lop})
        MERGE (tk:TaiKhoan {tenDangNhap: $ten})
          ON CREATE SET tk.id = randomUUID(), tk.matKhauBam = $bam, tk.bietDanh = $bietDanh,
                        tk.vaiTro = $vaiTro, tk.ngayTao = datetime(), tk.soLanSai = 0
        MERGE (tk)-[:HOC_LOP]->(l)
        """;

    private readonly IGraphDb _db;

    public TaiKhoanRepository(IGraphDb db) => _db = db;

    public async Task<string?> TaoAsync(string tenDangNhap, string matKhauBam, string bietDanh, int lop)
    {
        var ban = await _db.WriteAsync(CypherTao, new
        {
            ten = tenDangNhap,
            bam = matKhauBam,
            bietDanh,
            lop
        });

        // Không có dòng nào nghĩa là chưa có nút Lop tương ứng (chưa chạy seed).
        return ban.Count > 0 ? ban[0]["id"].As<string>() : null;
    }

    public async Task<TaiKhoanBanGhi?> TimTheoTenAsync(string tenDangNhap)
    {
        var ban = await _db.ReadAsync(CypherTimTheoTen, new { ten = tenDangNhap });
        if (ban.Count == 0)
        {
            return null;
        }

        var r = ban[0];
        return new TaiKhoanBanGhi(
            r["id"].As<string>(),
            r["tenDangNhap"].As<string>(),
            r["matKhauBam"].As<string>(),
            r["bietDanh"].As<string>(),
            r["vaiTro"].As<string>(),
            r["lop"].As<int>(),
            r["soLanSai"].As<int>(),
            DocThoiDiem(r["khoaDen"]));
    }

    public async Task<(int SoLanSai, DateTimeOffset? KhoaDen)> GhiNhanSaiAsync(string tenDangNhap)
    {
        var ban = await _db.WriteAsync(CypherGhiNhanSai, new
        {
            ten = tenDangNhap,
            soLanToiDa = QuyTacTaiKhoan.SoLanSaiToiDa,
            phutKhoa = (int)QuyTacTaiKhoan.ThoiGianKhoa.TotalMinutes
        });

        if (ban.Count == 0)
        {
            return (0, null);
        }

        return (ban[0]["soLanSai"].As<int>(), DocThoiDiem(ban[0]["khoaDen"]));
    }

    public Task DatLaiSoLanSaiAsync(string tenDangNhap)
        => _db.WriteAsync(CypherDatLaiSoLanSai, new { ten = tenDangNhap });

    public Task TaoNeuChuaCoAsync(
        string tenDangNhap, string matKhauBam, string bietDanh, string vaiTro, int lop)
        => _db.WriteAsync(CypherTaoNeuChuaCo, new
        {
            ten = tenDangNhap,
            bam = matKhauBam,
            bietDanh,
            vaiTro,
            lop
        });

    /// <summary>Đổi <c>datetime</c> của Neo4j sang <see cref="DateTimeOffset"/>.</summary>
    internal static DateTimeOffset? DocThoiDiem(object? gia)
        => gia switch
        {
            null => null,
            ZonedDateTime z => z.ToDateTimeOffset(),
            LocalDateTime l => new DateTimeOffset(l.ToDateTime(), TimeSpan.Zero),
            DateTimeOffset d => d,
            DateTime dt => new DateTimeOffset(dt),
            _ => null
        };
}
