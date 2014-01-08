//-----------------------------------------------------------------------------
// UIBattleMenu comes up when you start a new game from the main menu.
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
	public class UIBattleMenu : UIScreen
    {
        //SpriteFont m_TitleFont;
        SpriteFont m_ButtonFont;
        //string m_TitleText;

		public UIBattleMenu(ContentManager Content) :
			base(Content)
		{
			m_bCanExit = true;

            //m_TitleFont = m_Content.Load<SpriteFont>("Fonts/FixedTitle");
			m_ButtonFont = m_Content.Load<SpriteFont>("Fonts/FixedButton");

            //m_TitleText = "New Game";
			// Create buttons
			Point vPos = new Point();
			vPos.X = (int) (GraphicsManager.Get().Width / 2.0f);
			vPos.Y = (int)(GraphicsManager.Get().Height / 2.0f);

            vPos.Y -= 50;
			m_Buttons.AddLast(new Button(vPos, "Normal Battle",
                m_ButtonFont, new Color(0, 0, 200),
                Color.White, NormalBattle, eButtonAlign.Center));

            vPos.Y += 50;
            m_Buttons.AddLast(new Button(vPos, "Crazy Battle",
                m_ButtonFont, new Color(0, 0, 200),
                Color.White, CrazyBattle, eButtonAlign.Center));

			vPos.Y += 50;
			m_Buttons.AddLast(new Button(vPos, "Cancel",
                m_ButtonFont, new Color(0, 0, 200),
                Color.White, Cancel, eButtonAlign.Center));
		}

		public void NormalBattle()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");

            GameState.Get().NumUnits = 10;
            GameState.Get().NumStockpileUnits = 20;

			GameState.Get().SetState(eGameState.Gameplay);
		}

        public void CrazyBattle()
        {
            SoundManager.Get().PlaySoundCue("MenuClick");

            GameState.Get().NumUnits = 50;
            GameState.Get().NumStockpileUnits = 200;

            GameState.Get().SetState(eGameState.Gameplay);
        }

		public void Cancel()
        {
            GameState.Get().PopUI();
            SoundManager.Get().PlaySoundCue("MenuClick");
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			// Draw background
			GraphicsManager g = GraphicsManager.Get();
			Rectangle rect = new Rectangle(g.Width / 2 - 200, g.Height / 2 - 115,
				400, 250);
			g.DrawFilled(DrawBatch, rect, Color.Black, 4.0f, Color.DarkRed);

			/*Vector2 vOffset = Vector2.Zero;
            vOffset.Y -= 75;
            DrawCenteredString(DrawBatch, m_TitleText, m_TitleFont, Color.White, vOffset);*/
						
			base.Draw(fDeltaTime, DrawBatch);
		}
	}
}
