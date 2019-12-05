using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour {

    public bool fadeInOnStart;

    private Image image;

    void Start() {
        image = GetComponent<Image>();

        if (fadeInOnStart) {
            StartCoroutine("FadeIn");
        }
    }

    private IEnumerator FadeIn() {
        yield return new WaitForSeconds(.5f);
        float i = 1;
        while (i > 0) {
            image.color = new Color(0, 0, 0, i);
            yield return null;
            i -= Time.deltaTime;
        }
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;
    }



}
