// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: CLSCompliant(false)]

namespace JustDownTheStreet
{
    public class JustDownTheStreet : Script      // This is the beginning of the script, public class
    {
        private readonly Random _rng = new Random();
        private readonly MD5 _hash = new MD5Cng();
        private bool _firstTick = true;
        private bool _playerIsInControl;
        private Vehicle _personalVehicle;
        private Blip _personalVehicleBlip;
        private readonly string _jsonFolder = AppDomain.CurrentDomain.BaseDirectory + "/jdts/";
        List<VehicleDefinition> _personalVehiclesMichael = new List<VehicleDefinition>();
        List<VehicleDefinition> _personalVehiclesFranklin = new List<VehicleDefinition>();
        List<VehicleDefinition> _personalVehiclesTrevor = new List<VehicleDefinition>();

        public JustDownTheStreet()
        {
            Tick += OnTick;         //
            KeyDown += OnKeyDown;   // These three are defined below.
            KeyUp += OnKeyUp;       //
        }

        private static bool IsPlayerSwitchingUnderArrestDeadOrLoading() // not always reliable, but... enough?
        {
            if (Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS) ||
                Function.Call<bool>(Hash.IS_ENTITY_DEAD, Function.Call<Ped>(Hash.PLAYER_PED_ID)) ||
                Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player.Character) ||
                Game.IsLoading)
            {
                return true;
            }
            return false;
        }

        private static Vector4 FindASpawnPoint()
        {
            //Logger.Log("FindASpawnPoint()");
            Wait(500); // maybe give them time to spawn?
            Vehicle[] allVehicles = World.GetAllVehicles();
            Logger.Log("FindASpawnPoint(): looking at " + allVehicles.Length + " possible vehicles.");
            Vector3 playerPosition = Game.Player.Character.Position;
            foreach (Vehicle thisVehicle in allVehicles)
            {
                Wait(0); // avoid slowing down gameplay
                Logger.Log("FindASpawnPoint() distance: " +
                            Math.Round(World.GetDistance(thisVehicle.Position, playerPosition), 3)
                                .ToString(CultureInfo.InvariantCulture)
                            + ", IsStopped: " + thisVehicle.IsStopped
                            + ", Driver: " + thisVehicle.Driver.Model);
                //Logger.Log("FindASpawnPoint() distance: " + World.GetDistance(thisVehicle.Position, playerPosition).ToString(CultureInfo.InvariantCulture));
                //Logger.Log("thisVehicle IsStopped: " + thisVehicle.IsStopped);
                //Logger.Log("thisVehicle Driver: " + thisVehicle.Driver.Model);
                if (World.GetDistance(thisVehicle.Position, playerPosition) > 25 &
                    World.GetDistance(thisVehicle.Position, playerPosition) < 125 &
                    thisVehicle.IsStopped &
                    thisVehicle.Driver.Model == 0x0) // lazy but should probably do OK for now.
                {
                    //Logger.Log("Found a parked car!");
                    Vector3 thisVehiclePosition = thisVehicle.Position;
                    float thisVehicleHeading = thisVehicle.Heading;
                    thisVehicle.Delete();
                    return new Vector4(thisVehiclePosition, thisVehicleHeading);
                }
            }
            Logger.Log("... couldn't find a suitable car...");
            throw new Exception();
        }

        private VehicleDefinition SelectAPersonalVehicle(string currentCharacterName)
        {
            VehicleDefinition selectedVehicleDefinition = new VehicleDefinition("", "", "");
            switch (currentCharacterName)
            {
                case "Michael":
                    {
                        if (_personalVehiclesMichael.Count == 0)
                        {
                            Logger.Log("SelectAPersonalVehicle(): Michael's list is empty! I guess we can't spawn him a vehicle...");
                            return null;
                        }
                        int randomNumber = _rng.Next(_personalVehiclesMichael.Count);
                        selectedVehicleDefinition = _personalVehiclesMichael[randomNumber];
                        Logger.Log("SelectAPersonalVehicle(): Selecting a vehicle from Michael's list: " + selectedVehicleDefinition.VehicleName + " with plate " + selectedVehicleDefinition.Plate);
                    }
                    break;
                case "Franklin":
                    {
                        if (_personalVehiclesFranklin.Count == 0)
                        {
                            Logger.Log("SelectAPersonalVehicle(): Franklin's list is empty! I guess we can't spawn him a vehicle...");
                            return null;
                        }
                        int randomNumber = _rng.Next(_personalVehiclesFranklin.Count);
                        selectedVehicleDefinition = _personalVehiclesFranklin[randomNumber];
                        Logger.Log("SelectAPersonalVehicle(): Selecting a vehicle from Franklin's list: " + selectedVehicleDefinition.VehicleName + " => " + selectedVehicleDefinition.Plate);
                    }
                    break;
                case "Trevor":
                    {
                        if (_personalVehiclesTrevor.Count == 0)
                        {
                            Logger.Log("SelectAPersonalVehicle(): Trevor's list is empty! I guess we can't spawn him a vehicle...");
                            return null;
                        }
                        int randomNumber = _rng.Next(_personalVehiclesTrevor.Count);
                        selectedVehicleDefinition = _personalVehiclesTrevor[randomNumber];
                        Logger.Log("SelectAPersonalVehicle(): Selecting a vehicle from Trevor's list: " + selectedVehicleDefinition.VehicleName + " => " + selectedVehicleDefinition.Plate);
                    }
                    break;
                default:
                    Logger.Log("SelectAPersonalVehicle(): Something went horribly wrong, I can't match this character to a name!...");
                    break;
            }
            if (!String.IsNullOrEmpty(selectedVehicleDefinition.VehicleName)) return selectedVehicleDefinition;
            Logger.Log("SelectAPersonalVehicle(): selectedVehicleDefinition was not assigned!");
            return null;
        }

        private void ConfigurePersonalVehicle(Vehicle vehicle, VehicleDefinition vehicleDefinition)
        {
            vehicle.NumberPlate = vehicleDefinition.Plate;
            vehicle.InstallModKit();
            vehicle.SetMod(VehicleMod.Spoilers, vehicleDefinition.Spoilers, true);
            vehicle.SetMod(VehicleMod.FrontBumper, vehicleDefinition.FrontBumper, true);
            vehicle.SetMod(VehicleMod.RearBumper, vehicleDefinition.RearBumper, true);
            vehicle.SetMod(VehicleMod.SideSkirt, vehicleDefinition.SideSkirt, true);
            vehicle.SetMod(VehicleMod.Exhaust, vehicleDefinition.Exhaust, true);
            vehicle.SetMod(VehicleMod.Frame, vehicleDefinition.Frame, true);
            vehicle.SetMod(VehicleMod.Grille, vehicleDefinition.Grille, true);
            vehicle.SetMod(VehicleMod.Hood, vehicleDefinition.Hood, true);
            vehicle.SetMod(VehicleMod.Fender, vehicleDefinition.Fender, true);
            vehicle.SetMod(VehicleMod.RightFender, vehicleDefinition.RightFender, true);
            vehicle.SetMod(VehicleMod.Roof, vehicleDefinition.Roof, true);
            vehicle.SetMod(VehicleMod.Engine, vehicleDefinition.Engine, true);
            vehicle.SetMod(VehicleMod.Brakes, vehicleDefinition.Brakes, true);
            vehicle.SetMod(VehicleMod.Transmission, vehicleDefinition.Transmission, true);
            vehicle.SetMod(VehicleMod.Horns, vehicleDefinition.Horns, true);
            vehicle.SetMod(VehicleMod.Suspension, vehicleDefinition.Suspension, true);
            vehicle.SetMod(VehicleMod.Armor, vehicleDefinition.Armor, true);
            vehicle.SetMod(VehicleMod.FrontWheels, vehicleDefinition.FrontWheel, true);
            vehicle.SetMod(VehicleMod.BackWheels, vehicleDefinition.RearWheel, true);
            vehicle.SetMod(VehicleMod.PlateHolder, vehicleDefinition.PlateHolder, true);
            vehicle.SetMod(VehicleMod.VanityPlates, vehicleDefinition.VanityPlates, true);
            vehicle.SetMod(VehicleMod.TrimDesign, vehicleDefinition.TrimDesign, true);
            vehicle.SetMod(VehicleMod.Ornaments, vehicleDefinition.Ornaments, true);
            vehicle.SetMod(VehicleMod.Dashboard, vehicleDefinition.Dashboard, true);
            vehicle.SetMod(VehicleMod.DialDesign, vehicleDefinition.DialDesign, true);
            vehicle.SetMod(VehicleMod.DoorSpeakers, vehicleDefinition.DoorSpeakers, true);
            vehicle.SetMod(VehicleMod.Seats, vehicleDefinition.Seats, true);
            vehicle.SetMod(VehicleMod.SteeringWheels, vehicleDefinition.SteeringWheels, true);
            vehicle.SetMod(VehicleMod.ColumnShifterLevers, vehicleDefinition.ColumnShifterLevers, true);
            vehicle.SetMod(VehicleMod.Plaques, vehicleDefinition.Plaques, true);
            vehicle.SetMod(VehicleMod.Speakers, vehicleDefinition.Speakers, true);
            vehicle.SetMod(VehicleMod.Trunk, vehicleDefinition.Trunk, true);
            vehicle.SetMod(VehicleMod.Hydraulics, vehicleDefinition.Hydraulics, true);
            vehicle.SetMod(VehicleMod.EngineBlock, vehicleDefinition.EngineBlock, true);
            vehicle.SetMod(VehicleMod.AirFilter, vehicleDefinition.AirFilter, true);
            vehicle.SetMod(VehicleMod.Struts, vehicleDefinition.Struts, true);
            vehicle.SetMod(VehicleMod.ArchCover, vehicleDefinition.ArchCover, true);
            vehicle.SetMod(VehicleMod.Aerials, vehicleDefinition.Aerials, true);
            vehicle.SetMod(VehicleMod.Trim, vehicleDefinition.Trim, true);
            vehicle.SetMod(VehicleMod.Tank, vehicleDefinition.Tank, true);
            vehicle.SetMod(VehicleMod.Windows, vehicleDefinition.Windows, true);
            vehicle.SetMod(VehicleMod.Livery, vehicleDefinition.Livery, true);
            vehicle.ToggleMod(VehicleToggleMod.Turbo, vehicleDefinition.Turbo);
            vehicle.ToggleMod(VehicleToggleMod.TireSmoke, vehicleDefinition.TireSmoke);
            vehicle.ToggleMod(VehicleToggleMod.XenonHeadlights, vehicleDefinition.XenonHeadlights);
            vehicle.CanTiresBurst = vehicleDefinition.CanTiresBurst;
            // colors
            // for now I'm skipping the customs, I'm not actually sure if I can even set those...
            vehicle.PrimaryColor = (VehicleColor) vehicleDefinition.Colors.Primary;
            vehicle.SecondaryColor = (VehicleColor) vehicleDefinition.Colors.Secondary;
            vehicle.RimColor = (VehicleColor) vehicleDefinition.Colors.Rim;
            //vehicle.NeonLightsColor = Color.FromArgb(0,vehicleDefinition.Colors.NeonR, vehicleDefinition.Colors.NeonG, vehicleDefinition.Colors.NeonB);
            vehicle.NeonLightsColor = vehicleDefinition.Colors.Neon;
            //vehicle.TireSmokeColor = Color.FromArgb(0, vehicleDefinition.Colors.TireSmokeR, vehicleDefinition.Colors.TireSmokeG, vehicleDefinition.Colors.TireSmokeB);
            vehicle.TireSmokeColor = vehicleDefinition.Colors.TireSmoke;
            vehicle.TrimColor = (VehicleColor)vehicleDefinition.Trim;
            vehicle.DashboardColor = (VehicleColor) vehicleDefinition.Dashboard;
        }

        private void CleanupPersonalVehicleAndBlip(ref Vehicle vehicle, ref Blip blip)
        {
            Wait(500); // or the player ped will be standing in the middle of the street.
            Logger.Log("CleanupPersonalVehicleAndBlip()");
            if (blip != null)
            {
                blip.Remove();
            }
            if (vehicle != null)
            {
                vehicle.Delete();
                vehicle = null;
            }
        }

        static string GetHash(MD5 hash, string input) // from MSDN, but I think modified slightly
        {
            byte[] data = hash.ComputeHash(Encoding.UTF8.GetBytes(input)); // Convert the input string to a byte array and compute the hash.
            StringBuilder sBuilder = new StringBuilder(); // Create a new Stringbuilder to collect the bytes and create a string.
            foreach (byte t in data) // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString(); // Return the hexadecimal string.
        }

        private void SaveCurrentVehicleToJson(string jsonfolder, string currentCharacterName)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle; // get the vehicle our player is in
            if (vehicle == null) return;
            Colors vehicleColors = new Colors(vehicle.PrimaryColor,vehicle.SecondaryColor,vehicle.PearlescentColor)
            {
                Rim = vehicle.RimColor,
                Neon = vehicle.NeonLightsColor,
                TireSmoke = vehicle.TireSmokeColor,
                Trim = vehicle.TrimColor,
                Dashboard = vehicle.DashboardColor
            };
            VehicleDefinition vehicleDefinition = new VehicleDefinition(((VehicleHash)vehicle.Model.Hash).ToString(), vehicle.NumberPlate, currentCharacterName)
            {
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
                Colors = vehicleColors
            };
            string jsonString = JsonConvert.SerializeObject(vehicleDefinition, Formatting.Indented);
            string vehiclehashcode = GetHash(_hash, jsonString);
            string fullPath = jsonfolder + currentCharacterName + "_" + vehicleDefinition.VehicleName + "_" +
                          vehiclehashcode + ".json";
            if (File.Exists(fullPath))
            {
                Logger.Log(fullPath + "- vehicle exists, skipping save.");
                return;
            }
            Logger.Log(fullPath + " - saving vehicle...");
            File.WriteAllText(fullPath, jsonString);
        }

        private List<VehicleDefinition> GetAllPersonalVehiclesFromJson(string jsonfolder, string name)
        {
            List<VehicleDefinition> list = new List<VehicleDefinition>();
            string[] jsonFiles = Directory.GetFiles(jsonfolder, "*.json");
            foreach (string t in jsonFiles)
            {
                Wait(0); // avoid slowing down gameplay
                VehicleDefinition thisPersonalVehicleDefinition = JsonConvert.DeserializeObject<VehicleDefinition>(File.ReadAllText(t));
                if (thisPersonalVehicleDefinition.Character == name)
                {
                    list.Add(thisPersonalVehicleDefinition);
                }
            }
            Logger.Log("GetAllPersonalVehiclesFromJson(" + name + "): found " + list.Count + " vehicles");
            return list;
        }

        private void Init()
        {
            Logger.Log("----------------------------------------------------------------");
            Logger.Log("Init(): Script initializing");
            //DirectoryInfo di =
            Directory.CreateDirectory(_jsonFolder);

            _personalVehiclesMichael = GetAllPersonalVehiclesFromJson(_jsonFolder, "Michael");
            _personalVehiclesFranklin = GetAllPersonalVehiclesFromJson(_jsonFolder, "Franklin");
            _personalVehiclesTrevor = GetAllPersonalVehiclesFromJson(_jsonFolder, "Trevor");

            while (!Game.Player.CanControlCharacter) // check if player has control, probably indicating that the game has finished loading
            {
                Wait(0);
            }
            _firstTick = false;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_firstTick) // every tick, check if this is the first tick. If it is, run Init()
            {
                Init();
            }
            if (_playerIsInControl && IsPlayerSwitchingUnderArrestDeadOrLoading()) // is the player switching but wasn't already?
            {
                Logger.Log("OnTick(): Player has lost control, destroying personal vehicle.");
                _playerIsInControl = false;
                CleanupPersonalVehicleAndBlip(ref _personalVehicle, ref _personalVehicleBlip);
                return;
            }
            if (!_playerIsInControl && !IsPlayerSwitchingUnderArrestDeadOrLoading())
            {
                Logger.Log("OnTick(): Player has regained control.");
                _playerIsInControl = true;
                if (_personalVehicle != null) return;
                Logger.Log("OnTick(): Player does not already have a personal vehicle.");
                try
                {
                    Vector4 aSpawnPoint = FindASpawnPoint();
                    Logger.Log("OnTick(): FindASpawnPoint() found a spawn point.");
                    VehicleDefinition vehicleDefinition = SelectAPersonalVehicle(((PedHash)Game.Player.Character.Model.Hash).ToString());
                    Vector3 createVehicleVector3BecauseCreateVehicleIsStupidAndDoesNotDefineAConstructorForPlainXyzCoordinates = new Vector3(aSpawnPoint.X, aSpawnPoint.Y, aSpawnPoint.Z);
                    _personalVehicle = World.CreateVehicle(vehicleDefinition.VehicleName, 
                        createVehicleVector3BecauseCreateVehicleIsStupidAndDoesNotDefineAConstructorForPlainXyzCoordinates, aSpawnPoint.H);
                    _personalVehicle.PlaceOnGround();
                    ConfigurePersonalVehicle(_personalVehicle, vehicleDefinition);
                    Logger.Log("OnTick(): World.CreateVehicle(): placed a " + _personalVehicle.PrimaryColor + " " + (VehicleHash)_personalVehicle.Model.Hash + " at (" + 
                        Math.Round(createVehicleVector3BecauseCreateVehicleIsStupidAndDoesNotDefineAConstructorForPlainXyzCoordinates.X,3) + ", " +
                        Math.Round(createVehicleVector3BecauseCreateVehicleIsStupidAndDoesNotDefineAConstructorForPlainXyzCoordinates.Y, 3) + ", " +
                        Math.Round(createVehicleVector3BecauseCreateVehicleIsStupidAndDoesNotDefineAConstructorForPlainXyzCoordinates.Z, 3) + ")");
                    _personalVehicleBlip = _personalVehicle.AddBlip();
                    _personalVehicleBlip.Sprite = BlipSprite.PersonalVehicleCar;
                }
                catch (Exception)
                {
                    Logger.Log("OnTick(): FindASpawnPoint() returned null, waiting...");
                    Wait(1000);
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)       // Code inside this is what happens when a key gets pushed down or held down
        {
            //if (e.KeyCode != Keys.OemQuestion) return;
            //UI.Notify("Cleanup and Create");
            //CleanupPersonalVehicleAndBlip(ref _personalVehicle, ref _personalVehicleBlip);
            //FindASpawnPoint();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)     // Code inside this is what happens when a key gets released
        {
            if (e.KeyCode == Keys.X)
            {
                Logger.Log("OnKeyUp(): X key pressed, calling SaveCurrentVehicleToJson()");
                SaveCurrentVehicleToJson(_jsonFolder, ((PedHash)Game.Player.Character.Model.Hash).ToString());
            }
        }

    }
}