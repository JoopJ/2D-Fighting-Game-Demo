using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    [SerializeField]
    Fighter fighterLeft;
    [SerializeField]
    Fighter fighterRight;

    [SerializeField]
    Slider leftHealthSlider;

    [SerializeField]
    Slider rightHealthSlider;
    [SerializeField]
    Text timeText;
    float time;
    float startTime = 60;

    void Start() {
        time = startTime;
    }
    
    void Update() {
        // start countdown at 60 seconds, tick down every second
        time -= Time.deltaTime;
        timeText.text = time.ToString();
    }

    public void TakeDamageLeft(int dmg) {
        Debug.Log("Left Adding damage: " + dmg);
        leftHealthSlider.value += dmg;
        Debug.Log(leftHealthSlider.value);
    }

    public void TakeDamageRight(int dmg) {
        Debug.Log("Right Adding damage: " + dmg);
        rightHealthSlider.value += dmg;
        Debug.Log(rightHealthSlider.value);
    }

    public void Winner() {
        Debug.Log("Game Over Reached!");
        Time.timeScale = 0;
    }

}
