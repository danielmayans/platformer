using UnityEngine;
using System.Collections;

public class GameMaster : MonoBehaviour {

    static int score = 0;
    static int highScore = 0;
    static GameMaster instance;

    private GUIText guiScore;

    static public void AddPoint()
    {
        //if (instance.player.dead)
        //    return;

        score+=5;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highScore", highScore);
        }
    }
    // Use this for initialization
    void Start () {
        guiScore = GameObject.FindGameObjectWithTag("GuiScore").GetComponent<GUIText>();


    }
	
	// Update is called once per frame
	void Update () {
        guiScore.text = "Score: " + score + "\nHigh Score: " + PlayerPrefs.GetInt("highScore");
    }
}
