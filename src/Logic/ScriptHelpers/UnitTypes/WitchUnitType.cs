using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WitchUnitType : IUnitType
    {
        public void Apply(ServerUnit unit)
        {
            unit.MoveDistance *= 2;
        }

        public void Initialize(ServerUnit unit)
        {
        }
    }
}
