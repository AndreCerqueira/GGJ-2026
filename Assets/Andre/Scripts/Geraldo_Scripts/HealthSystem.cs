using System;
using System.Linq;
using Andre.Scripts.Systems;
using UnityEngine;

namespace Andre.Scripts
{
    public class HealthSystem : MonoBehaviour
    {
        public static HealthSystem Instance { get; private set; }
        
        public event Action OnDeath;

        [Header("Health")]
        [SerializeField] private int _lives = 1; 
        
        [Header("Tombstone & Visuals")]
        [SerializeField] private GameObject _tombstonePrefab;
        [Tooltip("Prefab do jogador a usar ao reviver desta lápide.")]
        [SerializeField] private GameObject _playerPrefabForRespawn;
        
        // --- NOVO CAMPO ---
        [Tooltip("Sprite que representa este jogador morto (para aparecer na lápide).")]
        [SerializeField] private Sprite _deadSprite; 

        public bool IsDead { get; private set; }

        private void Awake()
        {
            IsDead = false;
        }

        // ... (Mantém o OnCollisionEnter, OnTriggerEnter, TakeDamage iguais) ...

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponent<EnemyView>() != null) TakeDamage(1);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<EnemyView>() != null) TakeDamage(1);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            _lives -= amount;
            if (_lives <= 0) Die();
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            Debug.Log($"[HealthSystem] {gameObject.name} morreu.");
            
            OnDeath?.Invoke();
            
            SpawnTombstone(); // Chama a função atualizada

            Destroy(gameObject);
            CheckAllPlayersDead();
        }

        private void SpawnTombstone()
        {
            var parent = transform.parent; 
            
            if (_tombstonePrefab == null)
            {
                Debug.LogWarning("[HealthSystem] Nenhum prefab de lápide atribuído; a ignorar spawn da lápide.");
                return;
            }

            var tomb = Instantiate(_tombstonePrefab, transform.position, Quaternion.identity, parent);

            var tombComp = tomb.GetComponent<Tombstone>();
            if (tombComp == null)
            {
                tombComp = tomb.AddComponent<Tombstone>();
            }

            // Configuração dos dados de respawn
            tombComp.PlayerPrefab = _playerPrefabForRespawn;
            tombComp.OriginalPlayerName = gameObject.name;

            // --- NOVA LINHA: Passa o sprite ---
            tombComp.SetDeadSprite(_deadSprite);
        }

        // ... (Mantém o Kill e CheckAllPlayersDead iguais) ...
        public void Kill() { if (!IsDead) Die(); }

        public bool CheckAllPlayersDead()
        {
            var players = PlayerView.AllPlayers;
            if (players == null || players.Count == 0)
            {
                var gsEmpty = GameSystem.GetOrFindInstance();
                if (gsEmpty != null) gsEmpty.LoseGame();
                return true;
            }

            var anyAlive = players.Any(p =>
            {
                var hs = p.GetComponent<HealthSystem>();
                return hs != null && !hs.IsDead;
            });

            if (!anyAlive)
            {
                var gs = GameSystem.GetOrFindInstance();
                if (gs != null) gs.LoseGame();
                return true;
            }
            return false;
        }
    }
}