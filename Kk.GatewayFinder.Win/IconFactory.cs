using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Kk.GatewayFinder.Win
{
    internal static class IconFactory
    {
        private static readonly Lazy<Icon> CachedIcon = new Lazy<Icon>(CreateIcon);

        public static Icon Create() => (Icon)CachedIcon.Value.Clone();

        private static Icon CreateIcon()
        {
            using var bitmap = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var rect = new RectangleF(0, 0, bitmap.Width, bitmap.Height);
                using (var background = new LinearGradientBrush(rect,
                           Color.FromArgb(0x34, 0x4A, 0x5F),
                           Color.FromArgb(0x25, 0x69, 0xAF),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(background, rect);
                }

                var body = new RectangleF(bitmap.Width * 0.13f, bitmap.Height * 0.42f, bitmap.Width * 0.62f, bitmap.Height * 0.32f);
                FillRounded(g, body, bitmap.Width * 0.06f, Color.FromArgb(0x5C, 0x86, 0xA6));

                var antenna = new RectangleF(bitmap.Width * 0.75f, bitmap.Height * 0.18f, bitmap.Width * 0.08f, bitmap.Height * 0.48f);
                FillRounded(g, antenna, bitmap.Width * 0.02f, Color.FromArgb(0x3D, 0x44, 0x56));

                DrawWifi(g, new PointF(body.X + body.Width / 2, bitmap.Height * 0.24f), bitmap.Width);

                var indicatorColors = new[]
                {
                    Color.FromArgb(0x72, 0xE1, 0xCC),
                    Color.FromArgb(0x72, 0xE1, 0xCC),
                    Color.FromArgb(0xFF, 0xD8, 0x5E),
                    Color.FromArgb(0xFF, 0xD8, 0x5E)
                };

                var diameter = bitmap.Width * 0.07f;
                for (var i = 0; i < indicatorColors.Length; i++)
                {
                    using var brush = new SolidBrush(indicatorColors[i]);
                    var x = body.X + bitmap.Width * 0.08f + i * (diameter * 0.95f);
                    var y = body.Bottom - bitmap.Height * 0.12f;
                    g.FillEllipse(brush, x, y, diameter, diameter);
                }
            }

            using var pngStream = new MemoryStream();
            bitmap.Save(pngStream, ImageFormat.Png);
            var pngBytes = pngStream.ToArray();

            using var iconStream = new MemoryStream();
            WriteIconHeader(iconStream, bitmap.Width, bitmap.Height, pngBytes.Length);
            iconStream.Write(pngBytes, 0, pngBytes.Length);
            iconStream.Position = 0;
            return new Icon(iconStream);
        }

        private static void FillRounded(Graphics g, RectangleF rect, float radius, Color color)
        {
            using var path = new GraphicsPath();
            var diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
        }

        private static void DrawWifi(Graphics g, PointF center, int size)
        {
            for (var i = 0; i < 3; i++)
            {
                var radius = size * 0.08f + i * size * 0.03f;
                var rect = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                using var pen = new Pen(Color.FromArgb(0xFF, 0x70, 0x70), size * 0.02f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                g.DrawArc(pen, rect, 205, 130);
            }
        }

        private static void WriteIconHeader(Stream stream, int width, int height, int pngLength)
        {
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)(width >= 256 ? 0 : width));
            writer.Write((byte)(height >= 256 ? 0 : height));
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write(pngLength);
            writer.Write(6 + 16);
        }
    }
}
