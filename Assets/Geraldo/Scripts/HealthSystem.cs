using System.Linq;
using UnityEngine;

/// <summary>
/// Attach to player GameObjects. Kills the player when it collides with an Enemy (EnemyView).
/// If all players are dead, tells GameSystem to end the game (LoseGame).
/// </summary>
public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private int _lives = 1; // number of hits before death
    [Header("Tombstone")]
    [SerializeField] private GameObject _tombstonePrefab;
    [Tooltip("Player prefab to use when respawning from this tombstone. Assign the player prefab here so tombstone can respawn it later.")]
    [SerializeField] private GameObject _playerPrefabForRespawn;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        IsDead = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if we hit an enemy, take damage / die
        if (collision.collider.GetComponent<Andre.Scripts.EnemyView>() != null)
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Andre.Scripts.EnemyView>() != null)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        _lives -= amount;
        Debug.Log($"[HealthSystem] {gameObject.name} took {amount} damage. Lives now {_lives}");

        if (_lives <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"[HealthSystem] {gameObject.name} died.");
        // Spawn tombstone at this position (if assigned), otherwise create a temporary cube
        SpawnTombstone();

        // Destroy the player GameObject so it is removed from the scene and registries
        Destroy(gameObject);

        // Check if all players are dead
        CheckAllPlayersDead();
    }

    private void SpawnTombstone()
    {
        var parent = transform.parent; // usually CharacterContainer (AreaView)
        if (_tombstonePrefab == null)
        {
            Debug.LogWarning("[HealthSystem] No tombstone prefab assigned; skipping tombstone spawn.");
            return;
        }

        var tomb = Instantiate(_tombstonePrefab, transform.position, Quaternion.identity, parent);

        // Try to set Tombstone data (player prefab) so respawn is possible later
        var tombComp = tomb.GetComponent<Tombstone>();
        if (tombComp == null)
        {
            tombComp = tomb.AddComponent<Tombstone>();
        }

        tombComp.PlayerPrefab = _playerPrefabForRespawn;
        tombComp.OriginalPlayerName = gameObject.name;
    }

    /// <summary>
    /// Kill immediately (force die) — convenience API for enemies.
    /// </summary>
    public void Kill()
    {
        if (IsDead) return;
        Die();
    }

    public bool CheckAllPlayersDead()
    {
        // Use the PlayerView registry (keeps track of players without Find* calls)
        var players = Andre.Scripts.PlayerView.AllPlayers;
        if (players == null)
        {
            Debug.LogWarning("[HealthSystem] Player registry is null when checking for loss condition.");
            return false;
        }

        // If the registry is empty it usually means all players were removed (dead),
        // so treat that as the lose condition rather than silently returning.
        if (players.Count == 0)
        {
            Debug.Log("[HealthSystem] Player registry empty -> assuming all players dead. Calling LoseGame().");
            var gsEmpty = GameSystem.GetOrFindInstance();
            if (gsEmpty != null)
            {
                gsEmpty.LoseGame();
            }
            else
            {
                Debug.LogError("[HealthSystem] Could not find GameSystem to report LoseGame() (empty registry path).");
            }
            return true;
        }

        var anyAlive = players.Any(p =>
        {
            var hs = p.GetComponent<HealthSystem>();
            return hs != null && !hs.IsDead;
        });

        if (!anyAlive)
        {
            Debug.Log("[HealthSystem] All players dead - calling GameSystem.LoseGame()");
            var gs = GameSystem.GetOrFindInstance();
            if (gs != null)
            {
                gs.LoseGame();
            }
            else
            {
                Debug.LogError("[HealthSystem] Could not find GameSystem to report LoseGame().");
            }
            return true;
        }

        return false;
    }
}
