namespace GeoQuad.Web.Infrastructure.Neo4j;

/// <summary>Cấu hình mục "Neo4j" trong appsettings.json.</summary>
public sealed class Neo4jOptions
{
    public const string Section = "Neo4j";

    public string Uri { get; set; } = "bolt://localhost:7687";
    public string User { get; set; } = "neo4j";
    public string Password { get; set; } = "geoquad123";
    public string Database { get; set; } = "neo4j";
}
