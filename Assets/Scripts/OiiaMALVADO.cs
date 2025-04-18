using System.Collections;
using UnityEngine;

public class OIiaMALVADO : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    [SerializeField] private int danoAtaque = 15;
    [SerializeField] private float forcaEmpurrao = 5f;
    [SerializeField] private float tempoEntreAtaques = 2f;

    [Header("Detecção")]
    [SerializeField] private float distanciaDetecao = 3f;
    [SerializeField] private LayerMask layerJogador;

    private bool podeAtacar = true;
    private Transform jogador;
    private Animator animator;
    private bool atacando = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        jogador = GameObject.FindGameObjectWithTag("Character").transform;
    }

    private void Update()
    {
        if (podeAtacar && PlayerNaFrente())
        {
            StartCoroutine(RealizarAtaque());
        }
    }

    private bool PlayerNaFrente()
    {
        Vector2 direcao = (jogador.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direcao,
            distanciaDetecao,
            layerJogador);

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private IEnumerator RealizarAtaque()
    {
        podeAtacar = false;
        atacando = true;

        // Dispara animação de cabeçada
        animator.SetTrigger("Atacar");

        // Tempo para a cabeçada conectar (ajuste conforme sua animação)
        yield return new WaitForSeconds(0.4f);

        atacando = false;
        yield return new WaitForSeconds(tempoEntreAtaques);
        podeAtacar = true;
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (atacando && colisao.gameObject.CompareTag("Character"))
        {


            // Empurra o jogador
            Vector2 direcaoEmpurrao = (colisao.transform.position - transform.position).normalized;
            colisao.gameObject.GetComponent<Rigidbody2D>().AddForce(direcaoEmpurrao * forcaEmpurrao, ForceMode2D.Impulse);
        }
    }
}