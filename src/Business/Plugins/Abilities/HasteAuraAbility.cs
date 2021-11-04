using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HasteAuraAbility : IAbility
    {
        private readonly PluginUtils pluginUtils;

        public HasteAuraAbility(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public AbilityType AbilityType => AbilityType.Automatic;

        public string Description => $"Haste Aura (Passive): \n Apply effect: {this.pluginUtils.FindEffect(Effect.Haste).Description}\n  for all near units. Distance: 1";

        public Ability Ability => Ability.HasteAura;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();

            BreadthFirstPathfinder.Search(game.AstarFly, actionUnit.Position, 1, out var visited);

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

                    result.Add(new ApplyEffectAppliedAction(Effect.Haste, 0, targetUnit.Value));
                    result.Add(new ChangeMoveDistanceAppliedAction((int)(targetUnit.Value.MoveDistance * 0.5f), targetUnit.Value));
                }
            }

            return result;
        }
    }
}
