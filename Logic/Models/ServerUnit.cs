using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerUnit
    {
        public Vector2 Position;
        public UnitType UnitType;
        public int MaxHp = 10;
        public int Hp = 10;

        public HashSet<Usable> Usables;
        public int MoveDistance = 5;
        public int VisionRange = 6;
        public int AttackDistance = 1;
        public int AttackRadius = 1;
        public int AttackDamage = 2;
        public readonly HashSet<Skill> Skills = new HashSet<Skill>();
    }
}