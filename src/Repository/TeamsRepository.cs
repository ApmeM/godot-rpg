using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Repository
{
    public class TeamsRepository
    {
        private const string TeamsFileName = "user://teams";

        public List<TransferConnectData> LoadTeams()
        {
            var file = new File();
            if (file.Open(TeamsFileName, File.ModeFlags.Read) == Error.Ok)
            {
                var teamsCount = (int)file.Get32();
                var result = new List<TransferConnectData>(teamsCount);

                for (var i = 0; i < teamsCount; i++)
                {
                    var item = new TransferConnectData();
                    item.TeamName = file.GetPascalString();

                    var unitsCount = (int)file.Get32();
                    item.Units = new List<TransferConnectData.UnitData>(unitsCount);
                    for (var j = 0; j < unitsCount; j++)
                    {
                        var unit = new TransferConnectData.UnitData();
                        unit.UnitType = (UnitType)(int)file.Get32();

                        var skillsCount = (int)file.Get32();
                        unit.Skills = new List<Skill>(skillsCount);
                        for (var k = 0; k < skillsCount; k++)
                        {
                            var skill = (Skill)(int)file.Get32();
                            unit.Skills.Add(skill);
                        }
                        item.Units.Add(unit);
                    }
                    result.Add(item);
                }

                file.Close();

                if (result.Count > 0)
                {
                    return result;
                }
            }

            var randomDreamTeam = new List<TransferConnectData>
            {
                new TransferConnectData
                {
                    TeamName = "Dream team!",
                    Units = new List<TransferConnectData.UnitData>
                    {
                        new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.FirstAid, Skill.Logistics}},
                        new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.EagleEye, Skill.Logistics}},
                        new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.Ballistics, Skill.Logistics}},
                        new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.FirstAid, Skill.Logistics}},
                    }
                }
            };

            SaveTeams(randomDreamTeam);
            return randomDreamTeam;
        }

        public void SaveTeams(List<TransferConnectData> teams)
        {
            var file = new File();
            if (file.Open(TeamsFileName, File.ModeFlags.Write) == Error.Ok)
            {
                file.Store32((uint)teams.Count);

                for (var i = 0; i < teams.Count; i++)
                {
                    file.StorePascalString(teams[i].TeamName);
                    file.Store32((uint)teams[i].Units.Count);
                    for (var j = 0; j < teams[i].Units.Count; j++)
                    {
                        file.Store32((uint)teams[i].Units[j].UnitType);
                        file.Store32((uint)teams[i].Units[j].Skills.Count);
                        for (var k = 0; k < teams[i].Units[j].Skills.Count; k++)
                        {
                            file.Store32((uint)teams[i].Units[j].Skills[k]);
                        }
                    }
                }

                file.Close();
            }
        }
    }
}
