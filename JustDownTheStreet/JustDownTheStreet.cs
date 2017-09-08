// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

enum Character { Michael, Franklin, Trevor, None = 99 }

struct APersonalVehicle
{
    public VehicleHash VehicleHash { get; set; }
    public String Plate { get; set; }
    public Character Character { get; set; }
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
    
    public APersonalVehicle(VehicleHash hash, String plate, Character character)
    {
        VehicleHash = hash;
        Plate = plate;
        Character = character;
        Spoilers = 0; FrontBumper = 0; RearBumper = 0; SideSkirt = 0; Exhaust = 0; Frame = 0; Grille = 0; Hood = 0;
        Fender = 0; RightFender = 0; Roof = 0; Engine = 0; Brakes = 0; Transmission = 0; Horns = 0; Suspension = 0;
        Armor = 0; FrontWheel = 0; RearWheel = 0; PlateHolder = 0; VanityPlates = 0; TrimDesign = 0; Ornaments = 0;
        Dashboard = 0; DialDesign = 0; DoorSpeakers = 0; Seats = 0; SteeringWheels = 0; ColumnShifterLevers = 0; Plaques = 0;
        Speakers = 0; Trunk = 0; Hydraulics = 0; EngineBlock = 0; AirFilter = 0; Struts = 0; ArchCover = 0; Aerials = 0;
        Trim = 0; Tank = 0; Windows = 0; Livery = 0;
    }

    //Other properties, methods, events...
}

/// <summary>
/// Static logger class that allows direct logging of anything to a text file
/// </summary>
public static class Logger
{
    public static void Log(object message)
    {
        File.AppendAllText("JustDownTheStreet.log", DateTime.Now + " : " + message + Environment.NewLine);
    }
}

public class JustDownTheStreet : Script      // This is the beginning of the script, public class
{
    private Player player = Game.Player;
    private Ped playerPed = Game.Player.Character;
    private Ped playerPedId = Function.Call<Ped>(Hash.PLAYER_PED_ID);
    private bool firstTick = true;
    private bool PlayerHasLostControl = false;
    private Vehicle personalVehicle;
    private Blip personalVehicleBlip;
    private String JsonFolder = AppDomain.CurrentDomain.BaseDirectory + "/jdts/";

    public JustDownTheStreet()
    {
        Tick += OnTick;         //
        KeyDown += OnKeyDown;   // These three are defined below.
        KeyUp += OnKeyUp;       //
    }

    private void Init(object sender, EventArgs e)
    {
        Logger.Log("--------------------------------");
        Logger.Log("Script initializing");
        DirectoryInfo di = Directory.CreateDirectory(JsonFolder);

        while (!player.CanControlCharacter) // check if player has control, probably indicating that the game has finished loading
        {
            Wait(0);
        }
        firstTick = false;
        CreatePersonalVehicle();
    }

    private void OnTick(object sender, EventArgs e)     // Here, stuff are run every frame of the gameplay
    {
        // This is where you add code
        //Logger.Log("Tick!");
        if (firstTick == true) // every tick, check if this is the first tick. If it is, run Init()
        {
            Init(sender, e);
        }
        if (!player.CanControlCharacter)
        {
            //Logger.Log("!player.CanControlCharacter");
            // Maybe I could replace my other function?!
        }
        if (!PlayerHasLostControl) // is the player not already switching?
        {
            if (!IsPlayerUnderArrestDeadOrLoading()) // has the player started switching?
            {
                Logger.Log("Player has lost control");
                PlayerHasLostControl = true;
                CleanupPersonalVehicle();
                return;
            }
        }
        if (PlayerHasLostControl) // has the player begun switching?
        {
            if (IsPlayerUnderArrestDeadOrLoading()) // has the player stopped switching?
            {
                Logger.Log("Player has regained control");
                PlayerHasLostControl = false;
                player = Game.Player;
                playerPed = Game.Player.Character;
                CreatePersonalVehicle();
                return;
            }
        }
        //Wait(500); // try not to stall the game?
    }

