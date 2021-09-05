using IsometricGame.Logic.Enums;
using MazeGenerators;
using MazeGenerators.Utils;

namespace IsometricGame.Logic.ScriptHelpers
{
    public class MapUtils
    {
        public static GeneratorResult GetMap(MapGeneratingType mapType)
        {
            var result = new GeneratorResult();

            switch (mapType)
            {
                case MapGeneratingType.Arena:
                    {
                        var settings = new GeneratorSettings
                        {
                            Width = 17,
                            Height = 17,
                            Mirror = GeneratorSettings.MirrorDirection.Rotate
                        };

                        CommonAlgorithm.GenerateField(result, settings);
                        RoomGeneratorAlgorithm.AddRoom(result, settings, new Rectangle(1, 1, settings.Width - 2, settings.Height - 2));
                        MirroringAlgorithm.Mirror(result, settings);
                        break;
                    }
                case MapGeneratingType.Random:
                    {
                        var settings = new GeneratorSettings
                        {
                            Width = 35,
                            Height = 35,
                            MinRoomSize = 5,
                            MaxRoomSize = 9,
                        };

                        CommonAlgorithm.GenerateField(result, settings);
                        RoomGeneratorAlgorithm.GenerateRooms(result, settings);
                        TreeMazeBuilderAlgorithm.GrowMaze(result, settings);
                        RegionConnectorAlgorithm.GenerateConnectors(result, settings);
                        DeadEndRemoverAlgorithm.RemoveDeadEnds(result, settings);
                        break;
                    }
            }

            return result;
        }
    }
}
