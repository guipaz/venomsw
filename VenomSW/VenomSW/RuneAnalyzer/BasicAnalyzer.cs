using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace VenomSW.RuneAnalyzer
{
    public class BasicAnalyzer
    {
        private TesseractEngine engine;

        public BasicAnalyzer()
        {
            engine = new TesseractEngine(@"E:\\dev\\venomsw\\venomsw\\tessdata", "eng", EngineMode.Default, "venom");
        }

        public bool ShouldGetRune(Bitmap image)
        {
            var shouldGetRune = true;

            using (Page page = engine.Process(image, PageSegMode.Auto))
            {
                var parsedText = page.GetText();
                var rune = GetRune(parsedText);

                //TODO define if should get
            }

            return shouldGetRune;
        }

        private Rune GetRune(string parsedText)
        {
            Rune rune = new Rune();

            string[] lines = parsedText.Split('\n');
            int substatsCount = 0;
            foreach (string line in lines)
            {
                if (line.Trim().Length == 0)
                    continue;

                if (rune.MainStat == null)
                {
                    rune.MainStat = new Stat(line);
                } else
                {
                    rune.SubStats.Add(new Stat(line));
                    substatsCount++;
                }
            }

            RuneRarity rarity = RuneRarity.Common;
            if (substatsCount == 1)
                rarity = RuneRarity.Magic;
            else if (substatsCount == 2)
                rarity = RuneRarity.Rare;
            else if (substatsCount == 3)
                rarity = RuneRarity.Hero;
            else if (substatsCount == 4)
                rarity = RuneRarity.Legendary;
            rune.Rarity = rarity;

            Console.WriteLine(rune.ToString());

            return rune;
        }

        private enum RuneType
        {
            Unknown,
            Energy,
            Blade,
            Violent,
            Despair
        }

        private enum RuneRarity
        {
            Unknown,
            Common,
            Magic,
            Rare,
            Hero,
            Legendary
        }

        private enum StatType
        {
            Unknown, HP, ATK, DEF, SPD, CRIRate, CRIDmg, Resistance, Accuracy
        }
        
        private class Rune
        {
            public RuneType Type { get; set; }
            public RuneRarity Rarity { get; set; }
            public Stat MainStat { get; set; }
            public List<Stat> SubStats { get; set; }

            public Rune()
            {
                Type = RuneType.Unknown;
                Rarity = RuneRarity.Unknown;
                SubStats = new List<Stat>();
            }

            public override string ToString()
            {
                return "Type: " + Type + "\nRarity: " + Rarity + "\nMain: " + MainStat?.ToString() +
                       "\n----------\n" + string.Join("\n", SubStats) + "\n----------";
            }
        }

        private class Stat
        {
            public StatType Type { get; set; }
            public bool IsPercentual { get; set; }

            public Stat(string line)
            {
                Parse(line);
            }

            private void Parse(string line)
            {
                if (line.Contains("%") || line.Contains("96") || line.Contains("95"))
                {
                    IsPercentual = true;
                }

                var stringName = "";
                if (line.Contains("+"))
                {
                    stringName = line.Substring(0, line.IndexOf('+'));
                }
                else if (line.Contains(" "))
                {
                    stringName = line.Substring(0, line.LastIndexOf(' '));
                }
                else
                {
                    stringName = line;
                }

                stringName.Trim();

                Type = StatType.Unknown;
                var types = Enum.GetValues(typeof(StatType));
                foreach (var type in types)
                {
                    if (CalculateSimilarity(stringName, type.ToString()) >= 0.5f)
                    {
                        Type = (StatType) type;
                        break;
                    }
                }
            }

            public override string ToString()
            {
                return Type.ToString() + " (" + (IsPercentual ? "%" : "Flat") + ")";
            }
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        private static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}
