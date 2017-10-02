using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustDownTheStreet {
  class PersonalVehicleHandler {
    private readonly string _jsonFolder;
    private List<VehicleDefinition> CurrentVehicleDefinitions = new List<VehicleDefinition>();
    private List<VehicleDefinition> PersonalVehiclesFranklin = new List<VehicleDefinition>();
    private List<VehicleDefinition> PersonalVehiclesMichael = new List<VehicleDefinition>();
    private List<VehicleDefinition> PersonalVehiclesTrevor = new List<VehicleDefinition>();
    private bool _personalVehicleIsDelivered;
    private Vehicle _personalVehicle;
    private Blip _personalVehicleBlip;
    private Ped _personalDriver;
    private readonly int _personalVehicleDespawnRange = 333;
    private readonly int _personalVehicleLockUnlockRange = 11;
    private readonly int _minimumAcceptableSpawnDistance = 66;
    private readonly int _maximumAcceptableSpawnDistance = 333;

    public PersonalVehicleHandler(string jsonfolder ) {
      _jsonFolder = AppDomain.CurrentDomain.BaseDirectory + jsonfolder;
      PersonalVehiclesFranklin = GetPersonalVehiclesFromJson("Franklin");
      PersonalVehiclesFranklin = GetPersonalVehiclesFromJson("Michael");
      PersonalVehiclesFranklin = GetPersonalVehiclesFromJson("Trevor");

    }

    internal void SaveCurrentVehicleToJson(string name) {
      Vehicle vehicle = Game.Player.Character.CurrentVehicle; // get the vehicle our player is in
      if (vehicle == null) return;
      Colors vehicleColors = new Colors(vehicle.PrimaryColor, vehicle.SecondaryColor, vehicle.PearlescentColor) {
        Rim = vehicle.RimColor,
        Neon = vehicle.NeonLightsColor,
        TireSmoke = vehicle.TireSmokeColor,
        Trim = vehicle.TrimColor,
        Dashboard = vehicle.DashboardColor
      };
      VehicleDefinition vehicleDefinition = new VehicleDefinition(((VehicleHash)vehicle.Model.Hash).ToString()
                                                                   , vehicle.NumberPlate, name) {
        Colors = vehicleColors,
        NumberPlateType = vehicle.NumberPlateType,
        WheelType = vehicle.WheelType,
        WindowTint = vehicle.WindowTint,
        CustomTires = Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, vehicle, 0),
        Spoilers = vehicle.GetMod(VehicleMod.Spoilers),
        FrontBumper = vehicle.GetMod(VehicleMod.FrontBumper),
        RearBumper = vehicle.GetMod(VehicleMod.RearBumper),
        SideSkirt = vehicle.GetMod(VehicleMod.SideSkirt),
        Exhaust = vehicle.GetMod(VehicleMod.Exhaust),
        Frame = vehicle.GetMod(VehicleMod.Frame),
        Grille = vehicle.GetMod(VehicleMod.Grille),
        Hood = vehicle.GetMod(VehicleMod.Hood),
        Fender = vehicle.GetMod(VehicleMod.Fender),
        RightFender = vehicle.GetMod(VehicleMod.RightFender),
        Roof = vehicle.GetMod(VehicleMod.Roof),
        Engine = vehicle.GetMod(VehicleMod.Engine),
        Brakes = vehicle.GetMod(VehicleMod.Brakes),
        Transmission = vehicle.GetMod(VehicleMod.Transmission),
        Horns = vehicle.GetMod(VehicleMod.Horns),
        Suspension = vehicle.GetMod(VehicleMod.Suspension),
        Armor = vehicle.GetMod(VehicleMod.Armor),
        FrontWheel = vehicle.GetMod(VehicleMod.FrontWheels),
        RearWheel = vehicle.GetMod(VehicleMod.BackWheels),
        PlateHolder = vehicle.GetMod(VehicleMod.PlateHolder),
        VanityPlates = vehicle.GetMod(VehicleMod.VanityPlates),
        TrimDesign = vehicle.GetMod(VehicleMod.TrimDesign),
        Ornaments = vehicle.GetMod(VehicleMod.Ornaments),
        Dashboard = vehicle.GetMod(VehicleMod.Dashboard),
        DialDesign = vehicle.GetMod(VehicleMod.DialDesign),
        DoorSpeakers = vehicle.GetMod(VehicleMod.DoorSpeakers),
        Seats = vehicle.GetMod(VehicleMod.Seats),
        SteeringWheels = vehicle.GetMod(VehicleMod.SteeringWheels),
        ColumnShifterLevers = vehicle.GetMod(VehicleMod.ColumnShifterLevers),
        Plaques = vehicle.GetMod(VehicleMod.Plaques),
        Speakers = vehicle.GetMod(VehicleMod.Speakers),
        Trunk = vehicle.GetMod(VehicleMod.Trunk),
        Hydraulics = vehicle.GetMod(VehicleMod.Hydraulics),
        EngineBlock = vehicle.GetMod(VehicleMod.EngineBlock),
        AirFilter = vehicle.GetMod(VehicleMod.AirFilter),
        Struts = vehicle.GetMod(VehicleMod.Struts),
        ArchCover = vehicle.GetMod(VehicleMod.ArchCover),
        Aerials = vehicle.GetMod(VehicleMod.Aerials),
        Trim = vehicle.GetMod(VehicleMod.Trim),
        Tank = vehicle.GetMod(VehicleMod.Tank),
        Windows = vehicle.GetMod(VehicleMod.Windows),
        Livery = vehicle.GetMod(VehicleMod.Livery),
        Turbo = vehicle.IsToggleModOn(VehicleToggleMod.Turbo),
        TireSmoke = vehicle.IsToggleModOn(VehicleToggleMod.TireSmoke),
        XenonHeadlights = vehicle.IsToggleModOn(VehicleToggleMod.XenonHeadlights),
        CanTiresBurst = vehicle.CanTiresBurst,
        VehicleExtra01 = vehicle.IsExtraOn(01),
        VehicleExtra02 = vehicle.IsExtraOn(02),
        VehicleExtra03 = vehicle.IsExtraOn(03),
        VehicleExtra04 = vehicle.IsExtraOn(04),
        VehicleExtra05 = vehicle.IsExtraOn(05),
        VehicleExtra06 = vehicle.IsExtraOn(06),
        VehicleExtra07 = vehicle.IsExtraOn(07),
        VehicleExtra08 = vehicle.IsExtraOn(08),
        VehicleExtra09 = vehicle.IsExtraOn(09),
        VehicleExtra10 = vehicle.IsExtraOn(10),
        VehicleExtra11 = vehicle.IsExtraOn(11),
        VehicleExtra12 = vehicle.IsExtraOn(12),
        VehicleExtra13 = vehicle.IsExtraOn(13),
        VehicleExtra14 = vehicle.IsExtraOn(14)
      };
      string jsonString = JsonConvert.SerializeObject(vehicleDefinition, Formatting.Indented);
      string vehiclehashcode = Main.GetHash(Main.Md5Hash, jsonString);
      string fullPath = _jsonFolder + name + "_" + vehicleDefinition.VehicleName + "_" + vehiclehashcode + ".json";
      if (File.Exists(fullPath)) {
        Logger.Log(fullPath + " -> vehicle exists, skipping save.");
        return;
      }
      Logger.Log(fullPath + " -> saving vehicle...");
      File.WriteAllText(fullPath, jsonString);
    }

    internal void RequestANewPersonalVehicleDelivery(string name, VehicleDefinition specificVehicleDefinition = null) {
      CleanupPersonalVehicleAndBlip(ref _personalVehicle, ref _personalVehicleBlip);
      GeneratePersonalVehicleAndBlip(name, ref _personalVehicle, ref _personalVehicleBlip, specificVehicleDefinition);
      DeliverPersonalVehicle(ref _personalVehicle, ref _personalDriver);
    }

    private List<VehicleDefinition> GetPersonalVehiclesFromJson(string name) {
      List<VehicleDefinition> list = new List<VehicleDefinition>();
      Directory.CreateDirectory(_jsonFolder);
      string[] jsonFiles = Directory.GetFiles(_jsonFolder, "*.json");
      foreach (string t in jsonFiles) {
        VehicleDefinition thisVehicleDefinition = JsonConvert.DeserializeObject<VehicleDefinition>(File.ReadAllText(t));
        if (thisVehicleDefinition.Character != name) continue;
        if (!new Model(thisVehicleDefinition.VehicleName).IsVehicle) {
          Logger.Log("GetAllPersonalVehiclesFromJson(): " + thisVehicleDefinition.VehicleName
                      + " does not appear to be a valid vehicle, skipping.");
          continue;
        }
        list.Add(thisVehicleDefinition);
      }
      Logger.Log("GetAllPersonalVehiclesFromJson(" + name + "): found " + list.Count + " vehicles");
      return list;
    }

    private void CleanupPersonalVehicleAndBlip(ref Vehicle vehicle, ref Blip blip) {
      Script.Wait(1333); // or the player ped will be standing in the middle of the street.
      Logger.Log("CleanupPersonalVehicleAndBlip(): removing vehicle and blip.");
      if (blip != null) {
        blip.Remove();
      }
      if (vehicle != null) {
        vehicle.MarkAsNoLongerNeeded();
        vehicle = null;
      }
    }

    private void GeneratePersonalVehicleAndBlip(string name, ref Vehicle vehicle, ref Blip blip, VehicleDefinition specificVehicleDefinition = null) {
      VehicleDefinition vehicleDefinition;
      Vector4 aSpawnPoint;
      int colorToUse;
      try {
        aSpawnPoint = FindASpawnPointForDelivery();
      } catch (InvalidOperationException) {
        return;
      }

      try {
        switch (_currentPlayerName) {
          case "Michael":
            vehicleDefinition = SelectARandomPersonalVehicle(PersonalVehiclesMichael);
            colorToUse = (int)BlipColor.Blue;
            break;
          case "Franklin":
            vehicleDefinition = SelectARandomPersonalVehicle(PersonalVehiclesFranklin);
            colorToUse = (int)BlipColor.Green;
            break;
          case "Trevor":
            vehicleDefinition = SelectARandomPersonalVehicle(PersonalVehiclesTrevor);
            colorToUse = 17; // wtf Trevor's color isn't even an enum?
            break;
          default:
            Logger.Log(
                "GeneratePersonalVehicleAndBlip(): couldn't match on character name. This should never happen.");
            throw new InvalidOperationException(
                "GeneratePersonalVehicleAndBlip(): couldn't match on character name. This should never happen.");
        }
      } catch (InvalidOperationException) {
        Logger.Log("GeneratePersonalVehicleAndBlip(): InvalidOperationException");
        return;
      }

      if (specificVehicleDefinition != null) vehicleDefinition = specificVehicleDefinition;

      Vector3 vehicle_position = new Vector3(aSpawnPoint.X, aSpawnPoint.Y, aSpawnPoint.Z);
      Model model = new Model(vehicleDefinition.VehicleName).Hash;
      model.Request();
      while (!model.IsLoaded) {
        Script.Yield();
      }
      vehicle = World.CreateVehicle(vehicleDefinition.VehicleName, vehicle_position, aSpawnPoint.H);
      if (vehicle == null) {
        Logger.Log("GeneratePersonalVehicleAndBlip(): World.CreateVehicle() failed to create a vehicle, which should never happen.");
        return;
      }
      vehicle.PlaceOnGround();
      ConfigurePersonalVehicle(vehicle, vehicleDefinition);
      vehicle.LockStatus = VehicleLockStatus.Locked;
      vehicle.PreviouslyOwnedByPlayer = true;
      Logger.Log("GeneratePersonalVehicleAndBlip(): placed a " + vehicle.PrimaryColor + " "
        + (VehicleHash)vehicle.Model.Hash + " at ("
        + Math.Round(vehicle_position.X, 3) + ", "
        + Math.Round(vehicle_position.Y, 3) + ", "
        + Math.Round(vehicle_position.Z, 3) + ")");
      blip = vehicle.AddBlip();
      blip.Sprite = BlipSprite.PersonalVehicleCar;
      blip.Name = vehicleDefinition.VehicleName;
      blip.Scale = 0.8888f;
      Function.Call(Hash.SET_BLIP_COLOUR, blip, colorToUse);
    }

    private void ConfigurePersonalVehicle(Vehicle vehicle, VehicleDefinition vehicleDefinition) {
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
      vehicle.SetMod(VehicleMod.Spoilers, vehicleDefinition.Spoilers, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.FrontBumper, vehicleDefinition.FrontBumper, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.RearBumper, vehicleDefinition.RearBumper, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.SideSkirt, vehicleDefinition.SideSkirt, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Exhaust, vehicleDefinition.Exhaust, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Frame, vehicleDefinition.Frame, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Grille, vehicleDefinition.Grille, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Hood, vehicleDefinition.Hood, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Fender, vehicleDefinition.Fender, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.RightFender, vehicleDefinition.RightFender, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Roof, vehicleDefinition.Roof, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Engine, vehicleDefinition.Engine, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Brakes, vehicleDefinition.Brakes, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Transmission, vehicleDefinition.Transmission, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Horns, vehicleDefinition.Horns, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Suspension, vehicleDefinition.Suspension, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Armor, vehicleDefinition.Armor, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.FrontWheels, vehicleDefinition.FrontWheel, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.BackWheels, vehicleDefinition.RearWheel, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.PlateHolder, vehicleDefinition.PlateHolder, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.VanityPlates, vehicleDefinition.VanityPlates, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.TrimDesign, vehicleDefinition.TrimDesign, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Ornaments, vehicleDefinition.Ornaments, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Dashboard, vehicleDefinition.Dashboard, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.DialDesign, vehicleDefinition.DialDesign, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.DoorSpeakers, vehicleDefinition.DoorSpeakers, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Seats, vehicleDefinition.Seats, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.SteeringWheels, vehicleDefinition.SteeringWheels, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.ColumnShifterLevers, vehicleDefinition.ColumnShifterLevers, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Plaques, vehicleDefinition.Plaques, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Speakers, vehicleDefinition.Speakers, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Trunk, vehicleDefinition.Trunk, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Hydraulics, vehicleDefinition.Hydraulics, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.EngineBlock, vehicleDefinition.EngineBlock, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.AirFilter, vehicleDefinition.AirFilter, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Struts, vehicleDefinition.Struts, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.ArchCover, vehicleDefinition.ArchCover, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Aerials, vehicleDefinition.Aerials, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Trim, vehicleDefinition.Trim, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Tank, vehicleDefinition.Tank, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Windows, vehicleDefinition.Windows, vehicleDefinition.CustomTires);
      vehicle.SetMod(VehicleMod.Livery, vehicleDefinition.Livery, vehicleDefinition.CustomTires);
      vehicle.ToggleMod(VehicleToggleMod.Turbo, vehicleDefinition.Turbo);
      vehicle.ToggleMod(VehicleToggleMod.TireSmoke, vehicleDefinition.TireSmoke);
      vehicle.ToggleMod(VehicleToggleMod.XenonHeadlights, vehicleDefinition.XenonHeadlights);
      vehicle.ToggleExtra(01, vehicleDefinition.VehicleExtra01);
      vehicle.ToggleExtra(02, vehicleDefinition.VehicleExtra02);
      vehicle.ToggleExtra(03, vehicleDefinition.VehicleExtra03);
      vehicle.ToggleExtra(04, vehicleDefinition.VehicleExtra04);
      vehicle.ToggleExtra(05, vehicleDefinition.VehicleExtra05);
      vehicle.ToggleExtra(06, vehicleDefinition.VehicleExtra06);
      vehicle.ToggleExtra(07, vehicleDefinition.VehicleExtra07);
      vehicle.ToggleExtra(08, vehicleDefinition.VehicleExtra08);
      vehicle.ToggleExtra(09, vehicleDefinition.VehicleExtra09);
      vehicle.ToggleExtra(10, vehicleDefinition.VehicleExtra10);
      vehicle.ToggleExtra(11, vehicleDefinition.VehicleExtra11);
      vehicle.ToggleExtra(12, vehicleDefinition.VehicleExtra12);
      vehicle.ToggleExtra(13, vehicleDefinition.VehicleExtra13);
      vehicle.ToggleExtra(14, vehicleDefinition.VehicleExtra14);
    }

    private void DeliverPersonalVehicle(ref Vehicle vehicle, ref Ped driver) {
      driver = _personalVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);
      driver.Alpha = 0;
      Vector3 targetPos = World.GetNextPositionOnStreet(Game.Player.Character.Position);
      Function.Call(Hash.SET_DRIVER_ABILITY, driver, 1.0f);
      //Function.Call(Hash.SET_DRIVER_ABILITY, driver, 100.0f);
      Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, driver, _personalVehicle, targetPos.X, targetPos.Y, targetPos.Z, 22.2222f, 262204, 3.0f);
      //Function.Call(Hash.TASK_VEHICLE_GOTO_NAVMESH, driver, _personalVehicle, targetPos.X, targetPos.Y, targetPos.Z, 100.0f, 262204, 3.0f);
      //driver.Task.DriveTo(_personalVehicle, World.GetNextPositionOnStreet(Game.Player.Character.Position), 0.0f, 100.0f, (int)DrivingStyle.Rushed);
      //Function.Call(Hash.SET_DRIVE_TASK_MAX_CRUISE_SPEED, driver, 120);
      //Function.Call(Hash.SET_DRIVE_TASK_CRUISE_SPEED, driver, 80);
      Logger.Log("DeliverPersonalVehicle");
    }

    private void ParkPersonalVehicleNearby(Vehicle vehicle, ref Ped driver) {
      Vector3 playerPos = Game.Player.Character.Position;
      Vector3 streetNode = World.GetNextPositionOnStreet(Game.Player.Character.Position);
      Vector3 sidewalkNode = World.GetNextPositionOnSidewalk(Game.Player.Character.Position);
      //Vector3 midpoint = new Vector3((streetNode.X - sidewalkNode.X)/2, (streetNode.Y - sidewalkNode.Y) / 2, (streetNode.Z - sidewalkNode.Z) / 2);
      //Function.Call(Hash.TASK_VEHICLE_PARK, driver, vehicle, midpoint.X, midpoint.Y, midpoint.Z, 0, 0, 20.0f, true);
      Function.Call(Hash.TASK_VEHICLE_PARK, driver, vehicle, sidewalkNode.X, sidewalkNode.Y, sidewalkNode.Z, 0, 0, 20.0f, true);
      vehicle.PreviouslyOwnedByPlayer = true;
      driver.Delete();
      Logger.Log("ParkPersonalVehicleNearby");
    }

    private Vector4 FindASpawnPointForDelivery() {
      //Vector3 properPosition = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(Main.Rng.Next(132, 334)), true);
      Vector3 properPosition = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(Main.Rng.Next(_minimumAcceptableSpawnDistance, _maximumAcceptableSpawnDistance)), true);
      Logger.Log("FindASpawnPoint(): found a spawn point.");
      return new Vector4(properPosition, 0);
    }

    private Vector4 FindASpawnPointForPlacement() {
      Vector3 playerPosition = Game.Player.Character.Position;
      List<Vehicle> allVehicles = World.GetAllVehicles().ToList();
      allVehicles.Sort(Main.PlayerDistanceCompare);
      Logger.Log("FindASpawnPoint(): looking at " + allVehicles.Count + " possible vehicles.");
      foreach (Vehicle thisVehicle in allVehicles) {
        Script.Wait(0); // avoid slowing down gameplay
        if (!(World.GetDistance(thisVehicle.Position, playerPosition) > _minimumAcceptableSpawnDistance
            & World.GetDistance(thisVehicle.Position, playerPosition) < _maximumAcceptableSpawnDistance
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
            & thisVehicle.ClassType != VehicleClass.Trains)) continue; // lazy but should probably do OK for now.
        Vector3 thisVehiclePosition = thisVehicle.Position;
        float thisVehicleHeading = thisVehicle.Heading;
        thisVehicle.Delete();
        Logger.Log("FindASpawnPoint(): found a spawn point.");
        return new Vector4(thisVehiclePosition, thisVehicleHeading);
      }
      Logger.Log("FindASpawnPoint(): couldn't find a suitable spawn point.");
      throw new InvalidOperationException("FindASpawnPoint(): couldn't find a suitable spawn point.");
    }
  }
}
