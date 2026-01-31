using System;
using UnityEngine;
// Certifica-te de que o PlayerView está acessível (namespace correto)
using Andre.Scripts; 

public enum GameState
{
    MAINMENU,
    START,
    PLAYERTURN,
    ENEMYTURN,
    EVENTS,
    WIN,
    LOSE,
    DEADSCREEN
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    public static event System.Action OnEnemyTurn;
    private int _pendingTurnActions = 0;
    [SerializeField] private float _turnStartDelay = 0.15f;
    [SerializeField] private float _turnEndDelay = 0.15f;

    public static GameSystem GetOrFindInstance()
    {
        if (Instance != null) return Instance;

        var found = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
        
        if (found != null)
        {
            Instance = found;
            return Instance;
        }
        return null;
    }

    private static event Action OnInitializedInternal;
    private bool _initialized = false;

    public static void RegisterOnInitialized(Action callback)
    {
        if (callback == null) return;
        if (Instance != null && Instance._initialized)
        {
            callback.Invoke();
            return;
        }

        OnInitializedInternal += callback;
    }

    public GameState state;
    
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        state = GameState.MAINMENU;
        StartGame();

        _initialized = true;
        OnInitializedInternal?.Invoke();
    }
    
    public void StartGame()
    {
        state = GameState.START;
        Debug.Log("Game Started!");
        PlayerTurn();
    }

    public void PlayerTurn()
    {
        state = GameState.PLAYERTURN;
        Debug.Log("Player's Turn!");
    }

    public void EnemyTurn()
    {
        StartCoroutine(EnemyTurnRoutine());
    }
    
    private System.Collections.IEnumerator EnemyTurnRoutine()
    {
        Debug.Log($"[GameSystem] EnemyTurnRoutine() starting (current state: {state})");
        state = GameState.ENEMYTURN;
        Debug.Log("Enemy's Turn!");

        if (_turnStartDelay > 0f) yield return new WaitForSeconds(_turnStartDelay);

        try
        {
            OnEnemyTurn?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameSystem] Exception while invoking OnEnemyTurn: {ex}");
        }

        var safety = 0;
        while (_pendingTurnActions > 0)
        {
            yield return null;
            safety++;
            if (safety > 600)
            {
                Debug.LogWarning("[GameSystem] EnemyTurnRoutine timed out waiting for pending actions.");
                break;
            }
        }

        if (_turnEndDelay > 0f) yield return new WaitForSeconds(_turnEndDelay);

        if (PlayerView.AllPlayers.Count == 0)
        {
            LoseGame();
        }
        else
        {
            Events(); 
        }
    }

    public void RegisterTurnAction()
    {
        _pendingTurnActions++;
    }

    public void CompleteTurnAction()
    {
        _pendingTurnActions = Mathf.Max(0, _pendingTurnActions - 1);
    }

    public void Events()
    {
        state = GameState.EVENTS;
        Debug.Log("Events!");
        PlayerTurn();
    }

    public void WinGame()
    {
        state = GameState.WIN;
        Debug.Log("You Win!");
    }

    public void LoseGame()
    {
        state = GameState.LOSE;
        Debug.Log("You Lose!");
        DeadScreen();
    }

    public void DeadScreen()
    {
        state = GameState.DEADSCREEN;
    }
}