using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;
using System.Collections.Generic;

[SceneReference("TeamSelector.tscn")]
public partial class TeamSelector : OptionButton
{
    private readonly TeamsRepository teamsRepository;

    public TeamSelector()
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
        this.Clear();
        foreach (var team in teams)
        {
            this.AddItem(team.TeamName);
        }
    }
}
