using GTA;
using GTA.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JustDownTheStreet
{
  public static class JsonController
  {
    public static readonly string JsonFolder = AppDomain.CurrentDomain.BaseDirectory + "/jdts/";
    public static readonly MD5 Md5Hash = new MD5Cng();

    public static string GetHash( MD5 hash, string input )
    {
      // Convert the input string to a byte array and compute the hash.
      byte[] data = hash.ComputeHash( Encoding.UTF8.GetBytes( input ) );
      StringBuilder sBuilder = new StringBuilder(); // Create a new Stringbuilder to collect the bytes and create a string.
      // Loop through each byte of the hashed data and format each one as a hexadecimal string.
      foreach ( byte t in data )
      {
        sBuilder.Append( t.ToString( "x2", CultureInfo.InvariantCulture ) );
      }
      return sBuilder.ToString(); // Return the hexadecimal string.
    }

    public static List<VehicleDefinition> GetAllPersonalVehiclesFromJson( string name )
    {
      List<VehicleDefinition> list = new List<VehicleDefinition>();
      Directory.CreateDirectory( JsonFolder );
      string[] jsonFiles = Directory.GetFiles( JsonFolder, "*.json" );
      foreach ( string t in jsonFiles )
      {
        //Wait(0); // avoid slowing down gameplay
        VehicleDefinition thisVehicleDefinition = JsonConvert.DeserializeObject<VehicleDefinition>( File.ReadAllText( t ) );
        if ( thisVehicleDefinition.Character != name ) continue;
        if ( !new Model( thisVehicleDefinition.VehicleName ).IsVehicle )
        {
          Logger.Log( "GetAllPersonalVehiclesFromJson(): " + thisVehicleDefinition.VehicleName
                      + " does not appear to be a valid vehicle, skipping." );
          continue;
        }
        list.Add( thisVehicleDefinition );
      }
      Logger.Log( "GetAllPersonalVehiclesFromJson(" + name + "): found " + list.Count + " vehicles" );
      return list;
    }

    public static void SaveCurrentVehicleToJson( string currentCharacterName )
    {
      Vehicle vehicle = Game.Player.Character.CurrentVehicle; // get the vehicle our player is in
      if ( vehicle == null ) return;
      Colors vehicleColors = new Colors( vehicle.PrimaryColor, vehicle.SecondaryColor, vehicle.PearlescentColor ) {
        Rim = vehicle.RimColor,
        Neon = vehicle.NeonLightsColor,
        TireSmoke = vehicle.TireSmokeColor,
        Trim = vehicle.TrimColor,
        Dashboard = vehicle.DashboardColor
      };
      VehicleDefinition vehicleDefinition = new VehicleDefinition( ( (VehicleHash)vehicle.Model.Hash ).ToString()
                                                                   , vehicle.NumberPlate, currentCharacterName ) {
        Colors = vehicleColors,
        NumberPlateType = vehicle.NumberPlateType,
        WheelType = vehicle.WheelType,
        WindowTint = vehicle.WindowTint,
        CustomTires = Function.Call<bool>( Hash.GET_VEHICLE_MOD_VARIATION, vehicle, 0 ),
        Spoilers = vehicle.GetMod( VehicleMod.Spoilers ),
        FrontBumper = vehicle.GetMod( VehicleMod.FrontBumper ),
        RearBumper = vehicle.GetMod( VehicleMod.RearBumper ),
        SideSkirt = vehicle.GetMod( VehicleMod.SideSkirt ),
        Exhaust = vehicle.GetMod( VehicleMod.Exhaust ),
        Frame = vehicle.GetMod( VehicleMod.Frame ),
        Grille = vehicle.GetMod( VehicleMod.Grille ),
        Hood = vehicle.GetMod( VehicleMod.Hood ),
        Fender = vehicle.GetMod( VehicleMod.Fender ),
        RightFender = vehicle.GetMod( VehicleMod.RightFender ),
        Roof = vehicle.GetMod( VehicleMod.Roof ),
        Engine = vehicle.GetMod( VehicleMod.Engine ),
        Brakes = vehicle.GetMod( VehicleMod.Brakes ),
        Transmission = vehicle.GetMod( VehicleMod.Transmission ),
        Horns = vehicle.GetMod( VehicleMod.Horns ),
        Suspension = vehicle.GetMod( VehicleMod.Suspension ),
        Armor = vehicle.GetMod( VehicleMod.Armor ),
        FrontWheel = vehicle.GetMod( VehicleMod.FrontWheels ),
        RearWheel = vehicle.GetMod( VehicleMod.BackWheels ),
        PlateHolder = vehicle.GetMod( VehicleMod.PlateHolder ),
        VanityPlates = vehicle.GetMod( VehicleMod.VanityPlates ),
        TrimDesign = vehicle.GetMod( VehicleMod.TrimDesign ),
        Ornaments = vehicle.GetMod( VehicleMod.Ornaments ),
        Dashboard = vehicle.GetMod( VehicleMod.Dashboard ),
        DialDesign = vehicle.GetMod( VehicleMod.DialDesign ),
        DoorSpeakers = vehicle.GetMod( VehicleMod.DoorSpeakers ),
        Seats = vehicle.GetMod( VehicleMod.Seats ),
        SteeringWheels = vehicle.GetMod( VehicleMod.SteeringWheels ),
        ColumnShifterLevers = vehicle.GetMod( VehicleMod.ColumnShifterLevers ),
        Plaques = vehicle.GetMod( VehicleMod.Plaques ),
        Speakers = vehicle.GetMod( VehicleMod.Speakers ),
        Trunk = vehicle.GetMod( VehicleMod.Trunk ),
        Hydraulics = vehicle.GetMod( VehicleMod.Hydraulics ),
        EngineBlock = vehicle.GetMod( VehicleMod.EngineBlock ),
        AirFilter = vehicle.GetMod( VehicleMod.AirFilter ),
        Struts = vehicle.GetMod( VehicleMod.Struts ),
        ArchCover = vehicle.GetMod( VehicleMod.ArchCover ),
        Aerials = vehicle.GetMod( VehicleMod.Aerials ),
        Trim = vehicle.GetMod( VehicleMod.Trim ),
        Tank = vehicle.GetMod( VehicleMod.Tank ),
        Windows = vehicle.GetMod( VehicleMod.Windows ),
        Livery = vehicle.GetMod( VehicleMod.Livery ),
        Turbo = vehicle.IsToggleModOn( VehicleToggleMod.Turbo ),
        TireSmoke = vehicle.IsToggleModOn( VehicleToggleMod.TireSmoke ),
        XenonHeadlights = vehicle.IsToggleModOn( VehicleToggleMod.XenonHeadlights ),
        CanTiresBurst = vehicle.CanTiresBurst,
        VehicleExtra01 = vehicle.IsExtraOn( 01 ),
        VehicleExtra02 = vehicle.IsExtraOn( 02 ),
        VehicleExtra03 = vehicle.IsExtraOn( 03 ),
        VehicleExtra04 = vehicle.IsExtraOn( 04 ),
        VehicleExtra05 = vehicle.IsExtraOn( 05 ),
        VehicleExtra06 = vehicle.IsExtraOn( 06 ),
        VehicleExtra07 = vehicle.IsExtraOn( 07 ),
        VehicleExtra08 = vehicle.IsExtraOn( 08 ),
        VehicleExtra09 = vehicle.IsExtraOn( 09 ),
        VehicleExtra10 = vehicle.IsExtraOn( 10 ),
        VehicleExtra11 = vehicle.IsExtraOn( 11 ),
        VehicleExtra12 = vehicle.IsExtraOn( 12 ),
        VehicleExtra13 = vehicle.IsExtraOn( 13 ),
        VehicleExtra14 = vehicle.IsExtraOn( 14 )
      };
      string jsonString = JsonConvert.SerializeObject( vehicleDefinition, Formatting.Indented );
      string vehiclehashcode = GetHash( Md5Hash, jsonString );
      string fullPath = JsonFolder + currentCharacterName + "_" + vehicleDefinition.VehicleName + "_" + vehiclehashcode + ".json";
      if ( File.Exists( fullPath ) )
      {
        Logger.Log( fullPath + " -> vehicle exists, skipping save." );
        return;
      }
      Logger.Log( fullPath + " -> saving vehicle..." );
      File.WriteAllText( fullPath, jsonString );
    }

  }
}
