using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace itp380.Objects
{
    public class Missile : GameObject
    {
        public const float HitRadius = 5;
        public const int HitDamage = 100;
        public readonly Vector3 Acceleration = new Vector3(0, 0, -10);

        eTeam Team;
        Vector3 InitialPos;
        float TimeElapsed;

        public Missile(Game game, eTeam team, Vector3 initialPos)
            : base(game)
        {
            m_ModelName = "Projectiles/Sphere";
            Team = team;
            InitialPos = initialPos;
            Position = initialPos;
            TimeElapsed = 0;
        }

        public override void Update(float fDeltaTime)
        {
            TimeElapsed += fDeltaTime;
            Position = InitialPos + Acceleration * TimeElapsed * TimeElapsed;
            if (Position.Z <= 0)
            {
                // hit ground, so explode
                double randNum = GameState.Get().m_Random.NextDouble() * 100;
                if (randNum < 33)      { SoundManager.Get().PlaySoundCue("Explosion1", 1); }
                else if (randNum < 66) { SoundManager.Get().PlaySoundCue("Explosion2", 1); }
                else                   { SoundManager.Get().PlaySoundCue("Explosion3", 1); }
                


                Position = new Vector3(Position.X, Position.Y, 0);
                for (int i = 0; i < GameState.Get().Units.Count; i++)
                {
                    Unit unit = GameState.Get().Units[i];
                    if (!unit.Flying && Vector3.DistanceSquared(unit.Position, Position) <= HitRadius * HitRadius)
                    {
                        if (unit.TakeHealth(HitDamage))
                        {
                            i--;
                        }
                    }
                }
                GameState.Get().RemoveMissile(this);
            }
            base.Update(fDeltaTime);
        }
    }
}
