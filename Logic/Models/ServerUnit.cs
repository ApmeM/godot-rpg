using Godot;

namespace IsometricGame.Logic.Models
{
    public class ServerUnit
    {
        public Vector2 Position;
        public int MoveDistance = 5;
        public int SightRange = 6;
        public int AttackDistance = 1;
        public int AttackRadius = 1;
        public int AttackPower = 50;
        public int Hp = 100;
    }
}