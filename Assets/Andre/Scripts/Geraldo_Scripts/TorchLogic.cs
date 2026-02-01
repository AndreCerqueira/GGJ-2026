using UnityEngine;

/// <summary>
/// Torch behaviour: controls a Spot Light (child) and toggles it based on who is standing on the tile.
/// - Light is OFF by default.
/// - When Player steps on the tile the torch lights up.
/// - When Enemy steps on the tile the torch becomes unlit.
///
/// Requirements: the tile GameObject should have a Collider with "Is Trigger" enabled (or a 2D trigger collider).
/// The colliding objects should have `PlayerView` and `EnemyView` components (or be parented under objects that do).
/// </summary>
public class TorchLogic : MonoBehaviour
{
    [Tooltip("Optional. If not set, the script will search for a child Light (Spot) at Awake time.")]
    [SerializeField] private Light _spotLight;

    // counters to handle multiple objects on the same tile
    private int _playersOnTile = 0;
    private int _enemiesOnTile = 0;

    void Reset()
    {
        // Try to auto-assign a child light in the editor when adding the component
        if (_spotLight == null)
        {
            _spotLight = GetComponentInChildren<Light>();
        }
    }

    void Awake()
    {
        if (_spotLight == null)
        {
            _spotLight = GetComponentInChildren<Light>();
        }
    }

    void Start()
    {
        if (_spotLight == null)
        {
            Debug.LogWarning($"[TorchLogic] No Light (Spot) found under '{gameObject.name}'. Please add a Light child or assign it in the inspector.");
        }
        else
        {
            // ensure torch is off by default
            _spotLight.enabled = false;
        }
    }

    // We avoid compile-time dependencies on PlayerView/EnemyView types by resolving them via reflection at runtime.
    private static System.Type _playerViewType;
    private static System.Type _enemyViewType;

    private System.Type GetTypeByName(string name)
    {
        // quick try
        var t = System.Type.GetType(name);
        if (t != null) return t;

        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var a in assemblies)
        {
            try
            {
                var types = a.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == name) return types[i];
                }
            }
            catch { /* skip assemblies that can't be reflected */ }
        }

        return null;
    }

    private bool IsPlayerCollider(Collider c)
    {
        if (c == null) return false;
        if (_playerViewType == null) _playerViewType = GetTypeByName("PlayerView");
        if (_playerViewType != null) return c.GetComponentInParent(_playerViewType) != null;
        // fallback to tag
        return c.CompareTag("Player");
    }

    private bool IsEnemyCollider(Collider c)
    {
        if (c == null) return false;
        if (_enemyViewType == null) _enemyViewType = GetTypeByName("EnemyView");
        if (_enemyViewType != null) return c.GetComponentInParent(_enemyViewType) != null;
        // fallback to tag
        return c.CompareTag("Enemy");
    }

    private void UpdateLightState()
    {
        if (_spotLight == null) return;

        // Enemy presence overrides player: if any enemy is on tile, light is off.
        bool shouldBeLit = (_enemiesOnTile == 0) && (_playersOnTile > 0);
        _spotLight.enabled = shouldBeLit;
    }

    // 3D trigger handlers
    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            _playersOnTile++;
            UpdateLightState();
        }
        else if (IsEnemyCollider(other))
        {
            _enemiesOnTile++;
            UpdateLightState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            _playersOnTile = Mathf.Max(0, _playersOnTile - 1);
            UpdateLightState();
        }
        else if (IsEnemyCollider(other))
        {
            _enemiesOnTile = Mathf.Max(0, _enemiesOnTile - 1);
            UpdateLightState();
        }
    }


    /// <summary>
    /// Force lamp state from code (useful for debugging or special cases).
    /// </summary>
    public void ForceLight(bool on)
    {
        if (_spotLight != null)
            _spotLight.enabled = on;
    }
}
