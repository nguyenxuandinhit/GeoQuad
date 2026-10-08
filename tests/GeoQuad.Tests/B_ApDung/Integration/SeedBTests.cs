using Neo4j.Driver;

namespace GeoQuad.Tests.B_ApDung.Integration;

[Collection(Neo4jCollection.Name)]
[Trait("Category", "Integration")]
public sealed class SeedBTests(Neo4jFixture fixture)
{
    private static readonly string[] Conditions = ["DK_MOT_CAP_CANH_DOI_SONG", "DK_CANH_DOI_SONG", "DK_CANH_DOI_BANG",
        "DK_MOT_CAP_SONG_SONG_BANG", "DK_GOC_DOI_BANG", "DK_CHEO_CAT_TRUNG_DIEM", "DK_BA_GOC_VUONG",
        "DK_MOT_GOC_VUONG", "DK_CHEO_BANG_NHAU", "DK_BON_CANH_BANG", "DK_HAI_CANH_KE_BANG",
        "DK_CHEO_VUONG_GOC", "DK_CHEO_PHAN_GIAC", "DK_HAI_GOC_KE_DAY_BANG"];
    private static readonly string[] Signs = ["DH_HT_1", "DH_HTC_1", "DH_HTC_2", "DH_HBH_1", "DH_HBH_2",
        "DH_HBH_3", "DH_HBH_4", "DH_HBH_5", "DH_HCN_1", "DH_HCN_2", "DH_HCN_3", "DH_THOI_1",
        "DH_THOI_2", "DH_THOI_3", "DH_THOI_4", "DH_HV_1", "DH_HV_2", "DH_HV_3", "DH_HV_4", "DH_HV_5"];

