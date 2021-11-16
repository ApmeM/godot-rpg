using Godot;
using IsometricGame.Business.Utils;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class BatUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Bat;

        public void Apply(ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Fly);
            unit.Abilities.Add(Ability.HasteAura);
            unit.Abilities.Add(Ability.Regeneration);
        }

        public void Initialize(ServerUnit unit)
        {
        }

        public SpriteFrames GetFrames()
        {
            var texture = ResourceLoader.Load<Texture>("assets/enemy/bat.png");
            var frames = AtlasHelper.SubtexturesFromAtlas(texture, 15, 15);

            var frames2 = new SpriteFrames();
            frames2.AddAnimation("idle", frames, 0, 1);
            frames2.AddAnimation("move", frames, 0, 1);
            frames2.AddAnimation("attack", frames, 2, 3, 0, 1);
            frames2.AddAnimation("dead", frames, 4, 5, 6);

            return frames2;
        }
    }
}
