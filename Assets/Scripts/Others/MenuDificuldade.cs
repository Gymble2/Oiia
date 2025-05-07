using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuDificuldade : MonoBehaviour
{
    public void SetNormal()
    {
        if (GameDifficultyManager.instance != null)
            GameDifficultyManager.instance.SetNormalMode();
    }

    public void SetHard()
    {
        if (GameDifficultyManager.instance != null)
            GameDifficultyManager.instance.SetHardMode();
    }

    public void IniciarJogo(string nomeCena)
    {
        SceneManager.LoadScene(nomeCena);
    }
}
