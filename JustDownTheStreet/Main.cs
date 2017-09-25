// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using Control = GTA.Control;

[assembly: CLSCompliant(false)]

namespace JustDownTheStreet {
  public class Main : Script {
    internal static readonly string JsonFolder = AppDomain.CurrentDomain.BaseDirectory + "/jdts/";
    internal static List<VehicleDefinition> CurrentVehicleDefinitions = new List<VehicleDefinition>();
    internal static List<VehicleDefinition> PersonalVehiclesFranklin = new List<VehicleDefinition>();
    internal static List<VehicleDefinition> PersonalVehiclesMichael = new List<VehicleDefinition>();
    internal static List<VehicleDefinition> PersonalVehiclesTrevor = new List<VehicleDefinition>();
    private static readonly MD5 Md5Hash = new MD5Cng();
    private static readonly Random Rng = new Random();
    private static string _currentPlayerName;
    private static int _millisecondsToDelayQualityOfLife = 333333;
    private static int _minimumAcceptableSpawnDistance = 66;
    private static int _minimumMultipleOfPlayerMoneyToPurchase = 11;
    private static Vehicle _personalVehicle;
    private static Blip _personalVehicleBlip;
    private static int _personalVehicleDespawnRange = 333;
    private static int _personalVehicleLockUnlockRange = 11;
    private static int _priceOfBodyArmor = 6666;
    private readonly System.Timers.Timer _findASpawnPointRetryTimer = new System.Timers.Timer( 3333 );
    private readonly Stopwatch _lastTimeFranklinWasInControl = new Stopwatch();
    private readonly Stopwatch _lastTimeMichaelWasInControl = new Stopwatch();
    private readonly Stopwatch _lastTimeTrevorWasInControl = new Stopwatch();
    private bool _playerIsInControl;
    private MainMenu _theScriptMainMenuInstance = new MainMenu();
    private bool _waiting;

    public Main()
    {
      Logger.Log("----------------------------------------------------------------");
      Logger.Log("Main(): Script initializing");
      Directory.CreateDirectory(JsonFolder);
      PersonalVehiclesMichael = GetAllPersonalVehiclesFromJson(JsonFolder, "Michael");
      PersonalVehiclesFranklin = GetAllPersonalVehiclesFromJson(JsonFolder, "Franklin"); // get our vehicles from files
      PersonalVehiclesTrevor = GetAllPersonalVehiclesFromJson(JsonFolder, "Trevor");
      // check if player has control, probably indicating that the game has finished loading
      while ( !Game.Player.CanControlCharacter) { 
        Wait(0);
      }
      _lastTimeMichaelWasInControl.Start();
      _lastTimeFranklinWasInControl.Start();
      _lastTimeTrevorWasInControl.Start();
      //_theScriptMainMenuInstance = new MainMenu();
      Tick += OnTick;
      KeyDown += KeyDownHandler;
      KeyUp += KeyUpHandler; // hook up the handlers
      _findASpawnPointRetryTimer.Elapsed += FindASpawnPointRetryTimerHandler; // init the retry timer
    }

    internal static void RequestANewPersonalVehicle( VehicleDefinition specificVehicleDefinition = null )
    {
      CleanupPersonalVehicleAndBlip( ref _personalVehicle, ref _personalVehicleBlip );
      GeneratePersonalVehicleAndBlip( ref _personalVehicle, ref _personalVehicleBlip, specificVehicleDefinition );
    }

    internal static void SaveCurrentVehicleToJson( string jsonfolder, string currentCharacterName )
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
      string fullPath = jsonfolder + currentCharacterName + "_" + vehicleDefinition.VehicleName + "_" + vehiclehashcode + ".json";
      if ( File.Exists( fullPath ) )
      {
        Logger.Log( fullPath + " -> vehicle exists, skipping save." );
        return;
      }
      Logger.Log( fullPath + " -> saving vehicle..." );
      File.WriteAllText( fullPath, jsonString );
    }

    private static void CleanupPersonalVehicleAndBlip( ref Vehicle vehicle, ref Blip blip )
    {
      Wait( 1333 ); // or the player ped will be standing in the middle of the street.
      Logger.Log( "CleanupPersonalVehicleAndBlip(): removing vehicle and blip." );
      if ( blip != null ) {
        blip.Remove();
      }
      if ( vehicle != null ) {
        vehicle.MarkAsNoLongerNeeded();
        vehicle = null;
      }
    }

