using UnityEngine;

public class Tombstone : MonoBehaviour
{
    // Removi o [HideInInspector] para conseguires ver na cena se necessário
    public int id = -1;
    public GameObject PlayerPrefab; 
    public string OriginalPlayerName;
    
    public GameObject Player1Prefab;
    public GameObject Player2Prefab;

    // Debug: Adiciona logs para entender porque é false
    public bool CanRespawn 
    {
        get
        {
            bool isValid = id == 0 || id == 1;
            return isValid;
        }
    }

    public GameObject Respawn()
    {
        var prefab = id == 1 ? Player1Prefab : Player2Prefab;
        var go = Instantiate(prefab, transform.position, Quaternion.identity, transform.parent);
        
        // Restaura o nome original ou usa o do prefab limpo
        if (!string.IsNullOrEmpty(OriginalPlayerName))
            go.name = OriginalPlayerName;
        else
            go.name = PlayerPrefab.name;

        Debug.Log($"[Tombstone] Sucesso! {go.name} foi revivido.");
        
        Destroy(gameObject);
        return go;
    }
}