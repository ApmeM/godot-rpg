using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

public class UnitActions : Control
{
	[Signal]
	public delegate void ActionSelected(CurrentAction action, Ability ability);

	private readonly List<Node> buttons = new List<Node>();

	public List<Ability> Abilities
	{
		set
		{
			foreach(var b in buttons)
			{
				b.QueueFree();
			}
			buttons.Clear();

			var abilityContainer = this.GetNode<Container>("AbilityContainer");

			for (var i = 0; i < value.Count; i++)
			{
				var ability = value[i];

				var abilityNode = new TextureButton
				{
					TextureNormal = ResourceLoader.Load<Texture>($"assets/Abilities/{ability}.png"),
					Expand = true,
					StretchMode = TextureButton.StretchModeEnum.KeepAspect,
					RectMinSize = Vector2.One * 100
				};

				buttons.Add(abilityNode);
				abilityContainer.AddChild(abilityNode);
				abilityNode.Connect("pressed", this, nameof(AttackButtonPressed), new Godot.Collections.Array { ability });
			}

			this.Visible = false;
			this.CallDeferred("set_visible", true);
		}
	}

	public override void _Ready()
	{
		var abilityContainer = this.GetNode<Container>("AbilityContainer");
		var abilityNode = new TextureButton
		{
			TextureNormal = ResourceLoader.Load<Texture>($"assets/Abilities/Move.png"),
			Expand = true,
			StretchMode = TextureButton.StretchModeEnum.KeepAspect,
			RectMinSize = Vector2.One * 100
		};

		abilityNode.Connect("pressed", this, nameof(MoveButtonPressed));
		abilityContainer.AddChild(abilityNode);
	}

	private void MoveButtonPressed()
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.Move, Ability.None);
	}

	private void AttackButtonPressed(Ability ability)
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.UseAbility, ability);
	}
}
