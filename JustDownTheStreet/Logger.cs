using System;
using System.IO;
using GTA;

namespace JustDownTheStreet
{
    /// <summary>
    /// Static logger class that allows direct logging of anything to a text file
    /// </summary>
    public static class Logger
    {
        private static int _tickCount = Environment.TickCount;

        public static void Log(object message)
        {
            File.AppendAllText("JustDownTheStreet.log", DateTime.Now + " : " + message + Environment.NewLine);
        }

        public static void LogFPS()
        {
            File.AppendAllText("JustDownTheStreet_FPS.log", DateTime.Now + ", " + (Environment.TickCount - _tickCount) + ", " + Game.FPS + Environment.NewLine);
            _tickCount = Environment.TickCount;
        }

        public static void LogPosition()
        {
            var position = Game.Player.Character.Position;
            var street = World.GetStreetName(position);
            File.AppendAllText("JustDownTheStreet.xyz", DateTime.Now + "," + street + "," + position.X + "," + position.Y + "," + position.Z + Environment.NewLine);
        }
    }

}