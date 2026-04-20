using Sirenix.OdinInspector;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private GameObject enemyPrefab;

    [BoxGroup("References"), Required]
    [SerializeField] private BoxCollider2D spawnArea;

    [BoxGroup("References")]
    [SerializeField] private ArenaManager arenaManager;

    [BoxGroup("Initial Spawn"), MinValue(0)]
    [SerializeField] private int initialEnemyCount = 5;

    [FoldoutGroup("Normal Mode (Safe / Warning)")]
    [SerializeField] private bool enableWaveSpawn = true;

    [FoldoutGroup("Normal Mode (Safe / Warning)"), MinValue(1f)]
    [SerializeField] private float normalWaveCooldown = 10f;

    [FoldoutGroup("Normal Mode (Safe / Warning)"), MinValue(1)]
    [SerializeField] private int normalEnemiesPerWave = 3;

    [FoldoutGroup("Transition Mode"), MinValue(0.5f)]
    [SerializeField] private float transitionWaveCooldown = 4f;

    [FoldoutGroup("Transition Mode"), MinValue(1)]
    [SerializeField] private int transitionEnemiesPerWave = 6;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool inTransitionMode;

    private float waveTimer;

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

    [Button("Spawn Wave Now"), BoxGroup("Debug")]
    private void SpawnWaveNow()
    {
        int count = inTransitionMode ? transitionEnemiesPerWave : normalEnemiesPerWave;
        for (int i = 0; i < count; i++)
            Spawn();
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
