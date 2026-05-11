import os
from anthropic import Anthropic
from dotenv import load_dotenv
from rich.console import Console
from rich.markdown import Markdown
from rich.panel import Panel
from rich.prompt import Prompt
from tools import TOOLS, execute_tool

load_dotenv()
client = Anthropic()
console = Console()

SYSTEM_PROMPT = """"You are a helpful and kind Eindhoven guide. 
You will give concrete and local recommendations about events and things to do in Eindhoven based on live data about events, the weather and business in the city. 
ALWAYS use the tools at your disposal to get the most up-to-date information. 
If you don't know the answer, use the tools to find out. 
Always provide a recommendation based on the information you have gathered. 
Be concise and to the point in your responses. 
Answer in English if the user asks in English, and Dutch if the user asks in Dutch. 
Short and kind with a maximum of 3 concrete recommendations.
Explain shortly why you recommend these things to do, (e.g. because of the weather, or because there is a special event).

FORMATTING:
- Use markdown: **bold** for event names, bullet points for lists
- Use relevant emojis (🏟️ sports, 🎵 music, 🍴 food, 🎨 art, ☀️ weather)
- Structure: brief intro, then 2-3 recommendations, then a short closing tip

"""

def run_conversation(user_message: str, history: list) -> tuple[str, list]:
    history.append({"role": "user", "content": user_message})

    with console.status("[dim]Thinking...[/dim]", spinner="dots"):
        response = client.messages.create(
            model="claude-haiku-4-5",
            max_tokens=1024,
            system=SYSTEM_PROMPT,
            tools=TOOLS,
            messages=history,
        )

    while response.stop_reason == "tool_use":
        tool_uses = [b for b in response.content if b.type == "tool_use"]

        tool_results = []
        for tool in tool_uses:
            console.print(f"  [dim cyan]using {tool.name}[/dim cyan]")
            result = execute_tool(tool.name, tool.input)
            tool_results.append({
                "type": "tool_result",
                "tool_use_id": tool.id,
                "content": result
            })

        history.append({"role": "assistant", "content": response.content})
        history.append({"role": "user", "content": tool_results})

        with console.status("[dim]Thinking...[/dim]", spinner="dots"):
            response = client.messages.create(
                model="claude-haiku-4-5",
                max_tokens=1024,
                system=SYSTEM_PROMPT,
                tools=TOOLS,
                messages=history,
            )

    text_block = next((b for b in response.content if b.type == "text"), None)
    answer = text_block.text if text_block else "No response received."
    history.append({"role": "assistant", "content": response.content})

    return answer, history

def main():
    console.print(Panel.fit(
        "[bold cyan]Eindhoven Mood Radar[/bold cyan]\n"
        "[dim]Ask me anything about events and things to do in Eindhoven.[/dim]\n"
        "[dim]Type 'exit' to quit, 'reset' to start over.[/dim]",
        border_style="cyan"
    ))

    history = []
    while True:
        try:
            user_input = Prompt.ask("\n[bold green]You[/bold green]").strip()
        except (EOFError, KeyboardInterrupt):
            console.print("\n[dim]Goodbye![/dim]")
            break

        if not user_input:
            continue
        if user_input.lower() == "exit":
            console.print("[dim]Goodbye![/dim]")
            break
        if user_input.lower() == "reset":
            history = []
            console.print("[yellow]Conversation reset.[/yellow]")
            continue

        try:
            answer, history = run_conversation(user_input, history)
            console.print()
            console.print(Panel(
                Markdown(answer),
                title="[bold magenta]Guide[/bold magenta]",
                border_style="magenta",
                padding=(1, 2)
            ))
        except Exception as e:
            console.print(f"[bold red]Error:[/bold red] {e}")

if __name__ == "__main__":
    main()