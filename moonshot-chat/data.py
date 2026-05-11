districts = [
    {"name": "Strijp-S", "description": "A former Philips industrial complex turned into a vibrant cultural and creative hub, known for its innovative architecture, art installations, and trendy cafes."},
    {"name": "Centrum", "description": "The heart of Eindhoven, featuring a mix of historic and modern architecture, bustling shopping streets, and a lively nightlife scene."},
    {"name": "Woensel", "description": "A diverse and multicultural district with a mix of residential areas, parks, and local shops, known for its community spirit and cultural events."},
    {"name": "Stratum", "description": "A residential district with a mix of modern and traditional housing, offering a quieter atmosphere while still being close to the city center."},
    {"name": "Gestel", "description": "A green and family-friendly district with parks, schools, and a strong sense of community, ideal for those seeking a more suburban lifestyle."},
    {"name": "Tongelre", "description": "A district with a mix of residential and industrial areas, known for its local markets, cultural festivals, and vibrant community life."}
]

events = [
    {
        "title": "Live jazz at Stroomhuis",
        "description": "Enjoy an evening of live jazz music at Stroomhuis, a popular venue in Strijp-S known for its eclectic programming and vibrant atmosphere.",
        "district": "Strijp-S",
        "start_time": "2024-07-15T20:00:00",
        "has_end_time": True,
        "end_time": "2024-07-15T22:00:00",
        "category": "music",
        "indoor": True
    },
    {
        "title": "Eindhoven Street Food Festival",
        "description": "Experience a variety of delicious street food from local vendors at the Eindhoven Street Food Festival, held in the city center.",
        "district": "Centrum",
        "start_time": "2024-07-20T12:00:00",
        "has_end_time": True,
        "end_time": "2024-07-20T22:00:00",
        "category": "food",
        "indoor": False
    },
    {
        "title": "Woensel Art Walk",
        "description": "Discover local art and culture with the Woensel Art Walk, featuring exhibitions, workshops, and performances throughout the district.",
        "district": "Woensel",
        "start_time": "2024-07-25T10:00:00",
        "has_end_time": True,
        "end_time": "2024-07-25T18:00:00",
        "category": "art",
        "indoor": True
    },
    {
        "title": "Stratum Park Yoga",
        "description": "Join a relaxing yoga session in Stratum Park, suitable for all levels and a great way to unwind in a beautiful outdoor setting.",
        "district": "Stratum",
        "start_time": "2024-07-30T09:00:00",
        "has_end_time": True,
        "end_time": "2024-07-30T10:30:00",
        "category": "wellness",
        "indoor": False
    },
    {
        "title": "Gestel Community Market",
        "description": "Support local businesses and artisans at the Gestel Community Market, offering a variety of handmade goods, fresh produce, and delicious treats.",
        "district": "Gestel",
        "start_time": "2024-08-05T10:00:00",
        "has_end_time": True,
        "end_time": "2024-08-05T16:00:00",
        "category": "market",
        "indoor": False
    },
    {
        "title": "Tongelre Cultural Festival",
        "description": "Celebrate the rich cultural diversity of Tongelre with a festival featuring music, dance, food, and art from around the world, held in various locations throughout the district.",
        "district": "Tongelre",
        "start_time": "2024-08-10T18:00:00",
        "has_end_time": True,
        "end_time": "2024-08-10T23:00:00",
        "category": "culture",
        "indoor": False
    },
    {
        "title": "Eindhoven Film Festival",
        "description": "Experience the best of independent cinema at the Eindhoven Film Festival, showcasing a diverse selection of films from around the world, held in various venues across the city.",
        "district": "Centrum",
        "start_time": "2024-08-15T18:00:00",
        "has_end_time": True,
        "end_time": "2024-08-20T22:00:00",
        "category": "film",
        "indoor": True
    },
    {
        "title": "PSV - Ajax Football Match",
        "description": "Catch the excitement of a football match between PSV and Ajax at the Philips Stadion, a must-see event for sports fans in Eindhoven.",
        "district": "Centrum",
        "start_time": "2024-08-25T20:00:00",
        "has_end_time": True,
        "end_time": "2024-08-25T22:00:00",
        "category": "sports",
        "indoor": False
    }
]

mood_scores = [
    {"district": "Strijp-S", "mood_score": 75, "crowd_level": "high", "events": ["Live jazz at Stroomhuis"]},
    {"district": "Centrum", "mood_score": 80, "crowd_level": "medium", "events": ["Eindhoven Street Food Festival", "Eindhoven Film Festival", "PSV - Ajax Football Match"]},
    {"district": "Woensel", "mood_score": 65, "crowd_level": "low", "events": ["Woensel Art Walk"]},
    {"district": "Stratum", "mood_score": 70, "crowd_level": "medium", "events": ["Stratum Park Yoga"]},
    {"district": "Gestel", "mood_score": 78, "crowd_level": "high", "events": ["Gestel Community Market"]},
    {"district": "Tongelre", "mood_score": 68, "crowd_level": "low", "events": ["Tongelre Cultural Festival"]}
]