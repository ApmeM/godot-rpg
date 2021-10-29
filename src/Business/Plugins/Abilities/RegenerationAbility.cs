using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class RegenerationAbility : IAbility
    {
        public AbilityType AbilityType => AbilityType.Automatic;

        public string Description => "Regeneration (Autoability): \n Increse Hp and Mp by 10% if unit not moved and no abilities selected this turn.";

        public Ability Ability => Ability.Regeneration;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            
            var unitMove = game.PlayersMove[actionUnit.Player.PlayerId].UnitActions[actionUnit.UnitId];

            if (unitMove.Count > 1)
            {
                return result;
            }

            result.Add(new ChangeHpAppliedAction(actionUnit.MaxHp / 10, actionUnit));
            result.Add(new ChangeMpAppliedAction(actionUnit.MaxMp / 10, actionUnit));

            return result;
        }
    }
}
