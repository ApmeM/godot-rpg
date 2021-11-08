using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class FireballAbility : IAbility
    {
        private readonly PluginUtils pluginUtils;

        public FireballAbility(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public AbilityType AbilityType => AbilityType.AreaOfEffect;

        public string Description => $"Fireball: \n Direct Damage: 2. \n Apply effect: { this.pluginUtils.FindEffect(Effect.Burn).Description } \n  Duration: 5\n Cost: 5";

        public Ability Ability => Ability.Fireball;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
            maze.BeginHighliting(Maze.HighliteType.AttackDistance, (int)(currentUnit.AOEAttackRadius * 2));
            maze.HighliteRadius(newPos, (int)(currentUnit.RangedAttackDistance * 5));
            maze.EndHighliting();
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();

            if (actionUnit.Mp < 5)
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.AstarFly, actionUnit.Position, (int)(actionUnit.RangedAttackDistance * 5), out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.AstarFly, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius * 2), out visited);

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

                    result.Add(new ChangeHpAppliedAction(-(int)(actionUnit.MagicPower * 2), targetUnit.Value));
                    result.Add(new ApplyEffectAppliedAction(Effect.Burn, 5, targetUnit.Value));
                    result.Add(new ApplyAbilityFromDirectionAction(actionUnit, this.Ability, targetUnit.Value));
                }
            }

            result.Add(new ApplyAbilityToDirectionAction(actionUnit, this.Ability, abilityDirection));
            result.Add(new ChangeMpAppliedAction(-5, actionUnit));
     
            return result;
        }
    }
}
