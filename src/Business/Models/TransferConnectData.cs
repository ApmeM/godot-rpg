using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic
{
    public class TransferConnectData
    {
        public string TeamName;
        public List<UnitData> Units;

        public class UnitData
        {
            public UnitType UnitType;
            public List<Skill> Skills = new List<Skill>();
        }
	}
}