using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace GeoQuad.Web.Infrastructure.Neo4j;

/// <summary>Cài đặt <see cref="IGraphDb"/> trên <see cref="IDriver"/> của Neo4j.Driver 5.x.</summary>
public sealed class GraphDb : IGraphDb
{
    private readonly IDriver _driver;
    private readonly string _database;

    public GraphDb(IDriver driver, IOptions<Neo4jOptions> options)
    {
        _driver = driver;
        _database = options.Value.Database;
    }

    public async Task<IReadOnlyList<IRecord>> ReadAsync(string cypher, object? parameters = null)
    {
        await using var session = _driver.AsyncSession(c => c.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(cypher, ThamSo(parameters));
            return (IReadOnlyList<IRecord>)await cursor.ToListAsync();
        });
    }

    public async Task<IReadOnlyList<IRecord>> WriteAsync(string cypher, object? parameters = null)
    {
        await using var session = _driver.AsyncSession(c => c.WithDatabase(_database));
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(cypher, ThamSo(parameters));
            return (IReadOnlyList<IRecord>)await cursor.ToListAsync();
        });
    }

    public async Task WriteTransactionAsync(Func<IAsyncQueryRunner, Task> work)
    {
        await using var session = _driver.AsyncSession(c => c.WithDatabase(_database));
        await session.ExecuteWriteAsync(async tx => await work(tx));
    }

    /// <summary>Chuẩn hoá tham số: driver nhận <c>IDictionary</c> hoặc đối tượng vô danh.</summary>
    private static IDictionary<string, object> ThamSo(object? parameters) => parameters switch
    {
        null => new Dictionary<string, object>(),
        IDictionary<string, object> d => d,
        _ => parameters.GetType()
                       .GetProperties()
                       .ToDictionary(p => p.Name, p => p.GetValue(parameters)!)
    };
}
