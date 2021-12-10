using Godot;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;

[SceneReference("TeamSelector.tscn")]
public partial class TeamSelector : Node
{
    private readonly TeamsRepository teamsRepository;

    public int SelectedTeam;

    [Signal]
    public delegate void SelectionChanged();

    public TeamSelector()
    {
        this.teamsRepository = DependencyInjector.teamsRepository;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.teamSelectorList.Connect(nameof(TeamSelectorList.SelectionChanged), this, nameof(TeamSelected));
    }

    private void TeamSelected(TeamSelectorUnit teamSelectorUnit)
    {
        this.SelectedTeam = teamSelectorUnit.TeamId;
        EmitSignal(nameof(SelectionChanged), teamSelectorUnit.TeamId);
    }
}
