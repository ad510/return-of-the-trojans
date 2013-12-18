using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace itp380.Objects
{
    public class Unit : GameObject
    {
        public const float RotateSpeed = 2;
        public const float MoveSpeed = 10;
        public const float StrafeSpeed = 4;
        public const float UpDownSpeed = 4;
        public const float Radius = 1.5f;
        public const float MinAltitude = 5;
        public const float FlyTargetAltitude = 50;
        public const float EndFlyTargetAltitude = -10;
        public const float SpawnMaxTime = 0.5f;
        public const float SpawnAltitude = -3;
        public const float HipAltitude = 1.7f;
        public const float DieMaxTime = 0.5f;
        public const int MaxHealth = 5;
        //public const int MaxHealth = 100; // For Debugging
        public const int ShootDamage = 1;

        public readonly Vector3 LaserOffset = new Vector3(1.5f, 0.2f, 2.0f);
        public Vector3 LaserStartPos()
        {
            return Position + (Right * LaserOffset.X) + (Forward * LaserOffset.Y) + (Up * LaserOffset.Z);
        }

        GameObject[] m_Legs;
        eTeam m_Team;
        public eTeam Team
        {
            get { return m_Team; }
        }
        public float ShootTime;
        public Vector3 ShootHitPos;
        int m_Health;
        public int Health
        {
            get { return m_Health; }
            set { m_Health = value; }
        }
        float m_SpawnTime;
        float m_DieTime;
        float m_LegMoveAmt;
        public Vector2 LastXYPosition;
        public float LastAngle;

        public bool m_FlyingState; // for AI use only
        public bool FlyingState
        {
            get { return m_FlyingState; }
            set { m_FlyingState = value; }
        }

        bool m_Flying;
        public bool Flying
        {
            get { return m_Flying; }
            set
            {
                m_Flying = value;
                // change model
                if (!m_Flying)
                {
                    Scale = 1f;
                    if (m_Team == eTeam.Jedi) {
                        m_ModelName = "Clone/Clone_Body";
                        m_Legs[0].m_ModelName = "Clone/Clone_Leg_Left";
                        m_Legs[1].m_ModelName = "Clone/Clone_Leg_Right";
                    }
                    else {
                        m_ModelName = "Clone/Clone_Enemy_Leg_Body";
                        m_Legs[0].m_ModelName = "Clone/Clone_Enemy_Leg_Left";
                        m_Legs[1].m_ModelName = "Clone/Clone_Enemy_Leg_Right";
                    }
                    Position = new Vector3(Position.X, Position.Y, 0);
                    m_bTransformDirty = true;
                }
                else
                {
                    m_ModelName = "Terrains/ship/xwing";
                    Altitude = MinAltitude;
                    m_FlyTime = 0;
                    Scale *= .015f;
                    m_CustomTransform = Matrix.Identity;
                    m_bTransformDirty = true;
                }
                Load();
                foreach (GameObject leg in m_Legs)
                {
                    leg.Load();
                }
            }
        }
        public float Altitude, TargetAltitude;
        float m_FlyTime;
        public int Bombs = 3;
        
        public Vector3 Forward { get { return Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationZ(Angle)); } }
        public Vector3 Right { get { return Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationZ(Angle - (float)Math.PI / 2)); } }
        public Vector3 Up { get { return Vector3.Cross(Right, Forward); } }

        // AI variables
        public Node CurrentNode;
        public float TimeUntilPathExpire;
        private Pathfind.Request PathfindRequest;

        public Unit(Game game, eTeam team) : base(game)
        {
            m_Legs = new GameObject[2];
            for (int i = 0; i < m_Legs.Length; i++)
            {
                m_Legs[i] = new GameObject(game);
            }
            m_Team = team;
            m_ModelRotation = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2), Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)Math.PI));
            ShootTime = 10000;
            m_Health = MaxHealth;
            m_SpawnTime = 0;
            m_DieTime = 0;
            TimeUntilPathExpire = 0;
            Flying = false;
            m_FlyingState = false;
            foreach (GameObject leg in m_Legs)
            {
                GameState.Get().SpawnGameObject(leg);
            }
        }

        public override void Update(float fDeltaTime)
        {
            if (m_Health > 0)
            {
                // note: collision detection is done in GameState.UpdateGameplay() instead of here because it helps to access units by index
                if (this != GameState.Get().Player)
                {
                    // AI for units other than player
                    Unit target = null;
                    Vector3 targetPos = Position;
                    // find closest enemy unit
                    foreach (Unit unit in GameState.Get().Units)
                    {
                        if (Team != unit.Team && !unit.Flying && (target == null || DistanceSquared2D(Position, unit.Position) < DistanceSquared2D(Position, target.Position)))
                        {
                            target = unit;
                        }
                    }
                    TimeUntilPathExpire -= fDeltaTime;
                    if (target != null && DistanceSquared2D(Position, target.Position) < Building.MinRadius * 2 * Building.MinRadius * 2)
                    {
                        // close to closest enemy unit, so go directly there
                        targetPos = target.Position;
                        // shooting when flying
                        // (this is handled here instead of below because we only want to drop bombs over units, not intermediate waypoints)
                        if (FlyingState && DistanceSquared2D(Position, targetPos) < Missile.HitRadius * Missile.HitRadius)
                        {
                            Shoot();
                        }
                    }
                    else
                    {
                        // otherwise, pathfind to target position
                        Pathfind pathfind = GameState.Get().m_Pathfind;
                        if (target != null && (CurrentNode == null || TimeUntilPathExpire <= 0))
                        {
                            if (PathfindRequest == null)
                            {
                                PathfindRequest = new Pathfind.Request(pathfind.BoundedGridPos(pathfind.WorldSpaceToGridRound(Position)), pathfind.BoundedGridPos(pathfind.WorldSpaceToGridRound(target.Position)), pathfind);
                            }
                            else if (PathfindRequest.Done.WaitOne(0))
                            {
                                Node StartNode = PathfindRequest.StartNode;
                                PathfindRequest = null;
                                if (StartNode != null && StartNode.Parent != null)
                                {
                                    CurrentNode = StartNode.Parent;
                                }
                                TimeUntilPathExpire = 5 + 5 * (float)GameState.Get().m_Random.NextDouble();
                            }
                        }
                        if (CurrentNode != null)
                        {
                            targetPos = pathfind.GridToWorldSpace(CurrentNode.GridPos3);
                            if (DistanceSquared2D(Position, targetPos) < pathfind.GridSize / 2 * pathfind.GridSize / 2)
                            {
                                CurrentNode = CurrentNode.Parent;
                            }
                        }
                    }
                    // move towards target position
                    if (targetPos != Position)
                    {
                        Vector3 diffNormalized = new Vector3(targetPos.X, targetPos.Y, 0) - new Vector3(Position.X, Position.Y, 0);
                        diffNormalized.Normalize();
                        Angle -= RotateSpeed * fDeltaTime * Vector3.Dot(diffNormalized, Right);
                        Position += Forward * MoveSpeed * fDeltaTime * Vector3.Dot(diffNormalized, Forward); // go forward
                    }
                    // shooting when not flying
                    if (!FlyingState && ShootCollision() != null)
                    {
                        Shoot();
                    }
                }
                if (m_SpawnTime < SpawnMaxTime)
                {
                    // unit spawning animation
                    m_SpawnTime += fDeltaTime;
                    if (m_SpawnTime > SpawnMaxTime)
                    {
                        m_SpawnTime = SpawnMaxTime;
                    }
                    Position = new Vector3(Position.X, Position.Y, SpawnAltitude * (SpawnMaxTime - m_SpawnTime) / SpawnMaxTime);
                }
                if (Flying)
                {
                    // handle flying
                    m_FlyTime += fDeltaTime;
                    while (m_FlyTime >= 0.02f)
                    {
                        Altitude += (TargetAltitude - Altitude) / 200;
                        m_FlyTime -= 0.02f;
                    }
                    Position = new Vector3(Position.X, Position.Y, Altitude);
                    if (Altitude < MinAltitude)
                    {
                        Flying = false;
                    }
                }
                if (FlyingState && !Flying)
                {
                    Flying = true;
                    TargetAltitude = Objects.Unit.FlyTargetAltitude;
                }
                if (Flying && TargetAltitude != Objects.Unit.EndFlyTargetAltitude)
                {
                    bool foundAnotherUnitOnTeam = false;
                    foreach (Unit unit in GameState.Get().Units)
                    {
                        if (this != unit && unit.Team == Team)
                        {
                            foundAnotherUnitOnTeam = true;
                            break;
                        }
                    }
                    if (Bombs <= 0 || !foundAnotherUnitOnTeam)
                    {
                        TargetAltitude = Objects.Unit.EndFlyTargetAltitude;
                        if (m_Team == eTeam.Sith)
                        {
                            GameState.Get().m_SithXWing = false;
                        }
                        FlyingState = false;
                    }
                }
            }
            else if (m_DieTime < DieMaxTime)
            {
                // unit is dead, so fall down
                m_DieTime += fDeltaTime;
                if (m_DieTime > DieMaxTime)
                {
                    m_DieTime = DieMaxTime;
                }
                m_CustomTransform = Matrix.CreateRotationX(m_DieTime / DieMaxTime * (float)Math.PI / 2);
                m_bTransformDirty = true;
                m_LegMoveAmt = 0;
            }
            // animate legs
            if (Flying)
            {
                m_LegMoveAmt = 0;
            }
            else
            {
                Vector2 XYPosition = new Vector2(Position.X, Position.Y);
                if (XYPosition != LastXYPosition)
                {
                    m_LegMoveAmt += Vector2.Distance(LastXYPosition, XYPosition);
                }
                else if (Angle != LastAngle)
                {
                    m_LegMoveAmt += 3 * Math.Abs(Angle - LastAngle);
                }
                else
                {
                    if (m_LegMoveAmt > (float)Math.PI / 2)
                    {
                        m_LegMoveAmt = (float)Math.PI - m_LegMoveAmt;
                    }
                    else if (m_LegMoveAmt < -(float)Math.PI / 2)
                    {
                        m_LegMoveAmt = -(float)Math.PI - m_LegMoveAmt;
                    }
                    float LastLegMoveAmt = m_LegMoveAmt;
                    m_LegMoveAmt = (Math.Abs(m_LegMoveAmt) - 5 * fDeltaTime) * Math.Sign(m_LegMoveAmt);
                    if (Math.Sign(m_LegMoveAmt) != Math.Sign(LastLegMoveAmt))
                    {
                        m_LegMoveAmt = 0;
                    }
                }
                while (m_LegMoveAmt > Math.PI)
                {
                    m_LegMoveAmt -= 2 * (float)Math.PI;
                }
                LastXYPosition = XYPosition;
                LastAngle = Angle;
            }
            UpdateLegs();
            base.Update(fDeltaTime);
        }

        private float DistanceSquared2D(Vector3 value1, Vector3 value2)
        {
            return Vector2.DistanceSquared(new Vector2(value1.X, value1.Y), new Vector2(value2.X, value2.Y));
        }

        public void UpdateLegs()
        {
            foreach (GameObject leg in m_Legs)
            {
                if (Flying)
                {
                    leg.Enabled = false;
                }
                else
                {
                    leg.Enabled = true;
                    leg.m_ModelRotation = m_ModelRotation;
                    leg.m_CustomTransform = Matrix.CreateTranslation(new Vector3(0, 0, -HipAltitude)) * Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.7f * (float)Math.Sin(m_LegMoveAmt) * (leg == m_Legs[0] ? 1 : -1)) * Matrix.CreateTranslation(new Vector3(0, 0, HipAltitude)) * m_CustomTransform;
                    leg.Angle = Angle;
                    leg.Position = Position;
                }
            }
        }

        public void Shoot()
        {
            if (!Reloaded())
            {
                return;
            }
            ShootTime = 0;
            if (this == GameState.Get().Player)
            {
                GameState.Get().StartVibration();
            }
            if (Flying)
            {
                if (Bombs > 0)
                {
                    // flying, so release missile
                    GameState.Get().SpawnMissile(m_Team, Position);
                    Bombs--;
                }
            }
            else
            {
                // not flying, so shoot laser
                // check if hit anyone
                Unit hit = ShootCollision(out ShootHitPos);
                if (hit != null)
                {
                    hit.TakeHealth(ShootDamage);
                }
            }
            SoundManager.Get().PlaySoundCue("Blaster", 0.2f);
        }

        public Unit ShootCollision()
        {
            Vector3 v;
            return ShootCollision(out v);
        }

        public Unit ShootCollision(out Vector3 hitPos)
        {
            Nullable<float> distToBuilding = null;
            Unit ret = null;
            float hitDist = 100;
            hitPos = CalcHitPos(hitDist);
            if (Flying)
            {
                return null;
            }
            foreach (Objects.Building building in GameState.Get().Buildings)
            {
                Nullable<float> dist = new Ray(LaserStartPos(), Forward).Intersects(building.Box());
                if (dist != null && (distToBuilding == null || dist < distToBuilding))
                {
                    distToBuilding = dist;
                    hitDist = (float)dist;
                }
            }
            foreach (Unit unit in GameState.Get().Units)
            {
                if (unit != this && unit.Team != Team && !unit.Flying // check if allowed to hit them
                    && Math.Abs(Vector3.Dot(unit.Position - Position, Right)) < Radius // check sideways intersection
                    && Vector3.Dot(unit.Position - Position, Forward) >= 0 // check forward intersection
                    && (distToBuilding == null || Vector3.Dot(unit.Position - Position, Forward) < distToBuilding) // check closer than building
                    && (ret == null || Vector3.DistanceSquared(unit.Position, Position) < Vector3.DistanceSquared(ret.Position, Position))) // check closer to current closest unit
                {
                    ret = unit;
                    hitDist = Vector3.Dot(ret.Position - Position, Forward);
                }
            }
            hitPos = CalcHitPos(hitDist);
            return ret;
        }

        private Vector3 CalcHitPos(float hitDist)
        {
            return Position + Forward * hitDist + Up * LaserOffset.Z;
        }

        public bool Reloaded()
        {
            return ShootTime >= Lasers.ReloadTime;
        }

        // returns whether died
        public bool TakeHealth(int amount)
        {
            m_Health -= amount;
            if (m_Health <= 0)
            {
                GameState.Get().RemoveUnit(this);
                return true;
            }
            return false;
        }
    }
}
