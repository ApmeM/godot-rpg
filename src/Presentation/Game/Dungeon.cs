using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[SceneReference("Dungeon.tscn")]
public partial class Dungeon : Node2D
{
    private CurrentAction currentAction = CurrentAction.None;
    private Ability? currentAbility = null;
    private float? Timeout;
    private float? MaxTimeout;

    [Export]
    public PackedScene UnitScene;

    private Communicator communicator;

    private TeamsRepository teamsRepository;
    private PluginUtils pluginUtils;

    public Dungeon()
    {
        this.teamsRepository = DependencyInjector.teamsRepository;
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        
        this.unitActions.Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
        this.nextTurnButton.Connect("pressed", this, nameof(NextTurnPressed));
        communicator = GetNode<Communicator>("/root/Communicator");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (Timeout.HasValue)
        {
            Timeout -= delta;
            nextTurnButton.Text = $"GO ({(int)Timeout})";
        }
    }

    public void NewGame(int selectedTeam)
    {
        var data = this.teamsRepository.LoadTeams()[selectedTeam];
        communicator.ConnectToServer(data);
    }

    public void Initialize(TransferInitialData initialData)
    {
        this.Timeout = initialData.Timeout;
        this.MaxTimeout = initialData.Timeout;

        nextTurnButton.Text = "GO";

        this.maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
        this.maze.NewVisibleMap(initialData.VisibleMap);
        this.maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

        foreach (var unit in initialData.YourUnits)
        {
            var unitSceneInstance = (Unit)UnitScene.Instance();

            unitSceneInstance.ClientUnit = new ClientUnit
            {
                PlayerId = initialData.YourPlayerId,
                UnitId = unit.UnitId,
                UnitType = unit.UnitType,
                MoveDistance = unit.MoveDistance,
                SightRange = unit.SightRange,
                RangedAttackDistance = unit.RangedAttackDistance,
                AOEAttackRadius = unit.AOEAttackRadius,
                MaxHp = unit.MaxHp,
                Hp = unit.MaxHp,
                MaxMp = unit.MaxMp,
                Mp = unit.MaxMp,
                Abilities = unit.Abilities.ToDictionary(a => a, a => this.pluginUtils.FindAbility(a)),
                Skills = unit.Skills.ToHashSet()
            };
            unitSceneInstance.Position = this.maze.GetSpritePositionForCell(unit.Position);
            unitSceneInstance.AddToGroup(Groups.MyUnits);
            this.maze.AddChild(unitSceneInstance);
        }

        foreach (var player in initialData.OtherPlayers)
        {
            foreach (var unit in player.Units)
            {
                var unitSceneInstance = (Unit)UnitScene.Instance();
                unitSceneInstance.ClientUnit = new ClientUnit
                {
                    PlayerId = player.PlayerId,
                    UnitId = unit.Id,
                    UnitType = unit.UnitType,
                    MaxHp = unit.MaxHp,
                    Hp = unit.MaxHp
                };
                unitSceneInstance.AddToGroup(Groups.OtherUnits);
                unitSceneInstance.Visible = false;
                this.maze.AddChild(unitSceneInstance);
            }
        }

        this.draggableCamera.Position = this.maze.GetSpritePositionForCell(initialData.YourUnits[0].Position);
    }

    private void UnitActionSelected(CurrentAction action, Ability ability)
    {
        this.currentAction = action;
        this.currentAbility = null;
        this.unitActions.Visible = false;

        var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
        var currentUnit = units.FirstOrDefault(a => a.IsSelected);

        switch (this.currentAction)
        {
            case CurrentAction.UseAbility:
                {
                    this.currentAbility = ability;
                    var oldPosition = this.maze.WorldToMap(currentUnit.Position);
                    var newPosition = currentUnit.NewPosition == null ? oldPosition : currentUnit.NewPosition.Value;
                    this.pluginUtils.FindAbility(ability).HighliteMaze(this.maze, oldPosition, newPosition, currentUnit.ClientUnit);
                    break;
                }
        }
    }

