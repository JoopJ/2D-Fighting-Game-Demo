using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFighterControl : FighterControl
{
    void Update() {
        GetPlayerInput();
        //PrintResults();
    }

    private void GetPlayerInput() {
        left = Input.GetButton("Left");
        right = Input.GetButton("Right");
        jump = Input.GetButton("Jump");
        block = Input.GetButton("Block");
        lightAttack = Input.GetButton("LightAttack");
        heavyAttack = Input.GetButton("HeavyAttack");
    }

    private void PrintResults() {
        if (left) { Debug.Log("Left: " + left); }
        if (right) { Debug.Log("Right: " + right); }
        if (jump) { Debug.Log("Jump: " + jump); }
        if (block) { Debug.Log("Block: " + block); }
        if (lightAttack) { Debug.Log("Light Attack: " + lightAttack); }
        if (heavyAttack) { Debug.Log("Heavy Attack: " + heavyAttack); }
    }
}
