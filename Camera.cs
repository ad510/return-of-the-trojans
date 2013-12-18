//-----------------------------------------------------------------------------
// Camera Singleton that for now, doesn't do much.
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

namespace itp380
{
	public class Camera
	{
        const float fMaxDeltaTime = 0.02f;

		Game m_Game;
		
		Matrix m_Camera;
		public Matrix CameraMatrix { get { return m_Camera; } }

        public enum CameraState
        {
            isLevelEditor = 0, isStartingIntroZoom, isIntroZoom,
            isGame, isStartingGameZoomIn, isGameZoomIn, isStartingGameZoomOut, isGameZoomOut,
            isFlight, isStartingFlight,
        }
        public CameraState cameraState;

        public void SetupCameraForGameplay() { cameraState = CameraState.isStartingIntroZoom; ClearData(); }
        public void SetupCameraForLevelEditor() { cameraState = CameraState.isLevelEditor; ClearData(); }

		public Camera(Game game)
		{
			m_Game = game;
            if (GameState.Get().State == eGameState.Gameplay)
            {
                ComputeMatrix(new Vector3(fHDistZoom, 0, fVDistZoom), Vector3.Zero, new Vector3(0, 0, -1.0f)); // Gameplay Initial Camera
            }
            else if (GameState.Get().State == eGameState.LevelEditor)
            {
                ComputeMatrix(Vector3.Zero, Vector3.Zero, Vector3.Zero); // Level Editor Initial Camera
            }
		}

        // Player Data
        Objects.Unit player; Vector3 playerForward, playerUp, playerPos, playerRight;

        Vector3 playerSkewUp = new Vector3(0, 0, 3.0f);
        //Vector3 playerSkewUp = new Vector3(0, 0, 4.50f);

        // Camera Data
        public Vector3 cameraPos, cameraForward, cameraLeft, cameraUp;
        public Vector3 cameraTarget, controllerAim = Vector3.Zero;
        Vector3 cameraOriginalPos, cameraFinalPos;

        // Universal Data
        float curTime = 1.0f;
        /* Adjustment Code for Not Full Zoom In curTime = 1.0f - curTime; instead of: curTime = 0.0f; Corrects Overshooting If Zoom Start/Stop Early */
        // Reticle Data
        float reticleScaleMax = 1.0f, reticleScaleMin = 0.4f;
        public float reticleScale = 0.0f;
        
        void ClearData() { curTime = 1.0f; }

        // Spring Follow Camera
        Vector3 vDisplacement = Vector3.Zero, vSpringAccel = Vector3.Zero, vCameraVelocity = Vector3.Zero;
        float fSpringConstant = 1000.0f, fDampConstant;


        // Camera Position for Orthographic to Perspective Zoom
        float fHDistZoom = 60.0f, fVDistZoom = 100.0f; 
        float fIntroZoomFactor = 0.225f;
        Vector3 OrthographicCameraPosition(Vector3 position, Vector3 forward, Vector3 up)
        {
            return (position - forward * fHDistZoom + up * fVDistZoom);
        }


        // Camera Position for 3D Fixed Camera
        float distHorzForward = 5.0f;
        float distHorzCamera = 5.0f, distVertCamera = 2.0f;
        Vector3 Follow3DCameraPosition(Vector3 position, Vector3 forward, Vector3 up)
        {
            //return new Vector3(0, 0, 150); // uncomment to enable spectator camera
            return position - forward * distHorzCamera + playerUp * distVertCamera;
        }


        // Camera Position for 3D Fixed Flying Camera
        float fFlightZoomFactor = 0.500f;
        float distHorzCameraFly = 12.0f, distVertCameraFly = 12.0f;
        Vector3 Flying3DCameraPosition(Vector3 position, Vector3 forward, Vector3 up)
        {
            return position - forward * distHorzCameraFly + playerUp * distVertCameraFly - flightSkewDown;
        }


        // Camera Position for Zoom In To Scope
        float zoomToForwardFactor = 0.80f, zoomToRightFactor = 0.90f, zoomToUpFactor = 0.20f; 
        float fGameZoomFactor = 3.00f;
        Vector3 flightSkewDown = new Vector3(0.0f, 0.0f, 3.0f);
        Vector3 ZoomToScopeCameraPosition(Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
        {
            return position - forward * zoomToForwardFactor + right * zoomToRightFactor + up * zoomToUpFactor;
        }

    
        // Camera Intersection With Building Adjustment
        BoundingBox cameraBounds;
        Vector3 BuildingCameraCollisionAdjustment()
        {
            cameraBounds = new BoundingBox(cameraPos + new Vector3(-4, -4, -4), cameraPos + new Vector3(4, 4, 4));

            List<Objects.Building> Buildings = GameState.Get().Buildings;
            foreach (Objects.Building building in Buildings) { if (building.Box().Intersects(cameraBounds)) { return new Vector3(0.5f, -0.5f, 1.50f); } }
            return Vector3.Zero;
        }


		public void Update(float fDeltaTime)
		{
            // Player Data
            player = GameState.Get().Player;
            playerForward = player.Forward;
            playerUp = player.Up;
            playerRight = player.Right;
            // Take Player Position as Position of Head
            playerPos = player.Position + playerSkewUp;


            // Camera Positions Based On Game States
            if(cameraState == CameraState.isIntroZoom || cameraState == CameraState.isStartingIntroZoom)
            {
                if(cameraState == CameraState.isStartingIntroZoom) { curTime = 1.0f - curTime; cameraState = CameraState.isIntroZoom; }
                curTime += fIntroZoomFactor * fDeltaTime;
                if (curTime >= 1.0f) { curTime = 1.0f; cameraState = CameraState.isGame; }

                cameraOriginalPos = OrthographicCameraPosition(playerPos, playerForward, playerUp);
                cameraFinalPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);
                cameraPos = Vector3.Lerp(cameraOriginalPos, cameraFinalPos, curTime);

                // Other
                reticleScale = 0.0f; 
            }

            else if (cameraState == CameraState.isFlight || cameraState == CameraState.isStartingFlight)
            {
                if (cameraState == CameraState.isStartingFlight) { curTime = 1.0f - curTime; cameraState = CameraState.isFlight; }
                curTime += fFlightZoomFactor * fDeltaTime;
                if (curTime >= 1.0f) { curTime = 1.0f; }

                cameraOriginalPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);
                Vector3 flying = new Vector3(3, 0, 0);
                cameraFinalPos = Flying3DCameraPosition(playerPos-flying, playerForward, playerUp);
                cameraPos = Vector3.Lerp(cameraOriginalPos, cameraFinalPos, curTime);

                // Other
                if (!player.Flying) { cameraState = CameraState.isGame; }
                reticleScale = 0.0f;
            }
            else if (cameraState == CameraState.isGameZoomIn || cameraState == CameraState.isStartingGameZoomIn)
            {
                if (cameraState == CameraState.isStartingGameZoomIn) { curTime = 1.0f - curTime; cameraState = CameraState.isGameZoomIn; }
                curTime += fGameZoomFactor * fDeltaTime;
                if (curTime >= 1.0f) { curTime = 1.0f; }

                cameraOriginalPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);
                cameraFinalPos = ZoomToScopeCameraPosition(playerPos, playerForward, playerUp, playerRight);
                cameraPos = Vector3.Lerp(cameraOriginalPos, cameraFinalPos, curTime);

