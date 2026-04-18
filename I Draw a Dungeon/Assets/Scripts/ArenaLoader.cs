using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaLoader : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private CameraFitLevel cameraFit;
    [SerializeField] private DoorIndicator doorIndicator;
    [SerializeField] private string[] arenaSceneNames;

    private int _currentIndex = 0;
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
        // registra conteúdo da primeira arena que já está carregada
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

        // carrega próxima arena
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

        // aguarda a spike wall da arena ANTERIOR chegar no limite
        _wallReachedEnd = false;
        Debug.Log($"[ArenaLoader] Aguardando SpikeWall para descarregar: {previousScene}");
        yield return new WaitUntil(() => _wallReachedEnd);

        if (SceneManager.GetSceneByName(previousScene).isLoaded)
        {
            Debug.Log($"[ArenaLoader] Descarregando arena anterior: {previousScene}");
            yield return SceneManager.UnloadSceneAsync(previousScene);
            Debug.Log($"[ArenaLoader] Arena descarregada: {previousScene}");
        }

        // registra conteúdo da nova arena após Arena1 ser descarregada
        yield return RegisterArenaContent(nextScene);

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
