using Godot;
using IsometricGame.Logic.Enums;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerUnit
    {
        public Vector2 Position;
        public UnitType UnitType;
        public int MaxHp = 10;
        public int Hp = 10;

        public HashSet<Ability> Abilities;
        public int MoveDistance = 5;
        public int SightRange = 6;

        public float RangedAttackDistance = 1;
        public float AOEAttackRadius = 1;
        public float AttackPower = 1;
        public float MagicPower = 1;
        public readonly List<EffectDuration> Effects = new List<EffectDuration>();
        public readonly HashSet<Skill> Skills = new HashSet<Skill>();
    }
}