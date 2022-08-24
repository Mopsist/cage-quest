using CageQuest.Core;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CageQuest.Services
{
    public class SavesService
    {
        public void SaveProgress(PlayerSession currentSession)
        {
            //maybe delete "WriteIndented = true" option after development
            var jsonProgress = JsonSerializer.Serialize(currentSession, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText("Resources/saved-progress.json", jsonProgress);
        }

        public PlayerSession LoadSavedGame()
        {
            PlayerSession savedGame = null;
            try
            {
                var path = "Resources/saved-progress.json";
                if (File.Exists(path))
                {
                    var jsonString = File.ReadAllText(path);
                    var playerSession = JsonSerializer.Deserialize<PlayerSession>(jsonString);
                    playerSession.Path = new Stack<Step>(playerSession.Path);

                    savedGame = playerSession;
                }
            }
            catch
            {
                Message.Info("Ошибка при загрузке сохранения :(");
            }

            return savedGame;
        }
    }
}
