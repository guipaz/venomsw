using ImageDiff;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenomSW
{
    public class Comparer
    {
        BitmapComparer innerComparer;

        static int comparedId = 0;

        public Comparer()
        {
            var options = new CompareOptions
            {
                AnalyzerType = AnalyzerTypes.CIE76,
                JustNoticeableDifference = 3,
                DetectionPadding = 2,
                Labeler = LabelerTypes.ConnectedComponentLabeling,
                BoundingBoxColor = Color.Red,
                BoundingBoxPadding = 2,
                BoundingBoxMode = BoundingBoxModes.Multiple
            };

            innerComparer = new BitmapComparer(options);
        }

        public bool IsValid(Pattern referencePattern, Bitmap destinationImage, float percentage = 0.8f)
        {
            if (referencePattern.coordinates.IsEmpty)
                return false;

            Bitmap b = Resize(destinationImage, referencePattern.original.Size);
            Bitmap cropped = Crop(b, referencePattern.coordinates);
            Bitmap pattern = referencePattern.reference;

            if (Runner.RUN_DEBUG)
            {
                b.Save("E:\\dev\\venomsw\\images\\saved\\" + comparedId + "_reference" + ".png");
                cropped.Save("E:\\dev\\venomsw\\images\\saved\\" + comparedId++ + "_compared" + ".png");
                pattern.Save("E:\\dev\\venomsw\\images\\saved\\" + comparedId++ + "_pattern" + ".png");
            }
            
            float equality = innerComparer.GetEquality(referencePattern.reference, cropped);
            if (Runner.RUN_DEBUG)
                Console.WriteLine("Equality: " + equality);

            return equality >= percentage;
        }

        public static Bitmap Resize(Bitmap src, Size size)
        {
            return new Bitmap(src, size);
        }

        public static Bitmap Crop(Bitmap src, Rectangle cropRect)
        {
            if (cropRect.IsEmpty)
                return src;

            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }
    }
}
