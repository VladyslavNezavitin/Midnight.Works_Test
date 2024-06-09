using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Initialize();
    }

    private void Initialize()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance.gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerData player = SaveSystem.Load();
        
        if (player == null)
        {
            List<CarConfig> defaultCars = ProjectContext.Instance.DefaultCars;
            player = new PlayerData("Unnamed", 100000, 100000, new CarCollection(defaultCars, defaultCars[0])); 
        }

        ProjectContext.Instance.Initialize(player);

        SaveSystem.Save();
        ProjectContext.Instance.SceneLoader.LoadSceneAsync(Constants.Scenes.MainMenu);
    }
}
