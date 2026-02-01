using UnityEngine;

public class MenuSound : MonoBehaviour
{
    public AudioSource menuSource;

    void OnEnable()
    {
        menuSource.Play();
    }
}