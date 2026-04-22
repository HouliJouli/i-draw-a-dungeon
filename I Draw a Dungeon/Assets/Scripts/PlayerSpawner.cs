using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [BoxGroup("Setup")]
    [SerializeField] private GameObject[] playerPrefabs;

    [BoxGroup("Setup")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        int count = Mathf.Min(playerPrefabs.Length, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            if (playerPrefabs[i] == null || spawnPoints[i] == null) continue;
            GameObject player = Instantiate(playerPrefabs[i], spawnPoints[i].position, Quaternion.identity);
            StartCoroutine(AssignDevices(player));
        }
    }

    private IEnumerator AssignDevices(GameObject player)
    {
        yield return null;

        if (!player.TryGetComponent(out PlayerInput playerInput)) yield break;

        string scheme = playerInput.defaultControlScheme;

        playerInput.neverAutoSwitchControlSchemes = true;

        if (scheme == "Keyboard&Mouse" && Keyboard.current != null && Mouse.current != null)
            playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        else if (scheme == "Gamepad" && Gamepad.all.Count > 0)
            playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);

    }
}
