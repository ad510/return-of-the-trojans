//-----------------------------------------------------------------------------
// InputManager checks for key binds and adds them to the active binds list
// as appropriate.
// The implementation is similar to the one discussed later in Chapter 5.
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
	public enum eBindType
	{
		JustPressed, // Was just pressed
		JustReleased, // Was just released
		Held // Was just pressed OR being held
	}

	public enum eBindings
	{
        // Universal
		UI_Exit = 0,
        
        // Keyboard
        Player_Rotate_Left, Player_Rotate_Right, Player_Left, Player_Right, Player_Forward, Player_Back,
        Player_Arrow_Left,  Player_Arrow_Right, Player_Arrow_Forward, Player_Arrow_Back, Player_Shoot,
        Player_Zoom_In, Player_Zoom_Out, Player_Board_Ship,

        // Controller 
        A, B, X, Y, UpD, DownD, LeftD, RightD, LB, LT, RT,

        Controller_Zoom_In, Controller_Zoom_Out, // RB

        rightStick_R, rightStick_L, rightStick_U, rightStick_D, leftStick_R, leftStick_L, leftStick_U, leftStick_D,
        
		NUM_BINDINGS
	}

	public class BindInfo
	{
		public BindInfo(Keys Key, eBindType Type)
		{
			m_Key = Key;
			m_Type = Type;
		}

        public BindInfo(Buttons Button, eBindType Type)
        {
            m_Button = Button;
            m_Type = Type;
        }

		public Keys m_Key;
		public eBindType m_Type;
        public Buttons m_Button;
	}

	public class InputManager : itp380.Patterns.Singleton<InputManager>
	{
		// Keyboard binding map
		private SortedList<eBindings, BindInfo> m_Bindings;
		private void InitializeBindings()
		{
			m_Bindings = new SortedList<eBindings, BindInfo>();
			// UI Bindings
			m_Bindings.Add(eBindings.UI_Exit, new BindInfo(Keys.Escape, eBindType.JustPressed));
           
            // Keyboard Bindings
            m_Bindings.Add(eBindings.Player_Rotate_Left, new BindInfo(Keys.Q, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Rotate_Right, new BindInfo(Keys.E, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Left, new BindInfo(Keys.A, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Right, new BindInfo(Keys.D, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Forward, new BindInfo(Keys.W, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Back, new BindInfo(Keys.S, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Arrow_Left, new BindInfo(Keys.Left, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Arrow_Right, new BindInfo(Keys.Right, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Arrow_Forward, new BindInfo(Keys.Up, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Arrow_Back, new BindInfo(Keys.Down, eBindType.Held));
            m_Bindings.Add(eBindings.Player_Shoot, new BindInfo(Keys.Space, eBindType.JustPressed));

            m_Bindings.Add(eBindings.Player_Zoom_In, new BindInfo(Keys.Z, eBindType.JustPressed));
            m_Bindings.Add(eBindings.Player_Zoom_Out, new BindInfo(Keys.Z, eBindType.JustReleased));

            m_Bindings.Add(eBindings.Player_Board_Ship, new BindInfo(Keys.B, eBindType.JustPressed));
		}

        private SortedList<eBindings, BindInfo> m_ControllerBindings;
        private void InitializeControllerBindings()
        {
            m_ControllerBindings = new SortedList<eBindings, BindInfo>();

            // UI Bindings
            m_ControllerBindings.Add(eBindings.UI_Exit, new BindInfo(Buttons.Start, eBindType.JustPressed));

            // Controller Button Bindings
            m_ControllerBindings.Add(eBindings.A, new BindInfo(Buttons.A, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.B, new BindInfo(Buttons.B, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.X, new BindInfo(Buttons.X, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.Y, new BindInfo(Buttons.Y, eBindType.JustPressed));

            m_ControllerBindings.Add(eBindings.RightD, new BindInfo(Buttons.DPadRight, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.LeftD, new BindInfo(Buttons.DPadLeft, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.UpD, new BindInfo(Buttons.DPadUp, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.DownD, new BindInfo(Buttons.DPadDown, eBindType.JustPressed));

            m_ControllerBindings.Add(eBindings.LB, new BindInfo(Buttons.LeftShoulder, eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.LT, new BindInfo(Buttons.LeftTrigger,  eBindType.JustPressed));
            m_ControllerBindings.Add(eBindings.RT, new BindInfo(Buttons.RightTrigger, eBindType.JustPressed));

            //m_ControllerBindings.Add(eBindings.RB, new BindInfo(Buttons.RightShoulder, eBindType.Held));
            m_ControllerBindings.Add(eBindings.Controller_Zoom_In, new BindInfo(Buttons.RightShoulder, eBindType.Held));
            m_ControllerBindings.Add(eBindings.Controller_Zoom_Out, new BindInfo(Buttons.RightShoulder, eBindType.JustReleased));

            // Controller Analog Bindings
            m_ControllerBindings.Add(eBindings.rightStick_R, new BindInfo(Buttons.RightThumbstickRight, eBindType.Held));
            m_ControllerBindings.Add(eBindings.rightStick_L, new BindInfo(Buttons.RightThumbstickLeft, eBindType.Held));
            m_ControllerBindings.Add(eBindings.rightStick_U, new BindInfo(Buttons.RightThumbstickUp, eBindType.Held));
            m_ControllerBindings.Add(eBindings.rightStick_D, new BindInfo(Buttons.RightThumbstickDown, eBindType.Held));

            m_ControllerBindings.Add(eBindings.leftStick_R, new BindInfo(Buttons.LeftThumbstickRight, eBindType.Held));
            m_ControllerBindings.Add(eBindings.leftStick_L, new BindInfo(Buttons.LeftThumbstickLeft, eBindType.Held));
            m_ControllerBindings.Add(eBindings.leftStick_U, new BindInfo(Buttons.LeftThumbstickUp, eBindType.Held));
            m_ControllerBindings.Add(eBindings.leftStick_D, new BindInfo(Buttons.LeftThumbstickDown, eBindType.Held));
        }

        // Keyboard and Gamepad Data
		private SortedList<eBindings, BindInfo> m_ActiveBinds = new SortedList<eBindings, BindInfo>();
        private SortedList<eBindings, BindInfo> m_ActiveControllerBinds = new SortedList<eBindings, BindInfo>(); 


		// Mouse Data
		private MouseState m_PrevMouse;
		private MouseState m_CurrMouse;

		// The mouse position according to Windows
		private Point m_DeviceMousePos = Point.Zero;
		// The mouse position taking into account deltas, no clamping
		private Point m_ActualMousePos = Point.Zero;
		// Mouse position with clamping
		private Point m_MousePos = Point.Zero;
		
		public Point MousePosition
		{
			get { return m_MousePos; }
		}

		// Keyboard Data
		private KeyboardState m_PrevKey;
		private KeyboardState m_CurrKey;

        // Controller Data
        private GamePadState m_PrevButton;
        private GamePadState m_CurrButton;


		public void Start()
		{
			InitializeBindings();
            InitializeControllerBindings();

			m_PrevMouse = Mouse.GetState();
			m_CurrMouse = Mouse.GetState();

			m_DeviceMousePos.X = m_CurrMouse.X;
			m_DeviceMousePos.Y = m_CurrMouse.Y;
			m_ActualMousePos = m_DeviceMousePos;
			m_MousePos = m_ActualMousePos;
			ClampMouse();

			m_PrevKey = Keyboard.GetState();
			m_CurrKey = Keyboard.GetState();

            m_PrevButton = GamePad.GetState(PlayerIndex.One);
            m_CurrButton = GamePad.GetState(PlayerIndex.One);
		}


		private void ClampMouse()
		{
			if (m_MousePos.X < 0)
			{
				m_MousePos.X = 0;
			}
			if (m_MousePos.Y < 0)
			{
				m_MousePos.Y = 0;
			}
			if (m_MousePos.X > GraphicsManager.Get().Width)
			{
				m_MousePos.X = GraphicsManager.Get().Width - GlobalDefines.iMouseCursorSize / 4;
			}
			if (m_MousePos.Y > GraphicsManager.Get().Height)
			{
				m_MousePos.Y = GraphicsManager.Get().Height - GlobalDefines.iMouseCursorSize / 4;
			}
		}


		public void UpdateMouse(float fDeltaTime)
		{
			m_PrevMouse = m_CurrMouse;
			m_CurrMouse = Mouse.GetState();

			m_DeviceMousePos.X = m_CurrMouse.X;
			m_DeviceMousePos.Y = m_CurrMouse.Y;

			m_ActualMousePos = m_DeviceMousePos;
			m_MousePos = m_ActualMousePos;
						
			ClampMouse();

			// Check for click
			if (JustPressed(m_PrevMouse.LeftButton, m_CurrMouse.LeftButton))
			{
				// If the UI doesn't handle it, send it to GameState
				if (GameState.Get().UICount == 0 ||
					!GameState.Get().GetCurrentUI().MouseClick(m_MousePos))
				{
					GameState.Get().MouseClick(m_MousePos);
				}
			}

            // Check for Mouse Position
            GameState.Get().MouseMove(m_MousePos);
		}


		public void UpdateKeyboard(float fDeltaTime)
		{
			m_PrevKey = m_CurrKey;
			m_CurrKey = Keyboard.GetState();
			m_ActiveBinds.Clear();

			// Build the list of bindings which were triggered this frame
			foreach (KeyValuePair<eBindings, BindInfo> k in m_Bindings)
			{
				Keys Key = k.Value.m_Key;
				eBindType Type = k.Value.m_Type;
				switch (Type)
				{
					case (eBindType.Held):
						if ((m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key)) ||
							(!m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key)))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
					case (eBindType.JustPressed):
						if (!m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
					case (eBindType.JustReleased):
						if (m_PrevKey.IsKeyDown(Key) &&
							!m_CurrKey.IsKeyDown(Key))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
				}
			}

			if (m_ActiveBinds.Count > 0)
			{
				// Send the list to the UI first, then any remnants to the game
				if (GameState.Get().UICount != 0)
				{
					GameState.Get().GetCurrentUI().KeyboardInput(m_ActiveBinds);
				}

				GameState.Get().KeyboardInput(m_ActiveBinds, fDeltaTime);
			}
		}


        public void UpdateController(float fDeltaTime)
        {
            m_PrevButton = m_CurrButton;
            m_CurrButton = GamePad.GetState(PlayerIndex.One);
            m_ActiveControllerBinds.Clear();

            // Build the list of bindings which were triggered this frame
            foreach (KeyValuePair<eBindings, BindInfo> k in m_ControllerBindings)
            {
                Buttons Button = k.Value.m_Button;
                eBindType Type = k.Value.m_Type;
                switch (Type)
                {
                    case (eBindType.Held):
                        if ((m_PrevButton.IsButtonDown(Button) &&
                            m_CurrButton.IsButtonDown(Button)) ||
                            (!m_PrevButton.IsButtonDown(Button) &&
                            m_CurrButton.IsButtonDown(Button)))
                        {
                            m_ActiveControllerBinds.Add(k.Key, k.Value);
                        }
                        break;

                    case (eBindType.JustPressed):
                        if (!m_PrevButton.IsButtonDown(Button) &&
                            m_CurrButton.IsButtonDown(Button))
                        {
                            m_ActiveControllerBinds.Add(k.Key, k.Value);
                        }
                        break;
                    case (eBindType.JustReleased):
                        if (m_PrevButton.IsButtonDown(Button) &&
                            !m_CurrButton.IsButtonDown(Button))
                        {
                            m_ActiveControllerBinds.Add(k.Key, k.Value);
                        }
                        break;
                }
            }

            if (m_ActiveControllerBinds.Count > 0)
            {
                // Send the list to the UI first, then any remnants to the game
                if (GameState.Get().UICount != 0)
                {
                    GameState.Get().GetCurrentUI().KeyboardInput(m_ActiveControllerBinds);
                }

                GameState.Get().ControllerInput(m_ActiveControllerBinds, fDeltaTime);
            }
        }


		public void Update(float fDeltaTime)
		{
			UpdateMouse(fDeltaTime);
			UpdateKeyboard(fDeltaTime);
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                UpdateController(fDeltaTime);
            }
		}


		protected bool JustPressed(ButtonState Previous, ButtonState Current)
		{
			if (Previous == ButtonState.Released &&
				Current == ButtonState.Pressed)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		// Convert key binding to string representing the name
		// TODO: THIS IS NOT LOCALIZED
		public string GetBinding(eBindings binding)
		{
			Keys k = m_Bindings[binding].m_Key;
			string name = Enum.GetName(typeof(Keys), k);
			if (k == Keys.OemPlus)
			{
				name = "+";
			}
			else if (k == Keys.OemMinus)
			{
				name = "-";
			}

			return name;
		}
	}
}
