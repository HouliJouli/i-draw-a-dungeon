using System;
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
    [SerializeField] private float safeDuration = 30f;
    [SerializeField] private float warningDuration = 10f;
    [SerializeField] private float transitionDuration = 15f;

    public ArenaState CurrentState { get; private set; }
    public event Action<ArenaState> OnArenaStateChanged;

    private float _timer;

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

    private void EnterState(ArenaState newState)
    {
        CurrentState = newState;

        _timer = newState switch
        {
            ArenaState.Safe       => safeDuration,
            ArenaState.Warning    => warningDuration,
            ArenaState.Transition => transitionDuration,
            _                     => 0f
        };

        Debug.Log($"[ArenaManager] Estado: {newState}");
        OnArenaStateChanged?.Invoke(newState);
    }
}
