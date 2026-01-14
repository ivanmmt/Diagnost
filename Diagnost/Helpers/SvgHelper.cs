using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Avalonia.Media;

namespace Diagnost.Helpers
{
    public static class SvgHelper
    {
        /// <summary>
        /// Загружает SVG по URI, меняет в нем все цвета на указанный RGB и возвращает Bitmap.
        /// </summary>
        public static Bitmap? LoadSvgWithColor(Uri assetUri, byte r, byte g, byte b)
        {
            try
            {
                // 1. Читаем файл SVG из ресурсов
                using var assetStream = Avalonia.Platform.AssetLoader.Open(assetUri);
                using var reader = new System.IO.StreamReader(assetStream, System.Text.Encoding.UTF8);
                string svgContent = reader.ReadToEnd();

                if (string.IsNullOrEmpty(svgContent)) return null;

                // 2. Меняем цвет внутри XML
                string coloredSvgContent = ChangeSvgColorXml(svgContent, r, g, b);

                // 3. Рендерим SVG в картинку (Bitmap)
                return RenderSvgToBitmap(coloredSvgContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обработки SVG: {ex.Message}");
                return null;
            }
        }

        private static string ChangeSvgColorXml(string svgContent, byte r, byte g, byte b)
        {
            // Убеждаемся, что мы используем System.Xml.Linq
            XDocument doc = System.Xml.Linq.XDocument.Parse(svgContent);
            string hexColor = $"#{r:X2}{g:X2}{b:X2}";

            foreach (var element in doc.Descendants())
            {
                // Меняем заливку (fill)
                var fill = element.Attribute("fill");
                if (fill != null && fill.Value != "none" && fill.Value != "transparent" && !fill.Value.StartsWith("url"))
                {
                    fill.Value = hexColor;
                }
                // Если заливки нет, добавляем её
                else if (fill == null && element.Name.LocalName != "svg" && element.Name.LocalName != "g")
                {
                    element.SetAttributeValue("fill", hexColor);
                }

                // Меняем обводку (stroke)
                var stroke = element.Attribute("stroke");
                if (stroke != null && stroke.Value != "none" && !stroke.Value.StartsWith("url"))
                {
                    stroke.Value = hexColor;
                }
            }

            return doc.ToString();
        }

        private static Bitmap? RenderSvgToBitmap(string svgContent)
        {
            // Используем Svg.Skia для отрисовки
            var svg = new SKSvg();
            
            // Загружаем SVG из строки через MemoryStream
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent)))
            {
                svg.Load(stream);
            }

            if (svg.Picture == null) return null;

            // Размеры для рендеринга
            const int targetSize = 400; 
            int width = targetSize;
            int height = targetSize;

            var info = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(info);
            if (surface == null) return null;

            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            
            // Масштабирование и центрирование
            var bounds = svg.Picture.CullRect;
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                var scaleX = width / bounds.Width;
                var scaleY = height / bounds.Height;
                var scale = Math.Min(scaleX, scaleY);

                canvas.Scale(scale);
                canvas.Translate((width / scale - bounds.Width) / 2, (height / scale - bounds.Height) / 2);
            }

            canvas.DrawPicture(svg.Picture);

            // Сохраняем результат в Bitmap
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var resultStream = new System.IO.MemoryStream();
            data.SaveTo(resultStream);
            resultStream.Position = 0;

            return new Bitmap(resultStream);
        }

        public static IImage? LoadSvg(Uri uri)
        {
            try
            {
                using var stream = AssetLoader.Open(uri);
                var svg = new SKSvg();
                svg.Load(stream);
                if (svg.Picture == null) return null;

                const int targetSize = 400;
                var info = new SKImageInfo(targetSize, targetSize);
                using var surface = SKSurface.Create(info);
                if (surface == null) return null;

                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                var bounds = svg.Picture.CullRect;
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    var scaleX = targetSize / bounds.Width;
                    var scaleY = targetSize / bounds.Height;
                    var scale = Math.Min(scaleX, scaleY);
                    canvas.Scale(scale);
                    canvas.Translate((targetSize / scale - bounds.Width) / 2, (targetSize / scale - bounds.Height) / 2);
                }

                canvas.DrawPicture(svg.Picture);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using var resultStream = new MemoryStream();
                data.SaveTo(resultStream);
                resultStream.Position = 0;
                return new Bitmap(resultStream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки SVG: {ex.Message}");
                return null;
            }
        }
    }
}