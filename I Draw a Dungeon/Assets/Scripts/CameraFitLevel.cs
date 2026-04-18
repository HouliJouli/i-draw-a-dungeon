using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitLevel : MonoBehaviour
{
    [Header("Zoom Limits")]
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float maxOrthoSize = 12f;
    [SerializeField] private float padding = 2f;

    [Header("Bounds")]
    [SerializeField] private BoxCollider2D levelBounds;

    private BoxCollider2D _transitionBounds;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 3f;

    [Header("Transition Push")]
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private float maxTransitionOffset = 3f;
    [SerializeField] private float transitionOffsetSpeed = 1f;

    private Camera cam;
    private float _currentOffset;
    private bool _inTransition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        if (newState == ArenaState.Transition)
            _inTransition = true;
    }

    public void SetBounds(BoxCollider2D newBounds)
    {
        levelBounds = newBounds;
        _inTransition = false;
        Debug.Log($"[CameraFitLevel] Bounds trocados para: {(newBounds != null ? newBounds.gameObject.scene.name : "null")}");
    }

    public void SetTransitionBounds(BoxCollider2D newTransitionBounds)
    {
        _transitionBounds = newTransitionBounds;
    }

    public void ClearBounds()
    {
        levelBounds = null;
    }

    private void LateUpdate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        int count = 0;
        Vector2 center = Vector2.zero;
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].activeInHierarchy) continue;
            center += (Vector2)players[i].transform.position;
            count++;
        }

        if (count == 0) return;

        center /= count;

        float maxDist = 0f;
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].activeInHierarchy) continue;
            for (int j = i + 1; j < players.Length; j++)
            {
                if (!players[j].activeInHierarchy) continue;
                float dist = Vector2.Distance(players[i].transform.position, players[j].transform.position);
                if (dist > maxDist) maxDist = dist;
            }
        }

        float targetSize = Mathf.Clamp((maxDist / 2f) + padding, minOrthoSize, maxOrthoSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, smoothSpeed * Time.deltaTime);

        float targetOffset = _inTransition ? maxTransitionOffset : 0f;
        _currentOffset = Mathf.Lerp(_currentOffset, targetOffset, transitionOffsetSpeed * Time.deltaTime);

        Vector3 targetPos = new Vector3(center.x + _currentOffset, center.y, transform.position.z);

        BoxCollider2D boundsToUse = _inTransition ? _transitionBounds : levelBounds;
        if (boundsToUse != null)
            targetPos = ClampToBounds(targetPos, cam.orthographicSize, boundsToUse);

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    private Vector3 ClampToBounds(Vector3 position, float orthoSize, BoxCollider2D boundsCollider)
    {
        Bounds b = boundsCollider.bounds;
        float halfH = orthoSize;
        float halfW = orthoSize * cam.aspect;

        float clampedX = Mathf.Clamp(position.x, b.min.x + halfW, b.max.x - halfW);
        float clampedY = Mathf.Clamp(position.y, b.min.y + halfH, b.max.y - halfH);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
