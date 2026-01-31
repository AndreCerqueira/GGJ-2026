using System;
using UnityEngine;

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

    // Event raised when the GameSystem enters the enemy turn; subscribers (enemies) can react.
    public static event System.Action OnEnemyTurn;
    private int _pendingTurnActions = 0;
    [SerializeField] private float _turnStartDelay = 0.15f;
    [SerializeField] private float _turnEndDelay = 0.15f;

    /// <summary>
    /// Returns the current Instance or tries to find one in the scene and assign it.
    /// Use this when callers may run before Awake() has set Instance.
    /// </summary>
    public static GameSystem GetOrFindInstance()
    {
        if (Instance != null) return Instance;

        var found = UnityEngine.Object.FindAnyObjectByType<GameSystem>();
        
        if (found != null)
        {
            Instance = found;
            //Debug.Log($"[GameSystem] GetOrFindInstance() - found and assigned Instance from scene: {Instance.gameObject.name}");
            return Instance;
        }

        //Debug.LogError("[GameSystem] GetOrFindInstance() - no GameSystem found in scene.");
        return null;
    }

    // Event fired once GameSystem has finished Start() initialization
    private static event Action OnInitializedInternal;
    private bool _initialized = false;

    /// <summary>
    /// Register a callback to be called when GameSystem is initialized. If already initialized the callback is invoked immediately.
    /// </summary>
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
        // Persistent singleton pattern
        if (Instance != null && Instance != this)
        {
            //Debug.LogWarning("[GameSystem] Another GameSystem instance already exists - destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        //Debug.Log($"[GameSystem] Awake() - Instance set to {Instance.gameObject.name}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = GameState.MAINMENU;
        //Debug.Log($"[GameSystem] Start() - initial state set to {state}");
        StartGame();

        // Mark initialized and notify listeners
        _initialized = true;
        OnInitializedInternal?.Invoke();
        //Debug.Log("[GameSystem] Initialization complete - OnInitialized invoked.");
    }
    

    public void StartGame()
    {
        //Debug.Log($"[GameSystem] StartGame() called (current state: {state})");
        state = GameState.START;
       // Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("Game Started!");
        PlayerTurn();
    }

    public void PlayerTurn()
    {
        //Debug.Log($"[GameSystem] PlayerTurn() called (current state: {state})");
        state = GameState.PLAYERTURN;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("Player's Turn!");
    }

    public void EnemyTurn()
    {
        // start coroutine to allow listeners to perform async work (animations/movement)
        StartCoroutine(EnemyTurnRoutine());
    }
    
    private System.Collections.IEnumerator EnemyTurnRoutine()
    {
        Debug.Log($"[GameSystem] EnemyTurnRoutine() starting (current state: {state})");
        state = GameState.ENEMYTURN;
        Debug.Log($"[GameSystem] state -> {state}");
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

        // wait until all registered turn actions complete
        var safety = 0;
        while (_pendingTurnActions > 0)
        {
            yield return null;
            safety++;
            if (safety > 600) // ~10s at 60fps
            {
                Debug.LogWarning("[GameSystem] EnemyTurnRoutine waiting timed out waiting for pending actions.");
                break;
            }
        }

        if (_turnEndDelay > 0f) yield return new WaitForSeconds(_turnEndDelay);

        // proceed to events (and then player turn)
        if (HealthSystem.Instance.CheckAllPlayersDead() == true)
        {
            LoseGame();
        }
        else
        {
            Events(); 
        }
            
    }

    /// <summary>
    /// Register a pending action that GameSystem will wait for before finishing the current enemy turn.
    /// </summary>
    public void RegisterTurnAction()
    {
        _pendingTurnActions++;
        //Debug.Log($"[GameSystem] RegisterTurnAction -> pending={_pendingTurnActions}");
    }

    /// <summary>
    /// Mark previously registered turn action as completed.
    /// </summary>
    public void CompleteTurnAction()
    {
        _pendingTurnActions = Mathf.Max(0, _pendingTurnActions - 1);
        //Debug.Log($"[GameSystem] CompleteTurnAction -> pending={_pendingTurnActions}");
    }
  public void Events()
    {
        //Debug.Log($"[GameSystem] Events() called (current state: {state})");
        state = GameState.EVENTS;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("Events!");
        PlayerTurn();
    }

    public void WinGame()
    {
        //Debug.Log($"[GameSystem] WinGame() called (current state: {state})");
        state = GameState.WIN;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("You Win!");
    }

    public void LoseGame()
    {
        //Debug.Log($"[GameSystem] LoseGame() called (current state: {state})");
        state = GameState.LOSE;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("You Lose!");
        DeadScreen();
    }

    public void DeadScreen()
    {
        //Debug.Log($"[GameSystem] DeadScreen() called (current state: {state})");
        state = GameState.DEADSCREEN;
    }
}