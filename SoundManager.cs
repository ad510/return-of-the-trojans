//-----------------------------------------------------------------------------
// SoundManager maintains a list of cues and their corresponding files.
// This is a very bare bones way to play sound files.
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace itp380
{
	public class SoundManager : Patterns.Singleton<SoundManager>
	{
		Dictionary<string, SoundEffect> m_Sounds = new Dictionary<string, SoundEffect>();
        Dictionary<string, SoundEffectInstance> m_Music = new Dictionary<string, SoundEffectInstance>();

		public SoundManager()
		{

		}

		// Load the SFX
		public void LoadContent(ContentManager Content)
		{
			m_Sounds.Add("Shoot", Content.Load<SoundEffect>("Sounds/Shoot"));
			m_Sounds.Add("MenuClick", Content.Load<SoundEffect>("Sounds/MenuClick"));
            m_Sounds.Add("Build", Content.Load<SoundEffect>("Sounds/Build"));
            m_Sounds.Add("GameOver", Content.Load<SoundEffect>("Sounds/GameOver"));
            m_Sounds.Add("Victory", Content.Load<SoundEffect>("Sounds/Victory"));
            m_Sounds.Add("Error", Content.Load<SoundEffect>("Sounds/Error"));
            m_Sounds.Add("Snared", Content.Load<SoundEffect>("Sounds/Snared"));
            m_Sounds.Add("Alarm", Content.Load<SoundEffect>("Sounds/Alarm"));

			// TODO: Add any additional sounds here

            // Sound Effects
            m_Sounds.Add("Blaster", Content.Load<SoundEffect>("Sounds/storm_trooper_blaster"));
            m_Sounds.Add("ObiWan", Content.Load<SoundEffect>("Sounds/obi_wan_warning"));
            m_Sounds.Add("Vader_Breathing", Content.Load<SoundEffect>("Sounds/darth_vader_breathing"));
            m_Sounds.Add("Vader", Content.Load<SoundEffect>("Sounds/darth_vader_warning"));
            m_Sounds.Add("R2D2", Content.Load<SoundEffect>("Sounds/happy_r2d2_sound"));
            m_Sounds.Add("Scream", Content.Load<SoundEffect>("Sounds/clone_scream"));
            m_Sounds.Add("Explosion1", Content.Load<SoundEffect>("Sounds/explosion_1"));
            m_Sounds.Add("Explosion2", Content.Load<SoundEffect>("Sounds/explosion_2"));
            m_Sounds.Add("Explosion3", Content.Load<SoundEffect>("Sounds/explosion_3"));

            // Music
            m_Music.Add("Cantina", Content.Load<SoundEffect>("Sounds/song_cantina").CreateInstance());
            m_Music["Cantina"].Volume = 0.2f;
            m_Music.Add("Duel", Content.Load<SoundEffect>("Sounds/song_duel_of_fates").CreateInstance());
            m_Music["Duel"].Volume = 0.5f;
            m_Music.Add("StarWars", Content.Load<SoundEffect>("Sounds/song_star_wars_main").CreateInstance());
            m_Music["StarWars"].Volume = 0.5f;
            m_Music.Add("Imperial", Content.Load<SoundEffect>("Sounds/song_imperial_march").CreateInstance());
            m_Music["Imperial"].Volume = 0.5f;
            foreach (SoundEffectInstance music in m_Music.Values)
            {
                music.IsLooped = true;
            }
		}

		public void PlaySoundCue(string cue, float volume = 0.4f) { m_Sounds[cue].Play(volume, 0.0f, 0.0f); }

        public void PlayMusicCue(string cue) { m_Music[cue].Play(); }

        public void StopAllMusic() { foreach (SoundEffectInstance music in m_Music.Values) { music.Stop(); } }
	}
}
