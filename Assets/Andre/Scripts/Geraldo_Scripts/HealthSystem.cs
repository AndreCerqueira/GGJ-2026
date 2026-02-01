using System.Linq;
using Andre.Scripts.Systems; // Para aceder ao GameSystem e GridSystem se necessário
using UnityEngine;

namespace Andre.Scripts
{
    /// <summary>
    /// Anexa aos GameObjects do jogador. Mata o jogador quando colide com um Inimigo (EnemyView).
    /// Se todos os jogadores estiverem mortos, diz ao GameSystem para terminar o jogo (LoseGame).
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        public static HealthSystem Instance { get; private set; }

        [Header("Health")]
        [SerializeField] private int _lives = 1; // número de vidas antes da morte
        
        [Header("Tombstone")]
        [SerializeField] private GameObject _tombstonePrefab;
        [Tooltip("Prefab do jogador a usar ao reviver desta lápide. Atribua o prefab do jogador aqui.")]
        [SerializeField] private GameObject _playerPrefabForRespawn;

        public int id;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            IsDead = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponent<EnemyView>() != null)
            {
                TakeDamage(1);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<EnemyView>() != null)
            {
                TakeDamage(1);
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            _lives -= amount;
            Debug.Log($"[HealthSystem] {gameObject.name} levou {amount} de dano. Vidas restantes: {_lives}");

            if (_lives <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            Debug.Log($"[HealthSystem] {gameObject.name} morreu.");
            
            SpawnTombstone();

            // Destrói o GameObject do jogador, removendo-o da cena e dos registos
            Destroy(gameObject);

            ExitManager exitManager = FindFirstObjectByType<ExitManager>();
            if (exitManager.VerifyEndGameEscaping())
                return;

            // Verifica se todos morreram
            CheckAllPlayersDead();
        }

        private void SpawnTombstone()
        {
            var parent = transform.parent; // geralmente CharacterContainer (AreaView)
            
            if (_tombstonePrefab == null)
            {
                Debug.LogWarning("[HealthSystem] Nenhum prefab de lápide atribuído; a ignorar spawn da lápide.");
                return;
            }

            var tomb = Instantiate(_tombstonePrefab, transform.position, Quaternion.identity, parent);

            // Configura os dados da Lápide para respawn futuro
            var tombComp = tomb.GetComponent<Tombstone>();
            if (tombComp == null)
            {
                tombComp = tomb.AddComponent<Tombstone>();
            }

            tombComp.id = id;
            tombComp.OriginalPlayerName = gameObject.name;
        }

        /// <summary>
        /// Mata imediatamente (force die) — API de conveniência para inimigos.
        /// </summary>
        public void Kill()
        {
            if (IsDead) return;
            Die();
        }

        public bool CheckAllPlayersDead()
        {
            // Usa o registo do PlayerView (mantém rasto dos jogadores sem chamadas Find*)
            var players = PlayerView.AllPlayers;
            
            if (players == null)
            {
                Debug.LogWarning("[HealthSystem] Registo de jogadores é null ao verificar condição de derrota.");
                return false;
            }

            // Se o registo está vazio, significa que todos os jogadores foram removidos (mortos e destruídos)
            if (players.Count == 0)
            {
                Debug.Log("[HealthSystem] Registo de jogadores vazio -> assumindo todos mortos. A chamar LoseGame().");
                var gsEmpty = GameSystem.GetOrFindInstance();
                if (gsEmpty != null)
                {
                    gsEmpty.LoseGame();
                }
                return true;
            }

            // Verifica se existe algum jogador que NÃO esteja morto
            // Nota: Como Destroy() não é imediato no mesmo frame, podemos ainda ter objetos marcados como IsDead
            var anyAlive = players.Any(p =>
            {
                var hs = p.GetComponent<HealthSystem>();
                return hs != null && !hs.IsDead;
            });

            if (!anyAlive)
            {
                Debug.Log("[HealthSystem] Todos os jogadores mortos - a chamar GameSystem.LoseGame()");
                var gs = GameSystem.GetOrFindInstance();
                if (gs != null)
                {
                    gs.LoseGame();
                }
                return true;
            }

            return false;
        }
    }
}