    public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
    {
        var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
        var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();

        var currentUnit = myUnits.FirstOrDefault(a => a.IsSelected);
     
        switch (this.currentAction)
        {
            case CurrentAction.None:
                {
                    var clickOnUnit = myUnits.FirstOrDefault(a => this.maze.WorldToMap(a.Position) == cell || a.NewPosition == cell);
                    if (currentUnit != null)
                    {
                        currentUnit.IsSelected = false;
                    }

                    this.unitActions.Visible = clickOnUnit != null && !clickOnUnit.IsDead;
                    this.unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
                    if (this.unitActions.Visible)
                    {
                        this.unitActions.Abilities = clickOnUnit.ClientUnit.Abilities.Select(a => a.Key).ToList();
                    }
                    this.unitDetailsCollapse.SelectUnit(clickOnUnit?.ClientUnit);

                    if (clickOnUnit != null)
                    {
                        clickOnUnit.IsSelected = true;
                    }
                    else
                    {
                        var enemyUnit = otherUnits.FirstOrDefault(a => this.maze.WorldToMap(a.Position) == cell);
                        if (enemyUnit != null)
                        {
                            this.unitDetailsCollapse.SelectUnit(enemyUnit.ClientUnit);
                        }
                    }
                    break;
                }
            case CurrentAction.UseAbility:
                {
                    if (moveAvailable)
                    {
                        var ability = this.pluginUtils.FindAbility(currentAbility.Value);
                        var moveAbility = ability as IMoveAbility;
                        this.currentAction = CurrentAction.None;

                        if (moveAbility != null && moveAbility.IsBasicMove)
                        {
                            currentUnit.MoveShadowTo(cell, moveAbility);
                        }
                        else
                        {
                            Unit target = null;
                            if (ability.AbilityType == AbilityType.TargetUnit)
                            {
                                target = myUnits
                                    .Union(otherUnits)
                                    .Where(unit => (unit.NewPosition == null ? this.maze.WorldToMap(unit.Position) : unit.NewPosition.Value) == cell)
                                    .FirstOrDefault();
                            }
                            currentUnit.AbilityShadowTo(currentAbility.Value, cell, target);
                        }   
                        
                        this.unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
                        this.unitActions.Visible = true;
                        this.maze.RemoveHighliting();
                    }
                    break;
                }
        }
    }

    public void NextTurnPressed()
    {
        var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
        foreach (var unit in myUnits)
        {
            unit.IsSelected = false;
        }

        this.unitActions.Visible = false;
        currentAction = CurrentAction.None;
        this.maze.RemoveHighliting();
        this.nextTurnButton.Visible = false;

        var data = new TransferTurnDoneData
        {
            UnitActions = myUnits.ToDictionary(a => a.ClientUnit.UnitId, a =>
            {
                var lists = new List<TransferTurnDoneData.UnitActionData>();

                if (a.NewPosition.HasValue)
                {
                    lists.Add(new TransferTurnDoneData.UnitActionData
                    {
                        Ability = a.NewPositionAbility.Value,
                        AbilityDirection = a.NewPosition.Value - this.maze.WorldToMap(a.Position)
                    });
                }

                if (a.Ability.HasValue)
                {
                    lists.Add(new TransferTurnDoneData.UnitActionData
                    {
                        Ability = a.Ability.Value,
                        AbilityDirection = a.AbilityDirection,
                        AbilityFullUnitId = a.AbilityUnitTarget == null ? -1 : UnitUtils.GetFullUnitId(a.AbilityUnitTarget.ClientUnit.PlayerId, a.AbilityUnitTarget.ClientUnit.UnitId)
                    });
                }

                return lists;
            })
        };

        communicator.PlayerMoved(data);
    }

