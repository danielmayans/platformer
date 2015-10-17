﻿using UnityEngine;
using System.Collections;

public class BackgroundScroller : MonoBehaviour {

    public float speed = 0;
    public static BackgroundScroller current;

    float pos = 0;

    // Use this for initialization
    void Start() {
        current = this;

    }
	// Update is called once per frame
	void Update () {
        pos += speed;
        if (pos > 1.0f)
            pos -= 1.0f;

        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(pos,  0);
	}
}
