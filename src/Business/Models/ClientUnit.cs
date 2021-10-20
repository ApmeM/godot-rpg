using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ClientUnit
    {
        public int UnitId;
        public int PlayerId;
        public int? MoveDistance;
        public int? SightRange;
        public float? RangedAttackDistance;
        public float? AOEAttackRadius;
        public float? AttackPower;
        public float? MagicPower;
        public UnitType UnitType;
        public int MaxHp;
        public int Hp;
        public int MaxMp;
        public int Mp;
        public Dictionary<Ability, IAbility> Abilities;
        public HashSet<Skill> Skills;
        public List<EffectDuration> Effects;
    }
}