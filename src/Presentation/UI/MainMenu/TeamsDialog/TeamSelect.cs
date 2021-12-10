using Godot;
using Godot.Collections;
using IsometricGame.Logic;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;
using System.Collections.Generic;

[SceneReference("TeamSelect.tscn")]
public partial class TeamSelect : WindowDialog
{
    [Export]
    public PackedScene UnitConfigurationScene;

    [Signal]
    public delegate void TeamsUpdated();

    private List<TransferConnectData> Teams;
    private int CurrentTeam;

    private TeamsRepository teamsRepository;

    public TeamSelect()
    {
        this.teamsRepository = DependencyInjector.teamsRepository;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.saveButton.Connect("pressed", this, nameof(OnSaveButtonPressed));
        this.resetButton.Connect("pressed", this, nameof(OnResetButtonPressed));
        this.addNewTeamButton.Connect("pressed", this, nameof(OnAddNewTeamButtonPressed));
        this.addNewUnitButton.Connect("pressed", this, nameof(OnAddNewUnitButtonPressed));
        this.teamSelectorList.Connect(nameof(TeamSelectorList.SelectionChanged), this, nameof(ItemSelected));
        this.teamDescriptionLineEdit.Editable = false;
        this.OnResetButtonPressed();
    }

    public void OnAddNewTeamButtonPressed()
    {
        var team = new TransferConnectData
        {
            TeamName = "New team name.",
            Units = new List<TransferConnectData.UnitData>()
        };
        this.Teams.Add(team);
        
        this.teamSelectorList.AddItem(team, this.Teams.Count - 1);
        this.teamSelectorList.Select(Teams.Count - 1);
    }

    public void OnAddNewUnitButtonPressed()
    {
        var unit = new TransferConnectData.UnitData();
        this.Teams[this.CurrentTeam].Units.Add(unit);
        ItemSelected(this.CurrentTeam);
    }

    private void ItemSelected(int index)
    {
        this.CurrentTeam = index;
        var team = Teams[index];
        
        this.teamDescriptionLineEdit.Text = team.TeamName;

        foreach (Node item in unitsContainer.GetChildren())
        {
            item.QueueFree();
        }

        for (var i = 0; i < team.Units.Count; i++)
        {
            var unit = team.Units[i];
            var unitScene = (TeamSelectUnit)UnitConfigurationScene.Instance();
            unitsContainer.AddChild(unitScene);
            unitScene.InitUnit(unit);

            Button deleteButton = new Button();
            deleteButton.Text = "X";
            deleteButton.Connect("pressed", this, nameof(UnitRemoved), new Array { i });
            unitsContainer.AddChild(deleteButton);
        }
        this.addNewUnitButton.Visible = team.Units.Count != ServerConfiguration.DefaultMaxUnits;

        this.contentContainer.Visible = false;
        this.contentContainer.CallDeferred("set_visible", true);
    }

    private void UnitRemoved(int unitIndex)
    {
        Teams[CurrentTeam].Units.RemoveAt(unitIndex);
        ItemSelected(CurrentTeam);
    }

    public void OnSaveButtonPressed()
    {
        this.teamsRepository.SaveTeams(Teams);
        EmitSignal(nameof(TeamsUpdated));
    }

    public void OnResetButtonPressed()
    {
        this.Teams = this.teamsRepository.LoadTeams();
        this.teamSelectorList.Reload(this.Teams);

        ItemSelected(0);
    }
}
