using Andre.Scripts;
using Andre.Scripts.Masks.Base;
using Andre.Scripts.Systems;
using Andre.Scripts.Toasts;
using Andre.Scripts.UI;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance { get; private set; }

    private enum EventType
    {
        ForgottenRelics,
        SharedResolve,
        CrushingPresence,
        DistortedReality,
        PredatoryLeap
    }
    private static readonly int EventTypeCount = System.Enum.GetValues(typeof(EventType)).Length;

    private Dictionary<EventType, int> activeEvents = new();

    [SerializeField] private ToastPresetSO _eventToastPreset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameSystem.Instance.OnPlayerTurn += OnPlayerTurn;
    }

    public void AddEventEffect()
    {
        EventType chosenEvent = (EventType)Random.Range(0, EventTypeCount);

        switch (chosenEvent)
        {
            case EventType.ForgottenRelics:
                ToastSystem.Instance.Show("Forgotten Relics\nSpawn extra Masks", _eventToastPreset);
                int numberOfMasks = Random.Range(1, 3);
                for (var i = 0; i < numberOfMasks; i++)
                    MaskSpawnerSystem.Instance.SpawnNewMask();
                break;
            case EventType.SharedResolve:
                ToastSystem.Instance.Show("Shared Resolve\nMove 1 More", _eventToastPreset);
                activeEvents[chosenEvent] = 1;
                AreaMovementSystem.Instance.playerMoves++;
                break;
            case EventType.CrushingPresence:
                ToastSystem.Instance.Show("Crushing Presence\nMove 1 Less", _eventToastPreset);
                activeEvents[chosenEvent] = 1;
                AreaMovementSystem.Instance.playerMoves--;
                break;
            case EventType.DistortedReality:
                ToastSystem.Instance.Show("Distorted Reality\nTeleport All Matter", _eventToastPreset);
                //Teleport to random tiles:
                //players
                PlayerView[] playersViews = FindObjectsByType<PlayerView>(FindObjectsSortMode.None);
                foreach (var playerView in playersViews)
                {
                    AreaView areaView = playerView.transform.parent.parent.GetComponent<AreaView>();
                    TeleportToRandomSpot(playerView.transform, areaView);
                }

                //masks
                MaskInstance[] maskInstances = FindObjectsByType<MaskInstance>(FindObjectsSortMode.None);
                foreach (var maskInstance in maskInstances)
                {
                    AreaView areaView = maskInstance.transform.parent.parent.GetComponent<AreaView>();
                    TeleportToRandomSpot(maskInstance.transform, areaView, true);
                }

                //Demons
                EnemyView[] enemyViews = FindObjectsByType<EnemyView>(FindObjectsSortMode.None);
                foreach (var enemyView in enemyViews)
                {
                    AreaView areaView = enemyView.transform.parent.parent.GetComponent<AreaView>();
                    TeleportToRandomSpot(enemyView.transform, areaView);
                }
                break;
            case EventType.PredatoryLeap:
                ToastSystem.Instance.Show("Predatory Leap\nJumps to the nearest child", _eventToastPreset);
                TryEnemyTwoCellJump();
                break;
        }
    }

    private void RemoveEventEffect(EventType eventTypeToRemove)
    {
        switch (eventTypeToRemove)
        {
            //case EventType.CrushingPresence:
            //    AreaMovementSystem.Instance.playerMoves++;
            //    break;
        }
    }

    private void OnPlayerTurn()
    {
        List<EventType> eventsToRemove = new();
        var keys = new List<EventType>(activeEvents.Keys);

        foreach (var eventType in keys)
        {
            activeEvents[eventType]--;

            if (activeEvents[eventType] < 0)
            {
                RemoveEventEffect(eventType);
                eventsToRemove.Add(eventType);
            }
        }

        foreach (var eventType in eventsToRemove)
            activeEvents.Remove(eventType);
    }


    //region Effects

    private void TeleportToRandomSpot(Transform transform, AreaView area, bool useMaskContainer = false)
    {
        if (GridSystem.Instance == null) return;

        var varValidSpots = new List<AreaView>();

        // 1. Procurar todas as áreas válidas no Grid
        foreach (var varArea in GridSystem.Instance.GetAllAreas())
        {
            // Regras: não ser a área atual, não ter obstáculos, não ter ninguém
            if (varArea != area && !varArea.HasObstacle && !varArea.IsOccupied)
            {
                varValidSpots.Add(varArea);
            }
        }

        // 2. Escolher uma aleatória
        if (varValidSpots.Count > 0)
        {
            var varRandomIndex = Random.Range(0, varValidSpots.Count);
            var varTargetArea = varValidSpots[varRandomIndex];

            // 3. Teleportar
            // Ao mudar o parent, o sistema de jogo saberá que o jogador está na nova área
            transform.SetParent(useMaskContainer? varTargetArea.MaskContainer : varTargetArea.CharacterContainer);

            // Resetar a posição local para (0,0,0) para ficar centralizado no novo tile
            transform.localPosition = Vector3.zero;

            Debug.Log($"[TeleportEffect] Teleportado de {area.name} para {varTargetArea.name}");
        }
        else
        {
            Debug.LogWarning("[TeleportEffect] Não há lugares livres para teleportar!");
        }
    }

    private List<Vector2Int> GetValidTwoCellJumps(EnemyView enemy, Vector2Int from)
    {
        List<Vector2Int> results = new();

        Vector2Int[] offsets =
        {
            new( 2, 0), new(-2, 0),
            new( 0, 2), new( 0,-2),
            new( 1, 1), new( 1,-1),
            new(-1, 1), new(-1,-1)
        };

        foreach (var offset in offsets)
        {
            var target = from + offset;

            if (!IsInsideGrid(target))
                continue;

            if (!GridSystem.Instance.TryGetArea(target, out var area))
                continue;

            if (!enemy.IsValidMove(area.Coordinate))
                continue;

            results.Add(target);
        }

        return results;
    }
    private bool IsInsideGrid(Vector2Int c)
    {
        AreaViewCreator areaViewCreator = FindFirstObjectByType<AreaViewCreator>();
        return c.x >= 0 && c.y >= 0 &&
               c.x <= areaViewCreator._gridSize && c.y <= areaViewCreator._gridSize;
    }
    public void TryEnemyTwoCellJump()
    {
        EnemyView[] enemies =
            FindObjectsByType<EnemyView>(FindObjectsSortMode.None);

        if (enemies.Length == 0) return;

        // Sort enemies by distance to nearest player
        System.Array.Sort(enemies, (a, b) =>
        {
            float da = DistanceToNearestPlayer(a);
            float db = DistanceToNearestPlayer(b);
            return da.CompareTo(db);
        });

        foreach (var enemy in enemies)
        {
            var enemyArea = enemy.GetComponentInParent<AreaView>();
            if (enemyArea == null) continue;

            var player = enemy.GetNearestPlayer(enemyArea.Coordinate);
            if (player == null) continue;

            var jumps = GetValidTwoCellJumps(enemy, enemyArea.Coordinate);

            // Pick the jump closest to the target player
            Vector2Int targetCoord =
                player.GetComponentInParent<AreaView>().Coordinate;

            Vector2Int? bestJump = null;
            float bestDist = float.MaxValue;

            foreach (var jump in jumps)
            {
                float d = Vector2Int.Distance(jump, targetCoord);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestJump = jump;
                }
            }

            if (bestJump.HasValue &&
                GridSystem.Instance.TryGetArea(bestJump.Value, out var area))
            {
                enemy.MoveToArea(area);
                return; // IMPORTANT: only one enemy acts
            }
        }
        // If we reach here → no enemy could do it
    }
    private float DistanceToNearestPlayer(EnemyView enemy)
    {
        var area = enemy.GetComponentInParent<AreaView>();
        if (area == null) return float.MaxValue;

        var player = enemy.GetNearestPlayer(area.Coordinate);
        if (player == null) return float.MaxValue;

        var playerArea = player.GetComponentInParent<AreaView>();
        return Vector2Int.Distance(area.Coordinate, playerArea.Coordinate);
    }

    //endregion
}
