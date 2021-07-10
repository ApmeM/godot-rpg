using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.Abilities;
using IsometricGame.Logic.ScriptHelpers.Effects;
using IsometricGame.Logic.ScriptHelpers.Skills;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers
{
    public static partial class UnitUtils
    {
        private static Dictionary<UnitType, IUnitType> SupportedUnitTypes = new Dictionary<UnitType, IUnitType>
        {
            { UnitType.Amazon, new AmazonUnitType() },
            { UnitType.Goatman, new GoatmanUnitType() },
        };

        private static Dictionary<Skill, ISkill> SupportedSkills = new Dictionary<Skill, ISkill>
        {
            { Skill.Archery,      new ArcherySkill() },
            { Skill.Ballistics,   new BallisticsSkill() },
            { Skill.EagleEye,     new EagleEyeSkill() },
            { Skill.Logistics,    new LogisticsSkill() },
            { Skill.Wisdom,       new WisdomSkill() },
            { Skill.FireMagic,    new FireMagicSkill() },
            { Skill.AirMagic,     new AirMagicSkill() },
            { Skill.Offence,      new OffenceSkill() },
            { Skill.Armourer,     new ArmourerSkill() },
            { Skill.FirstAid,     new FirstAidSkill() },
        };

        private static Dictionary<Ability, IAbility> SupportedAbilities = new Dictionary<Ability, IAbility>
        {
            { Ability.RangedAttack, new RangedAttackAbility() },
            { Ability.MeleeAttack, new MeleeAttackAbility() },
            { Ability.Heal, new HealAbility() },
            { Ability.Fireball, new FireballAbility() },
            { Ability.Haste, new HasteAbility() },
        };

        private static Dictionary<Effect, IEffect> SupportedEffects = new Dictionary<Effect, IEffect>
        {
            { Effect.Burn, new BurnEffect() },
            { Effect.Haste, new HasteEffect() },
        };

        public static IAbility FindAbility(Ability ability)
        {
            return SupportedAbilities[ability];
        }

        public static void RefreshUnit(ServerPlayer player, ServerUnit existingUnit)
        {
            existingUnit.MaxHp = 10;
            existingUnit.MoveDistance = 5;
            existingUnit.SightRange = 6;
            existingUnit.RangedAttackDistance = 1;
            existingUnit.AOEAttackRadius = 1;
            existingUnit.AttackPower = 1;
            existingUnit.MagicPower = 1;
            existingUnit.Abilities.Clear();

            SupportedUnitTypes[existingUnit.UnitType].Apply(existingUnit);
            foreach (var skill in existingUnit.Skills)
            {
                SupportedSkills[skill].Apply(player, existingUnit);
            }

            foreach (var effect in existingUnit.Effects)
            {
                SupportedEffects[effect.Effect].Apply(existingUnit);
                effect.Duration--;
            }

            existingUnit.Effects.RemoveAll(a => a.Duration <= 0);
        }

        public static ServerUnit BuildUnit(ServerPlayer player, UnitType unitType, List<Skill> skills)
        {
            var existingUnit = new ServerUnit
            {
                UnitType = unitType,
                Hp = 10,
            };

            SupportedUnitTypes[existingUnit.UnitType].Initialize(existingUnit);

            foreach (var skill in skills)
            {
                existingUnit.Skills.Add(skill);
            }

            RefreshUnit(player, existingUnit);
            return existingUnit;
        }
    }
}
