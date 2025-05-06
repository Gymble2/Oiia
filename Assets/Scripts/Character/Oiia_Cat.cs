using System;
using System.Collections;
using UnityEngine;

public class Oiia_Cat : MonoBehaviour
{
    #region Singleton
    public static Oiia_Cat instance;
    #endregion

    #region Components
    private BoxCollider2D playerCollider;  // Collider do personagem
    public Rigidbody2D rigidBody;          // Rigidbody do personagem
    #endregion

    #region Serialized Variables
    [SerializeField] private LayerMask groundLayer; // Camada do chão
    [SerializeField] private Animator animator;       // Controlador de animação
    [SerializeField] private SpriteRenderer spriteRenderer; // Renderizador de sprite
    #endregion

    #region Movement Variables
    private Vector2 calculatedPosition;   // Posição base ajustada à base do collider
    private Vector2 colliderSize;         // Tamanho do collider do personagem
    public float jumpForce = 10f;         // Força do pulo
    public float jumpBufferTime = 0.1f;   // Tempo antes de atingir o chao que o personagem ainda irá pula
    public float coyoteTime = 0.09f;       // Permite pular mesmo logo após ter saido do chao
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    public float walkSpeed = 7f;          // Velocidade de caminhada
    public float runSpeedModifier = 4f;   // Adicional de velocidade para corrida
    private float slopeCheckDistance = 0.1f; // Distância para checar inclinações
    private float slopeAngle;                // Ângulo da inclinação detectada

    // Variáveis de ataque
    private bool isAttacking = false;
    [SerializeField] private float attackDuration = 0.5f; // Tempo em que o ataque é mantido
    private float attackTimer = 0f;

    // Sensor de direção: 1 = direita, -1 = esquerda.
    public int direction { get; private set; } = 1;
    #endregion

    #region Vida
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 5;
    [SerializeField] private int vidaAtual;
    [SerializeField] private HealthBarUI healthBar;
    private Vector3 posicaoInicial;

    [Header("Invencibilidade")]
    [SerializeField] private float tempoInvencibilidade = 2f;
    private bool invencivel = false;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 18f;

    [Header("Piscar Dano")]
    [SerializeField] private float tempoPiscar = 0.15f;
    private Coroutine coroutinePiscar;
    #endregion

    #region Unity Methods

    /// <summary>
    /// Inicializa os componentes, define a instância singleton e referências essenciais.
    /// </summary>
    void Start()
    {
        healthBar.SetMaxHealth(vidaMaxima);
        healthBar.SetHealth(vidaAtual);
    }


    /// <summary>
    /// Inicializa os componentes, define a instância singleton e referências essenciais.
    /// </summary>
    void Awake()
    {
        instance = this;
        InitializeComponents();
        posicaoInicial = transform.position;
        vidaAtual = vidaMaxima;
    }

    /// <summary>
    /// Atualiza o movimento, as animações e a detecção de inclinações a cada frame.
    /// </summary>
    void Update()
    {
        // Atualiza o estado do ataque.
        UpdateAttackState();

        // calculatedPosition = transform.position - new Vector3(0f, colliderSize.y / 2f, 0f);

        if (!isAttacking)
        {
            HandleHorizontalMovement();
        }
        HandleVerticalMovement();
        HandleJumpAnimation();
        HandleAttackAnimation();
        HandleMovementAnimation();
    }

    #endregion

    #region Movement Handlers

    public void SetHealth(int HealthChange)
    {
        vidaAtual += HealthChange;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);

