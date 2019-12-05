using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    void Start() {
        
    }

    void Update() {
        
    }

    public void StartButton() {
        SceneManager.LoadSceneAsync("Game Permanents");
    }
}
