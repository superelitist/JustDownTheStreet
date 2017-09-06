// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
// 
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
// 
using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Media;

public class template : Script      // This is the beginning of the script, public class
{
    private Ped playerPed = Game.Player.Character;  // Used for coding later on
    private Player player = Game.Player;            // Used for coding later on as well

    public template()
    {
        Tick += OnTick;         // 
        KeyDown += OnKeyDown;   // These three are defined below.
        KeyUp += OnKeyUp;       // 
    }

    private void OnTick(object sender, EventArgs e)     // Here, stuff are run every frame of the gameplay
    {
        // This is where you add code
    }

    private void OnKeyDown(object sender, KeyEventArgs e)       // Code inside this is what happens when a key gets pushed down or held down
    {
        // This is where you add code, I'll add a quick example for this one

        if (e.KeyCode == Keys.X)    // This checks if the key "X" is pressed
        {
            UI.Notify("Key ~b~X ~w~pressed!");      // This puts a notification above the minimap with whatever you put inside the quotation marks
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