    private static int PlayerDistanceCompare( Vehicle vehicle1, Vehicle vehicle2 ) {
      float aDistance = World.GetDistance( Game.Player.Character.Position, vehicle1.Position );
      float bDistance = World.GetDistance( Game.Player.Character.Position, vehicle2.Position );
      return aDistance.CompareTo( bDistance );
    }

    private static void ConfigurePersonalVehicle( Vehicle vehicle, VehicleDefinition vehicleDefinition ) {
      vehicle.NumberPlate = vehicleDefinition.Plate;
      // for now I'm skipping the custom colors, I'm not actually sure if I can even set those...
      vehicle.PrimaryColor = vehicleDefinition.Colors.Primary;
      vehicle.SecondaryColor = vehicleDefinition.Colors.Secondary;
      vehicle.PearlescentColor = vehicleDefinition.Colors.Pearlescent;
      vehicle.RimColor = vehicleDefinition.Colors.Rim;
      vehicle.NeonLightsColor = vehicleDefinition.Colors.Neon;
      vehicle.TireSmokeColor = vehicleDefinition.Colors.TireSmoke;
      vehicle.TrimColor = (VehicleColor)vehicleDefinition.Trim;
      vehicle.DashboardColor = (VehicleColor)vehicleDefinition.Dashboard;
      vehicle.CanTiresBurst = vehicleDefinition.CanTiresBurst;
      vehicle.NumberPlateType = vehicleDefinition.NumberPlateType;
      vehicle.WheelType = vehicleDefinition.WheelType;
      vehicle.WindowTint = vehicleDefinition.WindowTint;
      vehicle.InstallModKit();
      vehicle.SetMod( VehicleMod.Spoilers, vehicleDefinition.Spoilers, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.FrontBumper, vehicleDefinition.FrontBumper, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.RearBumper, vehicleDefinition.RearBumper, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.SideSkirt, vehicleDefinition.SideSkirt, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Exhaust, vehicleDefinition.Exhaust, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Frame, vehicleDefinition.Frame, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Grille, vehicleDefinition.Grille, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Hood, vehicleDefinition.Hood, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Fender, vehicleDefinition.Fender, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.RightFender, vehicleDefinition.RightFender, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Roof, vehicleDefinition.Roof, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Engine, vehicleDefinition.Engine, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Brakes, vehicleDefinition.Brakes, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Transmission, vehicleDefinition.Transmission, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Horns, vehicleDefinition.Horns, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Suspension, vehicleDefinition.Suspension, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Armor, vehicleDefinition.Armor, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.FrontWheels, vehicleDefinition.FrontWheel, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.BackWheels, vehicleDefinition.RearWheel, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.PlateHolder, vehicleDefinition.PlateHolder, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.VanityPlates, vehicleDefinition.VanityPlates, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.TrimDesign, vehicleDefinition.TrimDesign, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Ornaments, vehicleDefinition.Ornaments, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Dashboard, vehicleDefinition.Dashboard, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.DialDesign, vehicleDefinition.DialDesign, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.DoorSpeakers, vehicleDefinition.DoorSpeakers, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Seats, vehicleDefinition.Seats, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.SteeringWheels, vehicleDefinition.SteeringWheels, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.ColumnShifterLevers, vehicleDefinition.ColumnShifterLevers, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Plaques, vehicleDefinition.Plaques, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Speakers, vehicleDefinition.Speakers, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Trunk, vehicleDefinition.Trunk, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Hydraulics, vehicleDefinition.Hydraulics, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.EngineBlock, vehicleDefinition.EngineBlock, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.AirFilter, vehicleDefinition.AirFilter, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Struts, vehicleDefinition.Struts, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.ArchCover, vehicleDefinition.ArchCover, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Aerials, vehicleDefinition.Aerials, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Trim, vehicleDefinition.Trim, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Tank, vehicleDefinition.Tank, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Windows, vehicleDefinition.Windows, vehicleDefinition.CustomTires );
      vehicle.SetMod( VehicleMod.Livery, vehicleDefinition.Livery, vehicleDefinition.CustomTires );
      vehicle.ToggleMod( VehicleToggleMod.Turbo, vehicleDefinition.Turbo );
      vehicle.ToggleMod( VehicleToggleMod.TireSmoke, vehicleDefinition.TireSmoke );
      vehicle.ToggleMod( VehicleToggleMod.XenonHeadlights, vehicleDefinition.XenonHeadlights );
      vehicle.ToggleExtra( 01, vehicleDefinition.VehicleExtra01 );
      vehicle.ToggleExtra( 02, vehicleDefinition.VehicleExtra02 );
      vehicle.ToggleExtra( 03, vehicleDefinition.VehicleExtra03 );
      vehicle.ToggleExtra( 04, vehicleDefinition.VehicleExtra04 );
      vehicle.ToggleExtra( 05, vehicleDefinition.VehicleExtra05 );
      vehicle.ToggleExtra( 06, vehicleDefinition.VehicleExtra06 );
      vehicle.ToggleExtra( 07, vehicleDefinition.VehicleExtra07 );
      vehicle.ToggleExtra( 08, vehicleDefinition.VehicleExtra08 );
      vehicle.ToggleExtra( 09, vehicleDefinition.VehicleExtra09 );
      vehicle.ToggleExtra( 10, vehicleDefinition.VehicleExtra10 );
      vehicle.ToggleExtra( 11, vehicleDefinition.VehicleExtra11 );
      vehicle.ToggleExtra( 12, vehicleDefinition.VehicleExtra12 );
      vehicle.ToggleExtra( 13, vehicleDefinition.VehicleExtra13 );
      vehicle.ToggleExtra( 14, vehicleDefinition.VehicleExtra14 );
    }

