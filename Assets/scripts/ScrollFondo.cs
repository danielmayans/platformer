using UnityEngine;
using System.Collections;

public class ScrollFondo : MonoBehaviour {

    public float velocidad = 0f;
    private float cameraX = 0f;
    private float cameraY = 0f;
    private float suavizado = 0.5f;

    public Component camera;

	// Use this for initialization
	void Start () {
	
	}
    
	
	// Update is called once per frame
	void Update () {
        cameraX = camera.transform.position.x;
        cameraY = camera.transform.position.y;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(cameraX * velocidad % 1, 0);
	}
}
