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
    public class UILevelEditor : UIScreen
    {
        SpriteFont m_FixedFont;
        SpriteFont m_FixedSmall;
        SpriteFont m_StatusFont;
        SpriteFont m_ButtonFont;

        public UILevelEditor(ContentManager Content) :
            base(Content)
        {
            m_FixedFont = Content.Load<SpriteFont>("Fonts/FixedText");
            m_FixedSmall = Content.Load<SpriteFont>("Fonts/FixedSmall");
            m_StatusFont = Content.Load<SpriteFont>("Fonts/FixedTitle");

            Point vPos = new Point();
            vPos.X = (int)(GraphicsManager.Get().Width / 4.0f);
            vPos.Y = (int)(GraphicsManager.Get().Height - 100f);
            m_Buttons.AddLast(new Button(vPos, "S.Point Jedi",
                m_FixedFont, Color.WhiteSmoke,
                Color.Yellow, SpawnJediUnit, eButtonAlign.Center));

            vPos.X += 200;

            m_Buttons.AddLast(new Button(vPos, "S.Point Sith",
                m_FixedFont, Color.WhiteSmoke,
                Color.Yellow, SpawnSithUnit, eButtonAlign.Center));

            vPos.X += 200;

            m_Buttons.AddLast(new Button(vPos, "Building",
                m_FixedFont, Color.WhiteSmoke,
                Color.Yellow, SpawnBuilding, eButtonAlign.Center));

            vPos.X += 200;

            m_Buttons.AddLast(new Button(vPos, "Save",
                m_FixedFont, Color.WhiteSmoke,
                Color.Yellow, SaveFile, eButtonAlign.Center));

            vPos.Y -= 100;

            m_Buttons.AddLast(new Button(vPos, "Building 2",
                m_FixedFont, Color.WhiteSmoke,
                Color.Yellow, SpawnBuilding2, eButtonAlign.Center));
        }

        public void SpawnBuilding()
        {
            GameState.Get().levelEditorSpawnBuilding(1);
        }

        public void SpawnBuilding2()
        {
            GameState.Get().levelEditorSpawnBuilding(2);
        }

        public void SpawnSithUnit()
        {
            GameState.Get().levelEditorSpawnUnit(eTeam.Sith);
        }

        public void SpawnJediUnit()
        {
            GameState.Get().levelEditorSpawnUnit(eTeam.Jedi);
        }

        public void SaveFile()
        {
            LevelEditor.Get().saveObjects();
        }

        public override void Update(float fDeltaTime)
        {
            base.Update(fDeltaTime);
        }

        public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            base.Draw(fDeltaTime, DrawBatch);
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
