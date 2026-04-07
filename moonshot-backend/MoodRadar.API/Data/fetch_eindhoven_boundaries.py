"""
Fetches GeoJSON boundaries for Eindhoven neighborhoods, quarters, and districts
from the Dutch CBS Wijk- en Buurtkaart via the PDOK OGC API Features endpoint.

This is the authoritative Dutch government dataset — authorised by CBS/Kadaster.

Usage:
    pip install requests
    python fetch_eindhoven_boundaries.py

Output (script directory only):
    boundaryMap_neighborhoods.cs   — C# dict for neighborhoods (buurten)
    boundaryMap_quarters.cs        — C# dict for quarters (wijken)
    boundaryMap_districts.cs       — C# dict for districts
"""

import json
import re
import unicodedata
from pathlib import Path

import requests

# ─────────────────────────────────────────────────────────────────────────────
# PDOK OGC API Features — CBS Wijken en Buurten 2023
# No CQL filter needed: we fetch all Eindhoven features by gemeente_code param
# ─────────────────────────────────────────────────────────────────────────────
OGC_BASE     = "https://api.pdok.nl/cbs/wijken-en-buurten-2023/ogc/v1"
GEMEENTE_CODE = "0772"   # Eindhoven (without "GM" prefix for this API)
SCRIPT_DIR = Path(__file__).resolve().parent

# WFS fallback (if OGC API is slow) — correct layer names confirmed from GetCapabilities
WFS_BASE     = "https://service.pdok.nl/cbs/wijkenbuurten/2023/wfs/v1_0"
# Layer names (no namespace prefix needed for this endpoint):
#   cbs_buurten_2023   → neighborhoods
#   cbs_wijken_2023    → quarters
# Filter field: gemeentecode = '0772'   (no GM prefix in the WFS attribute)

# ─────────────────────────────────────────────────────────────────────────────
# Discovery: Check what collections/layers are actually available
# ─────────────────────────────────────────────────────────────────────────────

def discover_ogc_collections() -> list[str]:
    """List available collections in the OGC API"""
    print("   Discovering OGC API collections...")
    try:
        r = requests.get(f"{OGC_BASE}/collections", timeout=30)
        if r.ok:
            data = r.json()
            collections = [c.get("id") for c in data.get("collections", [])]
            print(f"   Found {len(collections)} collections:")
            for c in collections:
                print(f"     - {c}")
            return collections
    except Exception as e:
        print(f"   Error discovering collections: {e}")
    return []


def discover_wfs_layers() -> list[str]:
    """List available layers in the WFS service"""
    print("   Discovering WFS layers...")
    try:
        params = {
            "SERVICE": "WFS",
            "VERSION": "2.0.0",
            "REQUEST": "GetCapabilities",
        }
        r = requests.get(WFS_BASE, params=params, timeout=30)
        if r.ok:
            # Parse XML to extract FeatureType names
            import xml.etree.ElementTree as ET
            root = ET.fromstring(r.text)
            # Extract all FeatureType/Name elements
            layers = []
            for ft in root.findall(".//{http://www.opengis.net/wfs/2.0}FeatureType"):
                name = ft.find("{http://www.opengis.net/wfs/2.0}Name")
                if name is not None and name.text:
                    layers.append(name.text)
            print(f"   Found {len(layers)} wfs layers:")
            for l in layers:
                print(f"     - {l}")
            return layers
    except Exception as e:
        print(f"   Error discovering WFS layers: {e}")
    return []

# ─────────────────────────────────────────────────────────────────────────────
# Name overrides: your DB name → CBS name
# Add entries after checking unmatched_*.txt
# ─────────────────────────────────────────────────────────────────────────────
NEIGHBORHOOD_OVERRIDES = {
    "Glaslaan (Strijp-S)":             "Strijp-S",
    "Bloemenplein (Bloemenbuurt)":      "Bloemenplein",
    "Groenewoud (Woensel-West)":        "Woensel-West",
    "Heistraat (Joriskwartier)":        "Joriskwartier",
    "Genneperzijde (Poelhekkelaan)":    "Genneperzijde",
    "Generalenbuurt (Rapenland-Oost)":  "Rapenland-Oost",
    "Tuindorp (Witte Dorp)":            "Tuindorp",
    "Zwaanstraat (Strijp-R en T)":      "Zwaanstraat",
    "'t Hofke":                         "'t Hofke",
    "'t Hool":                          "'t Hool",
    "TU/e terrain":                     "TU-terrein",
    "Witte dame":                       "Witte Dame",
    "Eindhoven Airport":                "Eindhoven Airport",
    "BeA2":                             "Be-A2",
}

