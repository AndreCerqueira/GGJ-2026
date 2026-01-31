using System.Linq;
using UnityEngine;

/// <summary>
/// Attach to player GameObjects. Kills the player when it collides with an Enemy (EnemyView).
/// If all players are dead, tells GameSystem to end the game (LoseGame).
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _lives = 1; // number of hits before death

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

        // disable visuals / collisions (simple approach)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        // Optionally destroy the GameObject after a delay
        // Destroy(gameObject);

        // Check if all players are dead
        CheckAllPlayersDead();
    }

    private void CheckAllPlayersDead()
    {
        // Use the PlayerView registry (keeps track of players without Find* calls)
        var players = Andre.Scripts.PlayerView.AllPlayers;
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[HealthSystem] No players registered when checking for loss condition.");
            return;
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
        }
    }
}
