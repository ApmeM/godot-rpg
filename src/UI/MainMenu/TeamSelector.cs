using Godot;
using IsometricGame.Logic;
using System;
using System.Collections.Generic;

public class TeamSelector : OptionButton
{
	public override void _Ready()
	{
		
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
