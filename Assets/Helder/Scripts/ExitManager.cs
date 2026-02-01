using Andre.Scripts;
using MoreMountains.Feedbacks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitManager : MonoBehaviour
{
    private bool player1Saved = false;
    private bool player2Saved = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerGO = other.gameObject;

            if (playerGO.name.Contains("1"))
                player1Saved = true;
            else
                player2Saved = true;

            MakePlayerDisappear(playerGO);

            VerifyEndGameEscaping();
        }
    }

    private void MakePlayerDisappear(GameObject playerGO)
    {
        playerGO.GetComponent<SpriteRenderer>().enabled = false;
        playerGO.GetComponent<BoxCollider>().enabled = false;
        playerGO.transform.parent = null;

        playerGO.GetComponentInChildren<Light>().enabled = false;
        playerGO.GetComponentInChildren<SpriteRenderer>().enabled = false;
        playerGO.GetComponentInChildren<PlayerMaskDisplay>().enabled = false;
    }

    public bool VerifyEndGameEscaping()
    {
        bool endGame = false;

        if (AreAllAlivePlayersSaved())
        {
            GameSystem.Instance.EndGame();

            endGame = true;
        }

        return endGame;
    }

    private bool AreAllAlivePlayersSaved()
    {
        foreach (PlayerView playerView in PlayerView.AllPlayers)
        {
            GameObject playerGO = playerView.gameObject;

            HealthSystem playerHealth = playerGO.GetComponent<HealthSystem>();
            if (playerHealth.IsDead)
                continue;

            bool isPlayerOne = playerGO.name.Contains("1");
            bool playerSaved = isPlayerOne ? player1Saved : player2Saved;

            if (!playerSaved)
                return false; 
        }

        return true; 
    }

}
