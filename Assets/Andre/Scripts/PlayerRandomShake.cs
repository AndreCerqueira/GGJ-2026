using UnityEngine;

namespace Andre.Scripts
{
    public class PlayerRandomShake : MonoBehaviour
    {
        [Header("Animator Settings")]
        [Tooltip("O nome exato do Trigger criado no Animator Controller.")]
        [SerializeField] private string _shakeTriggerName = "Shake";

        [Header("Timing Settings")]
        [Tooltip("Tempo mínimo (em segundos) para esperar entre tremidas.")]
        [SerializeField] private float _minTimeBetweenShakes = 5f;

        [Tooltip("Tempo máximo (em segundos) para esperar entre tremidas.")]
        [SerializeField] private float _maxTimeBetweenShakes = 15f;

        [Header("Initial Timing")]
        [Tooltip("Tempo máximo para a PRIMEIRA tremida ao iniciar (gera valor entre 0 e este número). Ajuda a desincronizar múltiplos objetos.")]
        [SerializeField] private float _maxInitialDelay = 1f;

        private Animator _animator;
        private float _timer;
        private int _shakeTriggerHash;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator == null)
            {
                Debug.LogWarning($"[PlayerRandomShake] Animator não encontrado em {gameObject.name} ou seus filhos. O script será desativado.");
                enabled = false;
                return;
            }

            _shakeTriggerHash = Animator.StringToHash(_shakeTriggerName);

            // AQUI É A MUDANÇA:
            // Para a primeira vez, usamos um range que começa em 0 até o delay inicial configurado.
            // Isso evita que todos esperem obrigatoriamente o _minTimeBetweenShakes (5s) para começar.
            _timer = Random.Range(0f, _maxInitialDelay);
        }

        private void Update()
        {
            // Se o jogo estiver pausado (Time.timeScale = 0), o timer não roda.
            if (Time.timeScale == 0f) return;

            // Conta o tempo regressivamente
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                TriggerShakeAnimation();
                ResetTimer();
            }
        }

        private void TriggerShakeAnimation()
        {
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                 // Dispara o trigger no animator
                _animator.SetTrigger(_shakeTriggerHash);
            }
        }

        private void ResetTimer()
        {
            // Para as próximas vezes, usa o tempo normal (ex: entre 5 e 15s)
            _timer = Random.Range(_minTimeBetweenShakes, _maxTimeBetweenShakes);
        }
        
        public void StopShaking()
        {
            enabled = false;
        }
    }
}