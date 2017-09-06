// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//
using GTA;
using GTA.Native;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
    private Ped playerPed = Game.Player.Character;  // Used for coding later on
    private Player player = Game.Player;            // Used for coding later on as well
    private bool firstTick = true;
    private bool playerSwitchInProgress = false;
    private Vehicle personalVehicle;
    private Blip personalVehicleBlip;

    public JustDownTheStreet()
    {
        Tick += OnTick;         //
        KeyDown += OnKeyDown;   // These three are defined below.
        KeyUp += OnKeyUp;       //
    }

    private void OnTick(object sender, EventArgs e)     // Here, stuff are run every frame of the gameplay
    {
        // This is where you add code

        if (firstTick == true) // every tick, check if this is the first tick. If it is, run OnFirstTick()
        {
            while (!player.CanControlCharacter) // check if player has control, probably indicating that the game has finished loading
            {
                Wait(0);
            }
            firstTick = false;
            CreatePersonalVehicle();
        }
        if (!playerSwitchInProgress) // is the player not already switching?
        {
            if (!IsPlayerInControl()) // has the player started switching?
            {
                playerSwitchInProgress = true;
                CleanupPersonalVehicle();
                return;
            }
        }
        if (playerSwitchInProgress) // has the player begun switching?
        {
            if (IsPlayerInControl()) // has the player stopped switching?
            {
                playerSwitchInProgress = false;
                CreatePersonalVehicle();
            }
        }
        Wait(250); // try not to stall the game?
    }

    private bool IsPlayerInControl()
    {
        if (Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS) || !playerPed.IsAlive || Function.Call<bool>(GTA.Native.Hash.IS_PED_BEING_ARRESTED) || Game.IsLoading)
        {
            return false;
        }
        return true;
    }

    private void CreatePersonalVehicle()
    {
        personalVehicle = World.CreateVehicle(VehicleHash.Adder, player.Character.Position + player.Character.ForwardVector * 3.0f, Game.Player.Character.Heading + 90);
        personalVehicleBlip = personalVehicle.AddBlip();
        personalVehicleBlip.Sprite = BlipSprite.PersonalVehicleCar;
        personalVehicle.CanTiresBurst = false;
        personalVehicle.CustomPrimaryColor = Color.FromArgb(38, 38, 38);
        personalVehicle.CustomSecondaryColor = Color.DarkOrange;
        personalVehicle.PlaceOnGround();
        personalVehicle.NumberPlate = player.Name;
    }

    private void CleanupPersonalVehicle()
    {
        personalVehicleBlip.Remove();
        personalVehicle.Delete();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)       // Code inside this is what happens when a key gets pushed down or held down
    {
        // This is where you add code, I'll add a quick example for this one

        if (e.KeyCode == Keys.X)    // This checks if the key "X" is pressed
        {
            UI.Notify("Cleanup and Create");      // This puts a notification above the minimap with whatever you put inside the quotation marks
            CleanupPersonalVehicle();
            CreatePersonalVehicle();
        }

        if (e.KeyCode == Keys.F2)   // This checks if the key "F2" is pressed
        {
            UI.Notify("Key ~b~F2 ~w~pressed!");     // This puts a notification above the minimap with whatever you put inside the quotation marks
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)     // Code inside this is what happens when a key gets released
    {
        // This is also where you add code
    }
}