using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaLoader : MonoBehaviour
{
    [BoxGroup("References"), Required]
    [SerializeField] private ArenaManager arenaManager;

    [BoxGroup("References"), Required]
    [SerializeField] private CameraFitLevel cameraFit;

    [BoxGroup("References")]
    [SerializeField] private DoorIndicator doorIndicator;

    [BoxGroup("Arena Sequence")]
    [SerializeField] private string[] arenaSceneNames;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private int _currentIndex = 0;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    private bool _loadingInProgress;

    private bool _wallReachedEnd;
    private SpikeWallController _currentSpikeWall;

    private void OnEnable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged += OnArenaStateChanged;
    }

    private void OnDisable()
    {
        if (arenaManager != null)
            arenaManager.OnArenaStateChanged -= OnArenaStateChanged;

        UnsubscribeSpikeWall();
    }

    private void Start()
    {
        if (arenaSceneNames.Length > 0)
            StartCoroutine(RegisterArenaContent(arenaSceneNames[0]));
    }

    private void OnArenaStateChanged(ArenaState newState)
    {
        if (newState == ArenaState.Transition)
            StartCoroutine(TransitionRoutine());
    }

    private void OnWallReachedEnd()
    {
        _wallReachedEnd = true;
    }

    private IEnumerator TransitionRoutine()
    {
        if (_loadingInProgress) yield break;
        _loadingInProgress = true;

        int nextIndex = _currentIndex + 1;

        if (nextIndex >= arenaSceneNames.Length)
        {
            Debug.Log("[ArenaLoader] Não há próxima arena. Fim da dungeon.");
            _loadingInProgress = false;
            yield break;
        }

        string nextScene = arenaSceneNames[nextIndex];
        string previousScene = arenaSceneNames[_currentIndex];

        if (!SceneManager.GetSceneByName(nextScene).isLoaded)
        {
            Debug.Log($"[ArenaLoader] Carregando arena: {nextScene}");
            yield return SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
            Debug.Log($"[ArenaLoader] Arena carregada: {nextScene}");
        }
        else
        {
            Debug.LogWarning($"[ArenaLoader] Arena já estava carregada: {nextScene}");
        }

        _currentIndex = nextIndex;

        _wallReachedEnd = false;
        Debug.Log($"[ArenaLoader] Aguardando SpikeWall para descarregar: {previousScene}");
        yield return new WaitUntil(() => _wallReachedEnd);

        if (SceneManager.GetSceneByName(previousScene).isLoaded)
        {
            Debug.Log($"[ArenaLoader] Descarregando arena anterior: {previousScene}");
            yield return SceneManager.UnloadSceneAsync(previousScene);
            Debug.Log($"[ArenaLoader] Arena descarregada: {previousScene}");
        }

        yield return RegisterArenaContent(nextScene);

        Scene newScene = SceneManager.GetSceneByName(nextScene);
        ArenaContent newContent = FindArenaContent(newScene);
        bool isLastArena = newContent == null || newContent.SpikeWall == null;

        if (!isLastArena && arenaManager != null)
            arenaManager.Restart();
        else
            Debug.Log("[ArenaLoader] Última arena detectada — ArenaManager não reinicia.");

        _loadingInProgress = false;
    }

    private IEnumerator RegisterArenaContent(string sceneName)
    {
        yield return UpdateVisualReferences(sceneName);
        yield return UpdateSpikeWallReference(sceneName);
    }

    private IEnumerator UpdateVisualReferences(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        yield return new WaitUntil(() => scene.isLoaded);

        ArenaContent content = FindArenaContent(scene);
        if (content == null)
        {
            Debug.LogWarning($"[ArenaLoader] ArenaContent não encontrado na cena: {sceneName}");
            yield break;
        }

        if (cameraFit != null && content.CameraBounds != null)
            cameraFit.SetBounds(content.CameraBounds);

        if (cameraFit != null)
            cameraFit.SetTransitionBounds(content.TransitionBounds);

        if (doorIndicator != null && content.Door != null)
            doorIndicator.SetDoor(content.Door);

        Debug.Log($"[ArenaLoader] Referências visuais atualizadas: {sceneName}");
    }

    private IEnumerator UpdateSpikeWallReference(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        yield return new WaitUntil(() => scene.isLoaded);

        ArenaContent content = FindArenaContent(scene);
        if (content == null) yield break;

        UnsubscribeSpikeWall();
        _currentSpikeWall = content.SpikeWall;
        if (_currentSpikeWall != null)
            _currentSpikeWall.OnWallReachedEnd += OnWallReachedEnd;

        Debug.Log($"[ArenaLoader] SpikeWall atualizada: {sceneName}");
    }

    private ArenaContent FindArenaContent(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            ArenaContent content = root.GetComponentInChildren<ArenaContent>();
            if (content != null) return content;
        }
        return null;
    }

    private void UnsubscribeSpikeWall()
    {
        if (_currentSpikeWall != null)
            _currentSpikeWall.OnWallReachedEnd -= OnWallReachedEnd;
        _currentSpikeWall = null;
    }
}
