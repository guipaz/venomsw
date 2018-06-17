using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenomSW
{
    public class State
    {
        public string name;
        public int cooldown;
        public Road[] roads;

        public State(string name, int cooldown, params Road[] roads)
        {
            this.name = name;
            this.cooldown = cooldown;
            this.roads = roads;
        }
    }
}