    private bool IsPlayerUnderArrestDeadOrLoading() // not always reliable, but... enough?
    {
        if (Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS) ||
            Function.Call<bool>(Hash.IS_ENTITY_DEAD, Function.Call<Ped>(Hash.PLAYER_PED_ID)) ||
            Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, player) ||
            Game.IsLoading)
        {
            //Logger.Log("NOT IN CONTROL");
            return false;
        }
        return true;
    }

    private void CreatePersonalVehicle()
    {
        Logger.Log("CreatePersonalVehicle()");
        Wait(500); // maybe give them time to spawn?
        Vehicle[] allVehicles = World.GetAllVehicles();
        //Array.ForEach<byte>(byteArray, b => b = 0xFF);
        for (int i = 0; i < allVehicles.Length; i++)
        {
            Vehicle thisVehicle = allVehicles[i];
            Logger.Log("thisVehicle distance: " + World.GetDistance(thisVehicle.Position, playerPed.Position).ToString());
            Logger.Log("thisVehicle IsStopped: " + thisVehicle.IsStopped.ToString());
            Logger.Log("thisVehicle Driver: " + thisVehicle.Driver.Model.ToString());
            if (World.GetDistance(thisVehicle.Position, playerPed.Position) > 50 &
                World.GetDistance(thisVehicle.Position, playerPed.Position) < 150 &
                thisVehicle.IsStopped &
                thisVehicle.Driver.Model == 0x0) // lazy but should probably do OK for now.
            {
                Logger.Log("Found a parked car! Replacing now...");
                Vector3 thisVehiclePosition = thisVehicle.Position;
                float thisVehicleHeading = thisVehicle.Heading;
                thisVehicle.Delete();
                personalVehicle = World.CreateVehicle(VehicleHash.Adder, thisVehiclePosition, thisVehicleHeading);
                personalVehicleBlip = personalVehicle.AddBlip();
                personalVehicleBlip.Sprite = BlipSprite.PersonalVehicleCar;
                personalVehicle.CanTiresBurst = false;
                personalVehicle.CustomPrimaryColor = Color.FromArgb(128, 64, 64);
                personalVehicle.CustomSecondaryColor = Color.Cyan;
                personalVehicle.PlaceOnGround();
                personalVehicle.NumberPlate = player.Name;
                return;
            }
        }
        Logger.Log("... couldn't find a suitable car...");
    }

    //private void CreatePersonalVehicle()
    //{
    //    Logger.Log("CreatePersonalVehicle()");
    //    Wait(500); // maybe give them time to spawn?
    //    //Logger.Log("player: " + CurrentPlayerCharacter());
    //    //Logger.Log("playerPed.Position: " + playerPed.Position.ToString());
    //    Vector3 position = World.GetNextPositionOnStreet(playerPed.Position, true);
    //    float thisVehicleHeading = playerPed.Heading + 90;
    //    personalVehicle = World.CreateVehicle(VehicleHash.Adder, position, thisVehicleHeading);
    //    personalVehicleBlip = personalVehicle.AddBlip();
    //    personalVehicleBlip.Sprite = BlipSprite.PersonalVehicleCar;
    //    personalVehicle.CanTiresBurst = false;
    //    personalVehicle.CustomPrimaryColor = Color.FromArgb(128, 64, 64);
    //    personalVehicle.CustomSecondaryColor = Color.Cyan;
    //    personalVehicle.PlaceOnGround();
    //    personalVehicle.NumberPlate = player.Name;
    //}

    private void CleanupPersonalVehicle()
    {
        Wait(500); // or the player ped will be standing in the middle of the street.
        Logger.Log("CleanupPersonalVehicle()");
        if (personalVehicleBlip != null)
        {
            personalVehicleBlip.Remove();
        }
        if (personalVehicle != null)
        {
            personalVehicle.Delete();
        }
        return;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)       // Code inside this is what happens when a key gets pushed down or held down
    {
        if (e.KeyCode == Keys.OemQuestion)    // This checks if the key "X" is pressed
        {
            UI.Notify("Cleanup and Create");
            CleanupPersonalVehicle();
            CreatePersonalVehicle();
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)     // Code inside this is what happens when a key gets released
    {
        if (e.KeyCode == Keys.X)
        {
            //Logger.Log("X key pressed...");
            Vehicle vehicle = playerPed.CurrentVehicle; // get the vehicle our player is in!
            APersonalVehicle aPersonalVehicle = new APersonalVehicle((VehicleHash)vehicle.Model.Hash, vehicle.NumberPlate, CurrentPlayerCharacter())
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
                //Character = playerPed.GetHashCode()
                Character = CurrentPlayerCharacter()
            };
            Logger.Log(JsonFolder + aPersonalVehicle.Plate + ".json" + ")");
            File.WriteAllText(JsonFolder + aPersonalVehicle.Plate + ".json", JsonConvert.SerializeObject(aPersonalVehicle));
            //File.WriteAllText("/scripts/jdts/" + aPersonalVehicle.Plate + ".json", JsonConvert.SerializeObject(aPersonalVehicle));
        }
    }

    private Character CurrentPlayerCharacter()
    {
        Logger.Log("CurrentPlayerCharacter()");
        Model modelZero = new Model("player_zero");
        int hashZero = modelZero.Hash;
        Model modelOne = new Model("player_one");
        int hashOne = modelOne.Hash;
        Model modelTwo = new Model("player_two");
        int hashTwo = modelTwo.Hash;

        if (playerPed.Model.Hash == hashZero) return Character.Michael;
        if (playerPed.Model.Hash == hashOne) return Character.Franklin;
        if (playerPed.Model.Hash == hashTwo) return Character.Trevor;
        return Character.None;
    }
}