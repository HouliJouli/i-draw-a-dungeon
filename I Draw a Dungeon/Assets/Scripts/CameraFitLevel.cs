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

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 3f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Filtra players inativos (mortos)
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

        // Maior distância entre qualquer par de players
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

        // Converte distância em orthographic size
        float targetSize = Mathf.Clamp((maxDist / 2f) + padding, minOrthoSize, maxOrthoSize);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, smoothSpeed * Time.deltaTime);

        Vector3 targetPos = new Vector3(center.x, center.y, transform.position.z);
        if (levelBounds != null)
            targetPos = ClampToBounds(targetPos, cam.orthographicSize);

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    private Vector3 ClampToBounds(Vector3 position, float orthoSize)
    {
        Bounds b = levelBounds.bounds;
        float halfH = orthoSize;
        float halfW = orthoSize * cam.aspect;

        float clampedX = Mathf.Clamp(position.x, b.min.x + halfW, b.max.x - halfW);
        float clampedY = Mathf.Clamp(position.y, b.min.y + halfH, b.max.y - halfH);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
