using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public class UnitUtils
    {
        private readonly PluginUtils pluginUtils;

        public UnitUtils(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public ServerUnit BuildUnit(ServerPlayer player, UnitType unitType, List<Skill> skills)
        {
            var existingUnit = new ServerUnit
            {
                UnitType = unitType,
            };

            this.pluginUtils.FindUnitType(existingUnit.UnitType).Initialize(existingUnit);

            foreach (var skill in skills)
            {
                existingUnit.Skills.Add(skill);
            }

            RefreshUnit(player, existingUnit);
            
            existingUnit.Hp = existingUnit.MaxHp;
            existingUnit.Mp = existingUnit.MaxMp;

            return existingUnit;
        }

        public void RefreshUnit(ServerPlayer player, ServerUnit existingUnit)
        {
            existingUnit.MaxHp = 10;
            existingUnit.MaxMp = 10;
            existingUnit.MoveDistance = 5;
            existingUnit.SightRange = 6;
            existingUnit.RangedAttackDistance = 1;
            existingUnit.AOEAttackRadius = 1;
            existingUnit.AttackPower = 1;
            existingUnit.MagicPower = 1;
            existingUnit.Abilities.Clear();

            this.pluginUtils.FindUnitType(existingUnit.UnitType).Apply(existingUnit);
            foreach (var skill in existingUnit.Skills)
            {
                this.pluginUtils.FindSkill(skill).Apply(player, existingUnit);
            }
        }

        public static long GetFullUnitId(ServerUnit unit)
        {
            return GetFullUnitId(unit.Player.PlayerId, unit.UnitId);
        }

        public static long GetFullUnitId(int playerId, int unitId)
        {
            return ((long)playerId << 32) | ((long)unitId & 0xFFFFFFFFL);
        }

        public static int GetPlayerId(long abilityFullUnitId)
        {
            return (int)(abilityFullUnitId >> 32);
        }
        public static int GetUnitId(long abilityFullUnitId)
        {
            return (int)abilityFullUnitId;
        }
    }
}