    [Fact]
    public async Task CatalogVaChungMinhDungPhuLuc()
    {
        await fixture.ResetAsync();
        await fixture.SeedAsync("10-kienthuc-A.cypher", "11-baitap-A.cypher", "12-tinhhuong-A.cypher",
            "20-dinhly-B.cypher", "21-chungminh-B.cypher");
        await AssertCatalogAsync();
        var expectedSigns = new[] {
            "DH_HT_1/TU_GIAC/HINH_THANG/DK_MOT_CAP_CANH_DOI_SONG", "DH_HTC_1/HINH_THANG/HINH_THANG_CAN/DK_HAI_GOC_KE_DAY_BANG",
            "DH_HTC_2/HINH_THANG/HINH_THANG_CAN/DK_CHEO_BANG_NHAU", "DH_HBH_1/TU_GIAC/HINH_BINH_HANH/DK_CANH_DOI_SONG",
            "DH_HBH_2/TU_GIAC/HINH_BINH_HANH/DK_CANH_DOI_BANG", "DH_HBH_3/TU_GIAC/HINH_BINH_HANH/DK_MOT_CAP_SONG_SONG_BANG",
            "DH_HBH_4/TU_GIAC/HINH_BINH_HANH/DK_GOC_DOI_BANG", "DH_HBH_5/TU_GIAC/HINH_BINH_HANH/DK_CHEO_CAT_TRUNG_DIEM",
            "DH_HCN_1/TU_GIAC/HINH_CHU_NHAT/DK_BA_GOC_VUONG", "DH_HCN_2/HINH_BINH_HANH/HINH_CHU_NHAT/DK_MOT_GOC_VUONG",
            "DH_HCN_3/HINH_BINH_HANH/HINH_CHU_NHAT/DK_CHEO_BANG_NHAU", "DH_THOI_1/TU_GIAC/HINH_THOI/DK_BON_CANH_BANG",
            "DH_THOI_2/HINH_BINH_HANH/HINH_THOI/DK_HAI_CANH_KE_BANG", "DH_THOI_3/HINH_BINH_HANH/HINH_THOI/DK_CHEO_VUONG_GOC",
            "DH_THOI_4/HINH_BINH_HANH/HINH_THOI/DK_CHEO_PHAN_GIAC", "DH_HV_1/HINH_CHU_NHAT/HINH_VUONG/DK_HAI_CANH_KE_BANG",
            "DH_HV_2/HINH_CHU_NHAT/HINH_VUONG/DK_CHEO_VUONG_GOC", "DH_HV_3/HINH_CHU_NHAT/HINH_VUONG/DK_CHEO_PHAN_GIAC",
            "DH_HV_4/HINH_THOI/HINH_VUONG/DK_MOT_GOC_VUONG", "DH_HV_5/HINH_THOI/HINH_VUONG/DK_CHEO_BANG_NHAU" };
        var actualSigns = await fixture.Db.ReadAsync("""
            MATCH (d:DauHieu)-[:YEU_CAU_LA]->(nen:KhaiNiem),
              (d)-[:KHANG_DINH]->(dich:KhaiNiem), (d)-[:YEU_CAU_CO]->(dk:DieuKien), (d)-[:THUOC_LOP]->(l:Lop)
            RETURN d.ma + '/' + nen.ma + '/' + dich.ma + '/' + dk.ma AS key, l.so AS lop
            ORDER BY key
            """);
        Assert.Equal(expectedSigns.Order(StringComparer.Ordinal), actualSigns.Select(r => r["key"].As<string>()));
        Assert.All(actualSigns, r => Assert.Equal(8, r["lop"].As<int>()));
        var errors = await fixture.Db.ReadAsync("""
            MATCH (d:DauHieu)
            WHERE d.noiDung IS NULL OR trim(d.noiDung) = '' OR d.nguon IS NULL OR
              d.trangThai IS NULL OR NOT d.trangThai IN ['NHAP','DA_RA_SOAT'] OR
              COUNT { (d)-[:YEU_CAU_LA]->(:KhaiNiem) } <> 1 OR
              COUNT { (d)-[:KHANG_DINH]->(:KhaiNiem) } <> 1 OR
              COUNT { (d)-[:THUOC_LOP]->(:Lop) } <> 1 OR
              COUNT { (d)-[:YEU_CAU_CO]->(:DieuKien) } < 1
            RETURN d.ma AS ma
            """);
        Assert.Empty(errors);
        var foundations = await fixture.Db.ReadAsync("""
            MATCH (d:DinhLy) WHERE d.ma STARTS WITH 'DL_NEN_'
            RETURN d.ma AS ma, d.noiDung AS content, d.nguon AS source,
              COUNT { (d)-[:THUOC_LOP]->(:Lop) } AS grades, labels(d) AS labels
            ORDER BY ma
            """);
        Assert.All(foundations, r => {
            Assert.False(string.IsNullOrWhiteSpace(r["content"].As<string>()));
            Assert.False(string.IsNullOrWhiteSpace(r["source"].As<string>()));
            Assert.Equal(1, r["grades"].As<int>());
            Assert.Equal(new[] { "DinhLy" }, r["labels"].As<List<string>>());
        });
        var proofs = await fixture.Db.ReadAsync("""
            MATCH (cm:ChungMinh)
            OPTIONAL MATCH (cm)-[r:CO_BUOC]->(b:Buoc)
            WITH cm,r,b ORDER BY r.thuTu
            RETURN cm.ma AS ma, cm.giaThiet AS giaThiet, cm.ketLuan AS ketLuan,
              collect(r.thuTu) AS orders, collect(b.noiDung) AS contents,
              COUNT { (cm)-[:CHUNG_MINH_CHO]->(:DauHieu)-[:KHANG_DINH]->(:KhaiNiem) } AS concepts,
              collect(COUNT { (b)-[:CAN_CU]->(:DinhLy) }) AS grounds
            ORDER BY ma
            """);
        Assert.Equal(new[] { "CM-01", "CM-02", "CM-03", "CM-04" }, proofs.Select(r => r["ma"].As<string>()));
        foreach (var proof in proofs)
        {
            Assert.False(string.IsNullOrWhiteSpace(proof["giaThiet"].As<string>()));
            Assert.False(string.IsNullOrWhiteSpace(proof["ketLuan"].As<string>()));
            var orders = proof["orders"].As<List<int>>();
            Assert.InRange(orders.Count, 3, 6);
            Assert.Equal(Enumerable.Range(1, orders.Count), orders);
            Assert.All(proof["contents"].As<List<string>>(), s => Assert.False(string.IsNullOrWhiteSpace(s)));
            Assert.All(proof["grounds"].As<List<int>>(), n => Assert.True(n > 0));
            Assert.Equal(1, proof["concepts"].As<int>());
        }
        var grounds = await fixture.Db.ReadAsync("""
            MATCH (c:ChungMinh)-[:CO_BUOC]->(:Buoc)-[:CAN_CU]->(d:DinhLy)
            WITH DISTINCT c.ma AS proof,d.ma AS ground ORDER BY proof,ground
            RETURN proof,collect(ground) AS grounds ORDER BY proof
            """);
        Assert.Equal(new[] { "DL_NEN_5,TC_HBH_2", "DL_NEN_2,DL_NEN_5,TC_HBH_1",
            "DL_NEN_2,TC_HBH_3", "DL_NEN_2,DL_NEN_5" },
            grounds.Select(r => string.Join(",", r["grounds"].As<List<string>>())));
        Assert.Empty(await fixture.Db.ReadAsync("""
            MATCH (n) WHERE (n:DauHieu OR n:ChungMinh OR (n:DinhLy AND n.ma STARTS WITH 'DL_NEN_'))
            AND (n.trangThai IS NULL OR n.trangThai <> 'NHAP') RETURN n.ma
            """));
    }

