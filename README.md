# 📖 QuizletBot — Ukrainian Idioms Flashcard Trainer

A Telegram bot for learning Ukrainian idioms (phraseologisms) as flashcards,
Quizlet-style. Full navigation system: main menu, all-time stats, language
switch (UA/EN), and an about section.

## 🗺 Menu structure

```
🏠 Main Menu
├── 🎴 Flashcards    → pick a count (5/15/25/50) → card session → summary
├── 📊 Statistics    → total cards reviewed all-time
├── ⚙️ Settings      → language toggle UA / EN
└── ℹ️ About Bot      → tech stack, developer contact
```

Every screen has a **🏠 Main Menu** button (static submenus also get
**◀️ Back**), so there's no dead end — you can always get back.

## ✨ How it works

1. `/start` → main menu with 4 sections
2. **🎴 Flashcards** → pick how many cards to review (5, 15, 25, or 50)
3. Bot shows an idiom → tap **🔄 Show explanation** → the card "flips"
4. Mark **⬅️ Don't know** / **➡️ Know** → bot immediately serves the next card
5. Once the limit is reached — a summary screen: cards reviewed, know/don't-know
   split, with **🔄 Try Again** (same count) and **🏠 Main Menu** buttons

> Telegram bots can't detect swipe gestures, so "swipe left/right" is
> implemented as a button tap instead. Functionally the same thing.

## 🗂 Data

**244 Ukrainian idioms** with explanations — from common ones to more obscure
ones.

## 🛠 Tech stack

- [.NET 8](https://dotnet.microsoft.com/) / C#
- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) — the official .NET wrapper for the Telegram Bot API
- Long polling (no webhooks — no need for a public HTTPS server)
- Simple file-based persistence (`userdata.json`) for stats and language — no external DB needed

## 🚀 Getting started

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
- A bot token from [@BotFather](https://t.me/BotFather)

### Steps

```bash
git clone https://github.com/your-username/QuizletBot.git
cd QuizletBot

# Copy the example config and add your token
cp appsettings.example.json appsettings.json
# Open appsettings.json and replace YOUR_BOT_TOKEN_HERE with your token

dotnet restore
dotnet run
```

Or pass the token as an environment variable instead:

```bash
export TELEGRAM_BOT_TOKEN="your_token_here"
dotnet run
```

On startup you'll see:
```
✅ Bot @your_bot_username started. Card pool: 244 idioms.
```

Open the bot in Telegram and send `/start`.

## 💬 Bot commands

| Command   | Action                          |
|-----------|-----------------------------------|
| `/start`  | Greeting + main menu               |
| `/menu`   | Show the main menu at any time     |

Everything else is inline-button navigation.

## 📁 Project structure

```
QuizletBot/
├── Program.cs                      # bootstrap: config, starts the bot
├── Handlers/
│   └── UpdateHandler.cs            # routes all callbacks and commands
├── Models/
│   ├── Phraseologism.cs            # flashcard model
│   ├── UserSession.cs              # in-memory session state (nav state, counters)
│   ├── UserData.cs                 # persistent data (language, all-time stats)
│   └── Language.cs                 # UA/EN enum
├── Services/
│   ├── PhraseologismRepository.cs  # loads and picks random cards, no repeats per session
│   ├── SessionManager.cs           # in-memory user sessions
│   ├── UserDataStore.cs            # file-based persistence (userdata.json)
│   └── UiRenderer.cs               # text + keyboard for every screen
├── Resources/
│   └── Strings.cs                  # all UA/EN UI text in one place
├── Data/
│   └── phraseologisms.json         # idiom database (244 entries)
├── appsettings.example.json        # config template (no real token)
└── appsettings.json                # your real token (gitignored, not in the repo)
```

## 🧭 How navigation is wired up (for devs)

Callback data always follows `namespace:action[:param]`:

| Example                | Meaning                                    |
|--------------------------|---------------------------------------------|
| `nav:main`               | Show the main menu                          |
| `nav:flashcards`         | Show the card count picker                  |
| `nav:stats` / `nav:settings` / `nav:about` | The matching static submenu   |
| `fc:count:15`            | Start a session with 15 cards                |
| `fc:flip:42`             | Flip card id=42                              |
| `fc:know:42` / `fc:dontknow:42` | Mark the card and show the next one   |
| `settings:lang:EN`       | Switch language to English                   |

User state (`SessionState`: Idle / ChoosingCount / Active / Ended) lives in
`UserSession` and is used to ignore stale callbacks (e.g. tapping a button on
a card from a session that already ended) — no crashes, no dead-end screens.

## ➕ Add your own idioms

Open `Data/phraseologisms.json` and add a new entry to the array:

```json
{
  "text": "Your idiom",
  "explanation": "What it means"
}
```

The `id` field is optional — it's assigned automatically if missing.

## 🔒 Token security

`appsettings.json` (with your real token) is intentionally excluded via
`.gitignore` and never makes it into the repo. If you clone this repo, create
your own `appsettings.json` based on `appsettings.example.json`, or just use
the `TELEGRAM_BOT_TOKEN` environment variable.

## 🗺 Possible improvements

- [ ] "Learn all cards" mode that skips already-known idioms between sessions
- [ ] Categories/tags for idioms
- [ ] Inline mode for using the bot in any chat
- [ ] Move from file-based storage to SQLite if the user base grows

## 📄 License

MIT — use it, modify it, share it freely.
