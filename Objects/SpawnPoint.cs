using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace itp380.Objects
{
    public class SpawnPoint : GameObject
    {
        public eTeam team;
        public SpawnPoint(Game game, eTeam t)
            : base(game)
        {
            team = t;
            name = "SpawnPoint"+t;
        }
    }
}
