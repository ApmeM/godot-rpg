using Godot;
using IsometricGame.Logic;
using System.Collections.Generic;

public class TeamSelect : CanvasLayer
{
	private List<TransferConnectData> Teams;

	[Signal]
	public delegate void StartGameEvent();

	[Export]
	public PackedScene UnitConfigurationScene;

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
		this.Teams = this.Teams ?? TransferConnectData.Load();
		var optionButton = GetNode<OptionButton>("OptionButton");
		optionButton.Connect("item_selected", this, nameof(ItemSelected));
		foreach (var team in this.Teams)
		{
			optionButton.AddItem(team.TeamName);
		}

		ItemSelected(0);
	}

	private void ItemSelected(int index)
	{
		var team = Teams[index];
		var teamDescription = GetNode<Label>("TeamDescription");
		var unitsContainer = GetNode<HBoxContainer>("UnitsContainer");
		teamDescription.Text = team.TeamName;
		for (var i = 0; i < team.Units.Count; i++)
		{
			var unit = team.Units[i];
			var unitScene = (TeamSelectUnit)UnitConfigurationScene.Instance();
			unitScene.InitUnit(unit);
			unitsContainer.AddChild(unitScene);
		}
	}

	public void OnStartButtonPressed()
	{
		TransferConnectData.Save(Teams);
		EmitSignal(nameof(StartGameEvent));
	}
}
