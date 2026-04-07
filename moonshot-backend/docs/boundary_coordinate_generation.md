# Boundary Coordinate Generation Script

## Purpose

The script at `MoodRadar.API/Data/fetch_eindhoven_boundaries.py` fetches official Eindhoven geometry data from PDOK/CBS and generates C# boundary maps used by the backend.

It is used to:
- refresh neighborhood, quarter, and district coordinates from the source dataset,
- keep geometry names aligned with Eindhoven's official Dutch naming,
- regenerate boundary C# files after mapping updates.

## Data Source

- Primary source: PDOK OGC API (CBS Wijken en Buurten 2023)
- Fallback source: PDOK WFS (same dataset family)
- Municipality filter: Eindhoven (`gemeente_code` 0772)

## Generated Files

Running the script writes exactly these files in `MoodRadar.API/Data`:
- `boundaryMap_neighborhoods.cs`
- `boundaryMap_quarters.cs`
- `boundaryMap_districts.cs`

## Prerequisites

From repository root (`Eindhoven-mood-radar`):

1. Use the project virtual environment (recommended):
   - Windows PowerShell:
     - `.\.venv\Scripts\Activate.ps1`

2. Install required package:
   - `pip install requests`

## How To Run

### Option A: Run from repository root

```powershell
c:/dev/fontys/Eindhoven-mood-radar/.venv/Scripts/python.exe moonshot-backend/MoodRadar.API/Data/fetch_eindhoven_boundaries.py
```

### Option B: Run from script directory

```powershell
cd moonshot-backend/MoodRadar.API/Data
python .\fetch_eindhoven_boundaries.py
```

## Expected Output In Terminal

The script prints:
- collection/layer discovery,
- fetched feature counts,
- Eindhoven-filtered counts,
- matched/unmatched name totals,
- generated file names.

When successful, the final lines include:
- `Done!`
- `Generated only boundaryMap_neighborhoods.cs, boundaryMap_quarters.cs, and boundaryMap_districts.cs`

## Name-Matching Notes

- Name matching uses normalized text and override dictionaries in the script:
  - `NEIGHBORHOOD_OVERRIDES`
  - `QUARTER_OVERRIDES`
- Districts are composed from official wijk groupings in:
  - `DISTRICT_COMPONENT_QUARTERS`

If unmatched names appear:
1. Check the printed CBS names in terminal output.
2. Compare official Eindhoven naming (Dutch) here:
   - https://nl.wikipedia.org/wiki/Lijst_van_buurten_en_wijken_in_Eindhoven
3. Update the corresponding override/grouping mapping in the script.
4. Re-run the script.

## Safety Notes

- The script overwrites the three `boundaryMap_*.cs` files.
- Commit regenerated files together with mapping changes so name-to-geometry updates stay traceable.
