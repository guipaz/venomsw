using ImageDiff.Analyzers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using VenomSW.RuneAnalyzer;

namespace VenomSW
{
    public class TesseractTest
    {
        private static BasicAnalyzer analyzer = new BasicAnalyzer();

        public static void Run()
        {
            var bitmap = new Bitmap("E:\\dev\\venomsw\\images\\cropped_rune.png");
            var scaled = ScaleBitmap(bitmap, bitmap.Width * 2, bitmap.Height * 2);
            
            //TODO debug only
            if (analyzer.ShouldGetRune(scaled))
                return;

            
            
            TesseractEngine engine = new TesseractEngine(@"E:\\dev\\venomsw\\venomsw\\tessdata", "eng",
                                                         EngineMode.Default, "venom");
            engine.SetVariable("language_model_penalty_non_freq_dict_word", "1");
            engine.SetVariable("language_model_penalty_non_dict_word", "1");
            
            using (Page page = engine.Process(scaled, PageSegMode.SingleBlock))
            {
                Console.WriteLine(page.GetText());
            }

            scaled.Save("E:\\dev\\venomsw\\images\\cropped_rune3_nobg.png");
            
        }

        public static void RunOld()
        {
            Bitmap bmp = new Bitmap("./rune_test.png");

            

            //List<Rectangle> rects = Split(newBitmap);

            //int i = 0;
            //foreach (Rectangle r in rects)
            //{
            //    Comparer.Crop(bmp, r).Save("./crops/b" + i + ".png");
            //    i++;
            //}
                

            Console.ReadKey();

            //newBitmap.Save("./newBmp.png");
        }

        private static Bitmap ScaleBitmap(Image original, int width, int height) {
            Bitmap result = new Bitmap(original, width, height);
            Graphics g = null;
            try
            {
                g = Graphics.FromImage(result);
                g.Clear(Color.Transparent);
                g.DrawImage(original, 0, 0, width, height);
            }
            finally
            {
                if (g != null)
                {
                    g.Dispose();
                }
            }

            return result;
        }

        private static Bitmap CropColor(Bitmap bmp)
        {
            var forbiddenColors = new List<Color>
            {
                Color.FromArgb(37, 24, 15)
            };

            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);

            for (var x = 0; x < bmp.Width; x++)
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    var firstColor = bmp.GetPixel(x, y);
                    var firstLab = CIELab.FromRGB(firstColor);

                    foreach (var secondColor in forbiddenColors)
                    {
                        var secondLab = CIELab.FromRGB(secondColor);

                        var score = Math.Sqrt(Math.Pow(secondLab.L - firstLab.L, 2) +
                                              Math.Pow(secondLab.a - firstLab.a, 2) +
                                              Math.Pow(secondLab.b - firstLab.b, 2));

                        if (score > 0.5f)
                        {
                            newBitmap.SetPixel(x, y, firstColor);
                        }
                    }
                }
            }

            return newBitmap;
        }

        private static List<Rectangle> Split(Bitmap bmp)
        {
            List<Rectangle> rects = new List<Rectangle>();
            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).A == 255)
                    {
                        bool contains = false;
                        foreach (Rectangle r in rects)
                        {
                            if (r.Contains(x, y))
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (contains)
                            continue;

                        rects.Add(SplitInner(bmp, new Rectangle(x, y, 1, 1)));
                    }
                }
            }
            
            return rects;
        }

        private static Rectangle SplitInner(Bitmap bmp, Rectangle origin)
        {
            bool added = false;

            // left
            int j = 5;
            while (j > 0)
            {
                if (IsValidPixel(bmp, origin.X - j, origin.Y) ||
                    IsValidPixel(bmp, origin.X - j, origin.Y + origin.Height))
                {
                    origin.X -= j;
                    added = true;
                    break;
                }

                j--;
            }

            // right
            j = 5;
            while (j > 0)
            {
                if (IsValidPixel(bmp, origin.X + origin.Width + j, origin.Y) ||
                    IsValidPixel(bmp, origin.X + origin.Width + j, origin.Y + origin.Height))
                {
                    origin.Width += j + 1;
                    added = true;
                    break;
                }

                j--;
            }

            // up
            j = 3;
            while (j > 0)
            {
                if (IsValidPixel(bmp, origin.X, origin.Y - j) ||
                    IsValidPixel(bmp, origin.X + origin.Width, origin.Y - j))
                {
                    origin.Y -= j;
                    added = true;
                    break;
                }

                j--;
            }

            // down
            j = 3;
            while (j > 0)
            {
                if (IsValidPixel(bmp, origin.X, origin.Y + origin.Height + j) ||
                    IsValidPixel(bmp, origin.X + origin.Width, origin.Y + origin.Height + j))
                {
                    origin.Height += j + 1;
                    added = true;
                    break;
                }

                j--;
            }
            
            if (!added)
                return origin;

            return SplitInner(bmp, origin);
        }

        private static bool IsValidPixel(Bitmap bmp, int x, int y)
        {
            return x >= 0 && y >= 0 && x < bmp.Width && y < bmp.Height && bmp.GetPixel(x, y).A == 255;
        }
    }
}
