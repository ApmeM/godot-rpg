using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Linq;

public class TeamSelectUnit : VBoxContainer
{
	private TransferConnectData.UnitData Unit;

	public override void _Ready()
	{
		base._Ready();
		var unitTypeCombo = this.GetNode<OptionButton>("UnitTypeCombo");
		unitTypeCombo.Connect("item_selected", this, nameof(UnitTypeChanged));
	}

	public void InitUnit(TransferConnectData.UnitData unit)
	{
		this.Unit = unit;
		var unitTypeCombo = this.GetNode<OptionButton>("UnitTypeCombo");
		var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
		for (var i = 0; i < unitTypes.Count; i++)
		{
			var unitType = unitTypes[i];
			unitTypeCombo.AddItem(unitType.ToString());
			if (unitType == unit.UnitType)
			{
				unitTypeCombo.Select(i);
			}
		}

		var skillsContainer = this.GetNode<VBoxContainer>("SkillsContainer");
		var skills = Enum.GetValues(typeof(Skill)).Cast<Skill>().ToList();
		for (var j = 0; j < unit.Skills.Count; j++)
		{
			var unitSkill = unit.Skills[j];
			var skillsCombo = new OptionButton();
			skillsCombo.Connect("item_selected", this, nameof(SkillChanged), new Godot.Collections.Array { j });
			skillsContainer.AddChild(skillsCombo);

			for (var i = 0; i < skills.Count; i++)
			{
				var skill = skills[i];
				skillsCombo.AddItem(skill.ToString());
				if (skill == unitSkill)
				{
					skillsCombo.Select(i);
				}
			}
		}

		UpdateUnitDetails();
	}

	private void SkillChanged(int index, int skillIndex)
	{
		this.Unit.Skills[skillIndex] = (Skill)index;
		UpdateUnitDetails();
	}

	private void UnitTypeChanged(int index)
	{
		this.Unit.UnitType = (UnitType)index;
		UpdateUnitDetails();
	}

	private void UpdateUnitDetails()
	{
		var unit = UnitUtils.BuildUnit(this.Unit.UnitType);
		var player = new ServerPlayer();
		for (var i = 0; i < this.Unit.Skills.Count; i++)
		{
			var skill = this.Unit.Skills[i];
			UnitUtils.ApplySkill(player, unit, skill);
		}

		this.GetNode<UnitDetails>("UnitDetails").SelectUnit(unit);
	}
}
