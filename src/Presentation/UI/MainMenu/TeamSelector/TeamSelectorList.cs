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
    
    private int selectedIdx;
    private bool selectedIdxDirty;

    [Signal]
    public delegate void SelectionChanged(int teamIdx);

    public TeamSelectorList()
    {
        this.teamsRepository = DependencyInjector.teamsRepository;
    }

    public int SelectedIdx => this.selectedIdx;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.Reload(this.teamsRepository.LoadTeams());
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (this.selectedIdxDirty)
        {
            this.selectedIdxDirty = false;
            ((TeamSelectorUnit)this.GetChild(this.selectedIdx)).IsSelected = true;
        }
    }

    public void Reload(List<TransferConnectData> teams)
    {
        this.ClearChildren();

        for (var i = 0; i < teams.Count; i++)
        {
            AddItem(teams[i]);
        }
    }

    public void AddItem(TransferConnectData team)
    {
        var scene = (TeamSelectorUnit)TeamSelectorUnitScene.Instance();
        this.AddChild(scene);

        scene.Team = team;
        scene.Connect(nameof(TeamSelectorUnit.TeamSelected), this, nameof(TeamSelected));
    }

    private void TeamSelected(TeamSelectorUnit teamSelectorUnit)
    {
        foreach (TeamSelectorUnit child in this.GetChildren())
        {
            if (teamSelectorUnit.GetIndex() == child.GetIndex())
            {
                continue;
            }

            child.IsSelected = false;
        }
        
        EmitSignal(nameof(SelectionChanged), teamSelectorUnit.GetIndex());
    }

    public void Select(int teamIdx)
    {
        this.selectedIdx = teamIdx;
        this.selectedIdxDirty = true;
    }

    public void RefreshSelected()
    {
        ((TeamSelectorUnit)this.GetChild(this.selectedIdx)).Refresh();
    }
}
