using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonRestart : MonoBehaviour
{
    public GameObject menu;
    // Este método pode ser chamado pelo evento OnClick do botão de restart
    public void RestartLevel()
    {
        if (menu != null)
            menu.SetActive(false); // Esconde todo o Canvas do menu
        // Carrega a próxima cena na ordem de build
        SceneManager.LoadScene("Scenes/SampleScene");
    }
}