QUARTER_OVERRIDES = {
    "Oud Kasteel (Gestelse Ontginning)": "Gestelse Ontginning",
    "Sintenbuurt":                        "Putten",
}

DISTRICT_OVERRIDES: dict = {}

# Official stadsdeel composition from Eindhoven's wijkindeling (Dutch naming)
DISTRICT_COMPONENT_QUARTERS = {
    "Centrum": ["Centrum"],
    "Stratum": ["Oud-Stratum", "Kortonjo", "Putten"],
    "Tongelre": ["De Laak", "Doornakkers", "Oud-Tongelre"],
    "Woensel-Zuid": ["Oud-Woensel", "Erp", "Begijnenbroek"],
    "Woensel-Noord": ["Ontginning", "Achtse Molen", "Aanschot", "Dommelbeemd"],
    "Strijp": ["Oud-Strijp", "Halve Maan", "Meerhoven"],
    "Gestel": ["Rozenknopje", "Oud-Gestel", "Gestelse Ontginning"],
}

# ─────────────────────────────────────────────────────────────────────────────
# DB name lists
# ─────────────────────────────────────────────────────────────────────────────
NEIGHBORHOOD_NAMES = [
    "Achtse Barrier-Gunterslaer", "Achtse Barrier-Hoeven", "Achtse Barrier-Spaaihoef",
    "Barrier", "BeA2", "Beemden", "Bennekel-Oost", "Bennekel-West", "Binnenstad",
    "Blaarthem", "Blixembosch-Oost", "Blixembosch-West", "Bloemenplein (Bloemenbuurt)",
    "Bokt", "Bos- en Zandrijk", "Castiliëlaan", "De Bergen", "Doornakkers-Oost",
    "Doornakkers-West", "Drents Dorp", "Driehoeksbos", "Eckart", "Eckartdal",
    "Eikenburg", "Eindhoven Airport", "Eliasterrein", "Elzent-Noord", "Elzent-Zuid",
    "Engelsbergen", "Esp", "Fellenoord", "Flight Forum", "Gagelbosch", "Geestenberg",
    "Genderbeemd", "Genderdal", "Generalenbuurt (Rapenland-Oost)", "Gennep",
    "Genneperzijde (Poelhekkelaan)", "Gerardusplein", "Gijzenrooi", "Gildebuurt",
    "Glaslaan (Strijp-S)", "Grasrijk", "Groenewoud (Woensel-West)", "Hagenkamp",
    "Hanevoet", "Heesterakker", "Heistraat (Joriskwartier)", "Hemelrijken",
    "Herdgang", "Het Ven", "Hondsheuvels", "Hurk", "Irisbuurt", "Jagershoef",
    "Karpen", "Kerkdorp Acht", "Kerstroosplein", "Koudenhoven", "Kronehoef",
    "Kruidenbuurt", "Lakerlopen", "Leenderheide", "Lievendaal", "Limbeek",
    "Looiakkers", "Luytelaer", "Meerbos", "Mensfort", "Mispelhoef", "Muschberg",
    "Nieuwe Erven", "Ooievaarsnest", "Oude Gracht-Oost", "Oude Gracht-West",
    "Oude Spoorbaan", "Oude Toren", "Park Forum", "Philipsdorp", "Prinsejagt",
    "Putten", "Rapelenburg", "Rapenland", "Rochusbuurt", "Roosten", "Schoot",
    "Schouwbroek", "Schrijversbuurt", "Schuttersbosch", "Sportpark Aalsterweg",
    "Tempel", "'t Hofke", "'t Hool", "Tivoli", "TU/e terrain",
    "Tuindorp (Witte Dorp)", "Urkhoven", "Vaartbroek", "Villapark", "Vlokhoven",
    "Vonderkwartier", "Vredeoord", "Waterrijk", "Wielewaal", "Winkelcentrum",
    "Witte dame", "Woenselse Heide", "Woenselse Watermolen",
    "Zwaanstraat (Strijp-R en T)",
]

