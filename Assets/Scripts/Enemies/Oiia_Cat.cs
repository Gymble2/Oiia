using System;
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

    #region Unity Methods

    
    /// <summary>
    /// Inicializa os componentes, define a instância singleton e referências essenciais.
    /// </summary>
    void Awake()
    {
        instance = this;
        InitializeComponents();
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
        if (rigidBody.position.y < -75f)
        {
            rigidBody.position = Vector2.zero;
            rigidBody.linearVelocity = Vector2.zero;
        }
    }

    #endregion

    #region Animation Handlers

    /// <summary>
    /// Gerencia a lógica de pulo e as transições de animação entre pulando e caindo.
    /// </summary>
    void HandleJumpAnimation()
    {
        if (IsGroundedComplex() && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
        {
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
                Debug.Log("Direção: " + direction);
            }
            else if (velocityX > 0.1f)
            {
                spriteRenderer.flipX = false; // Mantém direita
                direction = 1;
                animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
                animator.SetFloat("Velocity", rigidBody.linearVelocity.x);
                Debug.Log("Direção: " + direction);
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
        gameObject.name = "Bob";
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
    /// Detecta inclinações (slopes) abaixo do personagem e registra o ângulo.
    /// </summary>
    // void DetectSlopes()
    // {
    //     RaycastHit2D slopeHit = Physics2D.Raycast(calculatedPosition, Vector2.down, slopeCheckDistance, groundLayer);
    //     if (slopeHit)
    //     {
    //         slopeAngle = Vector2.Angle(slopeHit.normal, Vector2.up);
    //         string slopeMessage = (slopeAngle > 0 && slopeAngle <= 80) ? "Inclinação detectada: " + slopeAngle : "Superfície muito inclinada!";
    //         Debug.Log(slopeMessage);
    //     }
    //     else
    //     {
    //         Debug.Log("Sem inclinação detectada.");
    //     }
    // }

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
    public void TriggerAttackAnimation()
    {
        HandleAttackAnimation();
    }

    #endregion
}