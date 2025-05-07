using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class BoxScript : MonoBehaviour
{

    [SerializeField] private int danoAtaque = 1;
    [SerializeField] Animator animator ;
    [SerializeField]
    private bool firstTouch = true;
    private float brokenBox = 1;
    private void Start()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        if(animator.GetBool("brokenBox") == true && brokenBox < 40)
        {
            brokenBox += 1;
        }
        else if(animator.GetBool("brokenBox") == true && brokenBox == 40)
        {
            Morrer();
        }
    }

    #region Colisão
    private void OnCollisionStay2D(Collision2D colisao)
    {
        if (
            colisao.gameObject.CompareTag("Character") && firstTouch || 
            colisao.gameObject.CompareTag("ForeGround") && firstTouch
            )
        {
            animator.SetBool("brokenBox", true);
            var personagem = colisao.gameObject.GetComponent<Oiia_Cat>();
            if (personagem != null)
            {
                personagem.ReceberDano(danoAtaque, transform.position);
            }
        }
        else if(colisao.gameObject.CompareTag("Enemy") && firstTouch)
        {
            animator.SetBool("brokenBox", true);
            danoAtaque = 10;
            var boss = colisao.gameObject.GetComponent<BossScript>();
            if (boss != null)
            {
                boss.ReceberDano(danoAtaque, transform.position);
            }
        }
    }
    #endregion

    private void Morrer()
    {
        Destroy(gameObject);
    }

}
