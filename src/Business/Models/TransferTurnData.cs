using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TransferTurnData
    {
        public Dictionary<int, YourUnitsData> YourUnits;
        public MapTile[,] VisibleMap;
        public Dictionary<int, OtherPlayersData> OtherPlayers;
        public bool GameOverWin;
        public bool GameOverLoose;

        public class YourUnitsData
        {
            public Vector2 Position;
            public int Hp;
            public int Mp;
            public List<(Ability, Vector2)> AppliedAbilities;
            public List<EffectDuration> Effects;
            public int MoveDistance;
            public int SightRange;
            public float RangedAttackDistance;
            public float AOEAttackRadius;
            public float AttackPower;
            public float MagicPower;
            public List<int> HpChanges;
            public List<int> MpChanges;
            public List<(Ability, Vector2)> ExecutedAbilities;
        }

        public class OtherPlayersData
        {
            public Dictionary<int, OtherUnitsData> Units;
        }
        public class OtherUnitsData
        {
            public Vector2 Position;
            public int Hp;
            public List<(Ability, Vector2)> AppliedAbilities;
            public List<EffectDuration> Effects;
            public List<int> HpChanges;
            public List<(Ability, Vector2)> ExecutedAbilities;
        }
    }
}
