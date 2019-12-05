using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PermanentController : MonoBehaviour {

    void Start() {
        SceneManager.LoadSceneAsync("Basic Level");
    }

}