    [Fact]
    public async Task SeedLapVaDaoThuTuGiuNguyenGraph()
    {
        await fixture.ResetAsync();
        await fixture.SeedAsync("10-kienthuc-A.cypher", "11-baitap-A.cypher", "12-tinhhuong-A.cypher", "20-dinhly-B.cypher", "21-chungminh-B.cypher");
        var first = await SnapshotAsync();
        await fixture.SeedAsync("10-kienthuc-A.cypher", "11-baitap-A.cypher", "12-tinhhuong-A.cypher", "20-dinhly-B.cypher", "21-chungminh-B.cypher");
        Assert.Equal(first, await SnapshotAsync());
        await fixture.ResetAsync();
        await fixture.SeedAsync("20-dinhly-B.cypher", "21-chungminh-B.cypher", "10-kienthuc-A.cypher", "11-baitap-A.cypher", "12-tinhhuong-A.cypher");
        Assert.Equal(first, await SnapshotAsync());
        await AssertCatalogAsync();
    }

    [Fact]
    public async Task SchemaVaKhungThatDayDu()
    {
        await fixture.ResetAsync();
        var constraints = await fixture.Db.ReadAsync("SHOW CONSTRAINTS YIELD type RETURN type");
        Assert.Equal(13, constraints.Count(r => r["type"].As<string>() == "UNIQUENESS"));
        var indexes = await fixture.Db.ReadAsync("SHOW INDEXES YIELD name, type RETURN name, type");
        Assert.Contains(indexes, r => r["name"].As<string>() == "kienThucTimKiem" && r["type"].As<string>() == "FULLTEXT");
        var counts = (await fixture.Db.ReadAsync("""
            RETURN COUNT { (:CapHoc) } AS cap, COUNT { (:Lop) } AS lop,
              COUNT { (:KhaiNiem {loai:'HINH'}) } AS hinh, COUNT { (:KhaiNiem {loai:'YEU_TO'}) } AS yeuto,
              COUNT { ()-[:LA_TRUONG_HOP_DAC_BIET_CUA]->() } AS dacbiet
            """)).Single();
        Assert.Equal(new[] { 3, 12, 7, 6, 7 }, new[] { "cap", "lop", "hinh", "yeuto", "dacbiet" }.Select(k => counts[k].As<int>()));
    }

    private async Task AssertCatalogAsync()
    {
        var rows = await fixture.Db.ReadAsync("MATCH (n:DieuKien) RETURN n.ma AS ma ORDER BY ma");
        Assert.Equal(Conditions.Order(StringComparer.Ordinal), rows.Select(r => r["ma"].As<string>()));
        rows = await fixture.Db.ReadAsync("MATCH (n:DauHieu) RETURN n.ma AS ma ORDER BY ma");
        Assert.Equal(Signs.Order(StringComparer.Ordinal), rows.Select(r => r["ma"].As<string>()));
        rows = await fixture.Db.ReadAsync("MATCH (n:DinhLy) WHERE n.ma STARTS WITH 'DL_NEN_' RETURN n.ma AS ma ORDER BY ma");
        Assert.Equal(Enumerable.Range(1, 6).Select(n => "DL_NEN_" + n), rows.Select(r => r["ma"].As<string>()));
    }

    private async Task<string[]> SnapshotAsync()
    {
        // Stable semantic snapshot includes labels, all properties and edge properties, not internal IDs.
        var nodes = await fixture.Db.ReadAsync("MATCH (n) RETURN labels(n) AS labels, properties(n) AS props");
        var edges = await fixture.Db.ReadAsync("""
            MATCH (a)-[r]->(b) RETURN coalesce(a.ma,toString(a.so),a.id) AS a,
              type(r) AS type, coalesce(b.ma,toString(b.so),b.id) AS b, properties(r) AS props
            """);
        string Properties(object value) => System.Text.Json.JsonSerializer.Serialize(value.As<Dictionary<string, object>>().OrderBy(p => p.Key, StringComparer.Ordinal));
        return nodes.Select(r => string.Join(",", r["labels"].As<List<string>>().Order(StringComparer.Ordinal)) + Properties(r["props"]))
            .Concat(edges.Select(r => $"{r["a"]}/{r["type"]}/{r["b"]}/{Properties(r["props"])}")).Order(StringComparer.Ordinal).ToArray();
    }
}
