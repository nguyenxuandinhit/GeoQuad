using System.Collections.Concurrent;
using System.Diagnostics;
using GeoQuad.Tests.B_ApDung.Http;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category","Integration")]
public sealed class PerformanceBTests(Neo4jFixture fixture)
{
    [Fact]
    public async Task GoiY100Concurrent1000RequestsP95AtMostTwoSeconds()
    {
        await fixture.SeedReviewedForTestsAsync();
        await using var server=await MayChuThu.StartAsync(fixture);
        const string route="/ApDung/GoiY?nen=HINH_BINH_HANH&dich=HINH_CHU_NHAT&co=DK_MOT_GOC_VUONG";
        for(var i=0;i<10;i++) { using var warm=await server.Client.GetAsync(route); Assert.True(warm.IsSuccessStatusCode); }
        var timings=new ConcurrentBag<double>(); var errors=0;
        var total=Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0,100).Select(async _ =>
        {
            for(var i=0;i<10;i++)
            {
                var watch=Stopwatch.StartNew();
                try
                {
                    using var response=await server.Client.GetAsync(route);
                    if(!response.IsSuccessStatusCode || !(await response.Content.ReadAsStringAsync()).Contains("DH_HCN_2",StringComparison.Ordinal)) Interlocked.Increment(ref errors);
                }
                catch(HttpRequestException) { Interlocked.Increment(ref errors); }
                catch(TaskCanceledException) { Interlocked.Increment(ref errors); }
                timings.Add(watch.Elapsed.TotalMilliseconds);
            }
        }));
        var sorted=timings.Order().ToArray(); var p95=sorted[(int)Math.Ceiling(.95*sorted.Length)-1];
        var report=new { date=DateTimeOffset.UtcNow,concurrency=100,requests=1000,warmup=10,errors,p95Ms=p95,
            totalSeconds=total.Elapsed.TotalSeconds,processorCount=Environment.ProcessorCount,os=Environment.OSVersion.ToString(),timingsMs=sorted };
        File.WriteAllText(Path.Combine(Neo4jFixture.Root,"plans/261008-0132-quan-b-ap-dung-tdd/reports/performance-B.json"),
            System.Text.Json.JsonSerializer.Serialize(report,new System.Text.Json.JsonSerializerOptions { WriteIndented=true }));
        Assert.Equal(1000,sorted.Length); Assert.Equal(0,errors); Assert.True(p95 <= 2000,$"p95={p95:0}ms, giới hạn 2000ms; xem performance-B.json.");
    }
}
