﻿using IsometricGame.Logic.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers
{
    public class PluginUtils
    {
        public void Initialize(
            List<IUnitType> unitTypes, 
            List<IEffect> effects, 
            List<IAbility> abilities, 
            List<ISkill> skills,
            List<IBot> bots)
        {
            this.SupportedUnitTypes = unitTypes.ToDictionary(a => a.UnitType);
            this.SupportedEffects = effects.ToDictionary(a => a.Effect);
            this.SupportedAbilities = abilities.ToDictionary(a => a.Ability);
            this.SupportedSkills = skills.ToDictionary(a => a.Skill);
            this.SupportedBots = bots.ToDictionary(a => a.Bot);
        }

        private Dictionary<UnitType, IUnitType> SupportedUnitTypes;

        private Dictionary<Skill, ISkill> SupportedSkills;

        private Dictionary<Ability, IAbility> SupportedAbilities;

        private Dictionary<Effect, IEffect> SupportedEffects;

        private Dictionary<Bot, IBot> SupportedBots;

        public ISkill FindSkill(Skill skill)
        {
            if (!SupportedSkills.ContainsKey(skill))
            {
                return null;
            }

            return SupportedSkills[skill];
        }

        public IAbility FindAbility(Ability ability)
        {
            if (!SupportedAbilities.ContainsKey(ability))
            {
                return null;
            }

            return SupportedAbilities[ability];
        }

        public IEffect FindEffect(Effect effect)
        {
            if (!SupportedEffects.ContainsKey(effect))
            {
                return null;
            }

            return SupportedEffects[effect];
        }

        public IUnitType FindUnitType(UnitType unitType)
        {
            if (!SupportedUnitTypes.ContainsKey(unitType))
            {
                return null;
            }

            return SupportedUnitTypes[unitType];
        }

        public IBot FindBot(Bot botName)
        {
            if (!SupportedBots.ContainsKey(botName))
            {
                return null;
            }

            return SupportedBots[botName];
        }

        public bool IsMoveAbility(Ability ability)
        {
            return ability == Ability.Fly || ability == Ability.Move;
        }
    }
}
