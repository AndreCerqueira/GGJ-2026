using System;
using UnityEngine;

public enum GameState
{
    MAINMENU,
    START,
    PLAYERTURN,
    ENEMYTURN,
    WIN,
    LOSE
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    /// <summary>
    /// Returns the current Instance or tries to find one in the scene and assign it.
    /// Use this when callers may run before Awake() has set Instance.
    /// </summary>
    public static GameSystem GetOrFindInstance()
    {
        if (Instance != null) return Instance;

        var found = FindObjectOfType<GameSystem>();
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
        //Debug.Log($"[GameSystem] EnemyTurn() called (current state: {state})");
        state = GameState.ENEMYTURN;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("Enemy's Turn!");
    }

    public void WinGame()
    {
        //Debug.Log($"[GameSystem] WinGame() called (current state: {state})");
        state = GameState.WIN;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("You Win!");
    }

    void LoseGame()
    {
        //Debug.Log($"[GameSystem] LoseGame() called (current state: {state})");
        state = GameState.LOSE;
        //Debug.Log($"[GameSystem] state -> {state}");
        Debug.Log("You Lose!");
    }
}