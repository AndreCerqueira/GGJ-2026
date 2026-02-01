using System;
using UnityEngine;
// Certifica-te de que o PlayerView está acessível (namespace correto)
using Andre.Scripts;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using MoreMountains.Feedbacks;
using UnityEngine.SceneManagement;
using DG.Tweening;

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

    [HideInInspector] public static int turnNum = 1;

    public event Action OnPlayerTurn;
    public event System.Action OnEnemyTurn;
    private int _pendingTurnActions = 0;
    [SerializeField] private float _turnStartDelay = 0.15f;
    [SerializeField] private float _turnEndDelay = 0.15f;

    [SerializeField] private MMF_Player _musicFeedback;
    [Header("Toast Presets")]
    [SerializeField] private ToastPresetSO _playerTurnPreset;
    [SerializeField] private ToastPresetSO _enemyTurnPreset;

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

    public static bool gameEnd = false;
    private static bool allDied = false;

    public void Awake()
    {
        if (gameEnd || allDied)
        {
            gameEnd = false;

            if (!allDied)
            {
                GameSystem.turnNum++;

                GameObject startGameFeedbackGO = GameObject.Find("START_GAME_FEEDBACK");
                MMF_Player mmfPlayer = startGameFeedbackGO.GetComponent<MMF_Player>();
                mmfPlayer.PlayFeedbacks();

                DOVirtual.DelayedCall(1f, () =>
                {
                    AreaViewCreator areaViewCreator = GameObject.FindFirstObjectByType<AreaViewCreator>();
                    areaViewCreator.Initialize(true);
                    
                    _musicFeedback?.PlayFeedbacks();
                });
            }

            allDied = false;
        }

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
        if (state == GameState.EVENTS)
        {
            if (Andre.Scripts.UI.ToastSystem.Instance != null)
            {
                Andre.Scripts.UI.ToastSystem.Instance.Show("Your Turn", _playerTurnPreset);
            }
        }

        state = GameState.PLAYERTURN;
        Debug.Log("Player's Turn!");

        // Dispara o evento para desbloquear os botões
        OnPlayerTurn?.Invoke();
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
        ToastSystem.Instance.Show("Enemy Turn", _enemyTurnPreset);

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
        DeadScreen();

        GameObject gameOverFeedbackGO = GameObject.Find("SHOW_GAMEOVER_FEEDBACK");
        MMF_Player mmfPlayer = gameOverFeedbackGO.GetComponent<MMF_Player>();
        mmfPlayer.PlayFeedbacks();

        DOVirtual.DelayedCall(3f, () =>
        {
            Instance.EndGame(true);
        });
    }

    public void EndGame(bool allDead)
    {
        allDied = allDead;
        gameEnd = !allDied;

        StopAllCoroutines();

        GameObject startGameFeedbackGO = GameObject.Find("RELOAD_SCENE_FEEDBACK");
        MMF_Player mmfPlayer = startGameFeedbackGO.GetComponent<MMF_Player>();
        mmfPlayer.PlayFeedbacks();
    }

    public void DeadScreen()
    {
        state = GameState.DEADSCREEN;
    }
}