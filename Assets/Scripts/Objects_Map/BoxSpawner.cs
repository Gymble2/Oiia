using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [Header("Prefab da Caixa")]
    public GameObject boxPrefab;

    [Header("Posições de Spawn (topo da tela)")]
    public Transform[] spawnPoints;

    [Header("Intervalo entre caixas (segundos)")]
    public float spawnInterval = 2f;
    
    [Header("GameObjects")]

    private float timer;
    private bool canSpawn = false;

    void Start()
    {
        float mult = GameDifficultyManager.instance != null ? GameDifficultyManager.instance.GetMultiplier() : 1f;
        spawnInterval /= mult; // caixas nascem mais rápido no hard
        timer = spawnInterval;
    }

    void Update()
    {
        if (!canSpawn) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnBoxAtNextPoint();
            timer = spawnInterval;
        }
    }

    public void EnableSpawning()
    {
        canSpawn = true;
    }

    void SpawnBoxAtNextPoint()
    {
        if (spawnPoints.Length == 0) return;
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(boxPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
    }
}
