using FateRandom;
using Godot;
using IsometricGame.Business.Models;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Godot.Vector2;

namespace IsometricGame.Logic
{
    public class GameLogic
    {
        private static readonly TransferTurnDoneData emptyMoves = new TransferTurnDoneData();
        private readonly MapRepository mapRepository;
        private readonly PluginUtils pluginUtils;
        private readonly UnitUtils unitUtils;

        public GameLogic(MapRepository mapRepository, PluginUtils pluginUtils, UnitUtils unitUtils)
        {
            this.mapRepository = mapRepository;
            this.pluginUtils = pluginUtils;
            this.unitUtils = unitUtils;
        }

        public GameData StartForLobby(LobbyData lobby)
        {
            var game = new GameData(lobby.Id)
            {
                Configuration = lobby.ServerConfiguration
            };

            var generatorData = this.mapRepository.CreateForType(lobby.ServerConfiguration.MapType);
            game.StartingPoints = generatorData.StartingPoints;
            game.Map = generatorData.Map;
            game.AstarFly = generatorData.AstarFly;
            game.AstarMove = generatorData.AstarMove;

            List<Tuple<IBot, int>> bots = new List<Tuple<IBot, int>>();
            var botNumber = 1;
            for (var i = 0; i < lobby.Players.Count; i++)
            {
                var player = lobby.Players[i];
                var playerId = player.ClientId;
                if (playerId < 0)
                {
                    var bot = pluginUtils.FindBot((Bot)player.ClientId);
                    bots.Add(new Tuple<IBot, int>(bot, -botNumber));
                    playerId = -botNumber;
                    botNumber++;
                }

                game.Players.Add(playerId, new ServerPlayer
                {
                    PlayerId = playerId,
                    PlayerName = lobby.Players[i].PlayerName
                });
            }

            for (var i = 0; i < bots.Count; i++)
            {
                var bot = bots[i];
                bot.Item1.StartGame(game, bot.Item2);
            }

            return game;
        }

        public bool Connect(
            GameData game,
            int playerId,
            TransferConnectData connectData,
            Action<TransferInitialData> initialize,
            Action<TransferTurnData> turnDone)
        {
            if (!game.Players.ContainsKey(playerId))
            {
                return false;
            }

            var player = game.Players[playerId];
            if (player.IsConnected)
            {
                return true;
            }

            player.IsConnected = true;
            player.InitializeMethod = initialize;
            player.TurnDoneMethod = turnDone;

            var connectedPlayers = game.Players.Count(a => a.Value.IsConnected);
            var center = game.StartingPoints[connectedPlayers - 1];
            foreach (var u in connectData.Units.Take(game.Configuration.MaxUnits))
            {
                var unitId = player.Units.Count + 1;
                var unit = this.unitUtils.BuildUnit(player, u.UnitType, u.Skills.Take(game.Configuration.MaxSkills).ToList());
                unit.UnitId = unitId;
                unit.Player = player;
                player.Units.Add(unitId, unit);
            }

            var positions = new List<Vector2>
            {
                Vector2.Left,
                Vector2.Right,
                Vector2.Up,
                Vector2.Down,
                Vector2.Zero
            };

            var index = 0;
            foreach (var unit in player.Units)
            {
                unit.Value.Position = center + positions[index];
                index++;
            }

            if (connectedPlayers == game.Players.Count())
            {
                foreach (var p in game.Players)
                {
                    foreach (var u in p.Value.Units)
                    {
                        this.unitUtils.RefreshUnit(p.Value, u.Value);
                    }
                }

                foreach (var p in game.Players)
                {
                    p.Value.InitializeMethod(GetInitialData(game, p.Key));
                    PlayerMove(game, p.Key, emptyMoves);
                }
            }

            return true;
        }

