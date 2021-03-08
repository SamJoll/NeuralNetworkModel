using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public void StartPredict()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("PredictScene");
    }
    public void StartTrain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainScene");
    }
    public void ExitMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}