    private static Vector4 FindASpawnPoint() {
      Vector3 playerPosition = Game.Player.Character.Position;
      List<Vehicle> allVehicles = World.GetAllVehicles().ToList();
      allVehicles.Sort( PlayerDistanceCompare );
      Logger.Log( "FindASpawnPoint(): looking at " + allVehicles.Count + " possible vehicles." );
      foreach ( Vehicle thisVehicle in allVehicles ) {
        Wait( 0 ); // avoid slowing down gameplay
        if ( World.GetDistance( thisVehicle.Position, playerPosition ) > 3 
             & World.GetDistance( thisVehicle.Position, playerPosition ) < _minimumAcceptableSpawnDistance 
             & thisVehicle.IsStopped 
             & thisVehicle.Driver.Model == 0x0
             & thisVehicle.PreviouslyOwnedByPlayer == false
             & thisVehicle.ClassType != VehicleClass.Boats
             & thisVehicle.ClassType != VehicleClass.Commercial
             & thisVehicle.ClassType != VehicleClass.Emergency
             & thisVehicle.ClassType != VehicleClass.Helicopters
             & thisVehicle.ClassType != VehicleClass.Industrial
             & thisVehicle.ClassType != VehicleClass.Military
             & thisVehicle.ClassType != VehicleClass.Planes
             & thisVehicle.ClassType != VehicleClass.Trains) { // lazy but should probably do OK for now.
          Vector3 thisVehiclePosition = thisVehicle.Position;
          float thisVehicleHeading = thisVehicle.Heading;
          thisVehicle.Delete();
          Logger.Log( "FindASpawnPoint(): found a spawn point." );
          return new Vector4( thisVehiclePosition, thisVehicleHeading );
        }
      }
      Logger.Log( "FindASpawnPoint(): couldn't find a suitable spawn point." );
      throw new InvalidOperationException( "FindASpawnPoint(): couldn't find a suitable spawn point." );
    }

    private static void GenerateAllPersonalVehicles() {
      float offset = (float)2.5;
      var position = Game.Player.Character.GetOffsetInWorldCoords( new Vector3( 0, offset, 0 ) ); // 5 meters in front of the player
      var heading = Game.Player.Character.Heading - 90; // At 90 degrees to the players heading
      foreach ( var vehicleDefinition in CurrentVehicleDefinitions ) {
        Vehicle vehicle = World.CreateVehicle( vehicleDefinition.VehicleName, position, heading );
        vehicle.PlaceOnGround();
        ConfigurePersonalVehicle( vehicle, vehicleDefinition );
        offset = offset + (float)3.5;
        position = Game.Player.Character.GetOffsetInWorldCoords( new Vector3( 0, offset, 0 ) );
      }
    }