        healthBar.SetHealth(vidaAtual);
    }

    /// <summary>
    /// Processa o movimento horizontal do personagem com base na entrada do usuário.
    /// </summary>
    void HandleHorizontalMovement()
    {
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        float targetVelocityX = 0f;
        if (leftPressed && rightPressed)
        {
            targetVelocityX = 0f;
            Debug.Log("Teclas A e D pressionadas simultaneamente.");
        }
        else if (leftPressed)
        {
            targetVelocityX = runPressed ? -(walkSpeed + runSpeedModifier) : -walkSpeed;
        }
        else if (rightPressed)
        {
            targetVelocityX = runPressed ? (walkSpeed + runSpeedModifier) : walkSpeed;
        }
        else
        {
            targetVelocityX = 0f;
        }
        rigidBody.linearVelocity = new Vector2(targetVelocityX, rigidBody.linearVelocity.y);
    }

    /// <summary>
    /// Gerencia o movimento vertical, resetando a posição caso o personagem caia muito.
    /// </summary>
    void HandleVerticalMovement()
    {
        if (rigidBody.position.y < -35f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    #endregion

    #region Animation Handlers

    /// <summary>
    /// Gerencia a lógica de pulo e as transições de animação entre pulando e caindo.
    /// </summary>
    void HandleJumpAnimation()
    {

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime; // Inicia o timer do buffer quando PULAR for clicado 
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (IsGroundedComplex())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            jumpBufferCounter = 0f;
            Vector2 currentVelocity = rigidBody.linearVelocity;
            currentVelocity.y = jumpForce;
            rigidBody.linearVelocity = currentVelocity;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", false);
            Debug.Log("Pulou!");
        }
        else if (rigidBody.linearVelocity.y < 0 && !IsGroundedComplex())
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", true);
        }
        else if (IsGroundedComplex())
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    /// <summary>
    /// Gerencia a animação de ataque. Ao clicar, o personagem para e dispara a animação de ataque.
    /// </summary>
    void HandleAttackAnimation()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking && IsGroundedComplex())
        {
            isAttacking = true;
            attackTimer = attackDuration;
            rigidBody.linearVelocity = Vector2.zero;
            animator.SetBool("Walk_Attack", true);
            Debug.Log("Ataque acionado!");
        }
    }

    void HandleChargerAttackAnimation()
    {
        if (Input.GetMouseButtonDown(1) && !isAttacking && IsGroundedComplex())
        {
            isAttacking = true;
            attackTimer = attackDuration;
            rigidBody.linearVelocity = Vector2.zero;
            animator.SetBool("Walk_Attack", true);
            Debug.Log("Ataque acionado!");
        }
    }

    /// <summary>
    /// Atualiza as animações de movimento e a direção do sprite.
    /// </summary>
    // Modifique o método HandleMovementAnimation:
    void HandleMovementAnimation()
    {
        float velocityX = rigidBody.linearVelocity.x;
        animator.SetBool("IsWalking", Mathf.Abs(velocityX) > 0.01f);

        if (!isAttacking)
        {
            // Flip pelo SpriteRenderer (mais seguro que localScale)
            if (velocityX < -0.1f)
            {
                spriteRenderer.flipX = true; // Inverte para esquerda
                direction = -1;
                animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
                animator.SetFloat("Velocity", rigidBody.linearVelocity.x);
                //Debug.Log("Direção: " + direction);
            }
            else if (velocityX > 0.1f)
            {
                spriteRenderer.flipX = false; // Mantém direita
                direction = 1;
                animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
                animator.SetFloat("Velocity", rigidBody.linearVelocity.x);
                //Debug.Log("Direção: " + direction);
            }
            else if (velocityX == 0f)
            {
                animator.SetBool("IsRunning", false);
            }
            animator.SetFloat("Velocity", rigidBody.linearVelocity.x);
        }
    }

    #endregion

    #region Auxiliary Methods

    /// <summary>
    /// Inicializa os componentes e variáveis essenciais do personagem.
    /// </summary>
    void InitializeComponents()
    {
        gameObject.name = "Kalina";
        playerCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        colliderSize = playerCollider.size;
    }

    /// <summary>
    /// Verifica se o personagem está tocando o chão utilizando um BoxCast.
    /// </summary>
    /// <returns>Verdadeiro se estiver no chão; caso contrário, falso.</returns>
    bool IsGroundedComplex()
    {
        Bounds bounds = playerCollider.bounds;
        float extraHeight = 0.1f;
        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        Debug.DrawRay(bounds.center, Vector2.down * (bounds.extents.y + extraHeight), Color.red);
        return hit.collider != null;
    }


    /// <summary>
    /// Atualiza o estado do ataque, decrementando o timer e resetando o flag ao final.
    /// </summary>
    void UpdateAttackState()
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                isAttacking = false;
                animator.SetBool("Walk_Attack", false);
                Debug.Log("Ataque finalizado.");
            }
        }
    }

    /// <summary>
    /// Método público para disparar a animação de ataque externamente (por exemplo, a partir do Bullet).
    /// </summary>
    public void TriggerAttackAnimation(int attackType)
    {
        if (attackType == 0) // Se for o ataque normal
        {
            HandleAttackAnimation();
        }
        else if (attackType == 1) // Se for o ataque especial
        {
            HandleChargerAttackAnimation();
        }
    }

    /// <summary>
    /// Método para receber dano e reiniciar a cena.
    /// </summary>
    public void ReceberDano(int dano, Vector3 origemDano)
    {
        if (invencivel) return;
        vidaAtual -= dano;
        healthBar.SetHealth(vidaAtual);
        AplicarKnockback(origemDano);
        if (coroutinePiscar != null)
            StopCoroutine(coroutinePiscar);
        coroutinePiscar = StartCoroutine(PiscarVermelho());
        if (vidaAtual <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            StartCoroutine(InvencibilidadeCoroutine());
        }
    }

    // Funcao para setar a vida do player de forma externa e minimamente segura
    public void setPlayerHealth(int health)
    {
        if (health < 0) return;

        vidaAtual += health;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
        healthBar.SetHealth(vidaAtual);

    }

    // Funcao para adicionar vida ao player de forma externa e minimamente segura
    public void addPlayerHealth(int health)
    {
        vidaAtual += health;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
        healthBar.SetHealth(vidaAtual);

        Debug.Log("Health added. Current health: " + vidaAtual);
    }

    private IEnumerator PiscarVermelho()
    {
        Color corOriginal = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(tempoPiscar);
        spriteRenderer.color = corOriginal;
        coroutinePiscar = null;
    }

    private void AplicarKnockback(Vector3 origemDano)
    {
        if (rigidBody != null)
        {
            Debug.Log("Knockback");
            float direcao = Mathf.Sign(transform.position.x - origemDano.x);
            if (direcao == 0) direcao = 1; // Se estiver exatamente alinhado, joga para a direita
            rigidBody.AddForce(new Vector2(direcao * forcaKnockback, 4f), ForceMode2D.Impulse);
        }
    }

    private IEnumerator InvencibilidadeCoroutine()
    {
        invencivel = true;
        yield return new WaitForSeconds(tempoInvencibilidade);
        invencivel = false;
    }

    #endregion
}