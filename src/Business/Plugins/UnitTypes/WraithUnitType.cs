using Godot;
using IsometricGame.Business.Utils;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WraithUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Wraith;

        public void Apply(ServerUnit unit)
        {
            unit.Abilities.Add(Ability.ImmaterialMove);
        }

        public void Initialize(ServerUnit unit)
        {
        }

        public SpriteFrames GetFrames()
        {
            var texture = ResourceLoader.Load<Texture>("assets/enemy/wraith.png");
            var frames = AtlasHelper.SubtexturesFromAtlas(texture, 15, 15);

            var frames2 = new SpriteFrames();
            frames2.AddAnimation("idle", frames, 0, 1);
            frames2.AddAnimation("move", frames, 0, 1);
            frames2.AddAnimation("attack", frames, 0, 2, 3);
            frames2.AddAnimation("dead", frames, 0, 4, 5, 6, 7);

            return frames2;
        }
    }
}
