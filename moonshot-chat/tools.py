import json
from data import events, mood_scores

TOOLS = [
    {
        "name": "get_events",
        "description": "Get a list of upcoming events in Eindhoven, including details such as title, description, district, start time, end time, category, and whether the event is indoor or outdoor.",
        "input_schema": {
            "type": "object",
            "properties": {
                "district": {
                    "type": "string",
                    "description": "The district where the event is taking place."
                }
            }
        }
    },
    {
        "name": "get_mood_score",
        "description": "Get the current mood score for a specific district in Eindhoven, which is calculated based on various factors such as weather, events, and social media sentiment.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    },
    {
        "name": "get_weather",
        "description": "Get the current weather conditions for a specific district in Eindhoven, including temperature, humidity, wind speed, and a brief description of the weather.",
        "input_schema": {
            "type": "object",
            "properties": {}
        }
    }
]

def execute_tool(name: str, tool_input: dict) -> str:
    if name == "get_events":
        district = tool_input.get("district")
        filtered = [e for e in events if not district or e["district"].lower() == district.lower()]
        return json.dumps(filtered, indent=2)
    elif name == "get_mood_score":
        return json.dumps(mood_scores, indent=2)
    elif name ==  "get_weather":
        return json.dumps({
            "temperature": 22,
            "condition": "Partly Cloudy",
            "forecast": "No rain expected, mild temperatures throughout the day."
        })
    
    return json.dumps({"error": f"Unknown tool: {name}"}, indent=2)