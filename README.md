# Eindhoven Mood Radar

A web application that aggregates live public event data and displays AI-generated mood labels for different Eindhoven city zones, with full transparency and confidence scoring.

## What is it?

The Mood Radar shows you the mood of different areas in Eindhoven in real-time:

| Mood | Colour | Typical Trigger |
|------|--------|-----------------|
| 🟨 **Energetic** | Pastel Yellow | Nightlife + weekend + warm evening |
| 🟧 **Intense** | Pastel Orange | PSV home match or large concert |
| 🟥 **Busy** | Pastel Coral | Market + many events + daytime |
| 🔵 **Relaxed** | Pastel Sky | Few events, mild weather, evening |
| 🟦 **Calm** | Pastel Teal | No events, early morning / late night |

Every mood label includes:

- **Confidence score** (0-100%)
- **Plain-language explanation** of what drove it
- **Data sources** and known limitations
- **Transparency Panel** explaining how moods are calculated

## Project Structure

```
Eindhoven-mood-radar/
├── ai-model/                      # AI model deliverables
│   └── README.md                  # AI model REAMDME
│
├── docs/                          # Additional docs and files
│
├── figma/                         # Figma UI deliverable
│   └── README.md                  # Figma UI REAMDME
│
├── moonshot-app/                  # Frontend
│   └── README.md                  # Frontend README
│
├── moonshot-backend/              # Backend
│   └── README.md                  # Backend REAMDME
│
└── README.md                      # This README
```

## Tech Stack

### Backend

- **Language**: C# 12.0
- **Framework**: ASP.NET Core 8.0 LTS
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core

### Frontend

- **Framework**: React Native + Expo
- **Language**: TypeScript
- **Routing**: Expo Router
- **Map**: Leaflet.js + OpenStreetMap
- **Styling**: Tailwind CSS (react-native compatible)

### Data Sources (Phase 1)

| Source | API |
|--------|-----|
| Events | Uit in Eindhoven (scraping) |
| PSV Matches | football-data.org |
| Weather | Open-Meteo |
| Holidays | Nager.Date |
