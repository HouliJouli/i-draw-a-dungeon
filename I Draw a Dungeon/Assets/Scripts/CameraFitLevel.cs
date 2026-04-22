using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitLevel : MonoBehaviour
{
    [FoldoutGroup("Zoom"), MinValue(0.1f)]
    [SerializeField] private float minOrthoSize = 5f;

    [FoldoutGroup("Zoom"), MinValue(0.1f)]
    [SerializeField] private float maxOrthoSize = 12f;

    [FoldoutGroup("Zoom"), MinValue(0f)]
    [SerializeField] private float padding = 2f;

    [BoxGroup("Bounds")]
    [SerializeField] private BoxCollider2D levelBounds;

    [BoxGroup("Bounds"), MinValue(0.1f)]
    [SerializeField] private float boundsTransitionSpeed = 2f;

    [BoxGroup("Smoothing"), MinValue(0.01f)]
    [SerializeField] private float smoothTime = 0.3f;

    [FoldoutGroup("Transition Push"), Required]
    [SerializeField] private ArenaManager arenaManager;

    [FoldoutGroup("Transition Push"), MinValue(0f)]
    [SerializeField] private float maxTransitionOffset = 3f;

    [FoldoutGroup("Transition Push"), MinValue(0f)]
    [SerializeField] private float transitionOffsetSpeed = 1f;

    [FoldoutGroup("Shake"), MinValue(0f)]
    private float shakeAmplitude = 0.05f;

    [FoldoutGroup("Shake"), MinValue(0f)]
    private float shakeFrequency = 50f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool _inTransition;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool _shaking;

    private BoxCollider2D _transitionBounds;
    private Camera cam;
    private float _currentOffset;
    private float _boundsLerpT = 1f;
    private Bounds _fromBounds;
    private Bounds _toBounds;
    private Bounds _cachedTransitionBounds;
    private Vector3 _velocity;
    private float _sizeVelocity;
    private float _shakeTime;

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
        {
            _inTransition = true;
            if (_transitionBounds != null)
                _cachedTransitionBounds = _transitionBounds.bounds;
        }
    }

    public void StartShake(float amplitude, float frequency)
    {
        shakeAmplitude = amplitude;
        shakeFrequency = frequency;
        _shaking = true;
        _shakeTime = 0f;
    }

    public void StopShake()
    {
        _shaking = false;
    }

    public void SetBounds(BoxCollider2D newBounds)
    {
        if (_inTransition)
        {
            _fromBounds = _cachedTransitionBounds;
            _toBounds = newBounds.bounds;
            _boundsLerpT = 0f;
        }

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
        if (_boundsLerpT < 1f)
            _boundsLerpT = Mathf.MoveTowards(_boundsLerpT, 1f, boundsTransitionSpeed * Time.deltaTime);

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

        float targetOffset = _inTransition ? maxTransitionOffset : 0f;
        _currentOffset = Mathf.Lerp(_currentOffset, targetOffset, transitionOffsetSpeed * Time.deltaTime);

        Vector3 targetPos = new Vector3(center.x + _currentOffset, center.y, transform.position.z);

        if (_inTransition && _transitionBounds != null)
        {
            Bounds tb = _transitionBounds.bounds;
            float maxSize = MaxOrthoSizeForBounds(tb);
            float targetSize = Mathf.Clamp((maxDist / 2f) + padding, minOrthoSize, Mathf.Min(maxOrthoSize, maxSize));
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref _sizeVelocity, smoothTime);
            targetPos = ClampToBounds(targetPos, cam.orthographicSize, tb);
        }
        else if (!_inTransition && levelBounds != null)
        {
            Bounds effectiveBounds = _boundsLerpT < 1f
                ? LerpBounds(_fromBounds, _toBounds, _boundsLerpT)
                : levelBounds.bounds;
            float maxSize = MaxOrthoSizeForBounds(effectiveBounds);
            float targetSize = Mathf.Clamp((maxDist / 2f) + padding, minOrthoSize, Mathf.Min(maxOrthoSize, maxSize));
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref _sizeVelocity, smoothTime);
            targetPos = ClampToBounds(targetPos, cam.orthographicSize, effectiveBounds);
        }
        else
        {
            float targetSize = Mathf.Clamp((maxDist / 2f) + padding, minOrthoSize, maxOrthoSize);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref _sizeVelocity, smoothTime);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);

        if (_shaking)
        {
            _shakeTime += Time.deltaTime * shakeFrequency;
            float offsetX = (Mathf.PerlinNoise(_shakeTime, 0f) - 0.5f) * 2f * shakeAmplitude;
            float offsetY = (Mathf.PerlinNoise(0f, _shakeTime) - 0.5f) * 2f * shakeAmplitude;
            transform.position += new Vector3(offsetX, offsetY, 0f);
        }
    }

    private Bounds LerpBounds(Bounds from, Bounds to, float t)
    {
        Vector3 center = Vector3.Lerp(from.center, to.center, t);
        Vector3 size = Vector3.Lerp(from.size, to.size, t);
        return new Bounds(center, size);
    }

    private Vector3 ClampToBounds(Vector3 position, float orthoSize, Bounds b)
    {
        float halfH = orthoSize;
        float halfW = orthoSize * cam.aspect;

        float clampedX = Mathf.Clamp(position.x, b.min.x + halfW, b.max.x - halfW);
        float clampedY = Mathf.Clamp(position.y, b.min.y + halfH, b.max.y - halfH);

        return new Vector3(clampedX, clampedY, position.z);
    }

    private float MaxOrthoSizeForBounds(Bounds b)
    {
        float maxFromHeight = b.size.y * 0.5f;
        float maxFromWidth  = b.size.x * 0.5f / cam.aspect;
        return Mathf.Min(maxFromHeight, maxFromWidth);
    }
}
