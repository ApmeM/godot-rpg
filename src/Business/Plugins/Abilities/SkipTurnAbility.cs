using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class SkipTurnAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => "Skip turn: \n Increse Hp and Mp by 10%. Used automatically if unit not moved and no other abilities selected this turn.";

        public Ability Ability => Ability.SkipTurn;

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 0, 0);
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            if (abilityDirection != Vector2.Zero)
            {
                GD.Print("HERE1", abilityDirection);
                return result;
            }

            var unitMove = game.PlayersMove[actionUnit.Player.PlayerId].UnitActions[actionUnit.UnitId];

            if (unitMove.Count > 1)
            {
                GD.Print("HERE2");
                return result;
            }

            result.Add(new ChangeHpAppliedAction(actionUnit.MaxHp / 10, actionUnit));
            result.Add(new ChangeMpAppliedAction(actionUnit.MaxMp / 10, actionUnit));

            return result;
        }
    }
}
