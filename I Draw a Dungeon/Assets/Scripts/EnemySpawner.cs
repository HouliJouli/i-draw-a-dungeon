using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Initial Spawn")]
    [SerializeField] private int initialEnemyCount = 5;

    [Header("Normal Mode (Safe / Warning)")]
    [SerializeField] private bool enableWaveSpawn = true;
    [SerializeField] private float normalWaveCooldown = 10f;
    [SerializeField] private int normalEnemiesPerWave = 3;

    [Header("Transition Mode")]
    [SerializeField] private float transitionWaveCooldown = 4f;
    [SerializeField] private int transitionEnemiesPerWave = 6;

    private float waveTimer;
    private bool inTransitionMode;

    private void Start()
    {
        if (arenaManager == null)
            arenaManager = FindAnyObjectByType<ArenaManager>();

        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;

        for (int i = 0; i < initialEnemyCount; i++)
            Spawn();

        waveTimer = normalWaveCooldown;
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void Update()
    {
        if (!enableWaveSpawn) return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f)
        {
            int count = inTransitionMode ? transitionEnemiesPerWave : normalEnemiesPerWave;
            for (int i = 0; i < count; i++)
                Spawn();

            waveTimer = inTransitionMode ? transitionWaveCooldown : normalWaveCooldown;
        }
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        if (newState == ArenaState.Transition)
        {
            inTransitionMode = true;
            waveTimer = transitionWaveCooldown;
            Debug.Log("[EnemySpawner] Modo Transição ativado — pressão aumentada.");
        }
        else if (inTransitionMode)
        {
            inTransitionMode = false;
            waveTimer = normalWaveCooldown;
            Debug.Log($"[EnemySpawner] Modo Normal restaurado ({newState}).");
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