    private static void GeneratePersonalVehicleAndBlip( ref Vehicle vehicle, ref Blip blip, 
                                                        VehicleDefinition specificVehicleDefinition = null ) {
      VehicleDefinition vehicleDefinition;
      Vector4 aSpawnPoint;
      int colorToUse;
      try {
        aSpawnPoint = FindASpawnPoint();
      } catch ( InvalidOperationException ) {
        return;
      }

      try {
        switch ( _currentPlayerName ) {
          case "Michael":
            vehicleDefinition = SelectARandomPersonalVehicle( PersonalVehiclesMichael );
            colorToUse = (int)BlipColor.Blue;
            break;
          case "Franklin":
            vehicleDefinition = SelectARandomPersonalVehicle( PersonalVehiclesFranklin );
            colorToUse = (int)BlipColor.Green;
            break;
          case "Trevor":
            vehicleDefinition = SelectARandomPersonalVehicle( PersonalVehiclesTrevor );
            colorToUse = 17; // wtf Trevor's color isn't even an enum?
            break;
          default:
            Logger.Log(
                "GeneratePersonalVehicleAndBlip(): couldn't match on character name. This should never happen." );
            throw new InvalidOperationException(
                "GeneratePersonalVehicleAndBlip(): couldn't match on character name. This should never happen." );
        }
      } catch ( InvalidOperationException ) {
        Logger.Log( "GeneratePersonalVehicleAndBlip(): InvalidOperationException" );
        return;
      }

      if ( specificVehicleDefinition != null ) vehicleDefinition = specificVehicleDefinition;

      Vector3 vehicle_position = new Vector3( aSpawnPoint.X, aSpawnPoint.Y, aSpawnPoint.Z );
      vehicle = World.CreateVehicle( vehicleDefinition.VehicleName, vehicle_position, aSpawnPoint.H );
      if ( vehicle == null ) {
        Logger.Log("GeneratePersonalVehicleAndBlip(): World.CreateVehicle() failed to create a vehicle, which should never happen." );
        return;
      }
      vehicle.PlaceOnGround();
      ConfigurePersonalVehicle( vehicle, vehicleDefinition );
      vehicle.LockStatus = VehicleLockStatus.Locked;
      vehicle.PreviouslyOwnedByPlayer = true;
      Logger.Log( "GeneratePersonalVehicleAndBlip(): placed a " + vehicle.PrimaryColor + " " 
        + (VehicleHash)vehicle.Model.Hash + " at (" 
        + Math.Round( vehicle_position.X, 3 ) + ", " 
        + Math.Round(vehicle_position.Y, 3 ) + ", " 
        + Math.Round(vehicle_position.Z, 3 ) + ")" );
      blip = vehicle.AddBlip();
      blip.Sprite = BlipSprite.PersonalVehicleCar;
      blip.Name = vehicleDefinition.VehicleName;
      blip.Scale = 0.8888f;
      Function.Call( Hash.SET_BLIP_COLOUR, blip, colorToUse );
    }

    private static List<VehicleDefinition> GetAllPersonalVehiclesFromJson( string jsonfolder, string name ) {
      List<VehicleDefinition> list = new List<VehicleDefinition>();
      string[] jsonFiles = Directory.GetFiles( jsonfolder, "*.json" );
      foreach ( string t in jsonFiles ) {
        //Wait(0); // avoid slowing down gameplay
        VehicleDefinition thisVehicleDefinition = JsonConvert.DeserializeObject<VehicleDefinition>( File.ReadAllText( t ) );
        if ( thisVehicleDefinition.Character != name ) continue;
        if ( !new Model( thisVehicleDefinition.VehicleName ).IsVehicle ) {
          Logger.Log( "GetAllPersonalVehiclesFromJson(): " + thisVehicleDefinition.VehicleName 
                      + " does not appear to be a valid vehicle, skipping." );
          continue;
        }
        list.Add( thisVehicleDefinition );
      }
      Logger.Log( "GetAllPersonalVehiclesFromJson(" + name + "): found " + list.Count + " vehicles" );
      return list;
    }

    private static string GetHash( MD5 hash, string input ) {
      // Convert the input string to a byte array and compute the hash.
      byte[]data = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
      StringBuilder sBuilder = new StringBuilder(); // Create a new Stringbuilder to collect the bytes and create a string.
      // Loop through each byte of the hashed data and format each one as a hexadecimal string.
      foreach ( byte t in data ) {
        sBuilder.Append(t.ToString("x2", CultureInfo.InvariantCulture));
      }
      return sBuilder.ToString(); // Return the hexadecimal string.
    }

