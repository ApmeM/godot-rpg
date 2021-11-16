using Godot;
using IsometricGame.Business.Utils;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WarlockUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Warlock;

        public void Apply(ServerUnit unit)
        {
            unit.SightRange = 7;
            unit.Abilities.Add(Ability.Move);
            unit.Abilities.Add(Ability.Regeneration);
            unit.Abilities.Add(Ability.RangedAttack);
        }

        public void Initialize(ServerUnit unit)
        {
        }

        public SpriteFrames GetFrames()
        {
            var texture = ResourceLoader.Load<Texture>("assets/enemy/warlock.png");
            var frames = AtlasHelper.SubtexturesFromAtlas(texture, 12, 15);

            var frames2 = new SpriteFrames();
            frames2.AddAnimation("idle", frames, 0, 0, 0, 1, 0, 0, 1, 1);
            frames2.AddAnimation("move", frames, 0, 2, 3, 4);
            frames2.AddAnimation("attack", frames, 0, 5, 6);
            frames2.AddAnimation("dead", frames, 0, 7, 8, 8, 9, 10);

            return frames2;
        }
    }
}
