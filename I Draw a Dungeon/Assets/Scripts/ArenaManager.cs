using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ArenaState
{
    Safe,
    Warning,
    Transition,
    Completed
}

public class ArenaManager : MonoBehaviour
{
    [BoxGroup("Durations"), MinValue(1f)]
    [SerializeField] private float safeDuration = 30f;

    [BoxGroup("Durations"), MinValue(1f)]
    [SerializeField] private float warningDuration = 10f;

    [BoxGroup("Durations"), MinValue(1f)]
    [SerializeField] private float transitionDuration = 15f;

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    public ArenaState CurrentState { get; private set; }

    [BoxGroup("Debug"), ShowInInspector, ReadOnly]
    [ProgressBar(0, "@_maxTimer", ColorMember = "@Color.cyan")]
    private float _timer;

    private float _maxTimer;

    public event Action<ArenaState> OnArenaStateChanged;

    private void Start()
    {
        EnterState(ArenaState.Safe);
    }

    private void Update()
    {
        if (CurrentState == ArenaState.Completed) return;

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
            AdvanceState();
    }

    private void AdvanceState()
    {
        switch (CurrentState)
        {
            case ArenaState.Safe:       EnterState(ArenaState.Warning);    break;
            case ArenaState.Warning:    EnterState(ArenaState.Transition); break;
            case ArenaState.Transition: EnterState(ArenaState.Completed);  break;
        }
    }

    [Button("Restart Arena"), BoxGroup("Debug")]
    public void Restart()
    {
        Debug.Log("[ArenaManager] Reiniciando para nova arena.");
        EnterState(ArenaState.Safe);
    }

    [Button("Force Warning"), BoxGroup("Debug")]
    private void ForceWarning() => EnterState(ArenaState.Warning);

    [Button("Force Transition"), BoxGroup("Debug")]
    private void ForceTransition() => EnterState(ArenaState.Transition);

    private void EnterState(ArenaState newState)
    {
        CurrentState = newState;

        _maxTimer = newState switch
        {
            ArenaState.Safe       => safeDuration,
            ArenaState.Warning    => warningDuration,
            ArenaState.Transition => transitionDuration,
            _                     => 0f
        };

        _timer = _maxTimer;

        Debug.Log($"[ArenaManager] Estado: {newState}");
        OnArenaStateChanged?.Invoke(newState);
    }
}
