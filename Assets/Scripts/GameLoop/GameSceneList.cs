using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneList
{
    private static readonly string[] GameScenes = new string[]
    {
        "Worm League",
        "GameScene - default",
    };
    
    public static string GetRandomGameScene()
    {
        string scene = GameScenes[Random.Range(0, GameScenes.Length)];
        return scene;
    }
    
    public static bool IsSceneAGameScene(string sceneName)
    {
        foreach (string scene in GameScenes)
        {
            if (scene == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}