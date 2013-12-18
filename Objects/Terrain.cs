using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace itp380.Objects
{
    public class Terrain : GameObject
    {
        readonly Vector3 Size = new Vector3(140.2f, 131.7f, 0);

        Vector3 Offset;

        public Terrain(Game game, Vector3 offset)
            : base(game)
        {
            m_ModelName = "Terrains/Terrain_1";
            Scale *= 5;
            m_ModelRotation = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2), Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)Math.PI));
            Offset = offset;
            if (Math.Abs((int)Offset.X) % 2 == 1)
            {
                m_CustomTransform *= Matrix.CreateScale(new Vector3(-1, 1, 1));
            }
            if (Math.Abs((int)Offset.Y) % 2 == 1)
            {
                m_CustomTransform *= Matrix.CreateScale(new Vector3(1, -1, 1));
            }
        }

        public override void Update(float fDeltaTime)
        {
            // set position to tiled position that moves with player, so player can never move off the ground
            Vector3 newPosition = Size * (Offset + 2 * new Vector3((float)Math.Floor(GameState.Get().Player.Position.X / Size.X / 2), (float)Math.Floor(GameState.Get().Player.Position.Y / Size.Y / 2), 0));
            if (newPosition != Position)
            {
                Position = newPosition;
            }
            base.Update(fDeltaTime);
        }
    }
}
