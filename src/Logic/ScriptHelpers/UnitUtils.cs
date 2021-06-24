using FateRandom;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.Skills;
using System.Collections.Generic;
using System.Linq;

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
                    AttackDamage = 3,
                    Usables = new HashSet<Usable>{ Usable.AmazonAOEAttack }
                }
            },
            {
                UnitType.Amazon, new ServerUnit
                {
                    SightRange = 7,
                    AttackDistance = 3,
                    Usables = new HashSet<Usable>{ Usable.AmazonAOEAttack }
                }
            },
        };

        private static List<UnitType> UnitTypeTemplateList = new List<UnitType>();

        static UnitUtils()
        {
            UnitTypeTemplateList.AddRange(UnitTypeTemplates.Keys);
        }

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
                AttackDistance = template.AttackDistance,
                AttackRadius = template.AttackRadius,
                AttackDamage = template.AttackDamage,
                Usables = new HashSet<Usable>(template.Usables)
            };
            return unit;
        }

        public static ServerUnit GetRandomUnit()
        {
            return BuildUnit(Fate.GlobalFate.Choose<UnitType>(UnitTypeTemplateList));
        }

        private static Dictionary<Skill, ISkill> SupportedSkills = new Dictionary<Skill, ISkill>
        {
            { Skill.EagleEye, new EagleEyeSkill() },
            { Skill.Logistics, new LogisticsSkill() },
            { Skill.Ballistics, new BallisticsSkill() },
            { Skill.FirstAid, new FirstAidSkill() },
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

        private static Dictionary<Usable, IUsable> SupportedUsables = new Dictionary<Usable, IUsable>
        {
            { Usable.AmazonAOEAttack, new AmazonAOEAttackUsable() },
            { Usable.Heal, new HealUsable() },
        };

        public static IUsable FindUsable(Usable usable)
        {
            return SupportedUsables[usable];
        }
    }
}
