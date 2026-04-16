using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Initial Spawn")]
    [SerializeField] private int initialEnemyCount = 5;

    [Header("Wave Spawn")]
    [SerializeField] private bool enableWaveSpawn = true;
    [SerializeField] private float waveCooldown = 10f;
    [SerializeField] private int enemiesPerWave = 3;

    private float waveTimer;

    private void Start()
    {
        for (int i = 0; i < initialEnemyCount; i++)
            Spawn();

        waveTimer = waveCooldown;
    }

    private void Update()
    {
        if (!enableWaveSpawn) return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            for (int i = 0; i < enemiesPerWave; i++)
                Spawn();

            waveTimer = waveCooldown;
        }
    }

    private void Spawn()
    {
        Bounds bounds = spawnArea.bounds;
        Vector2 position = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        Instantiate(enemyPrefab, position, Quaternion.identity);
    }
}
