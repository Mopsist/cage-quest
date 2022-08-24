using System;
using System.Media;

namespace CageQuest
{
    public class SoundsService
    {
        private SoundPlayer _player;

        public SoundsService()
        {
            _player = new SoundPlayer();
            _player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "Resources\\scott-buckley-i-walk-with-ghosts.wav";
        }

        public void PlayBackgroundMusic()
        {
            _player.PlayLooping();
        }

        public void StopBackgroundMusic()
        {
            _player.Stop();
        }
    }
}
