using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public int dano = 1;
    public float intervaloDano = 0.5f; // tempo entre danos enquanto encostando

    private float timer = 0f;

    private void OnEnable()
    {
        timer = 0f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Character"))
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                var personagem = other.GetComponent<Oiia_Cat>();
                if (personagem != null)
                {
                    personagem.ReceberDano(dano, transform.position);
                }
                timer = intervaloDano;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Character"))
        {
            timer = 0f; // reseta o timer ao sair do fogo
        }
    }
}
