﻿//-----------------------------------------------------------------------------
// Base GameObject class that every other class in the Objects namespace
// inherits from. It's both drawable and updatable.
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

namespace itp380
{
	public class GameObject
	{
		protected Game m_Game;
		public Model m_Model;
		protected Matrix[] m_ModelBones;
		public string m_ModelName;
        public string name = System.String.Empty;
        public int style = 1;

		protected Matrix m_WorldTransform = Matrix.Identity;
		protected bool m_bTransformDirty = false;

		protected eDrawOrder m_DrawOrder = eDrawOrder.Default;
		public eDrawOrder DrawOrder
		{
			get { return m_DrawOrder; }
		}

		// Anything that's timer logic is assumed to be affected by time factor
		protected Utils.Timer m_Timer = new Utils.Timer();

		float m_fAngle = 0.0f;
		public float Angle
		{
			get { return m_fAngle; }
			set { m_fAngle = value; m_bTransformDirty = true; }
		}


		Vector3 m_vPos = Vector3.Zero;
		public Vector3 Position
		{
			get { return m_vPos; }
			set { m_vPos = value; m_bTransformDirty = true; }
		}

        Vector3 m_vVel = Vector3.Zero;
        public Vector3 Velocity
        {
            get { return m_vVel; }
            set { m_vVel = value; /*m_bTransformDirty = true;*/ }
        }


		float m_fScale = 1.0f;
		public float Scale
		{
			get { return m_fScale; }
			set { m_fScale = value; m_bTransformDirty = true; }
		}

        public Quaternion m_ModelRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), 0);

        public Matrix m_CustomTransform = Matrix.Identity; // must manually set m_bTransformDirty = true after changing this

		public void RebuildWorldTransform()
		{
			m_bTransformDirty = false;
			// Scale, rotation, translation
			m_WorldTransform = Matrix.CreateFromQuaternion(m_ModelRotation) * Matrix.CreateScale(m_fScale) * m_CustomTransform *
				Matrix.CreateRotationZ(m_fAngle) * Matrix.CreateTranslation(m_vPos);
		}

		public bool m_bEnabled = true;
		public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; }
		}

		public GameObject(Game game)
		{
			m_Game = game;
		}

		public virtual void Load()
		{
			if (m_ModelName != "")
			{
				m_Model = m_Game.Content.Load<Model>(m_ModelName);
				m_ModelBones = new Matrix[m_Model.Bones.Count];
				m_Model.CopyAbsoluteBoneTransformsTo(m_ModelBones);
			}

			RebuildWorldTransform();
		}

		public virtual void Unload()
		{

		}

		public virtual void Update(float fDeltaTime)
		{


			if (m_bTransformDirty)
			{
				RebuildWorldTransform();
			}

			m_Timer.Update(fDeltaTime);
		}

        public string getName()
        {
            return m_ModelName;
        }

		public virtual void Draw(float fDeltaTime)
		{
			if (m_bTransformDirty)
			{
				RebuildWorldTransform();
			}

			foreach (ModelMesh mesh in m_Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.World = m_ModelBones[mesh.ParentBone.Index] * m_WorldTransform;
					effect.View = GameState.Get().CameraMatrix;
					effect.Projection = GraphicsManager.Get().Projection;
					effect.EnableDefaultLighting();
					effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.Enabled = false;
					effect.DirectionalLight1.Enabled = false;
					effect.DirectionalLight2.Enabled = false;
					effect.PreferPerPixelLighting = true;
				}
				mesh.Draw();
			}
		}
	}
}
