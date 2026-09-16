# 🎓 NMT Learner Bot

A Telegram bot for NMT (Ukrainian national exam) prep: idioms, word stress,
and lexical mistakes, as flashcards or multiple-choice quizzes. Full
navigation system: main menu, per-mode stats, language switch (UA/EN), and a
banner image on every screen.

## 🗺 Menu structure

```
🏠 Main Menu
├── 🎴 Flashcards
│   ├── pick a mode: 🔄 Flip Cards / ❓ Choose Correct
│   ├── pick a deck: 📖 Idioms / 🔤 Word Stress / ✏️ Lexical Mistakes
│   ├── pick a count: 5 / 15 / 25 / 50
│   └── → session → summary
├── 📊 Statistics    → stats split by mode (Flip Cards vs Choose Correct)
├── ⚙️ Settings      → language toggle UA / EN
└── ℹ️ About Bot      → tech stack, developer contact
```

Every screen has a **🏠 Main Menu** button (static submenus also get
**◀️ Back**), so there's no dead end — you can always get back.

## ✨ How it works

1. `/start` → main menu
2. **🎴 Flashcards** → pick a mode:
   - **🔄 Flip Cards** — see the front, tap to reveal the back, mark
     **⬅️ Don't know** / **➡️ Know**
   - **❓ Choose Correct** — see the front, pick one of 3 answers, bot tells
     you right away if you got it, tap **➡️ Next** to continue
3. Pick a deck (Idioms / Word Stress / Lexical Mistakes)
4. Pick how many cards to review (5, 15, 25, or 50)
5. Bot runs the session, one card/question at a time
6. Once the limit is reached — a summary screen: cards reviewed, know/don't-know
   split, with **🔄 Try Again** (same mode, deck, and count) and **🏠 Main Menu**

> Telegram bots can't detect swipe gestures, so "swipe left/right" is
> implemented as a button tap instead. Functionally the same thing.

## 🖼 Images

Every screen sends a themed banner image alongside its text — a different
one per deck (idioms/stress/lexical) and per static screen (main menu, mode
picker, stats, settings, about, session summary). These are generated flat
icons under `Assets/Images/`, not photos of individual card content — with
800+ cards across three decks, illustrating each one individually isn't
practical, so the deck's banner stays on screen throughout that deck's
session instead.

## 🗂 Data

Three card decks, each loaded from its own JSON file in `Data/`:

- **📖 Idioms** (`phraseologisms.json`) — 244 Ukrainian idioms with explanations
- **🔤 Word Stress** (`naholosy.json`) — 232 words where the stressed syllable
  is easy to get wrong (front shows the plain word, back shows the correct stress)
- **✏️ Lexical Mistakes** (`leksychni_pomylky.json`) — 328 common wrong-word
  usages and their correct counterparts

In **Choose Correct** mode, the 2 wrong answers are picked randomly from
other cards in the same deck each time — they're not fixed distractors.

## 🛠 Tech stack

- [.NET 8](https://dotnet.microsoft.com/) / C#
- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) — the official .NET wrapper for the Telegram Bot API
- Long polling (no webhooks — no need for a public HTTPS server)
- Simple file-based persistence (`userdata.json`) for stats and language — no external DB needed

> **Upgrading from an older version:** stats are now tracked separately per
> game mode. If you have an existing `userdata.json` from before Choose
> Correct mode existed, its old combined totals won't carry over — stats
> start fresh at zero. Nothing else is affected.

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
✅ NMT Learner (@your_bot_username) запущено.
   📖 Фразеологізми: 244 карток
   🔤 Наголоси: 232 карток
   ✏️ Лексичні помилки: 328 карток
```

Open the bot in Telegram and send `/start`.

### Renaming the bot itself

The code controls what the bot *says*, but its Telegram display name and
`@username` are separate — set those with [@BotFather](https://t.me/BotFather):
send `/setname` to rename the display name (e.g. "NMT Learner Bot"), or
`/setusername` to change the `@handle`. No code or restart needed.

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
│   └── UpdateHandler.cs            # routes all callbacks and commands, sends photo screens
├── Models/
│   ├── Phraseologism.cs            # card model (used by every deck)
│   ├── UserSession.cs              # in-memory session state (nav state, mode, deck, choices)
│   ├── UserData.cs                 # persistent data (language, stats per game mode)
│   ├── GameMode.cs                 # Flip / ChooseCorrect enum
│   └── Language.cs                 # UA/EN enum
├── Services/
│   ├── PhraseologismRepository.cs  # loads cards, random picks, distractors for quizzes
│   ├── SessionManager.cs           # in-memory user sessions
│   ├── UserDataStore.cs            # file-based persistence (userdata.json)
│   └── UiRenderer.cs               # text + keyboard + image for every screen
├── Resources/
│   ├── Strings.cs                  # all UA/EN UI text in one place
│   ├── Decks.cs                    # deck definitions: data file, name, labels, banner image
│   └── ScreenImages.cs             # banner image filenames for non-deck screens
├── Data/
│   ├── phraseologisms.json         # idioms deck
│   ├── naholosy.json               # word stress deck
│   └── leksychni_pomylky.json      # lexical mistakes deck
├── Assets/Images/                  # generated banner images, one per screen/deck
├── appsettings.example.json        # config template (no real token)
└── appsettings.json                # your real token (gitignored, not in the repo)
```

## 🧭 How navigation is wired up (for devs)

Callback data always follows `namespace:action[:param]`:

| Example                | Meaning                                    |
|--------------------------|---------------------------------------------|
| `nav:main`               | Show the main menu                          |
| `nav:flashcards`         | Show the mode picker (Flip / Choose Correct) |
| `nav:stats` / `nav:settings` / `nav:about` | The matching static submenu   |
| `mode:choose:choose`     | Pick "Choose Correct" mode, then show the deck picker |
| `deck:choose:stress`     | Pick the "Word Stress" deck, then show the count picker |
| `fc:count:15`            | Start a session with 15 cards (current mode + deck) |
| `fc:flip:42`             | Flip mode: flip card id=42                   |
| `fc:know:42` / `fc:dontknow:42` | Flip mode: mark the card, show the next one |
| `fc:pick:1`              | Choose Correct mode: submit answer option 1 (0-indexed) |
| `fc:next`                | Choose Correct mode: advance after seeing the result |
| `settings:lang:EN`       | Switch language to English                   |

User state (`SessionState`: Idle / ChoosingMode / ChoosingDeck / ChoosingCount /
Active / Ended) lives in `UserSession` and is used to ignore stale callbacks
(e.g. tapping a button on a card from a session that already ended) — no
crashes, no dead-end screens.

## ➕ Add your own cards

Open the matching file in `Data/` and add a new entry to the array:

```json
{
  "text": "front side text",
  "explanation": "back side text"
}
```

The `id` field is optional — it's assigned automatically if missing.

## ➕ Add a whole new deck

1. Drop a new JSON file in `Data/` (same `{ "text", "explanation" }` shape)
2. Add it to `QuizletBot.csproj` under `CopyToOutputDirectory`
3. Drop a banner image (same 800×450 style as the others) in `Assets/Images/`
4. Add a new `DeckDefinition` entry in `Resources/Decks.cs` with front/back
   labels and the image filename

It'll show up in the deck picker automatically — no changes needed anywhere else.

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
