using Godot;
using Godot.Collections;
using IsometricGame.Logic;
using System.Collections.Generic;

public class TeamSelect : Container
{
	private List<TransferConnectData> Teams;
	private int CurrentTeam;

	[Signal]
	public delegate void StartGameEvent();

	[Export]
	public PackedScene UnitConfigurationScene;

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/HBoxContainer2/StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
		this.GetNode<Button>("VBoxContainer/ScrollContainer/HBoxContainer/CenterContainer/AddNewUnitButton").Connect("pressed", this, nameof(OnAddNewUnitButtonPressed));
		this.GetNode<Button>("VBoxContainer/HBoxContainer/AddNewTeamButton").Connect("pressed", this, nameof(OnAddNewTeamButtonPressed));
		this.Teams = this.Teams ?? TransferConnectData.Load();
		var optionButton = GetNode<OptionButton>("VBoxContainer/HBoxContainer/OptionButton");
		optionButton.Connect("item_selected", this, nameof(ItemSelected));
		foreach (var team in this.Teams)
		{
			optionButton.AddItem(team.TeamName);
		}

		ItemSelected(0);
	}

	public void OnAddNewTeamButtonPressed()
	{
		var team = new TransferConnectData
		{
			TeamName = "New team name.",
			Units = new List<TransferConnectData.UnitData>()
		};
		this.Teams.Add(team);
		
		var optionButton = GetNode<OptionButton>("VBoxContainer/HBoxContainer/OptionButton");
		optionButton.AddItem(team.TeamName);
		optionButton.Selected = Teams.Count - 1;
		ItemSelected(Teams.Count - 1);
	}

	public void OnAddNewUnitButtonPressed()
	{
		var unit = new TransferConnectData.UnitData();
		this.Teams[this.CurrentTeam].Units.Add(unit);
		var unitScene = (TeamSelectUnit)UnitConfigurationScene.Instance();
		unitScene.InitUnit(unit);
		var unitsContainer = GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/HBoxContainer/UnitsContainer");
		unitsContainer.AddChild(unitScene);

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}

	private void ItemSelected(int index)
	{
		this.CurrentTeam = index;
		var team = Teams[index];
		var teamDescription = GetNode<Label>("VBoxContainer/TeamDescription");
		var unitsContainer = GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/HBoxContainer/UnitsContainer");
		teamDescription.Text = team.TeamName;

		foreach (Node item in unitsContainer.GetChildren())
		{
			item.QueueFree();
		}

		for (var i = 0; i < team.Units.Count; i++)
		{
			var unit = team.Units[i];
			var unitScene = (TeamSelectUnit)UnitConfigurationScene.Instance();
			unitScene.InitUnit(unit);
			unitScene.Connect(nameof(TeamSelectUnit.UnitRemoved), this, nameof(UnitRemoved), new Array { i });
			unitsContainer.AddChild(unitScene);
		}

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}

	private void UnitRemoved(int unitIndex)
	{
		Teams[CurrentTeam].Units.RemoveAt(unitIndex);
		ItemSelected(CurrentTeam);
	}

	public void OnStartButtonPressed()
	{
		TransferConnectData.Save(Teams);
		EmitSignal(nameof(StartGameEvent));
	}
}
