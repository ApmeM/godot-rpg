namespace IsometricGame.Logic
{
    public class ServerConfiguration : Godot.Object
    {
        public static int DefaultMaxSkills = 8;
        public static int DefaultMaxUnits = 4;

        public bool FullMapVisible;
        public int PlayersCount = 2;
        public int MaxUnits = DefaultMaxUnits;
        public int MaxSkills = DefaultMaxSkills;
    }
}