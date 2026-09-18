# NMT Learner Bot — довідник екранів і промптів для зображень

Стиль для всіх зображень (додавайте до кожного промпту нижче):
> Photorealistic desk product photography, soft cream/beige background with
> pastel blue accent shapes, warm natural lighting, shallow depth of field,
> minimalist flat lay composition, no text in the image, consistent with a
> clean educational Telegram bot brand, blue and navy color accents (#1976D2,
> #0D47A1), square 1:1 or 800×450 composition.

---

## 1. 🏠 Головне меню
**Файл:** `main_menu.png`
**Кнопки:** 🚀 Розпочати · 📊 Статистика · ⚙️ Налаштування · ℹ️ Про бота
**Промпт:** A graduation cap resting on a small stack of closed books on a
clean desk, soft blue accent shape in the background, welcoming and
minimal composition, nothing else on the desk.

## 2. 🎴 Що вивчаємо? (вибір теми)
**Файл:** `deck_picker.png`
**Кнопки:** 📖 Фразеологізми · 🔤 Наголоси · ✏️ Лексичні помилки · ◀️ Назад
**Промпт:** A neat stack of three closed hardcover books of different pastel
colors (blue, white, navy), viewed at a slight angle, plain spines with no
visible text, soft shadow beneath.

## 3. 🎴 Вибір режиму
**Файл:** `mode_picker.png`
**Кнопки:** 🔄 Флеш-картки · ❓ Вибери правильну · ◀️ Назад
**Промпт:** Two rounded flip-card tiles side by side on a desk, one plain and
one with a small question-mark cutout shape, symbolizing two study modes,
soft blue lighting.

## 4. 🎴 Кількість карток
**Файл:** тема, яку обрали (`deck_idioms.png` / `deck_stress.png` / `deck_lexical.png`)
**Кнопки:** 5 / 15 / 25 / 50 · 🚀 Старт · ◀️ Назад
**Промпти (по одному на тему):**
- **Ідіоми:** An open notebook with a speech-bubble sticky note attached,
  pen resting beside it.
- **Наголоси:** A notebook page with a single large handwritten letter "А"
  with a small accent mark above it, pencil beside it.
- **Лексичні помилки:** A checklist notepad with two small checkbox icons
  drawn (one checked, one crossed out), pen resting on top.

## 5. 🎴 Флеш-картка — лицева сторона
**Динамічне зображення** (текст картки на суцільному синьому фоні, без промпту — генерується кодом)
**Кнопки:** 🔄 Показати пояснення · 🏠 Головне меню

## 6. 🎴 Флеш-картка — зворотна сторона
**Динамічне зображення** (бірюзовий фон + текст пояснення)
**Кнопки:** ⬅️ Не знаю · ➡️ Знаю · 🏠 Головне меню

## 7. 🎴 Тест "Вибери правильну" — питання
**Динамічне зображення** (синій фон + текст картки)
**Кнопки:** 1️⃣ / 2️⃣ / 3️⃣ · 🏠 Головне меню

## 8. 🎴 Тест "Вибери правильну" — результат
**Динамічне зображення** (зелений/червоний фон + текст пояснення)
**Кнопки:** ➡️ Далі · 🏠 Головне меню

## 9. ✅ Підсумок сесії
**Файл:** `session_end.png`
**Кнопки:** 🔄 Спробувати ще раз · 🏠 Головне меню
**Промпт:** A small gold medal with a checkmark engraved on it, lying on a
closed notebook, soft warm highlight, celebratory but minimal.

## 10. 📊 Статистика
**Файл:** `stats.png`
**Кнопки:** ◀️ Назад · 🏠 Головне меню
**Промпт:** A neat notebook page with three simple hand-drawn bar-chart
columns of increasing height, pencil resting beside it.

## 11. ⚙️ Налаштування
**Файл:** `settings.png`
**Кнопки:** 🇺🇦 UA · 🇬🇧 EN · ◀️ Назад
**Промпт:** A small metal gear/cog object resting on a desk next to a closed
notebook, soft blue accent lighting.

## 12. ℹ️ Про бота
**Файл:** `about.png`
**Кнопки:** ◀️ Назад · 🏠 Головне меню
**Промпт:** A smartphone lying on a desk showing a simple blue circular
info icon on its screen, a plant and pen nearby, same desk styling as the
reference image.

---

### Після генерації
Надішліть мені готові PNG (бажано 800×450 або квадратні — я підганю розмір),
і я вставлю їх у `Assets/Images/` замість поточних намальованих іконок.
Динамічні картки (пункти 5–8) лишаються текстовими — генерувати їх окремо
не потрібно, вони формуються кодом щоразу з новим текстом.
