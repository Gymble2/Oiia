using UnityEngine;
using System.Collections;

public class BossScript : MonoBehaviour
{
    #region Variáveis

    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform jogador;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    // --- Detecção e Movimento ---
    [Header("Detecção e Movimento")]
    [SerializeField] private float distanciaDetecao = 8f;
    [SerializeField] private LayerMask layerJogador;
    [SerializeField] private float velocidadePerseguicao = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.25f;

    // --- Ataque ---
    [Header("Ataque")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int danoAtaque = 2;
    private bool atacando = false;
    private bool isDashing = false;
    private float attackCooldownTimer = 0f;

    // --- Vida e Dano ---
    [Header("Vida e Dano")]
    [SerializeField] private float vidaMaxima = 50f;
    [SerializeField] private float vidaAtual;
    [SerializeField] private float tempoPiscar = 0.15f;
    [SerializeField] private float forcaKnockback = 7f;
    private Coroutine coroutinePiscar;
    private bool invulneravel = false;
    [SerializeField] private float tempoInvulneravel = 2f;

    // --- Controle de Estado ---
    [Header("Controle de Estado")]
    private bool perseguido = false;

    #endregion

    #region Inicialização
    private void Start()
    {
        float mult = GameDifficultyManager.instance != null ? GameDifficultyManager.instance.GetMultiplier() : 1f;
        vidaMaxima = Mathf.RoundToInt(vidaMaxima * mult);
        vidaAtual = vidaMaxima;
        velocidadePerseguicao *= mult;
        dashSpeed *= mult;
        forcaKnockback *= mult;
        // Ajuste: Boss só 1.3x maior no hard
        if (GameDifficultyManager.instance != null && GameDifficultyManager.instance.difficulty == GameDifficulty.Hard)
            transform.localScale *= 1.3f;

        // Inicialização do Boss
        Debug.Log("Boss Initialized");
        animator = GetComponent<Animator>();
        jogador = GameObject.FindGameObjectWithTag("Character").transform;
        animator.SetBool("Patrulha", true);
        rb = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region Loop Principal
    private void Update()
    {
        // Garante que a referência ao personagem está sempre atualizada
        if (jogador == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Character");
            if (found != null)
                jogador = found.transform;
        }

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        bool playerVisto = false;
        if (jogador != null)
            playerVisto = PlayerNaFrente();

        if (playerVisto)
        {
            perseguido = true;
            // Corrigido: só chama dash se não estiver dashing e cooldown acabou
            if (!isDashing && attackCooldownTimer <= 0f)
            {
                StartCoroutine(DashAttack());
                attackCooldownTimer = attackCooldown; // Garante cooldown correto
            }
            animator.SetBool("Perseguindo", true);
        }
        else
        {
            animator.SetBool("Patrulha", true);
        }
        Debug.Log("Boss Update");
    }
    #endregion

    #region Ataque
    private IEnumerator DashAttack()
    {
        isDashing = true;
        animator.SetTrigger("Atacar"); // Use um trigger para animação de ataque/dash
        Vector2 direcaoPlayer = (jogador.position - transform.position).normalized;
        float dashTime = 0f;
        while (dashTime < dashDuration)
        {
            atacando = true;
            float groundCheckOffset = 0.7f;
            float groundCheckDistance = 1.5f;
            Vector2 origem = (Vector2)transform.position + Vector2.right * direcaoPlayer * groundCheckOffset;
            RaycastHit2D chao = Physics2D.Raycast(origem, Vector2.down, groundCheckDistance, LayerMask.GetMask("ForeGround"));
            Debug.DrawRay(origem, Vector2.down * groundCheckDistance, Color.red);
            if (chao.collider == null)
            {
                rb.linearVelocity = Vector2.zero;
                break;
            }
            rb.linearVelocity = new Vector2(direcaoPlayer.x * dashSpeed, rb.linearVelocity.y);
            dashTime += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        atacando = false;
        // attackCooldownTimer já é setado no Update
    }
    #endregion

    #region Detecção
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
    #endregion

    #region Colisão
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
    #endregion

    #region Dano e Vida
    public void ReceberDano(float dano, Vector2 origemDoDano)
    {
        if (invulneravel) return;
        vidaAtual -= dano;
        AplicarKnockback(origemDoDano);
        if (coroutinePiscar != null)
            StopCoroutine(coroutinePiscar);
        coroutinePiscar = StartCoroutine(PiscarVermelho());
        if (vidaAtual <= 0)
        {
            Morrer();
        }
        else
        {
            StartCoroutine(InvulnerabilidadeCoroutine());
        }
    }

    private IEnumerator InvulnerabilidadeCoroutine()
    {
        invulneravel = true;
        yield return new WaitForSeconds(tempoInvulneravel);
        invulneravel = false;
    }

    private IEnumerator PiscarVermelho()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(tempoPiscar);
        spriteRenderer.color = Color.white; // ou a cor original do seu sprite
        coroutinePiscar = null;
    }

    private void AplicarKnockback(Vector2 origemDano)
    {
        if (rb != null)
        {
            Debug.Log("Knockback");
            float direcao = Mathf.Sign(transform.position.x - origemDano.x);
            if (direcao == 0) direcao = 1; // Se estiver exatamente alinhado, joga para a direita
            rb.AddForce(new Vector2(direcao * forcaKnockback, 4f), ForceMode2D.Impulse);
        }
    }

    private void Morrer()
    {
        // Aqui pode-se adicionar animação de morte, efeitos, etc.
        Destroy(gameObject);
    }
    #endregion
}
