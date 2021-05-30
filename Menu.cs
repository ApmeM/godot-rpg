using Godot;
using System.Threading.Tasks;

public class Menu : CanvasLayer
{
	[Signal]
	public delegate void StartGameEvent();
	
	private int bestScore = 0;

	public override void _Ready()
	{
		if (bestScore == 0)
		{
			var file = new File();
			var result = file.Open("user://bestScore", File.ModeFlags.Read);
			if (result == Error.Ok)
			{
				bestScore = (int)file.Get32();
				GetNode<Label>("BestScoreLabel").Text = $"Best score: {bestScore}";
				file.Close();
			}
		}

		base._Ready();
	}

	private async Task ShowMessage(string text)
	{
		var message = GetNode<Label>("MessageLabel");
		message.Text = text;
		message.Show();

		var timer = GetNode<Timer>("MessageTimer");
		timer.Start();
		await GetTree().ToSignal(timer, "timeout");
		message.Hide();
	}
	
	public async void GameOver(int score)
	{
		if (bestScore < score) 
		{
			bestScore = score;
			GetNode<Label>("BestScoreLabel").Text = $"Best score: {bestScore}";
			var file = new File();
			file.Open("user://bestScore", File.ModeFlags.Write);
			file.Store32((uint)score);
			file.Close();
		}

		GetNode<Label>("LastScoreLabel").Text = $"Last score: {score}";

		await ShowMessage("Game Over");

		var message = GetNode<Label>("MessageLabel");
		message.Text = "Dodge the\nCreeps!";
		message.Show();

		var timer = GetNode<Timer>("StartButtonTimer");
		timer.Start();
		await GetTree().ToSignal(timer, "timeout");

		GetNode<Button>("StartButton").Show();
	}

	public async void OnStartButtonPressed()
	{
		await ShowMessage("Get ready.");
		EmitSignal(nameof(StartGameEvent));
	}
}
