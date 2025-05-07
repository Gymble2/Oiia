using UnityEngine;

public enum GameDifficulty { Normal, Hard }

public class GameDifficultyManager : MonoBehaviour
{
    public static GameDifficultyManager instance;
    public GameDifficulty difficulty = GameDifficulty.Normal;
    public float hardMultiplier = 1.5f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetMultiplier()
    {
        return difficulty == GameDifficulty.Hard ? hardMultiplier : 1f;
    }

    public void SetHardMode()
    {
        difficulty = GameDifficulty.Hard;
    }

    public void SetNormalMode()
    {
        difficulty = GameDifficulty.Normal;
    }
}
