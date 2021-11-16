using Godot;
using IsometricGame.Business.Utils;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class GnollUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Gnoll;

        public void Apply(ServerUnit unit)
        {
            unit.MaxHp = 20;
            unit.Abilities.Add(Ability.Move);
            unit.Abilities.Add(Ability.Regeneration);
            unit.Abilities.Add(Ability.MeleeAttack);
        }

        public void Initialize(ServerUnit unit)
        {
            unit.Hp = 20;
        }

        public SpriteFrames GetFrames()
        {
            var texture = ResourceLoader.Load<Texture>("assets/enemy/gnoll.png");
            var frames = AtlasHelper.SubtexturesFromAtlas(texture, 12, 15);

            var frames2 = new SpriteFrames();
            frames2.AddAnimation("idle", frames, 0, 0, 0, 1, 0, 0, 1, 1);
            frames2.AddAnimation("move", frames, 4, 5, 6, 7);
            frames2.AddAnimation("attack", frames, 2, 3, 0);
            frames2.AddAnimation("dead", frames, 8, 9, 10);

            return frames2;
        }
    }
}