QUARTER_NAMES = [
    "Aanschot", "Achtse Molen", "Begijnenbroek", "Centrum", "De Laak",
    "Dommelbeemd", "Erp", "Halve Maan", "Kortonjo", "Meerhoven", "Ontginning",
    "Oud-Gestel", "Oud Kasteel (Gestelse Ontginning)", "Oud-Stratum", "Oud-Strijp",
    "Oud-Tongelre", "Oud-Woensel", "Rozenknopje", "Sintenbuurt",
]

DISTRICT_NAMES = [
    "Centrum", "Gestel", "Stratum", "Strijp", "Tongelre",
    "Woensel-Noord", "Woensel-Zuid",
]


# ─────────────────────────────────────────────────────────────────────────────
# Fetchers — try OGC API first, fall back to WFS
# ─────────────────────────────────────────────────────────────────────────────

MUNICIPALITY_CODE_FIELDS = (
    "gemeentecode",
    "gemeente_code",
    "gm_code",
    "gemcode",
)

MUNICIPALITY_NAME_FIELDS = (
    "gemeentenaam",
    "gemeente_naam",
    "gm_naam",
    "municipality",
    "municipality_name",
)


def _get_prop_text(props: dict, fields: tuple[str, ...]) -> str:
    for field in fields:
        value = props.get(field)
        if value is None:
            continue
        text = str(value).strip()
        if text:
            return text
    return ""


def _normalize_municipality_code(raw_code: str) -> str:
    digits_only = re.sub(r"\D", "", raw_code or "")
    return digits_only.zfill(4) if digits_only else ""


def _has_municipality_metadata(feature: dict) -> bool:
    props = feature.get("properties") or {}
    return bool(
        _get_prop_text(props, MUNICIPALITY_CODE_FIELDS)
        or _get_prop_text(props, MUNICIPALITY_NAME_FIELDS)
    )


def _is_eindhoven_feature(feature: dict) -> bool:
    props = feature.get("properties") or {}

    code = _normalize_municipality_code(_get_prop_text(props, MUNICIPALITY_CODE_FIELDS))
    if code == GEMEENTE_CODE:
        return True

    municipality_name = norm(_get_prop_text(props, MUNICIPALITY_NAME_FIELDS))
    if municipality_name == "eindhoven":
        return True

    return False


def filter_features_to_eindhoven(features: list[dict], label: str) -> list[dict]:
    if not features:
        return []

    if not any(_has_municipality_metadata(feature) for feature in features):
        print(
            f"   Warning: {label} has no municipality metadata; "
            "falling back to bbox-only filtering."
        )
        return features

    filtered = [feature for feature in features if _is_eindhoven_feature(feature)]
    print(f"   Eindhoven filter: {len(filtered)}/{len(features)} features")
    return filtered


def fetch_via_wfs(layer: str) -> list[dict]:
    """
    Fetch features via WFS 2.0 using bounding box for Eindhoven.
    Eindhoven approx bounds: [5.3, 51.3, 5.6, 51.55] (minLon, minLat, maxLon, maxLat)
    """
    print(f"   Trying WFS layer '{layer}'…")
    # Eindhoven bounding box to filter features
    bbox = "5.3,51.3,5.6,51.55"  # minLon, minLat, maxLon, maxLat (EPSG:4326)
    params = {
        "SERVICE":      "WFS",
        "VERSION":      "2.0.0",
        "REQUEST":      "GetFeature",
        "TYPENAMES":    layer,
        "OUTPUTFORMAT": "application/json",
        "SRSNAME":      "EPSG:4326",
        "BBOX":         bbox,
        "COUNT":        "2000",
    }
    r = requests.get(WFS_BASE, params=params, timeout=120)
    if not r.ok:
        print(f"   WFS error {r.status_code}: {r.text[:300]}")
        return []
    data = r.json()
    features = data.get("features", [])
    print(f"   [OK] WFS: {len(features)} features")
    return features


def fetch_via_ogc_api(collection: str) -> list[dict]:
    """
    Fetch all features from the PDOK OGC API Features endpoint.
    Paginates automatically (limit=1000 per page).
    Uses bounding box to limit to Eindhoven.
    """
    print(f"   Trying OGC API collection '{collection}'…")
    features = []
    url = f"{OGC_BASE}/collections/{collection}/items"
    # Eindhoven bounding box [minLon, minLat, maxLon, maxLat]
    bbox = "5.3,51.3,5.6,51.55"
    params = {
        "f": "json",
        "limit":  1000,
        "bbox": bbox,
    }
    while url:
        r = requests.get(url, params=params, timeout=120)
        if not r.ok:
            print(f"   OGC API error {r.status_code}: {r.text[:300]}")
            return []
        data = r.json()
        page = data.get("features", [])
        features.extend(page)
        # Follow next link if present
        url = None
        params = {}
        for link in data.get("links", []):
            if link.get("rel") == "next":
                url = link["href"]
                break
    print(f"   [OK] OGC API: {len(features)} features")
    return features