                // Other
                reticleScale = reticleScaleMin + (reticleScaleMax - reticleScaleMin) * curTime;
            }
            else if (cameraState == CameraState.isGameZoomOut || cameraState == CameraState.isStartingGameZoomOut)
            {
                if (cameraState == CameraState.isStartingGameZoomOut) { curTime = 1.0f - curTime; cameraState = CameraState.isGameZoomOut; }
                curTime += fGameZoomFactor * fDeltaTime;
                if (curTime >= 1.0f) { curTime = 1.0f; cameraState = CameraState.isGame; }

                cameraOriginalPos = ZoomToScopeCameraPosition(playerPos, playerForward, playerUp, playerRight);
                cameraFinalPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);
                cameraPos = Vector3.Lerp(cameraOriginalPos, cameraFinalPos, curTime);

                // Other
                reticleScale = reticleScaleMax + (reticleScaleMin - reticleScaleMax) * curTime;
            }
            else if (cameraState == CameraState.isGame)
            {
                float fDeltaTime2 = fDeltaTime;

                cameraOriginalPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);

                do
                {
                    // Spring Follow Camera
                    float fDeltaTime3 = Math.Min(fDeltaTime2, fMaxDeltaTime);
                    vDisplacement = cameraPos - cameraOriginalPos;
                    fDampConstant = 2.0f * (float)Math.Sqrt((double)fSpringConstant);
                    vSpringAccel = (-fSpringConstant * vDisplacement) - fDampConstant * vCameraVelocity;
                    vCameraVelocity += vSpringAccel * fDeltaTime3;
                    cameraPos += vCameraVelocity * fDeltaTime3;
                    fDeltaTime2 -= fMaxDeltaTime;
                } while (fDeltaTime2 > fMaxDeltaTime); // update spring camera in multiple intervals if game lags

                // Other 
                reticleScale = reticleScaleMin;
                if (player.Flying) { reticleScale = 0.0f; }
            }
            else if (cameraState == CameraState.isLevelEditor)
            {
                cameraPos = Follow3DCameraPosition(playerPos, playerForward, playerUp);

                // Other
                reticleScale = 0.0f;
            }


            // Adjustment For Building Camera Collision
            cameraPos += BuildingCameraCollisionAdjustment();


            // Camera Data With Input From Controller/Mouse
            cameraTarget = playerPos + playerForward * distHorzForward + controllerAim;

            cameraForward = Vector3.Normalize(cameraTarget - cameraPos);
            cameraLeft = Vector3.Cross(playerUp, cameraForward);
            cameraUp = Vector3.Cross(cameraForward, cameraLeft);

            ComputeMatrix(cameraPos, cameraTarget, cameraUp);
		}


        public void ToggleFlightCamera()
        {
            if (cameraState == CameraState.isStartingFlight || cameraState == CameraState.isFlight)
            {
                cameraState = CameraState.isGame;
            }
            else
            {
                cameraState = CameraState.isStartingFlight;
            }
        }


        public void ZoomIn()
        {
            if (cameraState != CameraState.isFlight && cameraState != CameraState.isStartingFlight)
            {
                if (cameraState == CameraState.isGame || cameraState == CameraState.isGameZoomOut
                    || cameraState == CameraState.isStartingGameZoomOut)
                {
                    cameraState = CameraState.isStartingGameZoomIn;
                }
            }
        }
        public void ZoomOut()
        {
            if (cameraState != CameraState.isFlight && cameraState != CameraState.isStartingFlight)
            {
                if (cameraState == CameraState.isStartingGameZoomIn || cameraState == CameraState.isGameZoomIn)
                {
                    cameraState = CameraState.isStartingGameZoomOut;
                }
            }
        }

        // Camera Matrix
		void ComputeMatrix(Vector3 cameraPosition, Vector3 targetPosition, Vector3 cameraUpVector)
		{
            m_Camera = Matrix.CreateLookAt(cameraPosition, targetPosition, cameraUpVector);
		}
	}
}
