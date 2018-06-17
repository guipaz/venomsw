using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VenomTools
{
    public class PrintSelectionPanel : Panel
    {
        public bool IsSet { get { return rectSet; } }

        List<BoundRect> rects;

        int currentType = 1;
        Rectangle currentRect;
        bool rectSet;
        Bitmap image;
        bool dragging;
        Point start;
        Point end;
        
        public List<BoundRect> GetRects()
        {
            return rects;
        }

        public void SetType(int type)
        {
            currentType = type;
        }

        public void Initialize(Bitmap image, Form form)
        {
            rects = new List<BoundRect>();
            this.image = image;

            Size = new Size(image.Width, image.Height);
            form.Size = new Size(Size.Width + 50, Size.Height + 50);

            dragging = false;
            rectSet = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            dragging = true;
            start = e.Location;
            end = start;

            // foda-se
            BoundRect delete = null;
            foreach (BoundRect b in rects)
            {
                if (b.Type == currentType)
                {
                    delete = b;
                    break;
                }
            }
            if (delete != null)
                rects.Remove(delete);

            Refresh();
        }

        public void SetRects(List<BoundRect> rects)
        {
            if (rects != null)
            {
                this.rects = new List<BoundRect>(rects);
                Refresh();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            dragging = false;
            rectSet = true;
            Parent.Text = currentRect.ToString();

            BoundRect r = new BoundRect();
            r.Type = currentType;
            r.Rectangle = currentRect;
            rects.Add(r);

            Refresh();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragging)
            {
                end = e.Location;

                currentRect.X = start.X;
                currentRect.Y = start.Y;
                currentRect.Width = end.X - start.X;
                currentRect.Height = end.Y - start.Y;

                Refresh();
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(image, Point.Empty);
            
            foreach (BoundRect r in rects)
            {
                Pen p = Pens.Red;
                if (r.Type == 2)
                    p = Pens.Yellow;
                else if (r.Type == 3)
                    p = Pens.Blue;

                e.Graphics.DrawRectangle(p, r.Rectangle);
            }

            if (dragging)
            {
                Pen p = Pens.Red;
                if (currentType == 2)
                    p = Pens.Yellow;
                else if (currentType == 3)
                    p = Pens.Blue;

                e.Graphics.DrawRectangle(p, currentRect);
            }
        }
    }
}