def fetch_features(
    wfs_layer: str,
    ogc_collection: str,
    label: str,
) -> list[dict]:
    print(f"\n[FETCHING] {label}…")

    # Try OGC API first (simpler, no filter issues)
    features = fetch_via_ogc_api(ogc_collection)
    if features:
        return filter_features_to_eindhoven(features, label)

    # Fall back to WFS
    features = fetch_via_wfs(wfs_layer)
    if features:
        return filter_features_to_eindhoven(features, label)

    print(f"   [ERROR] Could not fetch {label} from either endpoint.")
    return []


# ─────────────────────────────────────────────────────────────────────────────
# Name normalisation & matching
# ─────────────────────────────────────────────────────────────────────────────

def _strip_accents(s: str) -> str:
    return "".join(
        c for c in unicodedata.normalize("NFD", s)
        if unicodedata.category(c) != "Mn"
    )


def norm(s: str) -> str:
    return re.sub(r"[^a-z0-9]", "", _strip_accents(s).lower())


def build_lookup(features: list[dict], name_field: str) -> dict:
    lookup: dict[str, tuple[str, dict]] = {}
    for f in features:
        cbs_name = (f.get("properties") or {}).get(name_field, "").strip()
        geom = f.get("geometry")
        if cbs_name and geom and geom.get("type") in ("Polygon", "MultiPolygon"):
            key = norm(cbs_name)
            if key not in lookup:
                lookup[key] = (cbs_name, geom)
    return lookup


def match(db_name: str, lookup: dict, overrides: dict):
    candidate = overrides.get(db_name, db_name)
    key = norm(candidate)

    if key in lookup:
        return lookup[key][1]

    if len(key) >= 4:
        for k, (_, geo) in lookup.items():
            if k.startswith(key) or key.startswith(k):
                return geo

    if len(key) >= 5:
        for k, (_, geo) in lookup.items():
            if key in k or k in key:
                return geo

    return None


# ─────────────────────────────────────────────────────────────────────────────
# C# output
# ─────────────────────────────────────────────────────────────────────────────

def to_cs_verbatim(geojson: dict) -> str:
    return json.dumps(geojson, separators=(",", ":")).replace('"', '""')


def to_multipolygon(geometry: dict) -> dict | None:
    if not geometry:
        return None

    geom_type = geometry.get("type")
    coords = geometry.get("coordinates")
    if not isinstance(coords, list):
        return None

    if geom_type == "Polygon":
        return {"type": "MultiPolygon", "coordinates": [coords]}

    if geom_type == "MultiPolygon":
        return {"type": "MultiPolygon", "coordinates": coords}

    return None


def combine_geometries(geometries: list[dict]) -> dict | None:
    combined_coords: list = []
    for geometry in geometries:
        multipolygon = to_multipolygon(geometry)
        if multipolygon:
            combined_coords.extend(multipolygon["coordinates"])

    if not combined_coords:
        return None

    return {"type": "MultiPolygon", "coordinates": combined_coords}


def write_csharp(results: dict, path: Path, class_name: str):
    lines = [
        "namespace MoodRadar.API.Data;",
        "",
        f"public static class {class_name}",
        "{",
        "    public static readonly Dictionary<string, string> Data = new(StringComparer.OrdinalIgnoreCase)",
        "    {",
    ]
    items = list(results.items())
    for i, (name, geojson) in enumerate(items):
        comma = "," if i < len(items) - 1 else ""
        lines.append(f'        {{ "{name}", @"{to_cs_verbatim(geojson)}" }}{comma}')
    lines.extend([
        "    };",
        "}",
        "",
    ])

    path.write_text("\n".join(lines), encoding="utf-8")
    print(f"   Wrote {path.name} ({len(results)} entries)")


# ─────────────────────────────────────────────────────────────────────────────
# Process one level (neighborhoods / quarters / districts)
# ─────────────────────────────────────────────────────────────────────────────

