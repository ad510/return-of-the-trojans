//-----------------------------------------------------------------------------
// The main GameState Singleton. All actions that change the game state,
// as well as any global updates that happen during gameplay occur in here.
// Because of this, the file is relatively lengthy.
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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace itp380
{
	public enum eGameState
	{
		None = 0,
		MainMenu,
		Gameplay,
        LevelEditor
	}

    public enum eTeam
    {
        Jedi,
        Sith
    }

	public class GameState : itp380.Patterns.Singleton<GameState>
	{
        const int NumBuildings = 15;
        public int NumUnits;
        public int NumStockpileUnits;

		Game m_Game;
		eGameState m_State;
		public eGameState State
		{
			get { return m_State; }
		}

		 eGameState m_NextState;
		Stack<UI.UIScreen> m_UIStack;
		bool m_bPaused = false;
		public bool IsPaused
		{
			get { return m_bPaused; }
			set	{ m_bPaused = value; }
		}
        bool m_bInGameHowToPlay = false;
        public bool isInGameHowToPlay
        {
            get { return m_bInGameHowToPlay; }
            set { m_bInGameHowToPlay = value; }
        }

        public Random m_Random = new Random();

        bool m_bRandomMap_NotLevelMap = false;
        public bool isRandomMapNotLevelMap
        {
            get { return m_bRandomMap_NotLevelMap; }
            set { m_bRandomMap_NotLevelMap = value; }
        }

		// Keeps track of all active game objects
		LinkedList<GameObject> m_GameObjects = new LinkedList<GameObject>();
        

        // initialize these in SetupGameplay(), not here
        List<Objects.Building> m_Buildings;
        public List<Objects.Building> Buildings
        {
            get { return m_Buildings; }
        }
        List<Objects.Unit> m_Units;
        public List<Objects.Unit> Units
        {
            get { return m_Units; }
        }
        List<Objects.Missile> m_Missiles;

        Dictionary<eTeam, int> m_Stockpile;
        public int GetStockPileFor(eTeam team)
        {
            return m_Stockpile[team];
        }

        Objects.Unit m_Player;
        public Objects.Unit Player
        {
            get { return m_Player; }
        }

        Objects.Lasers m_Lasers;
        List<GameObject> m_LevelObjects;
        public Pathfind m_Pathfind;
        public bool m_SithXWing;


		// Camera Information
		Camera m_Camera;
		public Camera Camera
		{
			get { return m_Camera; }
		}

		public Matrix CameraMatrix
		{
			get { return m_Camera.CameraMatrix; }
		}

		// Timer class for the global GameState
		Utils.Timer m_Timer = new Utils.Timer();

		UI.UIGameplay m_UIGameplay;
        UI.UILevelEditor m_UILevelEditor;
		
		public void Start(Game game)
		{
			m_Game = game;
			m_State = eGameState.None;
			m_UIStack = new Stack<UI.UIScreen>();

			m_Camera = new Camera(m_Game);
		}

		public void SetState(eGameState NewState)
		{
			m_NextState = NewState;
		}

		private void HandleStateChange()
		{
			if (m_NextState == m_State)
				return;

			switch (m_NextState)
			{
				case eGameState.MainMenu:
					m_UIStack.Clear();
					m_UIGameplay = null;
                    m_UILevelEditor = null;
					m_Timer.RemoveAll();
					m_UIStack.Push(new UI.UIMainMenu(m_Game.Content));

                    SoundManager.Get().StopAllMusic();
                    SoundManager.Get().PlaySoundCue("ObiWan", 1);

					ClearGameObjects();
					break;
                case eGameState.Gameplay:
                    SetupGameplay();
                    Camera.SetupCameraForGameplay();

                    SoundManager.Get().StopAllMusic();
                    SoundManager.Get().PlayMusicCue("Imperial");
                    break;

                case eGameState.LevelEditor:
					SetupGameplay();
                    Camera.SetupCameraForLevelEditor();

                    SoundManager.Get().StopAllMusic();
                    SoundManager.Get().PlayMusicCue("Cantina");
					break;
			}

			m_State = m_NextState;
		}

		protected void ClearGameObjects()
		{
			// Clear out any and all game objects
			foreach (GameObject o in m_GameObjects)
			{
				RemoveGameObject(o, false);
			}
			m_GameObjects.Clear();
		}

		public void SetupGameplay()
		{
			ClearGameObjects();
			m_UIStack.Clear();
            if (m_NextState == eGameState.Gameplay)
            {
                m_UIGameplay = new UI.UIGameplay(m_Game.Content);
                m_UIStack.Push(m_UIGameplay);
            }
            else if (m_NextState == eGameState.LevelEditor)
            {
                m_UILevelEditor = new UI.UILevelEditor(m_Game.Content);
                m_UIStack.Push(m_UILevelEditor);
            }

			m_bPaused = false;
			GraphicsManager.Get().ResetProjection();
			
			m_Timer.RemoveAll();

            // TODO: Add any gameplay setup here
            m_Pathfind = new Pathfind(100, 100, 3, new Vector3(-150, -150, 0));
            m_Buildings = new List<Objects.Building>();
            m_Units = new List<Objects.Unit>();
            m_Missiles = new List<Objects.Missile>();
            m_Stockpile = new Dictionary<eTeam, int>();
            m_LevelObjects = new List<GameObject>();
            // spawn terrain tiles
            // by looping from -7 to 7, furthest tiles are guaranteed to be beyond far plane set at 500 in GraphicsManager
            for (int i = -7; i <= 7; i++)
            {
                for (int j = -7; j <= 7; j++)
                {
                    SpawnGameObject(new Objects.Terrain(m_Game, new Vector3(i, j, 0)));
                }
            }
            if (m_NextState == eGameState.Gameplay)
            {
                if (isRandomMapNotLevelMap)
                {
                    for (int i = 0; i < NumBuildings; i++)
                    {
                        int type = m_Random.Next(1, 3);
                        SpawnBuilding(SpawnBuildingRandomPos(type), type);
                    }
                }
                else
                {
                    try
                    {
                        LevelEditor.Get().loadObjects();
                    }
                    catch { } // in case file not found
                }
                for (int i = 0; i < NumUnits; i++)
                {
                    SpawnUnit(eTeam.Jedi, SpawnUnitRandomPos(eTeam.Jedi), (float)(m_Random.NextDouble() * Math.PI * 2));
                }
                for (int i = 0; i < NumUnits; i++)
                {
                    SpawnUnit(eTeam.Sith, SpawnUnitRandomPos(eTeam.Sith), (float)(m_Random.NextDouble() * Math.PI * 2));
                }
                m_Stockpile[eTeam.Jedi] = NumStockpileUnits;
                m_Stockpile[eTeam.Sith] = NumStockpileUnits;
                m_Timer.AddTimer("how to play", 0, ShowHowToPlay, false); // For How To Play Screen
            }
            else if (m_NextState == eGameState.LevelEditor)
            {
                SpawnUnit(eTeam.Jedi, SpawnUnitRandomPos(eTeam.Jedi), (float)(m_Random.NextDouble() * Math.PI * 2));
                LevelEditor.Get().addObject(m_Units[0]);
            }
            m_Player = m_Units[0];
            SpawnLasers();
            m_SithXWing = false;
		}

        public void Update(float fDeltaTime)
		{
			HandleStateChange();

			switch (m_State)
			{
				case eGameState.MainMenu:
					UpdateMainMenu(fDeltaTime);
					break;
				case eGameState.Gameplay:
					UpdateGameplay(fDeltaTime);
					break;
                case eGameState.LevelEditor:
                    UpdateGameplay(fDeltaTime);
					break;
			}

			foreach (UI.UIScreen u in m_UIStack)
			{
				u.Update(fDeltaTime);
			}
		}

		void UpdateMainMenu(float fDeltaTime)
		{

		}

		void UpdateGameplay(float fDeltaTime)
		{
            if (!IsPaused)
            {
                m_Camera.Update(fDeltaTime);

                // Update objects in the world
                // We have to make a temp copy in case the objects list changes
                LinkedList<GameObject> temp = new LinkedList<GameObject>(m_GameObjects);
                foreach (GameObject o in temp)
                {
                    if (o.Enabled)
                    {
                        o.Update(fDeltaTime);
                    }
                }
                m_Timer.Update(fDeltaTime);

                // TODO: Any update code not for a specific game object should go here
                if (m_State == eGameState.Gameplay)
                {
                    // collision detection/response
                    // this is done here instead of in Unit.Update() because it helps to access units by index
                    for (int i = 0; i < m_Units.Count - 1; i++)
                    {
                        // with units
                        for (int j = i + 1; j < m_Units.Count; j++)
                        {
                            if (Vector3.DistanceSquared(m_Units[i].Position, m_Units[j].Position) <= Objects.Unit.Radius * Objects.Unit.Radius)
                            {
                                Vector3 diff = m_Units[i].Position - m_Units[j].Position;
                                diff.Z = 0;
                                m_Units[i].Position += diff * (Objects.Unit.Radius - diff.Length()) / 2;
                                m_Units[j].Position -= diff * (Objects.Unit.Radius - diff.Length()) / 2;
                                m_Units[i].UpdateLegs();
                                m_Units[j].UpdateLegs();
                            }
                        }
                        // with buildings
                        foreach (Objects.Building building in m_Buildings)
                        {
                            Vector3 diff = m_Units[i].Position - building.Position;
                            if (Math.Abs(diff.X) < building.Radius + Objects.Unit.Radius && Math.Abs(diff.Y) < building.Radius + Objects.Unit.Radius
                                && diff.Z < building.BoundsMax.Z)
                            {
                                Vector3 unitPos = m_Units[i].Position;
                                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                                {
                                    unitPos.X += (building.Radius + Objects.Unit.Radius - Math.Abs(diff.X)) * diff.X / Math.Abs(diff.X);
                                }
                                else
                                {
                                    unitPos.Y += (building.Radius + Objects.Unit.Radius - Math.Abs(diff.Y)) * diff.Y / Math.Abs(diff.Y);
                                }
                                m_Units[i].Position = unitPos;
                                m_Units[i].UpdateLegs();
                            }
                        }
                    }
                    //}
                    if (!m_SithXWing && m_Stockpile[eTeam.Sith] > 0)
                    {
                        for (int i = m_Units.Count - 1; i >= 0; i--) // iterate in reverse so more likely to pick someone near the starting point
                        {
                            Objects.Unit u = m_Units[i];
                            if (u.Team == eTeam.Sith && u.Bombs > 0)
                            {
                                u.FlyingState = true;
                                m_SithXWing = true;
                                break;
                            }
                        }
                    }
                }
            }
		}

		public void SpawnGameObject(GameObject o)
		{
			o.Load();
			m_GameObjects.AddLast(o);
			GraphicsManager.Get().AddGameObject(o);
		}

		public void RemoveGameObject(GameObject o, bool bRemoveFromList = true)
		{
			o.Enabled = false;
			o.Unload();
			GraphicsManager.Get().RemoveGameObject(o);
			if (bRemoveFromList)
			{
				m_GameObjects.Remove(o);
			}
		}

        public void levelEditorSpawnUnit(eTeam team)
        {
            Objects.SpawnPoint sp = new Objects.SpawnPoint(m_Game, team);
            LevelEditor.Get().addObject(sp);
        }

        public void levelEditorSpawnBuilding(int i)
        {
            GameObject building = SpawnBuilding(m_Player.Position, i);
            LevelEditor.Get().addObject(building);
        }

        public Objects.Building SpawnBuilding(Vector3 position, int type)
        {
            Objects.Building building = new Objects.Building(m_Game, type);
            building.Position = position;
            m_Buildings.Add(building);
            m_LevelObjects.Add(building);
            SpawnGameObject(building);
            m_Pathfind.OccupyRectangle(building.Position + building.BoundsMin - new Vector3(Objects.Unit.Radius), building.Position + building.BoundsMax + new Vector3(Objects.Unit.Radius));
            return building;
        }

        private Objects.Unit SpawnUnit(eTeam team, Vector3 position, float angle)
        {
            Objects.Unit unit = new Objects.Unit(m_Game, team);
            unit.Position = new Vector3(position.X, position.Y, Objects.Unit.SpawnAltitude);
            unit.Angle = angle;
            m_Units.Add(unit);
            SpawnGameObject(unit);
            return unit;
        }

        private Vector3 SpawnBuildingRandomPos(int type)
        {
            Vector3 ret;
            bool intersect = false;
            /*do
            {
                ret = new Vector3((float)m_Random.NextDouble() * 200 - 100, (float)m_Random.NextDouble() * 200 - 100, 0);
            } while (Math.Abs(ret.X) < 25 && Math.Abs(ret.Y) < 40);*/ // buildings are scenery for now, so don't spawn any in main game area
            do
            {
                ret = new Vector3((float)m_Random.NextDouble() * 160 - 80, (float)m_Random.NextDouble() * 160 - 80, 0);
                // ensure building isn't spawned intersecting with another building
                Objects.Building testBuilding = new Objects.Building(m_Game, type);
                testBuilding.Position = ret;
                intersect = false;
                foreach (Objects.Building building in m_Buildings)
                {
                    if (testBuilding.Box().Intersects(building.Box()))
                    {
                        intersect = true;
                        break;
                    }
                }
            } while (intersect);
            return ret;
        }

        private Vector3 SpawnUnitRandomPos(eTeam team)
        {
            switch (team)
            {
                case eTeam.Jedi:
                    return new Vector3((float)m_Random.NextDouble() * 100 - 50, (float)m_Random.NextDouble() * 15 - 7.5f - /*15*/100, 0);
                case eTeam.Sith:
                    return new Vector3((float)m_Random.NextDouble() * 100 - 50, (float)m_Random.NextDouble() * 15 - 7.5f + /*15*/100, 0);
            }
            throw new ArgumentException("invalid team");
        }

        public void RemoveUnit(Objects.Unit unit)
        {
            bool win = true;
            if (m_Stockpile[unit.Team] > 0)
            {
                // spawn replacement unit from stockpile
                Objects.Unit replacement = SpawnUnit(unit.Team, SpawnUnitRandomPos(unit.Team), (float)(m_Random.NextDouble() * Math.PI * 2));
                m_Stockpile[unit.Team]--;
                if (unit == m_Player)
                {
                    m_Player = replacement;
                }
            }
            // remove unit
            m_Units.Remove(unit);
            //RemoveGameObject(unit); // don't remove game object so can show dying animation
            SoundManager.Get().PlaySoundCue("Scream", 0.2f);
            // if player killed then let the player control another unit
            if (unit == m_Player)
            {
                foreach (Objects.Unit unit2 in Units)
                {
                    if (unit2.Team == m_Player.Team)
                    {
                        m_Player = unit2;
                        break;
                    }
                }
                // if no one left on player's team then defeat
                if (unit == m_Player)
                {
                    GameOver(false);
                }
            }
            // if everyone left is on player's team then victory
            foreach (Objects.Unit unit2 in Units)
            {
                if (unit2.Team != m_Player.Team)
                {
                    win = false;
                    break;
                }
            }
            if (win)
            {
                GameOver(true);
            }
        }

        public Objects.Missile SpawnMissile(eTeam team, Vector3 position)
        {
            Objects.Missile missile = new Objects.Missile(m_Game, team, position);
            m_Missiles.Add(missile);
            SpawnGameObject(missile);
            return missile;
        }

        public void RemoveMissile(Objects.Missile missile)
        {
            m_Missiles.Remove(missile);
            RemoveGameObject(missile);
        }

        private void SpawnLasers()
        {
            m_Lasers = new Objects.Lasers(m_Game);
            SpawnGameObject(m_Lasers);
        }

		public void MouseClick(Point Position)
		{
			if (m_State == eGameState.Gameplay && !IsPaused)
			{
				// TODO: Respond to mouse clicks here
			}
		}



        public void MouseMove(Point Position)
        {
            if (!GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                Point center = new Point(GlobalDefines.WindowedWidth / 2, GlobalDefines.WindowHeight / 2);
                //if (Position.X > 0 && Position.X < GlobalDefines.WindowedWidth && Position.Y > 0 && Position.Y < GlobalDefines.WindowHeight)
                //{
                    m_Camera.controllerAim = new Vector3(0.0f, 0.0f, 1.0f) * (center.Y - Position.Y) * 0.020f;
                //}
                /*
                else
                {
                    m_Camera.controllerAim = Vector3.Zero;
                }
                */
            }
        }


        bool isInverted = false;

        
        public void ControllerInput(SortedList<eBindings, BindInfo> binds, float fDeltaTime)
        {
            if ((m_State == eGameState.Gameplay || m_State == eGameState.LevelEditor) && !IsPaused)
            {
                if (GamePad.GetState(PlayerIndex.One).IsConnected)
                {
                    // Left Controller Stick Up/Down
                    m_Player.Position += m_Player.Forward * Objects.Unit.MoveSpeed * fDeltaTime
                        * GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y;

                    // Left Controller Stick Left/Right
                    m_Player.Position += m_Player.Right * Objects.Unit.StrafeSpeed * fDeltaTime
                        * GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X;



                    if (isInverted)
                    {
                        // Right Controller Stick Left/Right
                        m_Player.Angle += Objects.Unit.RotateSpeed * fDeltaTime
                            * GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X;

                        // Right Controller Stick Up/Down
                        float vertAimSpeed = 5.0f;
                        m_Camera.controllerAim -= new Vector3(0.0f, 0.0f, 1.0f) * vertAimSpeed * fDeltaTime
                            * GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y;
                    }
                    else
                    {
                        // Right Controller Stick Left/Right
                        m_Player.Angle -= Objects.Unit.RotateSpeed * fDeltaTime
                            * GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X;

                        // Right Controller Stick Up/Down
                        float vertAimSpeed = 5.0f;
                        m_Camera.controllerAim += new Vector3(0.0f, 0.0f, 1.0f) * vertAimSpeed * fDeltaTime
                            * GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y;
                    }
                    //float min = -3.00f, max = 4.0f;
                    //if (m_Camera.controllerAim.Z < min) { m_Camera.controllerAim.Z = min; }
                    //if (m_Camera.controllerAim.Z > max) { m_Camera.controllerAim.Z = max; }


                    if(binds.ContainsKey(eBindings.UpD))
                    {
                        isInverted = !isInverted;
                    }


                    if (binds.ContainsKey(eBindings.A))
                    {
                        GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                    }

                    if (binds.ContainsKey(eBindings.B))
                    {
                        BoardAndFly();
                    }

                    if (binds.ContainsKey(eBindings.Controller_Zoom_In))
                    {
                        Camera.ZoomIn();
                    }
                    if (binds.ContainsKey(eBindings.Controller_Zoom_Out))
                    {
                        Camera.ZoomOut();
                    }


                    if (binds.ContainsKey(eBindings.LB) || binds.ContainsKey(eBindings.LT)
                             || binds.ContainsKey(eBindings.RT))
                    {
                        m_Player.Shoot();
                    }
                }
            }
        }

        public void StartVibration()
        {
            if (GamePad.GetState(PlayerIndex.One).IsConnected) { GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f); m_Timer.AddTimer("stop vibration", 0.5f, StopVibration, false); }
        }

        public void StopVibration() { GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f); }

		// I'm the last person to get keyboard input, so don't need to remove
		public void KeyboardInput(SortedList<eBindings, BindInfo> binds, float fDeltaTime)
		{
			if ((m_State == eGameState.Gameplay || m_State == eGameState.LevelEditor) && !IsPaused)
			{
                // TODO: Add keyboard input handling for Gameplay
                if (binds.ContainsKey(eBindings.Player_Rotate_Left))
                {
                    m_Player.Angle += Objects.Unit.RotateSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Rotate_Right))
                {
                    m_Player.Angle -= Objects.Unit.RotateSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Left) || binds.ContainsKey(eBindings.Player_Arrow_Left))
                {
                    m_Player.Position -= m_Player.Right * Objects.Unit.StrafeSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Right) || binds.ContainsKey(eBindings.Player_Arrow_Right))
                {
                    m_Player.Position += m_Player.Right * Objects.Unit.StrafeSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Forward) || binds.ContainsKey(eBindings.Player_Arrow_Forward))
                {
                    m_Player.Position += m_Player.Forward * Objects.Unit.MoveSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Back) || binds.ContainsKey(eBindings.Player_Arrow_Back))
                {
                    m_Player.Position -= m_Player.Forward * Objects.Unit.MoveSpeed * fDeltaTime;
                }
                if (binds.ContainsKey(eBindings.Player_Shoot))
                {
                    m_Player.Shoot();
                }

                if (binds.ContainsKey(eBindings.Player_Zoom_In))
                {
                    Camera.ZoomIn();
                }          
                if (binds.ContainsKey(eBindings.Player_Zoom_Out))
                {
                    Camera.ZoomOut();
                }

                if (binds.ContainsKey(eBindings.Player_Board_Ship))
                {
                    BoardAndFly();
                }
			}
		}


        public void BoardAndFly()
        {
            if (m_Player.TargetAltitude != Objects.Unit.FlyTargetAltitude)
            {
                m_Player.TargetAltitude = Objects.Unit.FlyTargetAltitude;
                if (!m_Player.Flying)
                {
                    m_Player.Flying = true;
                    m_Camera.ToggleFlightCamera();
                }
            }
            else
            {
                m_Player.TargetAltitude = Objects.Unit.EndFlyTargetAltitude;
            }
        }


		public UI.UIScreen GetCurrentUI()
		{
			return m_UIStack.Peek();
		}

		public int UICount
		{
			get { return m_UIStack.Count; }
		}

		// Has to be here because only this can access stack!
		public void DrawUI(float fDeltaTime, SpriteBatch batch)
		{
			// We draw in reverse so the items at the TOP of the stack are drawn after those on the bottom
			foreach (UI.UIScreen u in m_UIStack.Reverse())
			{
				u.Draw(fDeltaTime, batch);
			}
		}

		// Pops the current UI
		public void PopUI()
		{
			m_UIStack.Peek().OnExit();
			m_UIStack.Pop();

            if (isInGameHowToPlay) { IsPaused = true; isInGameHowToPlay = false; }
            else { IsPaused = false; }
		}

        public void ShowHowToPlay()
        {
            IsPaused = true;
            m_UIStack.Push(new UI.UIHowToPlay(m_Game.Content));
        }

		public void ShowPauseMenu()
		{
			IsPaused = true;
			m_UIStack.Push(new UI.UIPauseMenu(m_Game.Content));
		}

		public void Exit()
		{
			m_Game.Exit();
		}

		void GameOver(bool victorious)
        {
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                StopVibration();
            }

            if (victorious) { SoundManager.Get().PlaySoundCue("R2D2", 1); }
            else { SoundManager.Get().PlaySoundCue("Vader", 1); }

            IsPaused = true;
            m_UIStack.Push(new UI.UIGameOver(m_Game.Content, victorious));
        }
	}
}
