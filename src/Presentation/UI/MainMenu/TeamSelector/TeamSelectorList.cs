using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Presentation.Utils;
using IsometricGame.Repository;
using System.Collections.Generic;

[SceneReference("TeamSelectorList.tscn")]
public partial class TeamSelectorList : Node
{
    private readonly TeamsRepository teamsRepository;

    [Export]
    public PackedScene TeamSelectorUnitScene;

    public int SelectedTeam;

    [Signal]
    public delegate void SelectionChanged(int teamId);

    public TeamSelectorList()
    {
        this.teamsRepository = DependencyInjector.teamsRepository;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Reload(this.teamsRepository.LoadTeams());
    }

    public void Reload(List<TransferConnectData> teams)
    {
        this.ClearChildren();

        for (var i = 0; i < teams.Count; i++)
        {
            AddItem(teams[i], i);
        }
    }

    public void AddItem(TransferConnectData team, int teamId)
    {
        var scene = (TeamSelectorUnit)TeamSelectorUnitScene.Instance();
        this.AddChild(scene);

        scene.Initialize(team, teamId);
        scene.Connect(nameof(TeamSelectorUnit.Selected), this, nameof(TeamSelected));
    }

    private void TeamSelected(TeamSelectorUnit teamSelectorUnit)
    {
        foreach (TeamSelectorUnit i in this.GetChildren())
        {
            if (teamSelectorUnit.TeamId == i.TeamId)
            {
                continue;
            }

            i.Deselect();
            break;
        }

        this.SelectedTeam = teamSelectorUnit.TeamId;
        EmitSignal(nameof(SelectionChanged), teamSelectorUnit.TeamId);
    }

    public void Select(int teamId)
    {
        foreach (TeamSelectorUnit i in this.GetChildren())
        {
            if (teamId != i.TeamId)
            {
                continue;
            }

            i.Select();
            break;
        }
    }
}
