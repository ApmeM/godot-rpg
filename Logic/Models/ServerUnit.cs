using Godot;
using IsometricGame.Logic.Enums;

namespace IsometricGame.Logic.Models
{
    public class ServerUnit
    {
        public UnitType UnitType;
        public Vector2 Position;
        public int MoveDistance = 5;
        public int VisionRange = 6;
        public int AttackDistance = 1;
        public int AttackRadius = 1;
        public int AttackDamage = 2;
        public bool AttackFriendlyFire = false;
        public int MaxHp = 10;
        public int Hp = 10;
    }
}