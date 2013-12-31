//-----------------------------------------------------------------------------
// UIMainMenu is the main menu UI.
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
	public class UIMainMenu : UIScreen
	{
		SpriteFont m_TitleFont;
		SpriteFont m_ButtonFont;
		string m_Title;

		public UIMainMenu(ContentManager Content) :
			base(Content)
		{
			m_TitleFont = m_Content.Load<SpriteFont>("Fonts/QuartzTitle");
			m_ButtonFont = m_Content.Load<SpriteFont>("Fonts/QuartzButton");

			// Create buttons
			Point vPos = new Point();
			vPos.X = (int) (GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(GraphicsManager.Get().Height / 2.0f);

            m_Title = "  Return of \nthe Trojans";

            vPos.Y += 40;
			m_Buttons.AddLast(new Button(vPos, "New Game - Random Map",
                m_ButtonFont, Color.Yellow,
                Color.White, NewGameRandomMap, eButtonAlign.Center));

            vPos.Y += 90;
            m_Buttons.AddLast(new Button(vPos, "New Game - Custom Map",
                m_ButtonFont, Color.Yellow,
                Color.White, NewGameLevelEditorMap, eButtonAlign.Center));

            vPos.Y += 90;
            m_Buttons.AddLast(new Button(vPos, "Level Editor",
                m_ButtonFont, Color.Yellow,
                Color.White, LevelEditor, eButtonAlign.Center));

			vPos.Y += 90;
			m_Buttons.AddLast(new Button(vPos, "Exit",
                m_ButtonFont, Color.Yellow,
                Color.White, Exit, eButtonAlign.Center));
		}

		public void NewGameRandomMap()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");
            GameState.Get().isRandomMapNotLevelMap = true;
            GameState.Get().ShowBattleMenu();
		}

        public void NewGameLevelEditorMap()
        {
            SoundManager.Get().PlaySoundCue("MenuClick");
            GameState.Get().isRandomMapNotLevelMap = false;
            GameState.Get().ShowBattleMenu();
        }

        public void LevelEditor()
        {
            SoundManager.Get().PlaySoundCue("MenuClick");
            GameState.Get().SetState(eGameState.LevelEditor);
        }

		public void Options(){}

		public void Exit()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");
			GameState.Get().Exit();
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			Vector2 vOffset = Vector2.Zero;
			vOffset.Y = -1.0f * GraphicsManager.Get().Height / 4.0f;
			DrawCenteredString(DrawBatch, m_Title, m_TitleFont, Color.Yellow, vOffset);

			base.Draw(fDeltaTime, DrawBatch);
		}
	}
}
