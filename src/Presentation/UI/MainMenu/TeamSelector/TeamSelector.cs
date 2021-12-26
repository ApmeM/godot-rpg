using Godot;
using IsometricGame.Presentation;

[SceneReference("TeamSelector.tscn")]
public partial class TeamSelector : Node
{
    public int SelectedTeam;

    [Signal]
    public delegate void SelectionChanged();


    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.teamSelectorList.Connect(nameof(TeamSelectorList.SelectionChanged), this, nameof(TeamSelected));
    }

    private void TeamSelected(int teamIdx)
    {
        this.SelectedTeam = teamIdx;
        EmitSignal(nameof(SelectionChanged), teamIdx);
    }
}
