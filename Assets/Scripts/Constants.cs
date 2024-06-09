using System.IO;

public static class Constants
{
    public static class Scenes
    {
        public const string MainMenu = "MainMenu";
    }

    public static class Tags
    {
        public const string Player = "Player";
    }

    public static class Resources
    {
        public static readonly string CarDataFolder;
        public static readonly string CarPrefabFolder;
        public const string PlayerDataFileName = "playerData.json";

        static Resources()
        {
            CarDataFolder = Path.Combine("Cars", "Data");
            CarPrefabFolder = Path.Combine("Cars", "Prefabs");
        }
    }
}