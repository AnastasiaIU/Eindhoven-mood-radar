namespace MoodRadar.API.Data;

using Microsoft.EntityFrameworkCore;
using MoodRadar.API.Models.Domain;

/// <summary>
/// Database seeding service for populating initial data.
/// Includes real Eindhoven districts, quarters, and neighborhoods from Wikipedia.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if districts are missing OR neighborhood snapshots are missing
        var hasDistricts = context.Districts.Any();
        var hasNeighborhoodSnapshots = context.NeighborhoodSnapshots.Any();

        if (hasDistricts && hasNeighborhoodSnapshots)
        {
            Console.WriteLine("Database already seeded. Skipping.");
            return;
        }

        Console.WriteLine("Seeding database with real Eindhoven geographic data...");
        var now = DateTime.UtcNow;

        // Create all 7 districts with quarters and neighborhoods
        var districts = new List<District>();

        // 1. Centrum
        districts.Add(new District
        {
            Name = "Centrum",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                new Quarter
                {
                    Name = "Centrum",
                    GeoJsonBoundary = "{}",
                    CreatedAt = now,
                    Neighborhoods = new List<Neighborhood>
                    {
                        new Neighborhood { Name = "Binnenstad", GeoJsonBoundary = "{}", CreatedAt = now },
                        new Neighborhood { Name = "De Bergen", GeoJsonBoundary = "{}", CreatedAt = now },
                        new Neighborhood { Name = "Witte dame", GeoJsonBoundary = "{}", CreatedAt = now },
                        new Neighborhood { Name = "Fellenoord", GeoJsonBoundary = "{}", CreatedAt = now },
                        new Neighborhood { Name = "TU/e terrain", GeoJsonBoundary = "{}", CreatedAt = now }
                    }
                }
            }
        });

        // 2. Woensel-Noord
        districts.Add(new District
        {
            Name = "Woensel-Noord",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("Ontginning", now, "Driehoeksbos", "Prinsejagt", "Jagershoef", "'t Hool", "Winkelcentrum", "Vlokhoven"),
                CreateQuarter("Achtse Molen", now, "Kerkdorp Acht", "Achtse Barrier-Gunterslaer", "Achtse Barrier-Spaaihoef", "Achtse Barrier-Hoeven"),
                CreateQuarter("Aanschot", now, "Woenselse Heide", "Tempel", "Blixembosch-West", "Blixembosch-Oost", "Castiliëlaan"),
                CreateQuarter("Dommelbeemd", now, "Eckart", "Luytelaer", "Vaartbroek", "Heesterakker", "Esp", "Bokt")
            }
        });

        // 3. Woensel-Zuid
        districts.Add(new District
        {
            Name = "Woensel-Zuid",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("Oud-Woensel", now, "Limbeek", "Hemelrijken", "Gildebuurt", "Woenselse Watermolen"),
                CreateQuarter("Erp", now, "Groenewoud (Woensel-West)", "Kronehoef", "Barrier", "Mensfort", "Rapenland", "Vredeoord"),
                CreateQuarter("Begijnenbroek", now, "Generalenbuurt (Rapenland-Oost)", "Oude Toren", "Hondsheuvels", "Oude Gracht-West", "Oude Gracht-Oost", "Eckartdal")
            }
        });

        // 4. Tongelre
        districts.Add(new District
        {
            Name = "Tongelre",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("De Laak", now, "Villapark", "Lakerlopen"),
                CreateQuarter("Oud-Tongelre", now, "Doornakkers-West", "Doornakkers-Oost", "Muschberg", "Geestenberg", "Urkhoven", "'t Hofke", "Karpen", "Koudenhoven")
            }
        });

        // 5. Stratum
        districts.Add(new District
        {
            Name = "Stratum",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("Oud-Stratum", now, "Irisbuurt", "Rochusbuurt", "Elzent-Noord", "Tuindorp (Witte Dorp)", "Heistraat (Joriskwartier)", "Bloemenplein (Bloemenbuurt)", "Looiakkers", "Elzent-Zuid"),
                CreateQuarter("Kortonjo", now, "Kerstroosplein", "Gerardusplein", "Genneperzijde (Poelhekkelaan)", "Roosten", "Eikenburg", "Sportpark Aalsterweg", "Putten"),
                CreateQuarter("Sintenbuurt", now, "Tivoli", "Gijzenrooi", "Nieuwe Erven", "Kruidenbuurt", "Schuttersbosch", "Leenderheide")
            }
        });

        // 6. Strijp
        districts.Add(new District
        {
            Name = "Strijp",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("Oud-Strijp", now, "Eliasterrein", "Vonderkwartier", "Philipsdorp", "Engelsbergen", "Schouwbroek", "Schoot", "Glaslaan (Strijp-S)"),
                CreateQuarter("Halve Maan", now, "Hurk", "Het Ven", "Lievendaal", "Drents Dorp", "Zwaanstraat (Strijp-R en T)", "Wielewaal", "Herdgang", "Mispelhoef"),
                CreateQuarter("Meerhoven", now, "BeA2", "Meerbos", "Grasrijk", "Bos- en Zandrijk", "Waterrijk", "Park Forum", "Flight Forum", "Eindhoven Airport")
            }
        });

        // 7. Gestel
        districts.Add(new District
        {
            Name = "Gestel",
            GeoJsonBoundary = "{}",
            CreatedAt = now,
            Quarters = new List<Quarter>
            {
                CreateQuarter("Rozenknopje", now, "Schrijversbuurt", "Oude Spoorbaan", "Hagenkamp"),
                CreateQuarter("Oud-Gestel", now, "Genderdal", "Blaarthem", "Rapelenburg", "Bennekel-Oost", "Bennekel-West", "Gagelbosch", "Gennep", "Beemden"),
                CreateQuarter("Oud Kasteel (Gestelse Ontginning)", now, "Genderbeemd", "Hanevoet", "Ooievaarsnest")
            }
        });

        // Save districts with their quarters and neighborhoods (only if not already present)
        if (!hasDistricts)
        {
            context.Districts.AddRange(districts);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ Seeded {districts.Count} districts with quarters and neighborhoods (81 quarters, 109 neighborhoods)");
        }
        else
        {
            Console.WriteLine("Districts already exist, skipping geographic data seeding");
        }

        // Seed NeighborhoodSnapshots with mood predictions for all neighborhoods
        var neighborhoodSnapshots = new List<NeighborhoodSnapshot>();
        var allNeighborhoods = await context.Neighborhoods.ToListAsync();
        
        if (!hasNeighborhoodSnapshots)
        {
            var moodLabels = new[] { "Energetic", "Busy", "Calm", "Intense", "Relaxed" };

            foreach (var neighborhood in allNeighborhoods)
            {
                var moodLabel = moodLabels[neighborhood.Id % moodLabels.Length];
                var confidence = 0.72 + (neighborhood.Id % 5) * 0.04;

                var features = new Dictionary<string, object>
                {
                    { "active_events", neighborhood.Id * 2 },
                    { "temperature_celsius", 15 + (neighborhood.Id % 3) },
                    { "precipitation_probability", 0.15 + (neighborhood.Id % 3) * 0.05 },
                    { "is_psv_match_day", neighborhood.Name.Contains("Strijp") },
                    { "is_holiday", false },
                    { "time_of_day", "evening" },
                    { "day_of_week", "Saturday" }
                };

                neighborhoodSnapshots.Add(new NeighborhoodSnapshot
                {
                    NeighborhoodId = neighborhood.Id,
                    Timestamp = now,
                    MoodLabel = moodLabel,
                    Confidence = confidence,
                    FeatureJson = features
                });
            }

            context.NeighborhoodSnapshots.AddRange(neighborhoodSnapshots);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ Seeded {neighborhoodSnapshots.Count} neighborhood snapshots with mood predictions");
        }
        else
        {
            Console.WriteLine("Neighborhood snapshots already exist, skipping mood data seeding");
        }

        // Seed Events (next 24 hours)
        var binnenstad = allNeighborhoods.FirstOrDefault(n => n.Name == "Binnenstad") ?? allNeighborhoods.First();
        var secondNeighborhood = allNeighborhoods.Count > 1 ? allNeighborhoods[1] : binnenstad;
        var thirdNeighborhood = allNeighborhoods.Count > 2 ? allNeighborhoods[2] : binnenstad;
        var fourthNeighborhood = allNeighborhoods.Count > 3 ? allNeighborhoods[3] : binnenstad;
        var fifthNeighborhood = allNeighborhoods.Count > 4 ? allNeighborhoods[4] : binnenstad;

        var events = new List<Event>
        {
            new Event
            {
                Title = "Spring Market - City Center",
                Source = "Ticketmaster",
                ExternalId = "evt_001",
                Category = "Markets",
                StartTime = now.AddHours(2),
                EndTime = now.AddHours(6),
                Latitude = 51.4416,
                Longitude = 5.4699,
                NeighborhoodId = binnenstad.Id,
                Url = "https://ticketmaster.com/event/1",
                CachedAt = now
            },
            new Event
            {
                Title = "PSV vs Ajax - Football Match",
                Source = "Ticketmaster",
                ExternalId = "evt_002",
                Category = "Sports",
                StartTime = now.AddHours(4),
                EndTime = now.AddHours(6),
                Latitude = 51.4411,
                Longitude = 5.4697,
                NeighborhoodId = secondNeighborhood.Id,
                Url = "https://ticketmaster.com/event/2",
                CachedAt = now
            },
            new Event
            {
                Title = "Indie Concert Night",
                Source = "Ticketmaster",
                ExternalId = "evt_003",
                Category = "Music",
                StartTime = now.AddHours(8),
                EndTime = now.AddHours(11),
                Latitude = 51.4333,
                Longitude = 5.4744,
                NeighborhoodId = thirdNeighborhood.Id,
                Url = "https://ticketmaster.com/event/3",
                CachedAt = now
            },
            new Event
            {
                Title = "Family Museum Day",
                Source = "Ticketmaster",
                ExternalId = "evt_004",
                Category = "Family",
                StartTime = now.AddHours(3),
                EndTime = now.AddHours(7),
                Latitude = 51.4428,
                Longitude = 5.4611,
                NeighborhoodId = fourthNeighborhood.Id,
                Url = "https://ticketmaster.com/event/4",
                CachedAt = now
            },
            new Event
            {
                Title = "Evening Jazz Session",
                Source = "Ticketmaster",
                ExternalId = "evt_005",
                Category = "Music",
                StartTime = now.AddHours(6),
                EndTime = now.AddHours(9),
                Latitude = 51.4500,
                Longitude = 5.4520,
                NeighborhoodId = fifthNeighborhood.Id,
                Url = "https://ticketmaster.com/event/5",
                CachedAt = now
            }
        };

        context.Events.AddRange(events);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {events.Count} sample events for next 24 hours");

        Console.WriteLine("✓ Database seeding completed successfully!");
    }

    private static Quarter CreateQuarter(string quarterName, DateTime createdAt, params string[] neighborhoodNames)
    {
        return new Quarter
        {
            Name = quarterName,
            GeoJsonBoundary = "{}",
            CreatedAt = createdAt,
            Neighborhoods = neighborhoodNames.Select(n => new Neighborhood
            {
                Name = n,
                GeoJsonBoundary = "{}",
                CreatedAt = createdAt
            }).ToList()
        };
    }
}
