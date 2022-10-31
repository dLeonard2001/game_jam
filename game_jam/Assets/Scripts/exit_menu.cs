using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class exit_menu : MonoBehaviour
{
    public Animator sceneTransition;
    public playerController player;

    public void resumeGame()
    {
        player.pauseGame();
    }

    public void loadMainMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(loadScene());
    }

    public void quitGame()
    {
        Time.timeScale = 1f;
        StartCoroutine(quitScene());
    }
    
    public IEnumerator quitScene()
    {
        
        sceneTransition.SetTrigger("exit");

        yield return new WaitForSeconds(4);

        Application.Quit();
    }

    public IEnumerator loadScene()
    {
        
        sceneTransition.SetTrigger("exit");

        yield return new WaitForSeconds(4);

        SceneManager.LoadScene("Main Menu");
    }
}
