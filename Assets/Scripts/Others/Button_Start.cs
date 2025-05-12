using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Button_Start : MonoBehaviour
{
    public GameObject menuBegin; // Referência ao Canvas principal
    public Animator buttonAnimator;

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Pressed");
            // Aguarda o tempo da animação (ajuste conforme necessário)
            yield return new WaitForSeconds(0.5f);
        }
        if (menuBegin != null)
            menuBegin.SetActive(false); // Esconde todo o Canvas do menu
        // Carrega a próxima cena na ordem de build
        SceneManager.LoadScene("Scenes/SampleScene");
    }
}
