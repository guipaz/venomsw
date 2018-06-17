using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenomSW
{
    public class Road
    {
        public Pattern pattern;
        public State destination;
        public int wait;
        public int clickX;
        public int clickY;
        public Action<Road> function;

        public Road(Pattern pattern, State destination, int clickX, int clickY, Action<Road> function = null, int wait = 0)
        {
            this.pattern = pattern;
            this.destination = destination;
            this.clickX = clickX;
            this.clickY = clickY;
            this.wait = wait;
            this.function = function;
        }
    }
}
