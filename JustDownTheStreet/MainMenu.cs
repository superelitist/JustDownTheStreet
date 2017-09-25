using GTA;
using GTA.Native;
using NativeUI;
using static JustDownTheStreet.Main;

namespace JustDownTheStreet {
  internal class MainMenu {
    private MenuPool _menuPool = new MenuPool();
    private UIMenu _mainMenu = new UIMenu( "You Have A Lot Of Cars", "~b~A SUBTEXT" );

    private void AddMenuItemSaveAVehicle( UIMenu menu ) {
      var newItem = new UIMenuItem( "Save Current Vehicle", "Save the vehicle you are currently in." );
      newItem.SetRightBadge( UIMenuItem.BadgeStyle.Car );
      menu.AddItem( newItem );
      menu.OnItemSelect += ( sender, item, index ) => {
        if ( item != newItem ) return;
        //Vehicle vehicle = Game.Player.Character.CurrentVehicle; // get the vehicle our player is in
        if ( Game.Player.Character.CurrentVehicle == null ) {
          UI.Notify( "Player is not in a vehicle..." );
          return;
        }
        UI.Notify( "Saving current vehicle..." );
        SaveCurrentVehicleToJson( JsonFolder, ( (PedHash)Game.Player.Character.Model.Hash ).ToString() );
      };
    }

    private void AddMenuItemRequestAVehicle( UIMenu menu ) {
      var newItem = new UIMenuItem( "Request A Vehicle", "Request a new personal vehicle nearby." );
      newItem.SetRightBadge( UIMenuItem.BadgeStyle.Car );
      menu.AddItem( newItem );
      menu.OnItemSelect += ( sender, item, index ) => {
        if ( item != newItem ) return;
        //string output = ketchup ? "You have ordered ~b~{0}~w~ ~r~with~w~ ketchup." : "You have ordered ~b~{0}~w~ ~r~without~w~ ketchup.";
        //UI.ShowSubtitle(String.Format(output, dish));
        UI.Notify( "Requesting a vehicle" );
        RequestANewPersonalVehicle();
      };
    }

    private void AddMenuRequestASpecificVehicle( UIMenu menu ) {
      var subMenu = _menuPool.AddSubMenu( menu, "Request A Specific Vehicle", "Request a specific personal vehicle nearby." );
      foreach ( var vehicleDefinition in CurrentVehicleDefinitions ) {
        var eachItem = new UIMenuItem( vehicleDefinition.VehicleName, vehicleDefinition.Colors.Primary.ToString() );
        subMenu.AddItem( eachItem );
        menu.OnItemSelect += ( sender, item, index ) => {
          if ( item != eachItem ) return;
          UI.Notify( "Requesting: " + item.Text );
          VehicleDefinition specificVehicleDefinition = CurrentVehicleDefinitions[index];
          RequestANewPersonalVehicle( specificVehicleDefinition );
        };
      }
    }

    public MainMenu() {
      //Logger.Log("MainMenu(): Initializing.");
      _menuPool.Add( _mainMenu );
      AddMenuItemSaveAVehicle( _mainMenu );
      AddMenuItemRequestAVehicle( _mainMenu );
      AddMenuRequestASpecificVehicle( _mainMenu );
    }

    public void OnTickHandler() {
      _menuPool.ProcessMenus();
    }

    public void ToggleMenu() {
      if ( !_menuPool.IsAnyMenuOpen() ) // Our menu on/off switch
        _mainMenu.Visible = !_mainMenu.Visible;
    }
  }
}