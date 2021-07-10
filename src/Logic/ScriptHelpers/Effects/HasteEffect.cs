﻿using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class HasteEffect : IEffect
    {
        public void Apply(ServerUnit unit)
        {
            unit.MoveDistance = (int)(unit.MoveDistance * 1.5f);
        }
    }
}
