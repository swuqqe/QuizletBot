using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace QuizletBot.Services;

// Draws a card's text (the word/phrase itself) onto its deck's badge image,
// so it shows up on the picture instead of only in the caption below it.
public class CardImageComposer
{
    private readonly FontFamily _fontFamily;
    private static readonly int[] FontSizes = { 44, 38, 32, 27, 23, 20 };

    public CardImageComposer(string fontPath)
    {
        var collection = new FontCollection();
        _fontFamily = collection.Add(fontPath);
    }

    public byte[] Compose(string baseImagePath, string text)
    {
        using var image = Image.Load<Rgba32>(baseImagePath);

        var maxWidth = image.Width * 0.8f;
        var maxHeight = image.Height * 0.32f;

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
        var centerY = image.Height * 0.82f;
        var startY = centerY - blockHeight / 2f;

        image.Mutate(ctx =>
        {
            var widestLine = lines.Max(line => TextMeasurer.MeasureSize(line, new TextOptions(font)).Width);
            var pillWidth = Math.Min(widestLine + 56, image.Width - 40);
            var pillHeight = blockHeight + 28;
            var pillRect = new RectangleF(
                image.Width / 2f - pillWidth / 2f,
                startY - 14,
                pillWidth,
                pillHeight);

            ctx.Fill(Color.FromRgba(0, 0, 0, 140), new RectangularPolygon(pillRect));

            var y = startY;
            foreach (var line in lines)
            {
                var size = TextMeasurer.MeasureSize(line, new TextOptions(font));
                var x = image.Width / 2f - size.Width / 2f;
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
