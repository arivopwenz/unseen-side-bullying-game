using System.IO;
using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.Save
{
    [System.Serializable]
    public class GameSaveData
    {
        public int currentChapter = 1;
        public POVType currentPOV = POVType.Victim;
        public int empathyScore = 0;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public GameSaveData CurrentData { get; private set; } = new GameSaveData();

        private string saveFilePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            LoadGame();
        }

        public void SaveGame()
        {
            string json = JsonUtility.ToJson(CurrentData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game tersimpan di: {saveFilePath}");
        }

        public void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                CurrentData = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("Save data berhasil dimuat!");
            }
            else
            {
                CurrentData = new GameSaveData();
            }
        }

        public void AddEmpathyScore(int amount)
        {
            CurrentData.empathyScore += amount;
            SaveGame();
        }
    }
}
