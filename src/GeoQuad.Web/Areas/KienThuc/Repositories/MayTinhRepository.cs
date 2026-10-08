using GeoQuad.Web.Areas.KienThuc.Models;
using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Web.Areas.KienThuc.Repositories;

public interface IMayTinhRepository
{
    /// <summary>Các hình chọn được ở máy tính hình học, giới hạn theo lớp (US-15).</summary>
    Task<IReadOnlyList<LienKetKhaiNiem>> LayCacHinhAsync(int lop, IReadOnlyList<string> maChoPhep);

    /// <summary>
    /// Đại lượng và công thức cho một hình: công thức của chính hình đó, hoặc của
    /// hình tổng quát **gần nhất** (FR-30 — US-15).
    /// </summary>
    Task<IReadOnlyList<DaiLuongTinh>> LayDaiLuongAsync(string hinh, int lop);
}

/// <summary>
/// Truy vấn máy tính hình học. Danh sách đại lượng và công thức lấy từ Neo4j,
/// không ghi cứng trong mã (yêu cầu của US-15).
/// </summary>
public sealed class MayTinhRepository : IMayTinhRepository
{
    // US-15: các hình mà máy tính hỗ trợ, lọc theo lớp đang hiển thị (BR-04).
    private const string CypherCacHinh = """
        MATCH (h:KhaiNiem {loai:'HINH', trangThai:'DA_RA_SOAT'})-[:THUOC_LOP]->(l:Lop)
        WHERE l.so <= $lop AND h.ma IN $maChoPhep
        RETURN h.ma AS ma, h.ten AS ten
        ORDER BY l.so, h.ten
        """;

    // FR-30 / UC-08 / SCR-10 (US-15): đi ngược quan hệ đặc biệt hoá từ 0 bước trở lên,
    // sắp theo khoảng cách rồi lấy công thức GẦN NHẤT cho mỗi đại lượng — nhờ vậy
    // hình vuông dùng chu vi hình vuông chứ không dùng chu vi tứ giác.
    private const string CypherDaiLuong = """
        MATCH p = (h:KhaiNiem {ma: $hinh})-[:LA_TRUONG_HOP_DAC_BIET_CUA*0..]->(g:KhaiNiem)
                  -[:CO_CONG_THUC]->(c:CongThuc {trangThai:'DA_RA_SOAT'})
        MATCH (c)-[:THUOC_LOP]->(l:Lop) WHERE l.so <= $lop
        WITH c, g, length(p) AS khoangCach ORDER BY khoangCach
        WITH c.daiLuong AS daiLuong,
             head(collect({ma: c.ma, ten: c.ten, bieuThuc: c.bieuThuc, bienSo: c.bienSo, tuHinh: g.ten})) AS congThuc
        RETURN daiLuong, congThuc
        ORDER BY daiLuong
        """;

    private readonly IGraphDb _db;

    public MayTinhRepository(IGraphDb db) => _db = db;

    public async Task<IReadOnlyList<LienKetKhaiNiem>> LayCacHinhAsync(
        int lop, IReadOnlyList<string> maChoPhep)
    {
        var ban = await _db.ReadAsync(CypherCacHinh, new { lop, maChoPhep });

        return ban.Select(r => new LienKetKhaiNiem(r["ma"].As<string>(), r["ten"].As<string>()))
                  .ToList();
    }

    public async Task<IReadOnlyList<DaiLuongTinh>> LayDaiLuongAsync(string hinh, int lop)
    {
        var ban = await _db.ReadAsync(CypherDaiLuong, new { hinh, lop });

        return ban.Select(r =>
        {
            var ct = r["congThuc"] as IDictionary<string, object> ?? new Dictionary<string, object>();

            return new DaiLuongTinh(
                DaiLuong: r["daiLuong"].As<string>(),
                MaCongThuc: Chuoi(ct, "ma"),
                Ten: Chuoi(ct, "ten"),
                BieuThuc: Chuoi(ct, "bieuThuc"),
                BienSo: DanhSachChuoi(ct, "bienSo"),
                TuHinh: Chuoi(ct, "tuHinh"));
        }).ToList();
    }

    private static string Chuoi(IDictionary<string, object> m, string khoa)
        => m.TryGetValue(khoa, out var v) && v is not null ? v.ToString() ?? string.Empty : string.Empty;

    private static IReadOnlyList<string> DanhSachChuoi(IDictionary<string, object> m, string khoa)
        => m.TryGetValue(khoa, out var v) && v is IEnumerable<object> ds
            ? ds.Select(x => x?.ToString() ?? string.Empty).ToList()
            : [];
}
