using Godot;
using Godot.Collections;
using IsometricGame.Logic;
using System.Collections.Generic;

public class TeamSelect : Container
{
	[Export]
	public PackedScene UnitConfigurationScene;

	[Signal]
	public delegate void TeamsUpdated();

	private List<TransferConnectData> Teams;
	private int CurrentTeam;

	private Container unitsContainer;
	private LineEdit teamDescription;
	private OptionButton chooseTeamOptionButton;
	private Button addNewUnitButton;

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("ContentContainer/ButtonsContainer/SaveButton").Connect("pressed", this, nameof(OnSaveButtonPressed));
		this.GetNode<Button>("ContentContainer/ButtonsContainer/ResetButton").Connect("pressed", this, nameof(OnResetButtonPressed));
		this.GetNode<Button>("ContentContainer/TeamContainer/AddNewTeamButton").Connect("pressed", this, nameof(OnAddNewTeamButtonPressed));

		this.unitsContainer = GetNode<Container>("ContentContainer/ScrollUnitContainer/UnitManagerContainer/UnitsContainer");
		this.teamDescription = GetNode<LineEdit>("ContentContainer/TeamDescriptionLineEdit");
		this.chooseTeamOptionButton = GetNode<OptionButton>("ContentContainer/TeamContainer/ChooseTeamOptionButton");
		this.addNewUnitButton = this.GetNode<Button>("ContentContainer/ScrollUnitContainer/UnitManagerContainer/AddNewUnitButtonContainer/AddNewUnitButton");
		addNewUnitButton.Connect("pressed", this, nameof(OnAddNewUnitButtonPressed));
		chooseTeamOptionButton.Connect("item_selected", this, nameof(ItemSelected));
		
		teamDescription.Editable = false;
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
		
		chooseTeamOptionButton.AddItem(team.TeamName);
		chooseTeamOptionButton.Selected = Teams.Count - 1;
		ItemSelected(Teams.Count - 1);
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
		
		this.teamDescription.Text = team.TeamName;

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

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}

	private void UnitRemoved(int unitIndex)
	{
		Teams[CurrentTeam].Units.RemoveAt(unitIndex);
		ItemSelected(CurrentTeam);
	}

	public void OnSaveButtonPressed()
	{
		TransferConnectData.Save(Teams);
		EmitSignal(nameof(TeamsUpdated));
	}

	public void OnResetButtonPressed()
	{
		this.Teams = TransferConnectData.Load();
		chooseTeamOptionButton.Clear();
		foreach (var team in this.Teams)
		{
			chooseTeamOptionButton.AddItem(team.TeamName);
		}

		ItemSelected(0);
	}
}
