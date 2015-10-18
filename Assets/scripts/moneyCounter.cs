using UnityEngine;
using System.Collections;

public class moneyCounter : MonoBehaviour {

    private GameMaster gm;

	// Use this for initialization
	void Start () {
        gm = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
	}
	
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            getCoin();
            Destroy(gameObject);
        }
    }

    void getCoin()
    {
        GameMaster.AddPoint();
    }

	// Update is called once per frame
	void Update () {
	
	}
}
