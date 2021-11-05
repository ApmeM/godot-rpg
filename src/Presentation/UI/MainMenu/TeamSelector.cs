using Godot;
using IsometricGame.Logic;
using IsometricGame.Presentation;
using System.Collections.Generic;

[SceneReference("TeamSelector.tscn")]
public partial class TeamSelector : OptionButton
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public void Refresh(List<TransferConnectData> teams)
    {
        this.Clear();
        foreach (var team in teams)
        {
            this.AddItem(team.TeamName);
        }
    }
}
