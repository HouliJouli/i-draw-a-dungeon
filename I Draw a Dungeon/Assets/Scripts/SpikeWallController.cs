using System;
using UnityEngine;

public class SpikeWallController : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private Collider2D wallCollider;
    [SerializeField] private SpriteRenderer wallSprite;

    [Header("End Boundary")]
    [Tooltip("Posição X que representa o limite direito da arena. Ao chegar aqui, a parede sinaliza fim.")]
    [SerializeField] private float endBoundaryX = 30f;

    public event Action OnWallReachedEnd;

    private Rigidbody2D rb;
    private bool _moving;
    private bool _reachedEnd;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        if (wallCollider != null) wallCollider.enabled = false;
        if (wallSprite != null) wallSprite.enabled = false;
    }

    private void Start()
    {
        if (arenaManager == null)
            arenaManager = FindAnyObjectByType<ArenaManager>();

        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void FixedUpdate()
    {
        if (!_moving || _reachedEnd) return;

        if (rb != null)
            rb.MovePosition(rb.position + Vector2.right * moveSpeed * Time.fixedDeltaTime);
        else
            transform.position += Vector3.right * moveSpeed * Time.fixedDeltaTime;

        if (transform.position.x >= endBoundaryX)
        {
            _reachedEnd = true;
            _moving = false;
            Debug.Log("[SpikeWallController] Parede atingiu o limite da arena.");
            OnWallReachedEnd?.Invoke();
        }
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        if (newState == ArenaState.Transition)
            Activate();
    }

    private void Activate()
    {
        if (wallCollider != null) wallCollider.enabled = true;
        if (wallSprite != null) wallSprite.enabled = true;
        _moving = true;
        Debug.Log("[SpikeWallController] Parede de espinhos ativada.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_moving) return;

        // players: desativa diretamente para ignorar invencibilidade do dash
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            player.gameObject.SetActive(false);
            return;
        }

        // spawners de inimigo
        EnemySpawner spawner = other.GetComponentInParent<EnemySpawner>();
        if (spawner != null)
        {
            Destroy(spawner.gameObject);
            return;
        }

        // inimigos: passa por TakeDamage normalmente
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(float.MaxValue);
    }
}
