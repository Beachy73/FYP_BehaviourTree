using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var enemies = GameObject.FindWithTag("Enemy");
        if (enemies == null)
        {
            Debug.Log("NO ENEMIES LEFT");
            SceneManager.LoadScene("WinScene");
        }
    }
}