        public void PlayerMove(GameData game, int forPlayer, TransferTurnDoneData moves)
        {
            if (game.PlayersGameOver.ContainsKey(forPlayer))
            {
                return;
            }

            /* Put player move into dictionary */
            if (!game.Players.ContainsKey(forPlayer))
            {
                return;
            }

            game.PlayersMove[forPlayer] = moves;

            if (game.PlayersMove.Count != game.Players.Count)
            {
                return;
            }

            /* When all players send their turns - apply all.*/
            game.Timeout = game.Configuration.TurnTimeout;

            /* Initialize turn delta. */
            var unitsTurnDelta = new Dictionary<long, ServerTurnDelta>();
            foreach (var actionPlayer in game.Players)
            {
                game.PlayersMove[actionPlayer.Key].UnitActions = game.PlayersMove[actionPlayer.Key].UnitActions ?? new Dictionary<int, List<TransferTurnDoneData.UnitActionData>>();
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
                    var fullId = UnitUtils.GetFullUnitId(actionUnit.Value);
                    unitsTurnDelta[fullId] = new ServerTurnDelta();
                    if (!game.PlayersMove[actionPlayer.Key].UnitActions.ContainsKey(actionUnit.Key))
                    {
                        game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key] = new List<TransferTurnDoneData.UnitActionData>();
                    }
                }
            }

            /* Execute abilities */
            var autoAbilities = game.Players.Select(a => new ServerAction { PlayerId = a.Key })
                .SelectMany(a => game.Players[a.PlayerId].Units.Select(b => new ServerAction(a) { UnitId = b.Key }))
                .SelectMany(a => game.Players[a.PlayerId].Units[a.UnitId].Abilities.Select(b => new ServerAction(a) { Ability = b, AbilityDirection = null, AbilityFullUnitId = null }))
                .Where(a => this.pluginUtils.FindAbility(a.Ability).AbilityType == AbilityType.Automatic)
                .Select(a => new ServerAction(a) { ExecuteOrder = 2 });

            var turnAbilities = game.PlayersMove.Select(a => new ServerAction { PlayerId = a.Key })
                .SelectMany(a => game.PlayersMove[a.PlayerId].UnitActions.Select(b => new ServerAction(a) { UnitId = b.Key }))
                .SelectMany(a => game.PlayersMove[a.PlayerId].UnitActions[a.UnitId].Select(b => new ServerAction(a) { Ability = b.Ability, AbilityDirection = b.AbilityDirection, AbilityFullUnitId = b.AbilityFullUnitId }))
                .Where(a => game.Players[a.PlayerId].Units[a.UnitId].Abilities.Contains(a.Ability));

            var moveAbilities = turnAbilities
                .Where(a =>
                {
                    var moveAbility = this.pluginUtils.FindAbility(a.Ability) as IMoveAbility;
                    return moveAbility != null && moveAbility.IsBasicMove;
                })
                .GroupBy(a => UnitUtils.GetFullUnitId(a.PlayerId, a.UnitId))
                .Select(a => new ServerAction(a.First()) { ExecuteOrder = 1 });

            var skillAbilities = turnAbilities
                .Where(a =>
                {
                    var moveAbility = this.pluginUtils.FindAbility(a.Ability) as IMoveAbility;
                    return moveAbility == null || !moveAbility.IsBasicMove;
                })
                .GroupBy( a => UnitUtils.GetFullUnitId(a.PlayerId, a.UnitId))
                .Select(a => new ServerAction(a.First()) { ExecuteOrder = 2 });

            var actionAbilities = autoAbilities.Union(moveAbilities).Union(skillAbilities).ToList();
            Fate.GlobalFate.Shuffle(actionAbilities);
            actionAbilities.Sort((a, b) => a.ExecuteOrder - b.ExecuteOrder);

            var appliedActions = new List<IAppliedAction>();
            foreach (var action in actionAbilities)
            {
                var actionUnit = game.Players[action.PlayerId].Units[action.UnitId];
                if (actionUnit.Hp <= 0)
                {
                    continue;
                }

                var ability = pluginUtils.FindAbility(action.Ability);
                Vector2 abilityDirection;
                switch (ability.AbilityType)
                {
                    case AbilityType.TargetUnit:
                        if (!action.AbilityFullUnitId.HasValue)
                        {
                            continue;
                        }

                        var targetPlayerId = UnitUtils.GetPlayerId(action.AbilityFullUnitId.Value);
                        var targetUnitId = UnitUtils.GetUnitId(action.AbilityFullUnitId.Value);
                        var targetPlayer = game.Players[targetPlayerId];
                        var targetUnit = targetPlayer.Units[targetUnitId];

                        abilityDirection = targetUnit.Position - actionUnit.Position;
                        break;
                    case AbilityType.AreaOfEffect:
                        if (!action.AbilityDirection.HasValue)
                        {
                            continue;
                        }

                        abilityDirection = action.AbilityDirection.Value;
                        break;
                    default:
                        abilityDirection = Vector2.Zero;
                        break;
                }
                appliedActions.AddRange(ability.Apply(actionUnit, game, abilityDirection));
            }

