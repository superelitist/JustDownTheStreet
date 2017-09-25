// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//

using GTA;

namespace JustDownTheStreet
{
  internal class VehicleDefinition
  {
    public string VehicleName { get; set; }
    public string Plate { get; set; }
    public string Character { get; set; }
    public Colors Colors { get; set; }
    public NumberPlateType NumberPlateType { get; set; }
    public VehicleWheelType WheelType { get; set; }
    public VehicleWindowTint WindowTint { get; set; }
    public bool CustomTires { get; set; }
    public int Spoilers { get; set; }
    public int FrontBumper { get; set; }
    public int RearBumper { get; set; }
    public int SideSkirt { get; set; }
    public int Exhaust { get; set; }
    public int Frame { get; set; }
    public int Grille { get; set; }
    public int Hood { get; set; }
    public int Fender { get; set; }
    public int RightFender { get; set; }
    public int Roof { get; set; }
    public int Engine { get; set; }
    public int Brakes { get; set; }
    public int Transmission { get; set; }
    public int Horns { get; set; }
    public int Suspension { get; set; }
    public int Armor { get; set; }
    public int FrontWheel { get; set; }
    public int RearWheel { get; set; }
    public int PlateHolder { get; set; }
    public int VanityPlates { get; set; }
    public int TrimDesign { get; set; }
    public int Ornaments { get; set; }
    public int Dashboard { get; set; }
    public int DialDesign { get; set; }
    public int DoorSpeakers { get; set; }
    public int Seats { get; set; }
    public int SteeringWheels { get; set; }
    public int ColumnShifterLevers { get; set; }
    public int Plaques { get; set; }
    public int Speakers { get; set; }
    public int Trunk { get; set; }
    public int Hydraulics { get; set; }
    public int EngineBlock { get; set; }
    public int AirFilter { get; set; }
    public int Struts { get; set; }
    public int ArchCover { get; set; }
    public int Aerials { get; set; }
    public int Trim { get; set; }
    public int Tank { get; set; }
    public int Windows { get; set; }
    public int Livery { get; set; }
    public bool Turbo { get; set; }
    public bool TireSmoke { get; set; }
    public bool XenonHeadlights { get; set; }
    public bool CanTiresBurst { get; set; }
    public bool VehicleExtra01 { get; set; }
    public bool VehicleExtra02 { get; set; }
    public bool VehicleExtra03 { get; set; }
    public bool VehicleExtra04 { get; set; }
    public bool VehicleExtra05 { get; set; }
    public bool VehicleExtra06 { get; set; }
    public bool VehicleExtra07 { get; set; }
    public bool VehicleExtra08 { get; set; }
    public bool VehicleExtra09 { get; set; }
    public bool VehicleExtra10 { get; set; }
    public bool VehicleExtra11 { get; set; }
    public bool VehicleExtra12 { get; set; }
    public bool VehicleExtra13 { get; set; }
    public bool VehicleExtra14 { get; set; }

    public VehicleDefinition( string hash, string plate, string character )
    {
      VehicleName = hash;
      Plate = plate;
      Character = character;
      //            Spoilers = -1; FrontBumper = -1; RearBumper = -1; SideSkirt = -1; Exhaust = -1; Frame = -1; Grille = -1; Hood = -1;
      //            Fender = -1; RightFender = -1; Roof = -1; Engine = -1; Brakes = -1; Transmission = -1; Horns = -1; Suspension = -1;
      //            Armor = -1; FrontWheel = -1; RearWheel = -1; PlateHolder = -1; VanityPlates = -1; TrimDesign = -1; Ornaments = -1;
      //            Dashboard = -1; DialDesign = -1; DoorSpeakers = -1; Seats = -1; SteeringWheels = -1; ColumnShifterLevers = -1; Plaques = -1;
      //            Speakers = -1; Trunk = -1; Hydraulics = -1; EngineBlock = -1; AirFilter = -1; Struts = -1; ArchCover = -1; Aerials = -1;
      //            Trim = -1; Tank = -1; Windows = -1; Livery = -1; Turbo = false; TireSmoke = false; XenonHeadlights = false; CanTiresBurst = false;
    }

    //Other properties, methods, events...
  }
}