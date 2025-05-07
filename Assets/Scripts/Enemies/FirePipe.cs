using UnityEngine;

public class FirePipe : MonoBehaviour
{
    [Header("Fogo")]
    public GameObject fogoAtivo; // Arraste aqui o GameObject do fogo (pode ser um filho)
    public float tempoFogoAtivo = 3f;
    public float tempoFogoDesligado = 5f;

    private float timer = 0f;
    private bool fogoLigado = false;

    void Start()
    {
        float mult = GameDifficultyManager.instance != null ? GameDifficultyManager.instance.GetMultiplier() : 1f;
        tempoFogoAtivo /= mult; // fogo fica ativo menos tempo se mais difícil
        tempoFogoDesligado /= mult; // intervalo menor se mais difícil
        if (mult > 1f && fogoAtivo != null)
            fogoAtivo.transform.localScale *= mult; // aumenta o tamanho do fogo
        timer = tempoFogoDesligado;
        if (fogoAtivo != null)
            fogoAtivo.SetActive(false);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (fogoLigado)
        {
            if (timer <= 0f)
            {
                // Desliga o fogo
                if (fogoAtivo != null)
                    fogoAtivo.SetActive(false);
                fogoLigado = false;
                timer = tempoFogoDesligado;
            }
        }
        else
        {
            if (timer <= 0f)
            {
                // Liga o fogo
                if (fogoAtivo != null)
                    fogoAtivo.SetActive(true);
                fogoLigado = true;
                timer = tempoFogoAtivo;
            }
        }
    }
}
