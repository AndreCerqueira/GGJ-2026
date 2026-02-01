using Andre.Scripts;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance { get; private set; }

    private enum EventType
    {
        ForgottenRelics
    }
    private static readonly int EventTypeCount = System.Enum.GetValues(typeof(EventType)).Length;

    private void Awake()
    {
        Instance = this;
    }

    public void MakeEvent()
    {
        EventType chosenEvent = (EventType)Random.Range(0, EventTypeCount);

        switch (chosenEvent)
        {
            case EventType.ForgottenRelics:
                int numberOfMasks = Random.Range(1, 3);
                for (var i = 0; i < numberOfMasks; i++)
                    MaskSpawnerSystem.Instance.SpawnNewMask();
                break;
        }
    }
}
