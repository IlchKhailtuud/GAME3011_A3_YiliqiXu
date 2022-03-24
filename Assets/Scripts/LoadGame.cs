using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGame : MonoBehaviour
{
    [SerializeField] private DifficultySO difficultySo;
    
    public void LoadTheGame(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnEasyClicked()
    {
        difficultySo.GameDifficulty = Difficulty.EASY;
        SceneManager.LoadScene("Game");
    }

    public void OnNormalClicked()
    {
        difficultySo.GameDifficulty = Difficulty.NORMAL;
        SceneManager.LoadScene("Game");
    }
    
    public void OnHardClicked()
    {
        difficultySo.GameDifficulty = Difficulty.HARD;
        SceneManager.LoadScene("Game");
    }
}
