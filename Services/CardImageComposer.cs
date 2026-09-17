using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace QuizletBot.Services;

// Renders a card's text as the entire image - a plain blue background with
// the word/phrase filling most of it. Nothing else on the picture: no deck
// icon, no badge, just the text itself, as large as it'll fit.
public class CardImageComposer
{
    private const int Width = 800;
    private const int Height = 450;

    // Material Blue, top to bottom
    private static readonly (byte r, byte g, byte b) Top = (66, 165, 245);
    private static readonly (byte r, byte g, byte b) Bottom = (13, 71, 161);

    private readonly FontFamily _fontFamily;
    private static readonly int[] FontSizes = { 88, 76, 64, 54, 46, 38, 32, 27 };

    public CardImageComposer(string fontPath)
    {
        var collection = new FontCollection();
        _fontFamily = collection.Add(fontPath);
    }

    public byte[] Compose(string text)
    {
        using var image = new Image<Rgba32>(Width, Height);

        image.Mutate(ctx =>
        {
            // Simple vertical gradient, drawn as thin horizontal bands - avoids
            // depending on a specific gradient-brush API surface.
            const int bands = 60;
            for (var i = 0; i < bands; i++)
            {
                var t = i / (float)(bands - 1);
                var r = (byte)(Top.r + (Bottom.r - Top.r) * t);
                var g = (byte)(Top.g + (Bottom.g - Top.g) * t);
                var b = (byte)(Top.b + (Bottom.b - Top.b) * t);
                var y0 = Height * i / (float)bands;
                var y1 = Height * (i + 1) / (float)bands;
                var band = new RectangleF(0, y0, Width, y1 - y0 + 1);
                ctx.Fill(Color.FromRgb(r, g, b), new RectangularPolygon(band));
            }
        });

        var maxWidth = Width * 0.84f;
        var maxHeight = Height * 0.72f;

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
        var startY = Height / 2f - blockHeight / 2f;

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