    public async Task TurnDone(TransferTurnData turnData)
    {
        this.Timeout = this.MaxTimeout;
        this.maze.NewVisibleMap(turnData.VisibleMap);
        var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
        var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();
        var unitsToHide = otherUnits.Where(a => !turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));
        var visibleUnits = otherUnits.Where(a => turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));

        foreach (var unit in myUnits)
        {
            unit.ClientUnit.Mp = turnData.YourUnits[unit.ClientUnit.UnitId].Mp;
            unit.ClientUnit.Effects = turnData.YourUnits[unit.ClientUnit.UnitId].Effects;
            unit.ClientUnit.MoveDistance = turnData.YourUnits[unit.ClientUnit.UnitId].MoveDistance;
            unit.ClientUnit.SightRange = turnData.YourUnits[unit.ClientUnit.UnitId].SightRange;
            unit.ClientUnit.RangedAttackDistance = turnData.YourUnits[unit.ClientUnit.UnitId].RangedAttackDistance;
            unit.ClientUnit.AOEAttackRadius = turnData.YourUnits[unit.ClientUnit.UnitId].AOEAttackRadius;
            unit.ClientUnit.AttackPower = turnData.YourUnits[unit.ClientUnit.UnitId].AttackPower;
            unit.ClientUnit.MagicPower = turnData.YourUnits[unit.ClientUnit.UnitId].MagicPower;
        }

        foreach (var unitToHide in unitsToHide)
        {
            unitToHide.Visible = false;
        }

        foreach (var unitToShow in visibleUnits)
        {
            var player = turnData.OtherPlayers[unitToShow.ClientUnit.PlayerId];

            unitToShow.ClientUnit.Effects = player.Units[unitToShow.ClientUnit.UnitId].Effects;
            if (!unitToShow.Visible)
            {
                unitToShow.Visible = true;
                unitToShow.Position = this.maze.MapToWorld(player.Units[unitToShow.ClientUnit.UnitId].Position);
            }
        }

        var signals = new List<SignalAwaiter>();

        foreach (var unit in myUnits)
        {
            foreach (var ability in turnData.YourUnits[unit.ClientUnit.UnitId].ExecutedAbilities)
            {
                var moveAbility = this.pluginUtils.FindAbility(ability.Item1) as IMoveAbility;
                if (moveAbility == null || !moveAbility.IsBasicMove)
                {
                    continue;
                }

                await unit.MoveUnitTo(ability.Item2, moveAbility);
            }
        }

        foreach (var unit in visibleUnits)
        {
            var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];

            foreach (var ability in player.Units[unit.ClientUnit.UnitId].ExecutedAbilities)
            {
                var moveAbility = this.pluginUtils.FindAbility(ability.Item1) as IMoveAbility;
                if (moveAbility == null || !moveAbility.IsBasicMove)
                {
                    continue;
                }

                await unit.MoveUnitTo(ability.Item2, moveAbility);
            }
        }

        foreach (var signal in signals)
        {
            await signal;
        }
        signals.Clear();

        foreach (var unit in myUnits)
        {
            unit.Mp = turnData.YourUnits[unit.ClientUnit.UnitId].Mp;
            foreach (var ability in turnData.YourUnits[unit.ClientUnit.UnitId].ExecutedAbilities)
            {
                var generalAbility = this.pluginUtils.FindAbility(ability.Item1);
                var moveAbility = generalAbility as IMoveAbility;

                if (moveAbility != null && moveAbility.IsBasicMove)
                {
                    continue;
                }

                if (moveAbility != null)
                    signals.Add(unit.MoveUnitTo(ability.Item2, moveAbility));
                else
                    signals.Add(unit.Attack(ability.Item2));
            }
        }

        foreach (var unit in visibleUnits)
        {
            var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];

            foreach (var ability in player.Units[unit.ClientUnit.UnitId].ExecutedAbilities)
            {
                var generalAbility = this.pluginUtils.FindAbility(ability.Item1);
                var moveAbility = generalAbility as IMoveAbility;

                if (moveAbility != null && moveAbility.IsBasicMove)
                {
                    continue;
                }

                if (moveAbility != null)
                    signals.Add(unit.MoveUnitTo(ability.Item2, moveAbility));
                else
                    signals.Add(unit.Attack(ability.Item2));
            }
        }

        foreach (var signal in signals)
        {
            await signal;
        }
        signals.Clear();

        foreach (var unit in myUnits)
        {
            signals.Add(unit.UnitHit(turnData.YourUnits[unit.ClientUnit.UnitId].AppliedAbilities.Cast<(Ability, Vector2)?>().FirstOrDefault()?.Item2, turnData.YourUnits[unit.ClientUnit.UnitId].Hp));
            unit.ShowHpChanges(
                turnData.YourUnits[unit.ClientUnit.UnitId].HpChanges,
                turnData.YourUnits[unit.ClientUnit.UnitId].MpChanges);
        }

        foreach (var unit in visibleUnits)
        {
            var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
            signals.Add(unit.UnitHit(player.Units[unit.ClientUnit.UnitId].AppliedAbilities.Cast<(Ability, Vector2)?>().FirstOrDefault()?.Item2, player.Units[unit.ClientUnit.UnitId].Hp));
            unit.ShowHpChanges(
                player.Units[unit.ClientUnit.UnitId].HpChanges,
                null);
        }

        foreach (var signal in signals)
        {
            await signal;
        }
        signals.Clear();

        this.nextTurnButton.Visible = true;
    }
}
