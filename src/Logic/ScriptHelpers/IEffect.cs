﻿using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IEffect
    {
        void Apply(ServerUnit unit);
    }
}