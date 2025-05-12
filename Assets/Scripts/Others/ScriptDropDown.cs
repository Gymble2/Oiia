using UnityEngine;
using UnityEngine.UI; // ou TMPro se usar TMP_Dropdown

public class DificuldadeDropdown : MonoBehaviour
{
    public void OnDropdownChanged(int value)
    {
        if (GameDifficultyManager.instance == null) return;
        if (value == 0)
            GameDifficultyManager.instance.SetNormalMode();
        else if (value == 1)
            GameDifficultyManager.instance.SetHardMode();
    }
}