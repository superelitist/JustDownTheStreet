using System.Drawing;
using GTA;

namespace JustDownTheStreet
{
    internal class Colors
    {
        public VehicleColor Primary { get; set; }
        public VehicleColor Secondary { get; set; }
        public VehicleColor Pearlescent { get; set; }
        //public bool CustomPrimary { get; set; }
        //public int CustomPrimaryR { get; set; }
        //public int CustomPrimaryG { get; set; }
        //public int CustomPrimaryB { get; set; }
        //public bool CustomSecondary { get; set; }
        //public int CustomSecondaryR { get; set; }
        //public int CustomSecondaryG { get; set; }
        //public int CustomSecondaryB { get; set; }
        public VehicleColor Rim { get; set; }
        public Color Neon { get; set; }
        public Color TireSmoke { get; set; }
        public VehicleColor Trim { get; set; }
        public VehicleColor Dashboard { get; set; }

        public Colors(VehicleColor primary, VehicleColor secondary, VehicleColor pearlescent)
        {
            Primary = primary;
            Secondary = secondary;
            Pearlescent = pearlescent;
            //CustomPrimary = default(bool);
            //CustomPrimaryR = default(int);
            //CustomPrimaryG = default(int);
            //CustomPrimaryB = default(int);
            //CustomSecondary = default(bool);
            //CustomSecondaryR = default(int);
            //CustomSecondaryG = default(int);
            //CustomSecondaryB = default(int);
            Rim = default(VehicleColor);
            Neon = default(Color);
            TireSmoke = default(Color);
            Trim = default(VehicleColor);
            Dashboard = default(VehicleColor);
        }
    }
}