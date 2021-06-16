using FateRandom;
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
                    AttackDamage = 3
                }
            },
            {
                UnitType.Amazon, new ServerUnit
                {
                    VisionRange = 7,
                    AttackDistance = 3
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
            var unit = new ServerUnit();
            var template = UnitTypeTemplates[unitType];
            unit.UnitType = unitType;
            unit.Position = template.Position;
            unit.VisionRange = template.VisionRange;
            unit.MoveDistance = template.MoveDistance;
            unit.MaxHp = template.MaxHp;
            unit.Hp = template.Hp;
            unit.AttackDistance = template.AttackDistance;
            unit.AttackRadius = template.AttackRadius;
            unit.AttackDamage = template.AttackDamage;
            unit.AttackFriendlyFire = template.AttackFriendlyFire;
            return unit;
        }

        public static ServerUnit GetRandomUnit()
        {
            return BuildUnit(Fate.GlobalFate.Choose<UnitType>(UnitTypeTemplateList));
        }

        private static Dictionary<Skill, ISkill> SupportedSkills = new Dictionary<Skill, ISkill>
        {
            { Skill.VisionRange, new VisionRangeSkill() },
            { Skill.MoveRange, new MoveRangeSkill() },
        };

        public static void ApplySkill(ServerPlayer player, ServerUnit unit, Skill skill)
        {
            SupportedSkills[skill].Apply(player, unit);
        }
    }
}
