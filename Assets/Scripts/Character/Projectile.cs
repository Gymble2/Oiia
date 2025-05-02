using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileDamage;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private int direction;
    private bool hasDirection;
    
    void Start()
    {
        Destroy(gameObject, lifeTime);
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(hasDirection)
        {
            rb.linearVelocity = new Vector2(direction * projectileSpeed, 0f);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if(sr != null)
            {
                sr.flipX = (direction == -1);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se colidir com o personagem (assumindo que a tag do personagem seja "Player"), ignora a colisão
        if (collision.gameObject.CompareTag("Character"))
            return;
        
        // Se colidir com um inimigo, aplica dano
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var inimigo = collision.gameObject.GetComponent<OIiaMALVADO>();
            if (inimigo != null)
            {
                inimigo.ReceberDano(projectileDamage);
            }
        }
        Explode();
    }

    private void Explode()
    {
        // Sem animação de explosão: simplesmente destrói o objeto
        Destroy(gameObject);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        hasDirection = true;
    }
}
