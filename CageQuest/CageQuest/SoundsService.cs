using System;
using System.Media;
using NAudio.Wave;

namespace CageQuest
{
    public class SoundsService
    {
        private SoundPlayer _player;

        public SoundsService()
        {
            var mp3Path = AppDomain.CurrentDomain.BaseDirectory + "Resources\\scott-buckley-i-walk-with-ghosts.mp3";
            var wavPath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\scott-buckley-i-walk-with-ghosts.wav";
            ConvertMp3ToWav(mp3Path, wavPath);

            _player = new SoundPlayer();
            _player.SoundLocation = wavPath;
        }

        public void PlayBackgroundMusic()
        {
            _player.PlayLooping();
        }

        public void StopBackgroundMusic()
        {
            _player.Stop();
        }

        private static void ConvertMp3ToWav(string _inPath_, string _outPath_)
        {
            using var mp3 = new Mp3FileReader(_inPath_);
            using var pcm = WaveFormatConversionStream.CreatePcmStream(mp3);
            WaveFileWriter.CreateWaveFile(_outPath_, pcm);
        }
    }
}
