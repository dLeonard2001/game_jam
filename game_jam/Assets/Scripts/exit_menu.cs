using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exit_menu : MonoBehaviour
{
    public playerController player;

    public void resumeGame()
    {
        player.pauseGame();
    }
}
