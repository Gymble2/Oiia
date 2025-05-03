using System;
using System.Collections;
using UnityEngine;

public class OIiaMALVADO : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    [SerializeField] private int danoAtaque = 2;
    [SerializeField] private float forcaEmpurrao = 5f;
    [SerializeField] private float tempoEntreAtaques = 1.2f;

    [Header("Detecção")]
    [SerializeField] private float distanciaDetecao = 8f; // Aumenta a distância de detecção
    [SerializeField] private LayerMask layerJogador;

    [Header("Movimentação")]
    [SerializeField] private float velocidadePatrulha = 2f;
    [SerializeField] private float tempoPatrulha = 3f;
    [SerializeField] private float velocidadePerseguicao = 5f;

    [Header("Animação")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private float tempoPiscar = 0.15f;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 50f;
    private float vidaAtual;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 7f;
    private Rigidbody2D rb;

    private bool podeAtacar = true;
    private Transform jogador;
    private bool atacando = false;
    private float patrulhaTimer = 0f;
    private int direcao = 1; // 1 = direita, -1 = esquerda
    private bool perseguindo = false;
    private Coroutine coroutinePiscar;
    private float velocidadeOriginal;

    private void Start()
    {
        animator = GetComponent<Animator>();
        jogador = GameObject.FindGameObjectWithTag("Character").transform;
        animator.SetBool("Patrulha", true);

        // Garante que o inimigo começa andando e olhando para a esquerda
        direcao = -1;

        vidaAtual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        velocidadeOriginal = velocidadePerseguicao;
    }

    private void Update()
    {
        bool playerVisto = PlayerNaFrente();
        if (playerVisto)
        {
            perseguindo = true;
            PerseguirPlayer();
        }
        else
        {
            if (perseguindo)
            {
                animator.SetBool("Perseguir", false);
                animator.SetBool("Patrulha", true);
                animator.SetBool("Atacar", false);
                perseguindo = false;
                patrulhaTimer = 0f;
                // Garante que o sprite volte a patrulhar olhando para a direção atual
                Vector3 escala = transform.localScale;
                escala.x = Mathf.Abs(escala.x) * direcao;
                transform.localScale = escala;
            }
            Patrulhar();
        }
    }

    private void PerseguirPlayer()
    {
        animator.SetBool("Perseguir", true);
        animator.SetBool("Patrulha", false);

        Vector2 direcaoPlayer = (jogador.position - transform.position).normalized;
        transform.position += (Vector3)direcaoPlayer * velocidadePerseguicao * Time.deltaTime;

        float escalaOriginal = Mathf.Abs(transform.localScale.x);
        if (direcaoPlayer.x > 0)
            transform.localScale = new Vector3(escalaOriginal, transform.localScale.y, transform.localScale.z);
        else if (direcaoPlayer.x < 0)
            transform.localScale = new Vector3(-escalaOriginal, transform.localScale.y, transform.localScale.z);

        // Só ataca se estiver suficientemente próximo do player (ex: distância menor que 1.2f)
        float distancia = Vector2.Distance(jogador.position, transform.position);
        //Debug.Log(distancia);
        if (podeAtacar && distancia < 1.6f)
            StartCoroutine(RealizarAtaque());
        
    }

    private bool PlayerNaFrente()
    {
        Vector2 direcao = (jogador.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direcao,
            distanciaDetecao,
            layerJogador);

        return hit.collider != null && hit.collider.CompareTag("Character");
    }

    private IEnumerator RealizarAtaque()
    {
        podeAtacar = false;
        atacando = true;
        float velocidadePerseguicaoBackup = velocidadePerseguicao;
        velocidadePerseguicao = velocidadePerseguicao * 0.2f; // Diminui bastante a velocidade
        animator.SetBool("Atacar", true);
        animator.SetBool("Perseguir", true);
        yield return new WaitForSeconds(0.4f);
        atacando = false;
        velocidadePerseguicao = velocidadePerseguicaoBackup; // Restaura velocidade
        yield return new WaitForSeconds(tempoEntreAtaques);
        podeAtacar = true;
        if (!PlayerNaFrente())
        {
            animator.SetBool("Atacar", false);
            animator.SetBool("Perseguir", false);
            animator.SetBool("Patrulhar", true);
        }
    }

    private void Patrulhar()
    {
        animator.SetBool("Patrulha", !PlayerNaFrente());
        patrulhaTimer += Time.deltaTime;

        // Sempre alinhe a escala X com a direção da patrulha
        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direcao;
        transform.localScale = escala;

        // Move o inimigo
        transform.position += Vector3.right * direcao * velocidadePatrulha * Time.deltaTime;

        // Detector de chão à frente (offset maior e ray mais longo)
        float groundCheckOffset = 0.7f;
        float groundCheckDistance = 1.5f;
        Vector2 origem = (Vector2)transform.position + Vector2.right * direcao * groundCheckOffset;
        RaycastHit2D chao = Physics2D.Raycast(origem, Vector2.down, groundCheckDistance, LayerMask.GetMask("ForeGround"));
        Debug.DrawRay(origem, Vector2.down * groundCheckDistance, Color.red);

        // Se não houver chão, inverte a direção e o sprite
        if (chao.collider == null)
        {
            direcao *= -1;
            spriteRenderer.flipX = true;
            patrulhaTimer = 0f;
            Vector3 escalaChao = transform.localScale;
            escalaChao.x = Mathf.Abs(escalaChao.x) * direcao;
            transform.localScale = escalaChao;
            return;
        } 

        // Vira ao final do tempo de patrulha
        if (patrulhaTimer >= tempoPatrulha)
        {
            direcao *= -1;
            patrulhaTimer = 0f;
            Vector3 escalaTempo = transform.localScale;
            escalaTempo.x = Mathf.Abs(escalaTempo.x) * direcao;
            transform.localScale = escalaTempo;
        }
    }

    private void OnCollisionStay2D(Collision2D colisao)
{
    if (colisao.gameObject.CompareTag("Character"))
    {
        var personagem = colisao.gameObject.GetComponent<Oiia_Cat>();
        if (personagem != null)
        {
            personagem.ReceberDano(danoAtaque, transform.position);
        }
    }

    if (atacando && colisao.gameObject.CompareTag("Character"))
    {
        // Empurra o jogador
        Vector2 direcaoEmpurrao = (colisao.transform.position - transform.position).normalized;
        colisao.gameObject.GetComponent<Rigidbody2D>().AddForce(direcaoEmpurrao * 2, ForceMode2D.Impulse);
    }
}

    // Método para receber dano
    public void ReceberDano(float dano)
    {
        vidaAtual -= dano;
        AplicarKnockback();
        if (coroutinePiscar != null)
            StopCoroutine(coroutinePiscar);
        coroutinePiscar = StartCoroutine(PiscarVermelho());
        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private IEnumerator PiscarVermelho()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(tempoPiscar);
        spriteRenderer.color = Color.white; // ou a cor original do seu sprite
        coroutinePiscar = null;
    }

    private void AplicarKnockback()
    {
        if (rb != null)
        {
            // Aplica força na direção oposta ao movimento atual (eixo X)
            float direcao = Mathf.Sign(rb.linearVelocity.x);
            if (direcao == 0) direcao = -1; // Se parado, joga para a esquerda
            rb.AddForce(new Vector2(-direcao * forcaKnockback, 3f), ForceMode2D.Impulse);
        }
    }

    private void Morrer()
    {
        // Aqui pode-se adicionar animação de morte, efeitos, etc.
        Destroy(gameObject);
    }
}