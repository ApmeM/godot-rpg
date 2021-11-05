using IsometricGame.Logic.Enums;

namespace IsometricGame.Logic
{
    public class ServerConfiguration
    {
        public static int DefaultMaxSkills = 8;
        public static int DefaultMaxUnits = 4;
        public static float DefaultTurnTimeout = 60;

        public bool FullMapVisible;
        public float? TurnTimeout = DefaultTurnTimeout;

        public int MaxUnits = DefaultMaxUnits;
        public int MaxSkills = DefaultMaxSkills;
        public MapGeneratingType MapType = MapGeneratingType.Random;
    }
}