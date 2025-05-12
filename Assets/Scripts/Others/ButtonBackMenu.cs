using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBackMenu : MonoBehaviour
{
    public void BackMenu()
    {
        SceneManager.LoadScene("Menu_Begin");
    }
}
