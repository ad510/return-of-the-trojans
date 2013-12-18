using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace itp380.Objects
{
    public class Lasers : GameObject
    {
        public const float DrawTime = 0.5f;
        public const float ReloadTime = 1;
        public const float Width = 0.2f;
        public readonly Color LaserColor = Color.Red;

        BasicEffect m_Effect;

        public Lasers(Game game) : base(game)
        {
            m_ModelName = "";
            m_Effect = new BasicEffect(m_Game.GraphicsDevice);
            m_Effect.VertexColorEnabled = true;
        }

        public override void Draw(float fDeltaTime)
        {
            foreach (Unit unit in GameState.Get().Units)
            {
                if (unit.ShootTime <= ReloadTime)
                {
                    if (unit.ShootTime <= DrawTime && !unit.Flying)
                    {
                        Camera myCamera = GameState.Get().Camera;

                        // draw laser
                        float width = Width * (1 - unit.ShootTime / DrawTime);
                        Vector3 start = unit.LaserStartPos();
                        
                        Vector3 end = unit.ShootHitPos;
                        if (unit == GameState.Get().Player) // draw laser for player so that it goes through reticle on screen
                        {
                            Vector3 laserVector = myCamera.cameraForward * Vector3.Distance(start, end);
                            end = myCamera.cameraTarget + laserVector;
                        }
                        

                        if (Vector3.Dot(start - myCamera.cameraPos, myCamera.cameraForward) > 0
                            || Vector3.Dot(end - myCamera.cameraPos, myCamera.cameraForward) > 0)
                        {
                            Vector3 perp = Vector3.Cross(start - myCamera.cameraPos, end - start);
                            perp.Normalize();
                            perp *= width / 2;
                            VertexPositionColor[] vertices = new VertexPositionColor[4];
                            vertices[0] = new VertexPositionColor(start + perp, LaserColor);
                            vertices[1] = new VertexPositionColor(start - perp, LaserColor);
                            vertices[2] = new VertexPositionColor(end + perp, LaserColor);
                            vertices[3] = new VertexPositionColor(end - perp, LaserColor);
                            m_Effect.View = GameState.Get().CameraMatrix;
                            m_Effect.Projection = GraphicsManager.Get().Projection;
                            foreach (EffectPass pass in m_Effect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                m_Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2, VertexPositionColor.VertexDeclaration);
                            }
                        }
                    }
                    unit.ShootTime += fDeltaTime;
                }
            }
        }
    }
}
