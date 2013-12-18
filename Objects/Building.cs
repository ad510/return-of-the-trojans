using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace itp380.Objects
{
    // I combined the Building2 and Obstacle classes back into the Building class because
    // that made it much easier to adjust collision bounds per building type without worrying about variable overriding
    public class Building : GameObject
    {
        public const float MinRadius = 7;

        public Vector3 BoundsMin;
        public Vector3 BoundsMax;
        // Radius is redundant with BoundsMin and BoundsMax and originally I was planning to remove it,
        // but turns out that wasn't necessary and would have unnecessarily complicated collision detection logic
        public float Radius;

        public Building(Game game, int type)
            : base(game)
        {
            style = type;
            // randomly determine building height
            float heightScale;
            if (GameState.Get().m_Random.NextDouble() < 0.2f)
            {
                // 20% probability to be normal height (tall)
                heightScale = 1;
            }
            else
            {
                // 80% probability to be short, so we can see over them when flying
                heightScale = 0.2f + 0.2f * (float)GameState.Get().m_Random.NextDouble();
            }
            // determine model and collision bounds using type parameter
            if (type == 1)
            {
                m_ModelName = "Terrains/tall_building_2/tall_building_3";
                Radius = 7;
                BoundsMin = new Vector3(-Radius, -Radius, 0);
                BoundsMax = new Vector3(Radius, Radius, 100 * heightScale);
            }
            else if (type == 2)
            {
                m_ModelName = "Terrains/tall_building/tall_building_1";
                Radius = 15;
                BoundsMin = new Vector3(-Radius, -Radius, 0);
                BoundsMax = new Vector3(Radius, Radius, 100 * heightScale);
            }
            else
            {
                throw new ArgumentException("invalid building type");
            }
            Scale *= .05f;
            name = "Building";
            m_ModelRotation = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2), Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)Math.PI));
            // randomly decide whether to lower building by scaling it or moving it down
            if (GameState.Get().m_Random.NextDouble() < 0.5f)
            {
                m_CustomTransform = Matrix.CreateScale(new Vector3(1, 1, heightScale));
            }
            else
            {
                m_CustomTransform = Matrix.CreateTranslation(new Vector3(0, 0, -100 + 100 * heightScale));
            }
        }

        public BoundingBox Box()
        {
            return new BoundingBox(Position + BoundsMin, Position + BoundsMax);
        }
    }
}
