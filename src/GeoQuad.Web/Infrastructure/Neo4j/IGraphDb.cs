using Neo4j.Driver;

namespace GeoQuad.Web.Infrastructure.Neo4j;

/// <summary>
/// Lớp truy cập Neo4j dùng chung cho mọi phần (PHẦN 0 sở hữu, A/B/C chỉ gọi).
/// Mọi câu Cypher phải truyền dữ liệu qua <paramref name="parameters"/>, không nối chuỗi (NFR-05).
/// </summary>
public interface IGraphDb
{
    /// <summary>Chạy một câu Cypher chỉ đọc trong giao dịch đọc.</summary>
    Task<IReadOnlyList<IRecord>> ReadAsync(string cypher, object? parameters = null);

    /// <summary>Chạy một câu Cypher có ghi trong giao dịch ghi.</summary>
    Task<IReadOnlyList<IRecord>> WriteAsync(string cypher, object? parameters = null);

    /// <summary>Chạy nhiều câu Cypher trong cùng một giao dịch ghi.</summary>
    Task WriteTransactionAsync(Func<IAsyncQueryRunner, Task> work);
}
