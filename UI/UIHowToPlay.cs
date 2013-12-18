//-----------------------------------------------------------------------------
// UIPauseMenu comes up when you hit Escape during gameplay.
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
    public class UIHowToPlay : UIScreen
    {
        SpriteFont m_TitleFont;
        SpriteFont m_ButtonFont;
        SpriteFont m_HowFont;
        string m_PausedText;

        string m_HowToPlayText;

        public UIHowToPlay(ContentManager Content) : base(Content)
        {
            m_bCanExit = true;

            m_TitleFont = m_Content.Load<SpriteFont>("Fonts/FixedTitle");
            m_ButtonFont = m_Content.Load<SpriteFont>("Fonts/FixedButton");
            m_HowFont = m_Content.Load<SpriteFont>("Fonts/HowText");

            m_PausedText = "How To Play";

            m_HowToPlayText =
                " Use Left Analog Stick (or WASD) to Move\n" +
                "Use Right Analog Stick (or Q and E) to Aim\n" +
                "    Use Right Bumper (or Z) to Zoom In\n" +
                "   Use Right Trigger (or Space) to Fire\n" +
                "        Use B To Toggle Flight Mode\n" +
                "       Use DPad Up To Invert Controls\n" +
                "\n" +
                "           Fight With Your Team\n" +
                "               To Survive!";


            // Create buttons
            Point vPos = new Point();

            vPos.X = (int)(GraphicsManager.Get().Width / 2.0f);
            vPos.Y = (int)(GraphicsManager.Get().Height / 2.0f) + 180;

            m_Buttons.AddLast(new Button(vPos, "Fight On!",
                m_ButtonFont, new Color(0, 0, 200),
                Color.White, Resume, eButtonAlign.Center));

            SoundManager.Get().PlaySoundCue("Vader_Breathing");
        }

        public void Resume()
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

            Rectangle rect = new Rectangle(g.Width / 2 - 400, g.Height / 2 - 250, 800, 500);
            g.DrawFilled(DrawBatch, rect, Color.Black, 10.0f, Color.DarkRed); 

            Vector2 vOffset = Vector2.Zero;

            vOffset.Y -= 180;
            DrawCenteredString(DrawBatch, m_PausedText, m_TitleFont, Color.White, vOffset);

            vOffset.Y += 180;
            DrawCenteredString(DrawBatch, m_HowToPlayText, m_HowFont, Color.Gray, vOffset);


            base.Draw(fDeltaTime, DrawBatch);
        }

        public override void OnExit()
        {
            GameState.Get().IsPaused = false;
        }
    }
}
