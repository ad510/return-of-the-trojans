using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace itp380
{
    public class LevelEditor : itp380.Patterns.Singleton<LevelEditor>
    {
        List<GameObject> m_GameObjects = new List<GameObject>();
        public LevelEditor()
        {

        }

        public void Start()
        {
        }

        public void addObject(GameObject g)
        {
            m_GameObjects.Add(g);
        }

        public void saveObjects()
        {
            FileStream fs = new FileStream("Content/Terrains/level_1.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < m_GameObjects.Count(); i++)
            {
                GameObject sp = m_GameObjects.ElementAt(i);
                sw.WriteLine(sp.name);
                sw.WriteLine(sp.Position.X);
                sw.WriteLine(sp.Position.Y);
                sw.WriteLine(sp.Position.Z);
                sw.WriteLine(sp.style);

            }

            sw.Close();
            fs.Close();
        }

        public void loadObjects()
        {
            string[] items = new string[100];
            int counter = 0;
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader("Content/Terrains/level_1.txt");
            while ((line = file.ReadLine()) != null)
            {
                items[counter] = (line);
                counter++;
            }
            file.Close();
            for (int i = 0; i < counter; i++)
            {
                if (items[i].CompareTo("Building") == 0)
                {
                    float x = float.Parse(items[i+1], System.Globalization.CultureInfo.InvariantCulture);
                    float y = float.Parse(items[i+2], System.Globalization.CultureInfo.InvariantCulture);
                    float z = float.Parse(items[i+3], System.Globalization.CultureInfo.InvariantCulture);
                    float style = float.Parse(items[i + 4], System.Globalization.CultureInfo.InvariantCulture);
                    int type = (int)style;
                    GameState.Get().SpawnBuilding(new Vector3(x, y, z), type);

                }
            }
            
        }
    }
}
