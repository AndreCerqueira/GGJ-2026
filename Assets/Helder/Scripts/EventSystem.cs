using Andre.Scripts;
using Andre.Scripts.Systems;
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
        CrushingPresence
    }
    private static readonly int EventTypeCount = System.Enum.GetValues(typeof(EventType)).Length;

    private Dictionary<EventType, int> activeEvents = new();

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
        EventType chosenEvent = (EventType)Random.Range(EventTypeCount-1, EventTypeCount);

        switch (chosenEvent)
        {
            case EventType.ForgottenRelics:
                int numberOfMasks = Random.Range(1, 3);
                for (var i = 0; i < numberOfMasks; i++)
                    MaskSpawnerSystem.Instance.SpawnNewMask();
                break;
            case EventType.SharedResolve:
                activeEvents[chosenEvent] = 1;
                AreaMovementSystem.Instance.playerMoves++;
                break;
            case EventType.CrushingPresence:
                activeEvents[chosenEvent] = 1;
                AreaMovementSystem.Instance.playerMoves--;
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
}
