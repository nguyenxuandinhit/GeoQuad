using GeoQuad.Web.Infrastructure.Neo4j;
using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

/// <summary>Forward to real transaction, consume DELETE, then throw to test actual rollback.</summary>
public sealed class GraphDbGayLoi(IGraphDb inner) : IGraphDb
{
    public Task<IReadOnlyList<IRecord>> ReadAsync(string cypher,object? parameters=null) => inner.ReadAsync(cypher,parameters);
    public Task<IReadOnlyList<IRecord>> WriteAsync(string cypher,object? parameters=null) => inner.WriteAsync(cypher,parameters);
    public Task WriteTransactionAsync(Func<IAsyncQueryRunner,Task> work) => inner.WriteTransactionAsync(tx => work(new Runner(tx)));
    private sealed class Runner(IAsyncQueryRunner inner) : IAsyncQueryRunner
    {
        public void Dispose() => inner.Dispose();
        public ValueTask DisposeAsync() => inner.DisposeAsync();
        public Task<IResultCursor> RunAsync(string query) => CheckAsync(inner.RunAsync(query),query);
        public Task<IResultCursor> RunAsync(string query,object parameters) => CheckAsync(inner.RunAsync(query,parameters),query);
        public Task<IResultCursor> RunAsync(string query,IDictionary<string,object> parameters) => CheckAsync(inner.RunAsync(query,parameters),query);
        public Task<IResultCursor> RunAsync(Query query) => CheckAsync(inner.RunAsync(query),query.Text);
        private static async Task<IResultCursor> CheckAsync(Task<IResultCursor> call,string query)
        {
            var cursor=await call;
            if(query.Contains("DELETE r",StringComparison.Ordinal))
            {
                await cursor.ConsumeAsync();
                throw new InvalidOperationException("B test: lỗi sau khi đã đổi thuộc tính và xóa quan hệ nội dung.");
            }
            return cursor;
        }
    }
}
