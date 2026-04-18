using System;
using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorSprite;

    public event Action OnDoorOpened;
    public event Action OnDoorClosed;

    [Header("Slide Animation")]
    [SerializeField] private Vector2 slideDirection = Vector2.up;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _closedPosition;
    private Coroutine _animationCoroutine;

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

    public void OpenDoor()
    {
        if (doorCollider != null) doorCollider.enabled = false;

        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(AnimateOpen());

        OnDoorOpened?.Invoke();
        Debug.Log($"[DoorController] {gameObject.name} abrindo.");
    }

    public void CloseDoor()
    {
        if (doorCollider != null) doorCollider.enabled = true;

        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

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

    private IEnumerator AnimateOpen()
    {
        Vector3 startPos = _closedPosition;
        Vector3 endPos = _closedPosition + (Vector3)(slideDirection.normalized * slideDistance);
        float startAlpha = doorSprite != null ? doorSprite.color.a : 1f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / animationDuration));

            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (doorSprite != null)
            {
                var c = doorSprite.color;
                c.a = Mathf.Lerp(startAlpha, 0f, t);
                doorSprite.color = c;
            }

            yield return null;
        }

        transform.position = endPos;

        if (doorSprite != null)
        {
            var c = doorSprite.color;
            c.a = 0f;
            doorSprite.color = c;
        }

        Debug.Log($"[DoorController] {gameObject.name} aberta.");
    }
}
