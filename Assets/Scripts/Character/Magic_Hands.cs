using UnityEngine;

public class Magic_Hands : MonoBehaviour
{
    [SerializeField] private GameObject baseProjectile;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform firePoint; // Ponto de disparo configurado no Inspector

    private Vector3 initialFirePointLocalPosition;
    private float magicCooldown = 0.6f;
    private float magicTimer = 0f;

    private void Start()
    {
        // Verifica se Magic_Hands é filho de Oiia_Cat
        if (transform.IsChildOf(Oiia_Cat.instance.transform))
        {
            Debug.Log("Magic_Hands é filho de Oiia_Cat!");
        }
        else
        {
            Debug.Log("Magic_Hands NÃO é filho de Oiia_Cat!");
        }

        // Armazena a posição local inicial do firePoint para referência
        if (firePoint != null)
            initialFirePointLocalPosition = firePoint.localPosition;
    }

    private void Update()
    {
        UpdateFirePointPosition();
        if (magicTimer > 0f) magicTimer -= Time.deltaTime;
        HandleShooting();
    }

    // Atualiza a posição local do firePoint com base na direção do personagem
    private void UpdateFirePointPosition()
    {
        if (Oiia_Cat.instance != null && firePoint != null)
        {
            int dir = Oiia_Cat.instance.direction; // 1 para direita, -1 para esquerda
            // Ajusta o offset horizontal mantendo os demais componentes inalterados
            Vector3 newLocalPos = initialFirePointLocalPosition;
            newLocalPos.x = Mathf.Abs(initialFirePointLocalPosition.x) * dir;
            firePoint.localPosition = newLocalPos;
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && magicTimer <= 0f)
        {
            Fire();
            magicTimer = magicCooldown;
        }
    }

    // Instancia o projétil a partir do firePoint com base na direção atual
    private void Fire()
    {
        int dir = Oiia_Cat.instance.direction; // 1 para direita; -1 para esquerda

        // Usa a posição atual do firePoint (que já é ajustada de acordo com a direção)
        Vector3 spawnPos = firePoint.position;

        GameObject newProjectile = Instantiate(baseProjectile, spawnPos, Quaternion.identity);
        newProjectile.GetComponent<Projectile>().SetDirection(dir);
        Oiia_Cat.instance.TriggerAttackAnimation(0);
    }
}
