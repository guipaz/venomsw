using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VenomTools
{
    public partial class ConfigurerPrint : Form
    {
        Configurer parent;
        string identifier;
        Bitmap image;
        bool readOnly;
        
        public ConfigurerPrint(Configurer parent, Bitmap image)
        {
            this.parent = parent;
            this.image = image;
            readOnly = true;

            InitializeComponent();
            printSelectionPanel1.Initialize(image, this);
            printSelectionPanel1.Enabled = false;
        }

        public ConfigurerPrint(Configurer parent, string identifier, List<BoundRect> rects, Bitmap image)
        {
            this.parent = parent;
            this.identifier = identifier;
            this.image = image;
            
            InitializeComponent();
            printSelectionPanel1.Initialize(image, this);
            printSelectionPanel1.SetRects(rects);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == (char) 13 && printSelectionPanel1.IsSet && !readOnly) // enter
            {
                parent.Set(identifier, printSelectionPanel1.GetRects(), image);
                Close();
            } else if (e.KeyChar == (char) 27) // esc
            {
                Close();
            } else if (e.KeyChar == (char) 49) // 1
            {
                printSelectionPanel1.SetType(1);
            } else if (e.KeyChar == (char) 50) // 2
            {
                printSelectionPanel1.SetType(2);
            } else if (e.KeyChar == (char) 51) // 3
            {
                printSelectionPanel1.SetType(3);
            }
        }
    }
}