            /* Calculate all effects */
            foreach (var actionPlayer in game.Players)
            {
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
                    foreach (var effect in actionUnit.Value.Effects.Where(a => a.Duration > 0))
                    {
                        var actions = pluginUtils.FindEffect(effect.Effect).Apply(actionUnit.Value);
                        appliedActions.AddRange(actions);
                        effect.Duration--;
                    }

                    actionUnit.Value.Effects.RemoveAll(a => a.Duration <= 0);
                }
            }

            /* Refresh unit values. */
            foreach (var actionPlayer in game.Players)
            {
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
                    this.unitUtils.RefreshUnit(actionPlayer.Value, actionUnit.Value);
                }
            }

            foreach (var action in appliedActions)
            {
                action.Apply(unitsTurnDelta);
            }
            appliedActions.Clear();

            /* Check survived units */
            foreach (var actionPlayer in game.Players)
            {
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
                    actionUnit.Value.Hp = Mathf.Clamp(actionUnit.Value.Hp, 0, actionUnit.Value.MaxHp);
                    actionUnit.Value.Mp = Mathf.Clamp(actionUnit.Value.Mp, 0, actionUnit.Value.MaxMp);
                    if (actionUnit.Value.Hp == 0)
                    {
                        var fullId = UnitUtils.GetFullUnitId(actionUnit.Value);
                        unitsTurnDelta[fullId].AppliedAbilities.Clear();
                    }
                }

                actionPlayer.Value.IsGameOver = actionPlayer.Value.IsGameOver || CheckGameOver(actionPlayer.Value);
            }

            /* Turn calculation done */
            game.PlayersMove.Clear();

            foreach (var p in game.Players)
            {
                p.Value.TurnDoneMethod(GetTurnData(game, p.Key, unitsTurnDelta));
            }

            foreach (var actionPlayer in game.Players.Where(a => a.Value.IsGameOver).ToList())
            {
                game.Players.Remove(actionPlayer.Key);
                game.PlayersGameOver[actionPlayer.Key] = actionPlayer.Value;
            }
        }

        public void PlayerExitGame(GameData gameData, int clientId)
        {
            foreach (var unit in gameData.Players[clientId].Units)
            {
                unit.Value.Hp = 0;
            }

            PlayerMove(gameData, clientId, emptyMoves);
        }

        private TransferInitialData GetInitialData(GameData game, int forPlayer)
        {
            return new TransferInitialData
            {
                Timeout = game.Configuration.TurnTimeout,
                YourPlayerId = forPlayer,
                YourUnits = game.Players[forPlayer].Units.Select(a => new TransferInitialData.YourUnitsData
                {
                    UnitId = a.Key,
                    Position = a.Value.Position,
                    MoveDistance = a.Value.MoveDistance,
                    SightRange = a.Value.SightRange,
                    RangedAttackDistance = a.Value.RangedAttackDistance,
                    AOEAttackRadius = a.Value.AOEAttackRadius,
                    AttackPower = a.Value.AttackPower,
                    MagicPower = a.Value.MagicPower,
                    MaxHp = a.Value.MaxHp,
                    MaxMp = a.Value.MaxMp,
                    UnitType = a.Value.UnitType,
                    Abilities = a.Value.Abilities.Where(b=>this.pluginUtils.FindAbility(b).AbilityType != AbilityType.Automatic).ToList(),
                    Skills = a.Value.Skills.ToList()
                }).ToList(),
                VisibleMap = GetVisibleMap(game, forPlayer, true),
                OtherPlayers = game.Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialData.OtherPlayerData
                {
                    PlayerId = a.Key,
                    PlayerName = a.Value.PlayerName,
                    Units = a.Value.Units.Select(b => new TransferInitialData.OtherUnitsData
                    {
                        Id = b.Key,
                        UnitType = b.Value.UnitType,
                        MaxHp = b.Value.MaxHp,
                    }).ToList(),
                }).ToList()
            };
        }

        private TransferTurnData GetTurnData(GameData game, int forPlayer, Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var player = game.Players[forPlayer];
            return new TransferTurnData
            {
                GameOverLoose = player.IsGameOver,
                GameOverWin = game.Players.Where(a => a.Key != forPlayer).All(a => a.Value.IsGameOver),
                YourUnits = player.Units.ToDictionary(a => a.Key, a =>
                {
                    var fullId = UnitUtils.GetFullUnitId(a.Value);
                    var delta = unitsTurnDelta[fullId];

                    return new TransferTurnData.YourUnitsData
                    {
                        Position = a.Value.Position,
                        ExecutedAbilities = delta.ExecutedAbilities,
                        Hp = a.Value.Hp,
                        Mp = a.Value.Mp,
                        AppliedAbilities = delta.AppliedAbilities,
                        Effects = a.Value.Effects,
                        MoveDistance = a.Value.MoveDistance,
                        SightRange = a.Value.SightRange,
                        RangedAttackDistance = a.Value.RangedAttackDistance,
                        AOEAttackRadius = a.Value.AOEAttackRadius,
                        AttackPower = a.Value.AttackPower,
                        MagicPower = a.Value.MagicPower,
                        HpChanges = delta?.HpChanges,
                        MpChanges = delta?.MpChanges,
                    };
                }),
                VisibleMap = this.GetVisibleMap(game, forPlayer, false),
                OtherPlayers = game.Players.Concat(game.PlayersGameOver).Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
                {
                    Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
                    {
                        var fullId = UnitUtils.GetFullUnitId(b.Value);
                        var delta = unitsTurnDelta[fullId];

                        return new TransferTurnData.OtherUnitsData
                        {
                            Position = b.Value.Position,
                            ExecutedAbilities = delta.ExecutedAbilities,
                            Hp = b.Value.Hp,
                            AppliedAbilities = delta.AppliedAbilities,
                            Effects = b.Value.Effects,
                            HpChanges = delta?.HpChanges,
                        };
                    })
                })
            };
        }

        private bool CheckGameOver(ServerPlayer player)
        {
            return player.Units.All(unit => unit.Value.Hp <= 0);
        }

        private MapTile[,] GetVisibleMap(GameData game, int forPlayer, bool isInitialize)
        {
            var player = game.Players[forPlayer];
            
            var result = (MapTile[,])game.Map.Clone();
            for (var x = 0; x < result.GetLength(0); x++)
                for (var y = 0; y < result.GetLength(1); y++)
                {
                    if (!IsVisible(player, x, y) && (!game.Configuration.FullMapVisible || !isInitialize))
                    {
                        result[x, y] = MapTile.Unknown;
                    }
                }

            return result;
        }

        private static bool IsVisible(ServerPlayer player, int x, int y)
        {
            foreach (var unit in player.Units.Values)
            {
                if (unit.Hp <= 0)
                {
                    continue;
                }

                if ((Math.Abs(x - unit.Position.x) + Math.Abs(y - unit.Position.y)) <= unit.SightRange)
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckTimeout(GameData game, float delta)
        {
            if (!game.Configuration.TurnTimeout.HasValue)
            {
                return;
            }

            game.Timeout = game.Timeout ?? game.Configuration.TurnTimeout;
            game.Timeout -= delta;
            if (game.Timeout > 0)
            {
                return;
            }

            game.Timeout = game.Configuration.TurnTimeout;
            var forcePlayerMove = game.Players.Keys.Where(player => !game.PlayersMove.ContainsKey(player)).ToList();
            foreach (var player in forcePlayerMove)
            {
                this.PlayerMove(game, player, emptyMoves);
            }
        }
    }
}
