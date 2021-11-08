using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HealAbility : IAbility
    {
        public AbilityType AbilityType => AbilityType.TargetUnit;

        public string Description => "Heal: \n Hp increase: 10.\n Cost: 2";

        public Ability Ability => Ability.Heal;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
            var myUnits = maze.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>();
            var targetCells = myUnits
                .Select(unit => unit.NewPosition == null ? maze.WorldToMap(unit.Position) : unit.NewPosition.Value)
                .ToList();

            BreadthFirstPathfinder.Search(maze.astarFly, newPos, (int)(currentUnit.RangedAttackDistance * 5), out var visited);

            maze.BeginHighliting(Maze.HighliteType.AttackDistance, (int)(currentUnit.AOEAttackRadius * 0));
            foreach (var cell in targetCells.Where(visited.ContainsKey))
            {
                maze.HighlitePoint(cell);
            }
            maze.EndHighliting();
        }
        
        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            if (actionUnit.Mp < 2)
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.AstarFly, actionUnit.Position, (int)(actionUnit.RangedAttackDistance * 5), out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.AstarFly, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius * 0), out visited);

            foreach (var targetPlayer in game.Players)
            {
                if (actionUnit.Player != targetPlayer.Value)
                {
                    continue;
                }

                foreach (var targetUnit in targetPlayer.Value.Units)
                {
                    if (!visited.ContainsKey(targetUnit.Value.Position))
                    {
                        continue;
                    }

                    result.Add(new ChangeHpAppliedAction(+(int)(actionUnit.MagicPower * 10), targetUnit.Value));
                    result.Add(new ApplyAbilityFromDirectionAction(actionUnit, this.Ability, targetUnit.Value));
                }
            }

            if (result.Count > 0)
            {
                result.Add(new ApplyAbilityToDirectionAction(actionUnit, this.Ability, abilityDirection));
                result.Add(new ChangeMpAppliedAction(-2, actionUnit));
            }


            return result;
        }
    }
}
