using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // Garante que tem um SpriteRenderer
public class Tombstone : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _renderer;

    // Campos originais necessários para o Respawn
    public GameObject PlayerPrefab; 
    public string OriginalPlayerName;

    private void Awake()
    {
        // Tenta encontrar o renderer automaticamente se não estiver atribuído
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
    }

    // Novo método para definir o visual
    public void SetDeadSprite(Sprite sprite)
    {
        if (_renderer != null && sprite != null)
        {
            _renderer.sprite = sprite;
        }
        else if (_renderer == null)
        {
            Debug.LogWarning($"[Tombstone] Falta o SpriteRenderer na Lápide {gameObject.name}!");
        }
    }

    public bool CanRespawn 
    {
        get 
        {
            bool isValid = PlayerPrefab != null;
            if (!isValid) Debug.LogWarning($"[Tombstone] CanRespawn é FALSE nesta lápide ({gameObject.name}) porque o PlayerPrefab está vazio!");
            return isValid;
        }
    }

    public GameObject Respawn()
    {
        if (PlayerPrefab == null)
        {
            Debug.LogError("[Tombstone] Erro Crítico: Tentei reviver mas o PlayerPrefab desapareceu.");
            return null;
        }

        var go = Instantiate(PlayerPrefab, transform.position, Quaternion.identity, transform.parent);
        
        if (!string.IsNullOrEmpty(OriginalPlayerName))
            go.name = OriginalPlayerName;
        else
            go.name = PlayerPrefab.name;

        Debug.Log($"[Tombstone] Sucesso! {go.name} foi revivido.");
        
        Destroy(gameObject);
        return go;
    }
}