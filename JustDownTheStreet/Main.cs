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
//using PerVehCtl = JustDownTheStreet.PersonalVehicleController;

[assembly: CLSCompliant(false)]

namespace JustDownTheStreet {
  public class Main : Script {

    
    //public static readonly Random Rng = new Random();
    public static string CurrentPlayerName;
    private static int _millisecondsToDelayQualityOfLife = 333333;
    private static int _minimumMultipleOfPlayerMoneyToPurchase = 11;
    private static readonly Random Rng = new Random();
    private static int _priceOfBodyArmor = 6666;
    private readonly Stopwatch _lastTimeFranklinWasInControl = new Stopwatch();
    private readonly Stopwatch _lastTimeMichaelWasInControl = new Stopwatch();
    private readonly Stopwatch _lastTimeTrevorWasInControl = new Stopwatch();
    private bool _playerIsInControl;
    private MainMenu _mainMenu = new MainMenu();
    //public PersonalVehicleController _personalVehicleController = new PersonalVehicleController();

    public Main() {
      Logger.Log("----------------------------------------------------------------");
      Logger.Log("Main(): Script initializing");
      
      // check if player has control, probably indicating that the game has finished loading
      while ( !Game.Player.CanControlCharacter) {
        Yield();
      }
      _lastTimeMichaelWasInControl.Start();
      _lastTimeFranklinWasInControl.Start();
      _lastTimeTrevorWasInControl.Start();
      Tick += OnTick;
      KeyDown += KeyDownHandler;
      KeyUp += KeyUpHandler; // hook up the handlers
    }

    public static bool IsPlayerSwitchingUnderArrestDeadOrLoading() {
      if (Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS)
          || Function.Call<bool>(Hash.IS_ENTITY_DEAD, Function.Call<Ped>(Hash.PLAYER_PED_ID))
          || Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player.Character)
          || Game.IsLoading) {
        return true;
      }
      return false;
    }

    public static void ProvideQualityOfLifeForCharacter() {
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

    public void KeyDownHandler( object sender, KeyEventArgs e ) {
    }

    public void KeyUpHandler( object sender, KeyEventArgs e ) {
      if ( e.KeyCode == Keys.OemCloseBrackets ) {
        //if (Game.IsKeyPressed(Keys.OemOpenBrackets)) GenerateAllPersonalVehicles();
        //if (Game.IsKeyPressed(Keys.OemPipe)) ProvideQualityOfLifeForCharacter();
      }
    }
        
    public void OnTick( object sender, EventArgs e ) { // roughly every 16ms (60 times per second)

      // when player switch begins
      if ( _playerIsInControl && IsPlayerSwitchingUnderArrestDeadOrLoading()) {
        Logger.Log("OnTick(): Player has lost control of " + CurrentPlayerName);
        switch (CurrentPlayerName) {
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
            Logger.Log( "OnTick(): couldn't match on character name. This should never happen." );
            _lastTimeMichaelWasInControl.Restart();
            _lastTimeFranklinWasInControl.Restart();
            _lastTimeTrevorWasInControl.Restart();
            break;
        }
        _playerIsInControl = false;
        return;
      }

      // when player switch ends
      if (!_playerIsInControl && !IsPlayerSwitchingUnderArrestDeadOrLoading()) {
        CurrentPlayerName = ((PedHash) Game.Player.Character.Model.Hash).ToString();
        Logger.Log("OnTick(): Player has gained control of " + CurrentPlayerName);
        _playerIsInControl = true;
        switch ( CurrentPlayerName ) {
          case "Michael":
            Logger.Log( "OnTick(): _lastTimeMichaelWasInControl.ElapsedMilliseconds " + _lastTimeMichaelWasInControl.ElapsedMilliseconds );
            if ( _lastTimeMichaelWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife ) ProvideQualityOfLifeForCharacter();
            break;
          case "Franklin":
            Logger.Log( "OnTick(): _lastTimeFranklinWasInControl.ElapsedMilliseconds " + _lastTimeFranklinWasInControl.ElapsedMilliseconds );
            if ( _lastTimeFranklinWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife ) ProvideQualityOfLifeForCharacter();
            break;
          case "Trevor":
            Logger.Log( "OnTick(): _lastTimeTrevorWasInControl.ElapsedMilliseconds " + _lastTimeTrevorWasInControl.ElapsedMilliseconds );
            if ( _lastTimeTrevorWasInControl.ElapsedMilliseconds > _millisecondsToDelayQualityOfLife ) ProvideQualityOfLifeForCharacter();
            break;
          default:
            Logger.Log( "OnTick(): couldn't match on character name. This should never happen." );
            break;
        }
      }

      _mainMenu.OnTickHandler();
      PersonalVehicleController.UpdateHandler( CurrentPlayerName, _playerIsInControl );
      XBoxControllerUpdate();
      //Logger.LogFPS(); // this file gets big fast...
    }

    public void XBoxControllerUpdate() {
      if (Game.IsControlPressed(0, Control.ScriptPadDown) && Game.IsControlJustReleased(0, Control.ScriptLB)) {
        //SaveCurrentVehicleToJson(_jsonFolder, ((PedHash) Game.Player.Character.Model.Hash).ToString());
        _mainMenu = new MainMenu();
        _mainMenu.ToggleMenu(); // this is not a real thing
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