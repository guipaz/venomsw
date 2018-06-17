using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VenomSW;

namespace VenomTools
{
    public partial class Configurer : Form
    {
        static string PATH = "E:\\dev\\venomsw\\configurer\\";

        Dictionary<string, List<BoundRect>> selectedRectangles = new Dictionary<string, List<BoundRect>>();
        Dictionary<string, Bitmap> selectedImages = new Dictionary<string, Bitmap>();

        public Configurer()
        {
            InitializeComponent();
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            string identifier = listBox1.SelectedItem.ToString();
            Bitmap image = User32.CaptureApplication("mobizen");
            
            ConfigurerPrint p = new ConfigurerPrint(this, identifier, null, image);
            p.ShowDialog();
        }

        internal void Set(string identifier, List<BoundRect> rects, Bitmap image)
        {
            selectedRectangles[identifier] = rects;
            selectedImages[identifier] = image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            string identifier = listBox1.SelectedItem.ToString();

            if (!selectedImages.ContainsKey(identifier) || !selectedRectangles.ContainsKey(identifier))
                return;

            List<BoundRect> rects = null;
            if (selectedRectangles.ContainsKey(identifier))
                rects = selectedRectangles[identifier];

            Bitmap image = selectedImages[identifier];

            ConfigurerPrint p = new ConfigurerPrint(this, identifier, rects, image);
            p.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            JArray patterns = new JArray();
            RectangleConverter rc = new RectangleConverter();

            foreach (var item in listBox1.Items)
            {
                string identifier = item.ToString();

                if (!selectedRectangles.ContainsKey(identifier) ||
                    !selectedImages.ContainsKey(identifier))
                    continue;

                JArray rectArray = new JArray();
                foreach (var r in selectedRectangles[identifier])
                {
                    JObject rectObj = new JObject();
                    rectObj["rect"] = rc.ConvertToString(r.Rectangle);
                    rectObj["type"] = r.Type;
                    rectArray.Add(rectObj);
                }
                    
                
                selectedImages[identifier].Save(PATH + identifier + ".png");

                JObject pattern = new JObject();
                pattern["identifier"] = identifier;
                pattern["rects"] = rectArray;
                patterns.Add(pattern);
            }

            JObject root = new JObject();
            root["patterns"] = patterns;
            File.WriteAllText(PATH + "config.json", JsonConvert.SerializeObject(root));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                RectangleConverter rc = new RectangleConverter();
                JObject root = JsonConvert.DeserializeObject(File.ReadAllText(ofd.FileName)) as JObject;
                JArray patterns = root["patterns"] as JArray;
                foreach (JObject pattern in patterns)
                {
                    string identifier = pattern["identifier"].ToString();
                    JArray rectsArray = pattern["rects"] as JArray;

                    selectedRectangles[identifier] = new List<BoundRect>();
                    foreach (var rs in rectsArray)
                    {
                        JObject obj = rs as JObject;

                        Rectangle rect = (Rectangle)rc.ConvertFromString((string)rs["rect"]);
                        int type = int.Parse((string)rs["type"]);

                        BoundRect br = new BoundRect();
                        br.Rectangle = rect;
                        br.Type = type;

                        selectedRectangles[identifier].Add(br);
                    }

                    selectedImages[identifier] = new Bitmap(PATH + identifier + ".png");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            string identifier = listBox1.SelectedItem.ToString();
            Bitmap image = new Bitmap(PATH + "example\\" + identifier + ".png");

            ConfigurerPrint p = new ConfigurerPrint(this, image);
            p.ShowDialog();
        }
    }
}
