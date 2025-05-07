using UnityEngine;

public class BeginSpawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Character"))
        {
            BoxSpawner spawner = FindObjectOfType<BoxSpawner>();
            if (spawner != null)
            {
                spawner.EnableSpawning();
            }
            // Opcional: destruir o trigger após ativar
            // Destroy(gameObject);
        }
    }
}
