using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers.Skills;
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
            return SupportedSkills[skill];
        }

        public IAbility FindAbility(Ability ability)
        {
            return SupportedAbilities[ability];
        }

        public IEffect FindEffect(Effect effect)
        {
            return SupportedEffects[effect];
        }

        public IUnitType FindUnitType(UnitType unitType)
        {
            return SupportedUnitTypes[unitType];
        }

        public IBot FindBot(Bot botName)
        {
            return SupportedBots[botName];
        }
    }
}
