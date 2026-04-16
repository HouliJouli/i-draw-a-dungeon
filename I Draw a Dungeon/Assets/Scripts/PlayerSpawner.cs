using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        int count = Mathf.Min(playerPrefabs.Length, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            if (playerPrefabs[i] == null || spawnPoints[i] == null) continue;
            Instantiate(playerPrefabs[i], spawnPoints[i].position, Quaternion.identity);
        }
    }
}
