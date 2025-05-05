using Unity.VisualScripting;
using UnityEngine;

public class HealthKit : MonoBehaviour
{
    [SerializeField] private int qtdCura = 5;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se colidir com o personagem (assumindo que a tag do personagem seja "Player"), ignora a colisão
        if (collision.gameObject.CompareTag("Character"))
        {    
            Oiia_Cat playerHealth = collision.GetComponent<Oiia_Cat>();

            if (playerHealth != null)
            {
                playerHealth.addPlayerHealth(qtdCura);
                Destroy(gameObject); // Remove o item
            }
        }
    }
}
