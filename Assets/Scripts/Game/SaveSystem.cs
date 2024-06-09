using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void Save()
    {
        string path = Path.Combine(Application.persistentDataPath, Constants.Resources.PlayerDataFileName);
        string json = GetPlayerJSON();
        File.WriteAllTextAsync(path, json);
    }

    public static string GetPlayerJSON() => GetPlayerJSON(ProjectContext.Instance.Player);

    public static string GetPlayerJSON(PlayerData playerData)
    {
        PlayerSerializationData serializationData = new PlayerSerializationData(playerData);
        return JsonUtility.ToJson(serializationData);
    }

    public static PlayerData Load()
    {
        string path = Path.Combine(Application.persistentDataPath, Constants.Resources.PlayerDataFileName);

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var playerSerializationData =
                    (PlayerSerializationData)JsonUtility.FromJson(json, typeof(PlayerSerializationData));

                return playerSerializationData.Deserialize();
            }
            catch
            {
                Debug.LogError("Player playerData is corrupted and unable to load.");
            }  
        }

        return null;
    }

    
}
