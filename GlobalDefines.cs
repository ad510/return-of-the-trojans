﻿//-----------------------------------------------------------------------------
// These defines don't affect the balance of the game, but change things like
// the graphics parameters and camera speed.
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

namespace itp380
{
	public static class GlobalDefines
	{
        public const int iMouseCursorSize = 32;
        public const float fMouseDefaultSpeed = 1.2f;
        public const float fCameraZoom = 20.0f;

        public const bool bVSync = true;

        // Enable For Full Screen Later
        public const bool bFullScreen = true;

		// Windowed resolution -- in full screen mode, it automatically
		// selects the desktop resolution.
        public const int WindowedWidth = 1024;
        public const int WindowHeight = 768;
	}
}
