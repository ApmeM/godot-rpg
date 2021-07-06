using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.Skills;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public static partial class UnitUtils
    {
        private static Dictionary<UnitType, ServerUnit> UnitTypeTemplates = new Dictionary<UnitType, ServerUnit>
        {
            {
                UnitType.Goatman, new ServerUnit
                {
                    MaxHp = 20,
                    Hp = 20,
                    Abilities = new HashSet<Ability>{ Ability.MeleeAttack }
                }
            },
            {
                UnitType.Amazon, new ServerUnit
                {
                    SightRange = 7,
                    Abilities = new HashSet<Ability>{ Ability.RangedAttack }
                }
            },
        };

        public static ServerUnit BuildUnit(UnitType unitType)
        {
            var template = UnitTypeTemplates[unitType];
            var unit = new ServerUnit
            {
                UnitType = unitType,
                Position = template.Position,
                SightRange = template.SightRange,
                MoveDistance = template.MoveDistance,
                MaxHp = template.MaxHp,
                Hp = template.Hp,
                RangedAttackDistance = template.RangedAttackDistance,
                AOEAttackRadius = template.AOEAttackRadius,
                AttackPower = template.AttackPower,
                MagicPower = template.MagicPower,
                Abilities = new HashSet<Ability>(template.Abilities)
            };
            return unit;
        }

        private static Dictionary<Skill, ISkill> SupportedSkills = new Dictionary<Skill, ISkill>
        {
            { Skill.Archery,      new ArcherySkill() },
            { Skill.Ballistics,   new BallisticsSkill() },
            //{ Skill.Diplomacy,    new DiplomacySkill() },
            { Skill.EagleEye,     new EagleEyeSkill() },
            //{ Skill.Estates,      new EstatesSkill() },
            //{ Skill.Leadership,   new LeadershipSkill() },
            { Skill.Logistics,    new LogisticsSkill() },
            //{ Skill.Luck,         new LuckSkill() },
            //{ Skill.Mysticism,    new MysticismSkill() },
            //{ Skill.Navigation,   new NavigationSkill() },
            //{ Skill.Necromancy,   new NecromancySkill() },
            //{ Skill.Pathfinding,  new PathfindingSkill() },
            //{ Skill.Scouting,     new ScoutingSkill() },
            //{ Skill.Wisdom,       new WisdomSkill() },
            //{ Skill.FireMagic,    new FireMagicSkill() },
            //{ Skill.AirMagic,     new AirMagicSkill() },
            //{ Skill.WaterMagic,   new WaterMagicSkill() },
            //{ Skill.EarthMagic,   new EarthMagicSkill() },
            //{ Skill.Scholar,      new ScholarSkill() },
            //{ Skill.Tactics,      new TacticsSkill() },
            //{ Skill.Artillery,    new ArtillerySkill() },
            //{ Skill.Learning,     new LearningSkill() },
            { Skill.Offence,      new OffenceSkill() },
            { Skill.Armourer,     new ArmourerSkill() },
            //{ Skill.Intelligence, new IntelligenceSkill() },
            //{ Skill.Sorcery,      new SorcerySkill() },
            //{ Skill.Resistance,   new ResistanceSkill() },
            { Skill.FirstAid,     new FirstAidSkill() },
        };

        public static void ApplySkill(ServerPlayer player, ServerUnit unit, Skill skill)
        {
            unit.Skills.Add(skill);
            if (!SupportedSkills. ContainsKey(skill))
            {
                return;
            }
            SupportedSkills[skill].Apply(player, unit);
        }

        private static Dictionary<Ability, IAbility> SupportedAbilities = new Dictionary<Ability, IAbility>
        {
            { Ability.RangedAttack, new RangedAttackAbility() },
            { Ability.MeleeAttack, new MeleeAttackAbility() },
            { Ability.Heal, new HealAbility() },
        };

        public static IAbility FindAbility(Ability ability)
        {
            return SupportedAbilities[ability];
        }
    }
}
