using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenomSW
{
    public class Pattern
    {
        public string name;
        public Bitmap original;
        public Bitmap reference;
        public Rectangle coordinates;
        public Rectangle click;
        public Rectangle option;

        static bool GENERATE_CROP = true;
        static Dictionary<string, Pattern> patterns = new Dictionary<string, Pattern>();

        public Pattern(string name, Bitmap reference, Rectangle coordinates, Rectangle? clickRect = null, Rectangle? optionRect = null)
        {
            this.name = name;
            this.original = reference;
            this.reference = reference;
            this.coordinates = coordinates;
            this.click = clickRect ?? Rectangle.Empty;
            this.option = optionRect ?? Rectangle.Empty;

            if (GENERATE_CROP && reference != null)
                this.reference = Comparer.Crop(reference, coordinates);
        }
        
        public static Pattern Get(string key)
        {
            if (patterns.ContainsKey(key))
                return patterns[key];
            return null;
        }

        public static void Create(string key, Bitmap reference, Rectangle rectangle, Rectangle? click, Rectangle? option)
        {
            patterns[key] = new Pattern(key, reference, rectangle, click, option);
        }

        public static List<Pattern> All
        {
            get
            {
                return patterns.Values.ToList();
            }
        }

        public static void Load()
        {
            RectangleConverter rc = new RectangleConverter();
            JObject root = JsonConvert.DeserializeObject(File.ReadAllText(Runner.PATH + "configurer\\config.json")) as JObject;
            JArray patterns = root["patterns"] as JArray;
            foreach (JObject pattern in patterns)
            {
                string identifier = pattern["identifier"].ToString();

                JArray rects = pattern["rects"] as JArray;

                Rectangle? coordRect = null;
                Rectangle? clickRect = null;
                Rectangle? optionRect = null;
                foreach (var jo in rects)
                {
                    Rectangle rect = (Rectangle)rc.ConvertFromString((string)jo["rect"]);
                    int type = int.Parse((string)jo["type"]);
                    if (type == 2)
                        clickRect = rect;
                    else if (type == 3)
                        optionRect = rect;
                    else
                        coordRect = rect;
                }
                
                Create(identifier, new Bitmap(Runner.PATH + "configurer\\" + identifier + ".png"), (Rectangle) coordRect, clickRect, optionRect);
            }
        }

        public float GetRatio(Bitmap current)
        {
            return current.Width / (float)original.Width;
        }
    }
}
