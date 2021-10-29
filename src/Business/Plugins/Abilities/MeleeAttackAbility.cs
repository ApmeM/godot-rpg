using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MeleeAttackAbility : IAbility
    {
        public AbilityType AbilityType => AbilityType.AreaOfEffect;

        public string Description => "Melee attack: \n Damage: 10.";

        public Ability Ability => Ability.MeleeAttack;

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 1, (int)currentUnit.AOEAttackRadius);
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();

            BreadthFirstPathfinder.Search(game.Astar, actionUnit.Position, 1, out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.Astar, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius), out visited);

            foreach (var targetPlayer in game.Players)
            {
                if (actionUnit.Player == targetPlayer.Value)
                {
                    continue;
                }

                foreach (var targetUnit in targetPlayer.Value.Units)
                {
                    if (!visited.ContainsKey(targetUnit.Value.Position))
                    {
                        continue;
                    }

                    result.Add(new ChangeHpAppliedAction(-(int)(actionUnit.AttackPower * 10), targetUnit.Value));
                    result.Add(new ApplyAbilityFromDirectionAction(actionUnit, targetUnit.Value));
                }
            }

            result.Add(new ApplyAbilityToDirectionAction(actionUnit, abilityDirection));

            return result;
        }
    }
}
