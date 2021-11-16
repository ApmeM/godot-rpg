using Godot;
using System.Collections.Generic;

namespace IsometricGame.Business.Utils
{
    public static class AtlasHelper
    {
        public static List<AtlasTexture> SubtexturesFromAtlas(
            Texture texture,
            int cellWidth,
            int cellHeight,
            int cellOffset = 0,
            int maxCellsToInclude = int.MaxValue)
        {
            var subtextures = new List<AtlasTexture>();

            var cols = texture.GetWidth() / cellWidth;
            var rows = texture.GetHeight() / cellHeight;
            var i = 0;

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    // skip everything before the first cellOffset
                    if (i++ < cellOffset)
                        continue;

                    subtextures.Add(
                        new AtlasTexture
                        {
                            Atlas = texture,
                            Region = new Rect2(x * cellWidth, y * cellHeight, cellWidth, cellHeight)
                        });

                    // once we hit the max number of cells to include bail out. were done.
                    if (subtextures.Count == maxCellsToInclude)
                        break;
                }
            }

            return subtextures;
        }

        public static void AddAnimation(this SpriteFrames spriteFrames, string animationName, List<AtlasTexture> frames, params int[] frameIds)
        {
            AddSingleAnimation(spriteFrames, animationName + "Up", frames, frameIds);
            AddSingleAnimation(spriteFrames, animationName + "Down", frames, frameIds);
            AddSingleAnimation(spriteFrames, animationName + "Left", frames, frameIds);
            AddSingleAnimation(spriteFrames, animationName + "Right", frames, frameIds);
        }

        public static void AddSingleAnimation(this SpriteFrames spriteFrames, string animationName, List<AtlasTexture> frames, params int[] frameIds) 
        {
            spriteFrames.AddAnimation(animationName);
            spriteFrames.SetAnimationSpeed(animationName, 5);
            foreach(var frameId in frameIds)
            {
                spriteFrames.AddFrame(animationName, frames[frameId]);
            }
        }
    }
}
