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
        //public int NeonR { get; set; }
        //public int NeonG { get; set; }
        //public int NeonB { get; set; }
        public Color TireSmoke { get; set; }
        //public int TireSmokeR { get; set; }
        //public int TireSmokeG { get; set; }
        //public int TireSmokeB { get; set; }
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
            //NeonR = default(int);
            //NeonG = default(int);
            //NeonB = default(int);
            TireSmoke = default(Color);
            //TireSmokeR = default(int);
            //TireSmokeG = default(int);
            //TireSmokeB = default(int);
            Trim = default(VehicleColor);
            Dashboard = default(VehicleColor);
        }
    }
}