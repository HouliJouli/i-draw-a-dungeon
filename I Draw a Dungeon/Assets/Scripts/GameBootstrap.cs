using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstArenaScene = "Arena1";

    private IEnumerator Start()
    {
        if (!SceneManager.GetSceneByName(firstArenaScene).isLoaded)
        {
            Debug.Log($"[GameBootstrap] Carregando primeira arena: {firstArenaScene}");
            yield return SceneManager.LoadSceneAsync(firstArenaScene, LoadSceneMode.Additive);
            Debug.Log($"[GameBootstrap] Arena carregada: {firstArenaScene}");
        }
    }
}
