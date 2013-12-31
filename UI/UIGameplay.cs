//-----------------------------------------------------------------------------
// UIGameplay is UI while in the main game state.
// Because there are so many aspects to the UI, this class is relatively large.
//
// __Defense Sample for Game Programming Algorithms and Techniques
// Copyright (C) Sanjay Madhav. All rights reserved.
//
// Released under the Microsoft Permissive License.
// See LICENSE.txt for full details.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace itp380.UI
{
	public class UIGameplay : UIScreen
	{
		SpriteFont m_FixedFont;
		SpriteFont m_FixedSmall;
        SpriteFont m_StatusFont;

        Texture2D crossHair, crossHairRed;

        Texture2D radarScreen, radarSpinner, radarCompass;
        Texture2D radarEnemy, radarEnemyFlying, radarBuilding, radarFriend;

        public enum eBlips
        {
            building=0,
            enemy,
            enemyFlying,
            friend,
        }

		public UIGameplay(ContentManager Content) : base(Content)
		{
			m_FixedFont = Content.Load<SpriteFont>("Fonts/FixedText");
			m_FixedSmall = Content.Load<SpriteFont>("Fonts/FixedSmall");
			m_StatusFont = Content.Load<SpriteFont>("Fonts/StatusText");

            crossHair = Content.Load<Texture2D>("crosshair");
            crossHairRed = Content.Load<Texture2D>("crosshair_red");

            radarScreen = Content.Load<Texture2D>("Radar/radar");
            radarSpinner = Content.Load<Texture2D>("Radar/radarSpinner");
            radarCompass = Content.Load<Texture2D>("Radar/radarCompass");

            radarEnemy = Content.Load<Texture2D>("Radar/radarEnemy");
            radarEnemyFlying = Content.Load<Texture2D>("Radar/radarEnemyFlying");
            radarFriend = Content.Load<Texture2D>("Radar/radarFriendly");
            radarBuilding = Content.Load<Texture2D>("Radar/radarBuilding");
		}


        public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
            base.Draw(fDeltaTime, DrawBatch);

            // Display Crosshair
            Camera m_Camera = GameState.Get().Camera;
            float scale = m_Camera.reticleScale;

            Vector3 cameraTarget = m_Camera.cameraTarget;
            Vector2 reticleLocation;
            reticleLocation = new Vector2(GraphicsManager.Get().Width / 2 - (crossHair.Width * scale / 2), GraphicsManager.Get().Height / 2 - (crossHair.Height * scale / 2));
 
            DrawBatch.Draw((GameState.Get().Player.ShootCollision() != null) ? crossHairRed : crossHair, reticleLocation, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // Status Text
            Vector2 healthPos = new Vector2(GraphicsManager.Get().Width / 2 - 80, 25);
            Vector2 bombsCachePos = new Vector2(GraphicsManager.Get().Width / 2 - 80, 60);
            Vector2 jediCachePos = new Vector2(0 + 30, 25);
            Vector2 sithCachePos = new Vector2(GraphicsManager.Get().Width - 330, 25);

            String healthString = GameState.Get().Player.Health.ToString();
            DrawBatch.DrawString(m_StatusFont, "Health: " + healthString, healthPos, Color.White);

            DrawBatch.DrawString(m_StatusFont, "Bombs: " + GameState.Get().Player.Bombs, bombsCachePos, Color.White);


            int jediCount = 0, sithCount = 0;
            foreach (Objects.Unit unit in GameState.Get().Units)
            {
                if (unit.Team == eTeam.Jedi) { jediCount++; }
                else if (unit.Team == eTeam.Sith) { sithCount++; }
            }

            String jediCacheString = (GameState.Get().GetStockPileFor(eTeam.Jedi) + jediCount).ToString();
            DrawBatch.DrawString(m_StatusFont, "Jedi Remaining: " + jediCacheString, jediCachePos, Color.White);

            String sithCacheString = (GameState.Get().GetStockPileFor(eTeam.Sith) + sithCount).ToString();
            DrawBatch.DrawString(m_StatusFont, "Sith Remaining: " + sithCacheString, sithCachePos, Color.White);

            

            DrawRadarOnScreen(fDeltaTime, DrawBatch);
		}



        // Drawing the Radar
        Vector2 radarCenter = Vector2.Zero;
        float curRotation = 0.0f, compassAngle = 0.0f;

        // Radar Units
        Objects.Unit player; Vector2 playerForward, playerPos;
        //float radarPixelRadius = 90.0f;
        float radarPixelRadius = 90.0f * 1.5f;
        float radarScreenRange = 110.0f;

        public void DrawRadarOnScreen(float fDeltaTime, SpriteBatch DrawBatch)
        {
            //radarCenter = new Vector2(GraphicsManager.Get().Width - 100, GraphicsManager.Get().Height - 100);
            radarCenter = new Vector2(GraphicsManager.Get().Width - 150, GraphicsManager.Get().Height - 150);
            
            // Draw Base Radar
            //DrawBatch.Draw(radarScreen, radarCenter, null, Color.White, 0.0f, new Vector2(radarScreen.Width/2, radarScreen.Height/2), 1.0f, SpriteEffects.None, 0.0f);
            DrawBatch.Draw(radarScreen, radarCenter, null, Color.White, 0.0f, new Vector2(radarScreen.Width / 2, radarScreen.Height / 2), 1.5f, SpriteEffects.None, 0.0f);

            // Draw Radar Spinner
            float rotateSpeed = 2.50f;
            curRotation += fDeltaTime * rotateSpeed % MathHelper.TwoPi;

            //DrawBatch.Draw(radarSpinner, radarCenter, null, Color.White, curRotation, new Vector2(radarSpinner.Width/2, radarSpinner.Height/2), 1.0f, SpriteEffects.None, 0.0f);
            DrawBatch.Draw(radarSpinner, radarCenter, null, Color.White, curRotation, new Vector2(radarSpinner.Width / 2, radarSpinner.Height / 2), 1.5f, SpriteEffects.None, 0.0f);



            // Draw Radar Compass
            player = GameState.Get().Player;
            playerPos = new Vector2(player.Position.X, player.Position.Y);
            playerForward = Vector2.Normalize(new Vector2(player.Forward.X, player.Forward.Y));

            compassAngle = (float)Math.Atan(playerForward.Y / playerForward.X);
            compassAngle -= MathHelper.PiOver2; // Making North 0 Rotation
            if (playerForward.X < 0) { compassAngle += MathHelper.Pi; } // Adjustments for Quadrants 2, 3 Transition

            //DrawBatch.Draw(radarCompass, radarCenter, null, Color.White, compassAngle, new Vector2(radarSpinner.Width / 2, radarSpinner.Height / 2), 1.0f, SpriteEffects.None, 1.0f);
            DrawBatch.Draw(radarCompass, radarCenter, null, Color.White, compassAngle, new Vector2(radarSpinner.Width / 2, radarSpinner.Height / 2), 1.5f, SpriteEffects.None, 1.0f);


            // Drawing Objects
            foreach (Objects.Building building in GameState.Get().Buildings)
            {
                Vector2 originalPos = new Vector2(building.Position.X, building.Position.Y);
                DrawRadarBlip(DrawBatch, originalPos, eBlips.building);
            }

            foreach (Objects.Unit unit in GameState.Get().Units)
            {
                Vector2 originalPos = new Vector2(unit.Position.X, unit.Position.Y);
                if (unit.Team == eTeam.Jedi)
                {
                    if (unit.Flying)
                    {
                        DrawRadarBlip(DrawBatch, originalPos, eBlips.enemyFlying);
                    }
                    else
                    {
                        DrawRadarBlip(DrawBatch, originalPos, eBlips.friend);
                    }
                }
                else 
                {
                    if (unit.Flying)
                    {
                        DrawRadarBlip(DrawBatch, originalPos, eBlips.enemyFlying);
                    }
                    else
                    {
                        DrawRadarBlip(DrawBatch, originalPos, eBlips.enemy);
                    }
                }
            }
        }


        void DrawRadarBlip(SpriteBatch DrawBatch, Vector2 originalPos, eBlips blipType)
        {
            Vector2 relativePosition = originalPos - playerPos;

            float magnitude = relativePosition.Length() / radarScreenRange;

            if (magnitude < 1.0f)
            {
                Vector2 normalRelativePos = Vector2.Normalize(relativePosition);

                double dotProd = Vector2.Dot(normalRelativePos, playerForward);
                double angleFromUp = Math.Acos(dotProd); // Measures Angle Between Forward Vector (Always Up) and Building

                double angleFromZero = 0.0;
                float zAxisRotation = normalRelativePos.X * playerForward.Y - normalRelativePos.Y * playerForward.X;
                if (zAxisRotation >= 0)
                {
                    angleFromZero = MathHelper.PiOver2 - angleFromUp;
                }
                else
                {
                    angleFromZero = MathHelper.PiOver2 + angleFromUp;
                }


                float radarX = (float)Math.Cos(angleFromZero) * magnitude * radarPixelRadius;
                float radarY = -(float)Math.Sin(angleFromZero) * magnitude * radarPixelRadius;
                Vector2 radarPos = new Vector2(radarX, radarY);

                Texture2D texture = null;
                if (blipType == eBlips.building) { texture = radarBuilding; }
                else if (blipType == eBlips.enemy) { texture = radarEnemy; }
                else if (blipType == eBlips.enemyFlying) { texture = radarEnemyFlying; }
                else if (blipType == eBlips.friend) { texture = radarFriend; }

                //DrawBatch.Draw(texture, radarCenter + radarPos, null, Color.White, 0.0f,
                    //new Vector2(radarBuilding.Width / 2, radarBuilding.Height / 2), 1.0f, SpriteEffects.None, 1.0f);

                DrawBatch.Draw(texture, radarCenter + radarPos, null, Color.White, 0.0f,
                    new Vector2(radarBuilding.Width / 2, radarBuilding.Height / 2), 1.5f, SpriteEffects.None, 1.0f);
            }
        }


		public override void KeyboardInput(SortedList<eBindings, BindInfo> binds)
		{
			GameState g = GameState.Get();
			if (binds.ContainsKey(eBindings.UI_Exit))
			{
				g.ShowPauseMenu();
				binds.Remove(eBindings.UI_Exit);
			}
			base.KeyboardInput(binds);
		}

        public override void ControllerInput(SortedList<eBindings, BindInfo> binds)
        {
            GameState g = GameState.Get();
            if (binds.ContainsKey(eBindings.UI_Exit))
            {
                g.ShowPauseMenu();
                binds.Remove(eBindings.UI_Exit);
            }
            base.ControllerInput(binds);
        }
	}
}
