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
    }

    public void SpawnEnemies()
    {
        int numOfEnemiesMoves2 = (int)((GameSystem.turnNum + 1) / 2f);
        int numOfEnemiesMoves1 = (GameSystem.turnNum + 1) % 2;
        enemiesToSpawn = new Dictionary<TypeOfEnemy, int>() {
            { TypeOfEnemy.Moves2, numOfEnemiesMoves2 },
            { TypeOfEnemy.Moves1, numOfEnemiesMoves1 } };

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
                        
                        var anim = newEnemyView.GetComponentInChildren<Animator>();
                        if (anim != null)
                        {
                            anim.SetBool("IsPequeno", true);
                        }
                        
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

            yield return EnemiesViews[i].WaitEnemyTurn();
        }

        bool theresEvent = Random.Range(0, 5) < 2;
        if (theresEvent)
            EventSystem.Instance.AddEventEffect();
    }
}
