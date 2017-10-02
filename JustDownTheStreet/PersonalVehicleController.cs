using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustDownTheStreet
{
  public class PersonalVehicleController
  {
    private static Vehicle _personalVehicle;
    private static Blip _personalVehicleBlip;
    private static Ped _personalDriver;
    private static int _personalVehicleDespawnRange = 333;
    private static int _LockUnlockRange = 11;
    private static int _minimumAcceptableSpawnDistance = 66;
    private static int _maximumAcceptableSpawnDistance = 333;
    private static int _tickCountAtLastSpawnAttempt = 0;
    private static bool _deliveryIsInProgress = false;
    private static bool _parkingIsInProgress = true;
    private static readonly Random Rng = new Random();
    private static List<VehicleDefinition> PersonalVehiclesFranklin = new List<VehicleDefinition>( JsonController.GetAllPersonalVehiclesFromJson( "Michael" ) );
    private static List<VehicleDefinition> PersonalVehiclesMichael = new List<VehicleDefinition>( JsonController.GetAllPersonalVehiclesFromJson( "Franklin" ) );
    private static List<VehicleDefinition> PersonalVehiclesTrevor = new List<VehicleDefinition>( JsonController.GetAllPersonalVehiclesFromJson( "Trevor" ) );



    private static int PlayerDistanceCompare( Vehicle vehicle1, Vehicle vehicle2 ) {
      float aDistance = World.GetDistance( Game.Player.Character.Position, vehicle1.Position );
      float bDistance = World.GetDistance( Game.Player.Character.Position, vehicle2.Position );
      return aDistance.CompareTo( bDistance );
    }

    private static Vector4 FindASpawnPoint( bool isForDelivery ) {
      if ( isForDelivery ) {
        Vector3 randomPosition = Game.Player.Character.Position.Around( Rng.Next( _minimumAcceptableSpawnDistance, _maximumAcceptableSpawnDistance ) );
        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();
        if ( Function.Call<bool>( Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, randomPosition.X, randomPosition.Y, randomPosition.Z, outArgA, outArgB, 1, 3, 0 ) ) {
          Vector3 properPosition = outArgA.GetResult<Vector3>();
          float heading = outArgB.GetResult<float>();
          Logger.Log( "FindASpawnPoint(forDelivery = true): found a spawn point: ( " + properPosition.X + ", " + properPosition.Y + ", " + properPosition.Z + ", ) Heading: " + heading );
          return new Vector4( properPosition, heading );
        } else {
          Logger.Log( "FindASpawnPoint(forDelivery = true): couldn't find a suitable spawn point." );
          throw new InvalidOperationException( "FindASpawnPoint(): couldn't find a suitable spawn point." );
        }
      } else {
        Vector3 playerPosition = Game.Player.Character.Position;
        List<Vehicle> allVehicles = World.GetAllVehicles().ToList();
        allVehicles.Sort( PlayerDistanceCompare );
        Logger.Log( "FindASpawnPoint(forDelivery = false): looking at " + allVehicles.Count + " possible vehicles." );
        foreach ( Vehicle thisVehicle in allVehicles ) {
          Script.Yield(); // avoid slowing down gameplay
          if ( !( World.GetDistance( thisVehicle.Position, playerPosition ) > _minimumAcceptableSpawnDistance
              & World.GetDistance( thisVehicle.Position, playerPosition ) < _maximumAcceptableSpawnDistance
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
              & thisVehicle.ClassType != VehicleClass.Trains ) ) continue; // lazy but should probably do OK for now.
          Vector3 thisVehiclePosition = thisVehicle.Position;
          float thisVehicleHeading = thisVehicle.Heading;
          thisVehicle.Delete();
          Logger.Log( "FindASpawnPoint(forDelivery = false): found a spawn point: ( " + thisVehiclePosition.X + ", " + thisVehiclePosition.Y + ", " + thisVehiclePosition.Z + ", ) Heading: " + thisVehicleHeading );
          return new Vector4( thisVehiclePosition, thisVehicleHeading );
        }
        Logger.Log( "FindASpawnPoint(forDelivery = false): couldn't find a suitable spawn point." );
        throw new InvalidOperationException( "FindASpawnPoint(forDelivery = false): couldn't find a suitable spawn point." );
      }
    }

    private static void DeliverPersonalVehicle() {
      Logger.Log( "DeliverPersonalVehicle()" );
      //int myDrivingStyle = 262144 + 32 + 16 + 8 + 4;
      int myDrivingStyle = 4 + 8 + 16 + 32 + 256 + 512 + 262144;
      _personalDriver = _personalVehicle.CreateRandomPedOnSeat( VehicleSeat.Driver );
      _personalDriver.Alpha = 0;
      Vector3 targetPos = World.GetNextPositionOnStreet( Game.Player.Character.Position );
      Function.Call( Hash.SET_DRIVER_ABILITY, _personalDriver, 1.0f );
      TaskSequence deliverySequence = new TaskSequence();
      Function.Call( Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, _personalVehicle, Game.Player.Character, 4, 60.0f, myDrivingStyle, 60.0f, 0.0f, true );
      Function.Call( Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, _personalVehicle, Game.Player.Character, 4, 30.0f, myDrivingStyle, 30.0f, 0.0f, true );
      Function.Call( Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, _personalVehicle, Game.Player.Character, 4, 15.0f, myDrivingStyle, 15.0f, 0.0f, true );
      //Function.Call( Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, _personalVehicle, Game.Player.Character, 4, 5.0f, myDrivingStyle, 5.0f, 0.0f, true );
      deliverySequence.Close();
      _personalDriver.Task.PerformSequence( deliverySequence );
      deliverySequence.Dispose();
      _deliveryIsInProgress = true;

    }

    private static void GeneratePersonalVehicleAndBlip( string playerName, VehicleDefinition vehicleDefinition, Vector4 spawnPoint ) {
      Model model = new Model( vehicleDefinition.VehicleName ).Hash;
      model.Request();
      while ( !model.IsLoaded ) {
        Script.Yield();
      }
      _personalVehicle = World.CreateVehicle( vehicleDefinition.VehicleName, new Vector3( spawnPoint.X, spawnPoint.Y, spawnPoint.Z ), spawnPoint.H );
      if ( _personalVehicle == null ) {
        Logger.Log( "GeneratePersonalVehicleAndBlip(): World.CreateVehicle() failed to create a vehicle, which should never happen." );
        return;
      }
      _personalVehicle.PlaceOnGround();
      ConfigurePersonalVehicle( _personalVehicle, vehicleDefinition );
      _personalVehicle.LockStatus = VehicleLockStatus.Locked;
      _personalVehicle.PreviouslyOwnedByPlayer = true;
      Logger.Log( "GeneratePersonalVehicleAndBlip(): placed a " + _personalVehicle.PrimaryColor + " "
        + (VehicleHash)_personalVehicle.Model.Hash + " at ("
        + Math.Round( spawnPoint.X, 3 ) + ", "
        + Math.Round( spawnPoint.Y, 3 ) + ", "
        + Math.Round( spawnPoint.Z, 3 ) + ")" );
      _personalVehicleBlip = _personalVehicle.AddBlip();
      _personalVehicleBlip.Sprite = BlipSprite.PersonalVehicleCar;
      _personalVehicleBlip.Name = vehicleDefinition.VehicleName;
      _personalVehicleBlip.Scale = 0.8888f;
      PlayerBlipColors colorToUse = (PlayerBlipColors)System.Enum.Parse( typeof( PlayerBlipColors ), playerName );
      Function.Call( Hash.SET_BLIP_COLOUR, _personalVehicleBlip, (int)colorToUse );
    }

    internal static List<VehicleDefinition> CurrentVehicleDefinitions( string playerName ) {
      List<VehicleDefinition> VehicleDefinitionList = new List<VehicleDefinition>();
      switch ( playerName ) {
        case "Michael":
          VehicleDefinitionList = PersonalVehiclesMichael;
          break;
        case "Franklin":
          VehicleDefinitionList = PersonalVehiclesFranklin;
          break;
        case "Trevor":
          VehicleDefinitionList = PersonalVehiclesTrevor;
          break;
        default:
          Logger.Log( "GeneratePersonalVehicleAndBlip(): couldn't match on character name. This should never happen. Regardless, we're going to use ALL the vehicles." );
          PersonalVehiclesMichael.ForEach( item => VehicleDefinitionList.Add( item ) );
          PersonalVehiclesFranklin.ForEach( item => VehicleDefinitionList.Add( item ) );
          PersonalVehiclesTrevor.ForEach( item => VehicleDefinitionList.Add( item ) );
          break;
      }
      return VehicleDefinitionList;
    }

    private static void CleanupPersonalVehicleAndBlip() {
      Logger.Log( "CleanupPersonalVehicleAndBlip(): removing vehicle and blip (and driver)." );
      if ( _personalDriver != null ) {
        _personalDriver.Delete();
        _personalDriver = null;
      }
      if ( _personalVehicleBlip != null ) {
        _personalVehicleBlip.Remove();
      }
      Script.Wait( 1333 ); // or the player ped will be standing in the middle of the street.
      if ( _personalVehicle != null ) {
        _personalVehicle.MarkAsNoLongerNeeded();
        _personalVehicle = null;
      }
    }

    internal static void DeployANewPersonalVehicle( string playerName, bool isForDelivery, VehicleDefinition specificVehicleDefinition = null) {
      Vector4 spawnPoint;
      CleanupPersonalVehicleAndBlip();
      try {
        spawnPoint = FindASpawnPoint( isForDelivery );
      } catch {
        if ( isForDelivery ) UI.Notify( "Can't deliver a vehicle, try again later." );
        return;
      }
      if (specificVehicleDefinition == null) {
        specificVehicleDefinition = CurrentVehicleDefinitions(playerName).PickRandom();
      }
      GeneratePersonalVehicleAndBlip( playerName, specificVehicleDefinition, spawnPoint );
      if ( isForDelivery ) DeliverPersonalVehicle();
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

    private static void ParkPersonalVehicleNearby() {
      Logger.Log( "ParkPersonalVehicleNearby()" );
      Vector3 playerPos = Game.Player.Character.Position;
      Vector3 streetNode = World.GetNextPositionOnStreet( Game.Player.Character.Position );
      Vector3 sidewalkNode = World.GetNextPositionOnSidewalk( Game.Player.Character.Position );
      OutputArgument outArgA = new OutputArgument();
      OutputArgument outArgB = new OutputArgument();
      if ( Function.Call<bool>( Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, sidewalkNode.X, sidewalkNode.Y, sidewalkNode.Z, outArgA, outArgB, 1, 3, 0 ) ) {
        Vector3 pos = outArgA.GetResult<Vector3>();
        float heading = outArgB.GetResult<float>();
        Function.Call( Hash.TASK_VEHICLE_PARK, 0, _personalVehicle, sidewalkNode.X, sidewalkNode.Y, sidewalkNode.Z, heading, 0, 30.0f, true );
        _parkingIsInProgress = true;
        //while ( _personalVehicle.Speed > 1.0f ) {
        //  Script.Yield();
        //}
        //_personalDriver.Delete();
        //_personalVehicle.PreviouslyOwnedByPlayer = true;
        
      } else
      {
        Logger.Log( "GET_CLOSEST_VEHICLE_NODE_WITH_HEADING failed, can't park!" );
        _personalDriver.Delete();
        _personalVehicle.PreviouslyOwnedByPlayer = true;
      }
    }

    private static void GenerateAllPersonalVehicles(string playerName) {
      float offset = (float)2.5;
      var position = Game.Player.Character.GetOffsetInWorldCoords( new Vector3( 0, offset, 0 ) ); // 5 meters in front of the player
      var heading = Game.Player.Character.Heading - 90; // At 90 degrees to the players heading
      foreach ( var vehicleDefinition in CurrentVehicleDefinitions(playerName) ) {
        Vehicle vehicle = World.CreateVehicle( vehicleDefinition.VehicleName, position, heading );
        vehicle.PlaceOnGround();
        ConfigurePersonalVehicle( vehicle, vehicleDefinition );
        offset = offset + (float)3.5;
        position = Game.Player.Character.GetOffsetInWorldCoords( new Vector3( 0, offset, 0 ) );
      }
    }

    internal static void UpdateHandler( String playerName, bool playerIsInControl) {
      
      if ( playerIsInControl && _personalVehicle == null && ( Environment.TickCount - _tickCountAtLastSpawnAttempt ) > 3333) {
        _tickCountAtLastSpawnAttempt = Environment.TickCount;
        Logger.Log( "OnTick(): " + playerName + " does not have a nearby personal vehicle." );
        DeployANewPersonalVehicle( playerName, isForDelivery: false );
        if ( _personalVehicle == null ) {
          Logger.Log( "OnTick(): GeneratePersonalVehicleAndBlip() seems to have failed, waiting a bit then trying again." );
        }
      }
      if ( _personalVehicle != null ) {
        _personalVehicle.LockStatus = World.GetDistance( _personalVehicle.Position, Game.Player.Character.Position ) < _LockUnlockRange ? VehicleLockStatus.Unlocked : VehicleLockStatus.Locked;
        if ( World.GetDistance( _personalVehicle.Position, Game.Player.Character.Position ) > _personalVehicleDespawnRange ) {
          CleanupPersonalVehicleAndBlip();
        }
      }
      if ( _personalVehicle != null && _deliveryIsInProgress && World.GetDistance( _personalVehicle.Position, Game.Player.Character.Position ) <= 7.0f && _personalVehicle.Speed < 7.0f ) {
        //_personalDriver.Delete();
        //_personalVehicle.PreviouslyOwnedByPlayer = true;
        _deliveryIsInProgress = false;
        ParkPersonalVehicleNearby();
        //Game.Player.Character.Task.LookAt(_personalVehicle, 3333);
        //Function.Call(Hash.SET_PED_PRIMARY_LOOKAT, Game.Player.Character, _personalDriver);
        //GameplayCamera.RelativeHeading = 90.0f;
        //GameplayCamera.RelativePitch = 90.0f;
        Camera gameplayCam = Function.Call<Camera>(Hash.GET_RENDERING_CAM);
      }
      if (_personalVehicle != null && _parkingIsInProgress && _personalVehicle.Speed < 1.0f && World.GetDistance( _personalVehicle.Position, Game.Player.Character.Position ) <= 1.0f ) {
        _personalDriver.Delete();
        _parkingIsInProgress = false;
        Logger.Log( "UpdateHandler(): Finished parking, deleting driver." );
      }
    }

  }
}
