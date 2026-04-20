using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private ArenaManager arenaManager;

    [BoxGroup("References"), Required]
    [SerializeField] private Collider2D doorCollider;

    [BoxGroup("References"), Required]
    [SerializeField] private SpriteRenderer doorSprite;

    public event Action OnDoorOpened;
    public event Action OnDoorClosed;

    [FoldoutGroup("Slide Animation")]
    [SerializeField] private Vector2 slideDirection = Vector2.up;

    [FoldoutGroup("Slide Animation"), MinValue(0.1f)]
    [SerializeField] private float slideDistance = 2f;

    [FoldoutGroup("Slide Animation"), MinValue(0.1f)]
    [SerializeField] private float animationDuration = 0.5f;

    [FoldoutGroup("Slide Animation")]
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _closedPosition;
    private Sequence _openSequence;

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void Start()
    {
        if (arenaManager == null)
            arenaManager = FindAnyObjectByType<ArenaManager>();

        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;

        _closedPosition = transform.position;
        CloseDoor();
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        if (newState == ArenaState.Transition)
            OpenDoor();
    }

    [Button("Open Door"), BoxGroup("Debug")]
    public void OpenDoor()
    {
        if (doorCollider != null) doorCollider.enabled = false;

        _openSequence?.Kill();

        Vector3 endPos = _closedPosition + (Vector3)(slideDirection.normalized * slideDistance);

        _openSequence = DOTween.Sequence();
        _openSequence.Join(transform.DOMove(endPos, animationDuration).SetEase(slideCurve));

        if (doorSprite != null)
            _openSequence.Join(doorSprite.DOFade(0f, animationDuration).SetEase(slideCurve));

        _openSequence.OnComplete(() => Debug.Log($"[DoorController] {gameObject.name} aberta."));

        OnDoorOpened?.Invoke();
        Debug.Log($"[DoorController] {gameObject.name} abrindo.");
    }

    [Button("Close Door"), BoxGroup("Debug")]
    public void CloseDoor()
    {
        if (doorCollider != null) doorCollider.enabled = true;

        _openSequence?.Kill();

        transform.position = _closedPosition;

        if (doorSprite != null)
        {
            var c = doorSprite.color;
            c.a = 1f;
            doorSprite.color = c;
        }

        OnDoorClosed?.Invoke();
        Debug.Log($"[DoorController] {gameObject.name} fechada.");
    }
}
