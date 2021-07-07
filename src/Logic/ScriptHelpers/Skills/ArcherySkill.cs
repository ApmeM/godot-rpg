﻿using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class ArcherySkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.RangedAttackDistance *= 1.5f;
        }
    }
}
