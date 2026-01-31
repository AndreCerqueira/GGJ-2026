using Andre.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : MonoBehaviour
{
    public static EnemySystem Instance { get; private set; }
    
    [SerializeField] private AreaViewCreator areaViewCreator;

    private enum TypeOfEnemy
    {
        Moves2,
        Moves1
    }
    private Dictionary<TypeOfEnemy, int> enemiesToSpawn;

    [HideInInspector] public List<EnemyView> EnemiesViews;

    private void Awake()
    {
        Instance = this;

        enemiesToSpawn = new Dictionary<TypeOfEnemy, int>() { 
            { TypeOfEnemy.Moves2, 1 }, 
            { TypeOfEnemy.Moves1, 1 } };
    }

    public void SpawnEnemies()
    {
        foreach (var entry in enemiesToSpawn)
        {
            TypeOfEnemy enemyType = entry.Key;
            int count = entry.Value;

            for (int i = 0; i < count; i++)
            {
                GameObject newEnemyGO = areaViewCreator.SpawnNewEnemy();
                
                EnemyView newEnemyView = newEnemyGO.GetComponent<EnemyView>();
                EnemiesViews.Add(newEnemyView);

                switch (enemyType)
                {
                    case TypeOfEnemy.Moves2:
                        newEnemyView.MOVES_PER_TURN = 2;
                        break;

                    case TypeOfEnemy.Moves1:
                        newEnemyView.MOVES_PER_TURN = 1;
                        break;
                }
            }
        }

    }

    public void ManageEnemiesTurn()
    {
        StartCoroutine(ManageEnemiesTurnCoroutine());
    }

    private IEnumerator ManageEnemiesTurnCoroutine()
    {
        for (int i = 0; i < EnemiesViews.Count; i++)
        {
            EnemiesViews[i].OnEnemyTurn();

            if (i < EnemiesViews.Count - 1)
                yield return EnemiesViews[i].WaitEnemyTurn();
        }
    }
}