    // not always reliable, but... enough?
    private static bool IsPlayerSwitchingUnderArrestDeadOrLoading() {
      if (Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS) 
          || Function.Call<bool>(Hash.IS_ENTITY_DEAD, Function.Call<Ped>(Hash.PLAYER_PED_ID)) 
          || Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player.Character) 
          || Game.IsLoading) {
        return true;
      }
      return false;
    }

    private static void ProvideQualityOfLifeForCharacter() {
      Game.Player.Character.Health = Game.Player.Character.MaxHealth;
      Logger.Log( "ProvideQualityOfLifeForCharacter(): Game.Player.Money = " + Game.Player.Money );
      if ( ( Game.Player.Character.Armor < 100 ) 
          && ( Game.Player.Money > _priceOfBodyArmor * _minimumMultipleOfPlayerMoneyToPurchase ) ) {
        Game.Player.Money -= _priceOfBodyArmor;
        Game.Player.Character.Armor = 100;
        Logger.Log( "ProvideQualityOfLifeForCharacter(): buying armor." );
      }
      foreach ( WeaponHash thisWeapon in Enum.GetValues( typeof( WeaponHash ) ) ) {
        if ( !Game.Player.Character.Weapons.HasWeapon( thisWeapon ) ) continue;
        int maxAmmo = Game.Player.Character.Weapons[thisWeapon].MaxAmmo;
        int currentAmmo = Game.Player.Character.Weapons[thisWeapon].Ammo;
        int ammunitionToPurchase = maxAmmo - currentAmmo;
        string thisWeaponGroupString = Enum.GetName( typeof( WeaponGroup ), Game.Player.Character.Weapons[thisWeapon].Group );
        if ( thisWeaponGroupString == null ) continue;
        AmmunitionPrices ammunitionPrice = (AmmunitionPrices)Enum.Parse( typeof( AmmunitionPrices ), thisWeaponGroupString );
        if ( thisWeapon == WeaponHash.Minigun ) ammunitionPrice = AmmunitionPrices.MG; // ugly hack because
        int ammunitionTotalPrice = ammunitionToPurchase * (int)ammunitionPrice;
        if ( Game.Player.Money <= ammunitionTotalPrice * _minimumMultipleOfPlayerMoneyToPurchase ) continue;
        Game.Player.Money -= ammunitionTotalPrice;
        Game.Player.Character.Weapons[thisWeapon].Ammo = Game.Player.Character.Weapons[thisWeapon].MaxAmmo;
        if ( ammunitionToPurchase > 0 )
          Logger.Log( "ProvideQualityOfLifeForCharacter(): buying " + ammunitionToPurchase + " rounds of " +
                     thisWeaponGroupString + " ammunition for " + ammunitionTotalPrice + " dollars." );
      }
    }

    private static VehicleDefinition SelectARandomPersonalVehicle( IReadOnlyList<VehicleDefinition> vehicleList ) {
      if (vehicleList.Count == 0) {
        Logger.Log("SelectARandomPersonalVehicle(): list is empty! I guess we can't spawn a vehicle...");
        throw new InvalidOperationException("SelectARandomPersonalVehicle(): list is empty! I guess we can't spawn a vehicle...");
      }
      int randomNumber = Rng.Next(vehicleList.Count);
      VehicleDefinition selectedVehicleDefinition = vehicleList[randomNumber];
      return selectedVehicleDefinition;
    }

    private void FindASpawnPointRetryTimerHandler( object source, ElapsedEventArgs e ) {
      _waiting = false;
      _findASpawnPointRetryTimer.Stop();
    }

    // not nearly as useful as KeyUpHandler
    private void KeyDownHandler( object sender, KeyEventArgs e ) {
    }

    private void KeyUpHandler( object sender, KeyEventArgs e ) {
      if ( e.KeyCode == Keys.OemCloseBrackets ) {
        //if (Game.IsKeyPressed(Keys.OemOpenBrackets)) GenerateAllPersonalVehicles();
        //if (Game.IsKeyPressed(Keys.OemPipe)) ProvideQualityOfLifeForCharacter();
      }
    }

    // roughly every 16ms (60 times per second)
    private void OnTick( object sender, EventArgs e ) {
      //Logger.LogFPS(); // this file gets big fast...
      _theScriptMainMenuInstance.OnTickHandler();
      if (_personalVehicle != null) {
        _personalVehicle.LockStatus = World.GetDistance(_personalVehicle.Position
          , Game.Player.Character.Position) < _personalVehicleLockUnlockRange ? VehicleLockStatus.Unlocked : VehicleLockStatus.Locked;
        if (World.GetDistance(_personalVehicle.Position, Game.Player.Character.Position) > _personalVehicleDespawnRange) {
          CleanupPersonalVehicleAndBlip(ref _personalVehicle, ref _personalVehicleBlip);
        }
      }

      // is the player switching but wasn't already?
      if ( _playerIsInControl && IsPlayerSwitchingUnderArrestDeadOrLoading()) {
        Logger.Log("OnTick(): Player has lost control of " + _currentPlayerName);
        switch (_currentPlayerName) {
          case "Michael":
            _lastTimeMichaelWasInControl.Restart();
            break;
          case "Franklin":
            _lastTimeFranklinWasInControl.Restart();
            break;
          case "Trevor":
            _lastTimeTrevorWasInControl.Restart();
            break;
          default:
            throw new InvalidOperationException("OnTick(): couldn't match on character name. This should never happen.");
        }
        _playerIsInControl = false;
        return;
      }

      if (!_playerIsInControl && !IsPlayerSwitchingUnderArrestDeadOrLoading()) {
        _currentPlayerName = ((PedHash) Game.Player.Character.Model.Hash).ToString();
        Logger.Log("OnTick(): Player has gained control of " + _currentPlayerName);
        _playerIsInControl = true;
        _waiting = false;
        try {
          switch (_currentPlayerName) { 
            case "Michael":
              CurrentVehicleDefinitions = PersonalVehiclesMichael;
              Logger.Log("OnTick(): _lastTimeMichaelWasInControl.ElapsedMilliseconds " + _lastTimeMichaelWasInControl.ElapsedMilliseconds);
              if (_lastTimeMichaelWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife) ProvideQualityOfLifeForCharacter();
              break;
            case "Franklin":
              CurrentVehicleDefinitions = PersonalVehiclesFranklin;
              Logger.Log("OnTick(): _lastTimeFranklinWasInControl.ElapsedMilliseconds " + _lastTimeFranklinWasInControl.ElapsedMilliseconds);
              if (_lastTimeFranklinWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife) ProvideQualityOfLifeForCharacter();
              break;
            case "Trevor":
              CurrentVehicleDefinitions = PersonalVehiclesTrevor;
              Logger.Log("OnTick(): _lastTimeTrevorWasInControl.ElapsedMilliseconds " + _lastTimeTrevorWasInControl.ElapsedMilliseconds);
              if (_lastTimeTrevorWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife) ProvideQualityOfLifeForCharacter();
              break;
            default:
              Logger.Log("OnTick(): couldn't match on character name. This should never happen.");
              throw new InvalidOperationException("OnTick(): couldn't match on character name. This should never happen.");
          }
        }
        catch ( InvalidOperationException ) { 
          return;
        }
      }

      if (_playerIsInControl && _personalVehicle == null && !_waiting)
      {
        Logger.Log("OnTick(): " + _currentPlayerName + " does not have a nearby personal vehicle.");
        RequestANewPersonalVehicle();
        if (_personalVehicle == null)
        {
          Logger.Log("OnTick(): GeneratePersonalVehicleAndBlip() seems to have failed, waiting a bit then trying again.");
          _findASpawnPointRetryTimer.Start();
          _waiting = true;
        }
      }
      
      XBoxControllerUpdate();
    }

    private void XBoxControllerUpdate() {
      if (Game.IsControlPressed(0, Control.ScriptPadDown) && Game.IsControlJustReleased(0, Control.ScriptLB)) {
        //SaveCurrentVehicleToJson(_jsonFolder, ((PedHash) Game.Player.Character.Model.Hash).ToString());
        _theScriptMainMenuInstance = new MainMenu();
        _theScriptMainMenuInstance.ToggleMenu(); // this is not a real thing
      }
      if (Game.IsControlPressed(0, Control.ScriptPadDown) && Game.IsControlJustReleased(0, Control.ScriptRS)) {
        //Logger.LogPosition();
      }
      if (Game.IsControlPressed(0, Control.ScriptPadDown) 
          && Game.IsControlJustReleased(0, Control.ScriptLT) && Game.IsControlJustReleased(0, Control.ScriptRT)) {
        //GenerateAllPersonalVehicles();
      }
    }
  }
}