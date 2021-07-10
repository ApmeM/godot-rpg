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
        public int MaxHp;
        public int Hp;

        public int MoveDistance;
        public int SightRange;

        public float RangedAttackDistance;
        public float AOEAttackRadius;
        public float AttackPower;
        public float MagicPower;
        public HashSet<Ability> Abilities = new HashSet<Ability>();
        public readonly List<EffectDuration> Effects = new List<EffectDuration>();
        public readonly HashSet<Skill> Skills = new HashSet<Skill>();
    }
}