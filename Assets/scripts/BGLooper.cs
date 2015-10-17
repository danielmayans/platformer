using UnityEngine;
using System.Collections;

public class BGLooper : MonoBehaviour
{
    int numBGPanels = 9;      // Array (list) of all the back- and foregrounds to be repeated

    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Background")
        {
            float widthOfBGObject = ((BoxCollider2D)collider).size.x;

            Vector3 pos = collider.transform.position;

            pos.x += widthOfBGObject * numBGPanels;

            collider.transform.position = pos;
        }
    }
}
