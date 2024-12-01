using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void ChangeScenePlay()
    {
        StartCoroutine(waitToChangePlay());
    }

    public void ChangeSceneMenu()
    {
        StartCoroutine(waitToChangeMenu());
    }


    public void Quit_Game()
    {
        Application.Quit();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        if (GameManager.instance.gameState != GameState.ObjectSystem) Cursor.visible = false;
    }

    IEnumerator waitToChangePlay()
    {
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene(1);
    }

    IEnumerator waitToChangeMenu()
    {
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene(0);
        Cursor.visible = true;
    }
}
