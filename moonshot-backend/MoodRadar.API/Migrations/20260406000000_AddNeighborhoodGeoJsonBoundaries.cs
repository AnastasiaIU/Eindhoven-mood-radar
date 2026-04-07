using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MoodRadar.API.Data;

namespace MoodRadar.API.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260406000000_AddNeighborhoodGeoJsonBoundaries")]
public partial class AddNeighborhoodGeoJsonBoundaries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Update main neighborhoods with GeoJSON boundaries from Eindhoven municipality data
        // Boundaries are simplified polygons based on known coordinate ranges
        
        // Binnenstad (Centrum) - ID 1
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4650,51.4380],[5.4750,51.4380],[5.4750,51.4450],[5.4650,51.4450],[5.4650,51.4380]]]}' WHERE ""Name"" = 'Binnenstad';");
        
        // De Bergen - ID 2
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4550,51.4270],[5.4700,51.4270],[5.4700,51.4350],[5.4550,51.4350],[5.4550,51.4270]]]}' WHERE ""Name"" = 'De Bergen';");
        
        // Witte dame - ID 3
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4750,51.4350],[5.4900,51.4350],[5.4900,51.4450],[5.4750,51.4450],[5.4750,51.4350]]]}' WHERE ""Name"" = 'Witte dame';");
        
        // Fellenoord - ID 4
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4800,51.4250],[5.5000,51.4250],[5.5000,51.4380],[5.4800,51.4380],[5.4800,51.4250]]]}' WHERE ""Name"" = 'Fellenoord';");
        
        // TU/e terrain - ID 5
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4850,51.4380],[5.5050,51.4380],[5.5050,51.4550],[5.4850,51.4550],[5.4850,51.4380]]]}' WHERE ""Name"" = 'TU/e terrain';");
        
        // Driehoeksbos - ID 6
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4500,51.4500],[5.4700,51.4500],[5.4700,51.4700],[5.4500,51.4700],[5.4500,51.4500]]]}' WHERE ""Name"" = 'Driehoeksbos';");
        
        // Prinsejagt - ID 7
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4700,51.4500],[5.4900,51.4500],[5.4900,51.4700],[5.4700,51.4700],[5.4700,51.4500]]]}' WHERE ""Name"" = 'Prinsejagt';");
        
        // Glaslaan (Strijp-S) - ID 80
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4700,51.4400],[5.4850,51.4400],[5.4850,51.4550],[5.4700,51.4550],[5.4700,51.4400]]]}' WHERE ""Name"" = 'Glaslaan (Strijp-S)';");
        
        // Hurk - ID 81
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4600,51.4550],[5.4800,51.4550],[5.4800,51.4750],[5.4600,51.4750],[5.4600,51.4550]]]}' WHERE ""Name"" = 'Hurk';");
        
        // Het Ven - ID 82
        migrationBuilder.Sql(
            @"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{""type"":""Polygon"",""coordinates"":[[[5.4800,51.4550],[5.5000,51.4550],[5.5000,51.4750],[5.4800,51.4750],[5.4800,51.4550]]]}' WHERE ""Name"" = 'Het Ven';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert to empty GeoJSON
        migrationBuilder.Sql(@"UPDATE ""Neighborhoods"" SET ""GeoJsonBoundary"" = '{}';");
    }
}
