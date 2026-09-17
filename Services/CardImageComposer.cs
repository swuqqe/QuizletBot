using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace QuizletBot.Services;

// Which state the card image should look like - each gets its own color and
// a small corner glyph, so the picture visibly changes when you flip a card
// or find out if you got a quiz question right, not just the caption text.
public enum CardStyle
{
    Question,   // front of the card / quiz question - blue
    Answer,     // flipped card back - teal
    Correct,    // quiz: picked the right answer - green
    Incorrect   // quiz: picked the wrong answer - red
}

// Renders a card's text as the entire image - a plain colored background with
// the word/phrase filling most of it, plus a small corner glyph that marks
// the card's state. No deck icon, no clutter - just text and a state marker.
public class CardImageComposer
{
    private const int Width = 800;
    private const int Height = 450;

    private static readonly Dictionary<CardStyle, ((byte r, byte g, byte b) top, (byte r, byte g, byte b) bottom)> Palettes = new()
    {
        [CardStyle.Question] = ((66, 165, 245), (13, 71, 161)),   // Material Blue
        [CardStyle.Answer] = ((38, 198, 218), (0, 121, 107)),     // Teal
        [CardStyle.Correct] = ((102, 187, 106), (27, 94, 32)),    // Green
        [CardStyle.Incorrect] = ((255, 112, 67), (183, 28, 28))   // Red/orange
    };

    private readonly FontFamily _fontFamily;
    private static readonly int[] FontSizes = { 88, 76, 64, 54, 46, 38, 32, 27 };

    public CardImageComposer(string fontPath)
    {
        var collection = new FontCollection();
        _fontFamily = collection.Add(fontPath);
    }

    public byte[] Compose(string text, CardStyle style = CardStyle.Question)
    {
        var (top, bottom) = Palettes[style];
        using var image = new Image<Rgba32>(Width, Height);

        image.Mutate(ctx =>
        {
            // Simple vertical gradient, drawn as thin horizontal bands - avoids
            // depending on a specific gradient-brush API surface.
            const int bands = 60;
            for (var i = 0; i < bands; i++)
            {
                var t = i / (float)(bands - 1);
                var r = (byte)(top.r + (bottom.r - top.r) * t);
                var g = (byte)(top.g + (bottom.g - top.g) * t);
                var b = (byte)(top.b + (bottom.b - top.b) * t);
                var y0 = Height * i / (float)bands;
                var y1 = Height * (i + 1) / (float)bands;
                var band = new RectangleF(0, y0, Width, y1 - y0 + 1);
                ctx.Fill(Color.FromRgb(r, g, b), new RectangularPolygon(band));
            }
        });

        DrawCornerGlyph(image, style);

        var maxWidth = Width * 0.84f;
        var maxHeight = Height * 0.62f;

        var font = _fontFamily.CreateFont(FontSizes[^1], FontStyle.Bold);
        var lines = new List<string> { text };

        foreach (var size in FontSizes)
        {
            var candidateFont = _fontFamily.CreateFont(size, FontStyle.Bold);
            var wrapped = WrapText(text, candidateFont, maxWidth);
            var totalHeight = wrapped.Count * candidateFont.Size * 1.25f;

            if (totalHeight <= maxHeight || size == FontSizes[^1])
            {
                font = candidateFont;
                lines = wrapped;
                break;
            }
        }

        var lineHeight = font.Size * 1.25f;
        var blockHeight = lines.Count * lineHeight;
        var startY = Height / 2f - blockHeight / 2f + 20; // nudge down a bit to clear the corner glyph

        image.Mutate(ctx =>
        {
            var y = startY;
            foreach (var line in lines)
            {
                var size = TextMeasurer.MeasureSize(line, new TextOptions(font));
                var x = Width / 2f - size.Width / 2f;
                ctx.DrawText(line, font, Color.White, new PointF(x, y));
                y += lineHeight;
            }
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    // A small state marker in the top-left corner: "?" for a question, "!" for
    // a revealed answer, "OK"/"X" for quiz results. Plain rectangle + text only -
    // no circle or path-stroke APIs, to keep this on well-proven ground.
    private void DrawCornerGlyph(Image<Rgba32> image, CardStyle style)
    {
        const int size = 68;
        const int margin = 24;

        var label = style switch
        {
            CardStyle.Correct => "OK",
            CardStyle.Incorrect => "X",
            CardStyle.Answer => "!",
            _ => "?"
        };

        var badgeFont = _fontFamily.CreateFont(30, FontStyle.Bold);

        image.Mutate(ctx =>
        {
            var badgeRect = new RectangleF(margin, margin, size, size);
            ctx.Fill(Color.FromRgba(255, 255, 255, 55), new RectangularPolygon(badgeRect));

            var textSize = TextMeasurer.MeasureSize(label, new TextOptions(badgeFont));
            var tx = margin + (size - textSize.Width) / 2f;
            var ty = margin + (size - textSize.Height) / 2f;
            ctx.DrawText(label, badgeFont, Color.White, new PointF(tx, ty));
        });
    }

    private static List<string> WrapText(string text, Font font, float maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var current = "";

        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : $"{current} {word}";
            var width = TextMeasurer.MeasureSize(candidate, new TextOptions(font)).Width;

            if (width > maxWidth && current.Length > 0)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0) lines.Add(current);
        return lines;
    }
}