def process(
    wfs_layer:      str,
    ogc_collection: str,
    name_field:     str,
    db_names:       list[str],
    overrides:      dict,
    cs_out:         Path,
    cs_class_name:  str,
    label:          str,
) -> dict:
    features = fetch_features(wfs_layer, ogc_collection, label)
    if not features:
        raise RuntimeError(
            f"No Eindhoven features found for {label}. "
            "Check endpoint availability and municipality filter fields."
        )

    lookup = build_lookup(features, name_field)

    print(f"\n   CBS names found for {label}:")
    for _, (name, _) in sorted(lookup.items()):
        print(f"     {name}")

    results: dict[str, dict] = {}
    missing: list[str] = []
    for db_name in db_names:
        geo = match(db_name, lookup, overrides)
        if geo:
            results[db_name] = geo
        else:
            missing.append(db_name)

    print(f"\n[OK] {label}: {len(results)}/{len(db_names)} matched")
    if missing:
        print(f"   Unmatched ({len(missing)}): {', '.join(missing)}")
        print("   Add mappings to *_OVERRIDES and re-run.\n")

    write_csharp(results, cs_out, cs_class_name)
    return lookup


def process_districts_from_quarters(
    quarter_lookup: dict,
    district_names: list[str],
    cs_out: Path,
    cs_class_name: str,
):
    print("\n[FETCHING] Districts…")
    print("   Building district geometries from official wijk composition…")

    results: dict[str, dict] = {}
    missing_districts: list[str] = []

    for district_name in district_names:
        quarter_names = DISTRICT_COMPONENT_QUARTERS.get(district_name, [])
        if not quarter_names:
            missing_districts.append(district_name)
            continue

        component_geometries: list[dict] = []
        missing_quarters: list[str] = []
        for quarter_name in quarter_names:
            geometry = match(quarter_name, quarter_lookup, QUARTER_OVERRIDES)
            if geometry:
                component_geometries.append(geometry)
            else:
                missing_quarters.append(quarter_name)

        merged_geometry = combine_geometries(component_geometries)
        if merged_geometry:
            results[district_name] = merged_geometry
        else:
            missing_districts.append(district_name)

        if missing_quarters:
            print(
                f"   Warning: {district_name} missing component wijk(en): "
                f"{', '.join(missing_quarters)}"
            )

    print(f"\n[OK] Districts: {len(results)}/{len(district_names)} matched")
    if missing_districts:
        print(f"   Unmatched ({len(missing_districts)}): {', '.join(missing_districts)}")
        print("   Update DISTRICT_COMPONENT_QUARTERS and re-run.\n")

    write_csharp(results, cs_out, cs_class_name)


# ─────────────────────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print("  Eindhoven boundary fetcher — PDOK CBS 2023")
    print("=" * 60)

    # First, discover what's actually available
    print("\n[DISCOVERY PHASE]\n")
    discover_ogc_collections()
    print()
    discover_wfs_layers()

    print("\n" + "=" * 60)
    print("  Starting fetch phase...\n")

    output_dir = SCRIPT_DIR

    # Neighborhoods (buurten)
    process(
        wfs_layer      = "wijkenbuurten:buurten",
        ogc_collection = "buurten",
        name_field     = "buurtnaam",
        db_names       = NEIGHBORHOOD_NAMES,
        overrides      = NEIGHBORHOOD_OVERRIDES,
        cs_out         = output_dir / "boundaryMap_neighborhoods.cs",
        cs_class_name  = "NeighborhoodBoundaryMap",
        label          = "Neighborhoods",
    )

    # Quarters (wijken)
    quarter_lookup = process(
        wfs_layer      = "wijkenbuurten:wijken",
        ogc_collection = "wijken",
        name_field     = "wijknaam",
        db_names       = QUARTER_NAMES,
        overrides      = QUARTER_OVERRIDES,
        cs_out         = output_dir / "boundaryMap_quarters.cs",
        cs_class_name  = "QuarterBoundaryMap",
        label          = "Quarters",
    )

    # Districts (stadsdelen) are composed from official sets of wijken.
    process_districts_from_quarters(
        quarter_lookup = quarter_lookup,
        district_names = DISTRICT_NAMES,
        cs_out         = output_dir / "boundaryMap_districts.cs",
        cs_class_name  = "DistrictBoundaryMap",
    )

    print("\nDone!")
    print("Generated only boundaryMap_neighborhoods.cs, boundaryMap_quarters.cs, and boundaryMap_districts.cs")


if __name__ == "__main__":
    main()
