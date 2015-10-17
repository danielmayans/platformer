using UnityEngine;
using System.Collections;

public class DieScreen : MonoBehaviour
{

    static bool sawOnce = false;

    public DieScreen car;
    private Vector2 velocity;
    private Vector3 rotation;


    public void isDeath()
    {
        velocity = GetComponent<Rigidbody2D>().velocity;

        rotation = car.transform.eulerAngles;

        if (rotation.z >= 96.0f || rotation.z <= 263.0f)
        {
            if (velocity == new Vector2(0.0f, 0.0f))
            {

                if (!sawOnce)
                {
                    GameObject.FindGameObjectWithTag("start_screen").GetComponent<SpriteRenderer>().enabled = true;
                    Time.timeScale = 0;
                }

                sawOnce = true;
            }
        }

    }
    // Use this for initialization
    void Start()
    {
        car = this;
        isDeath();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            Time.timeScale = 1;
            GetComponent<SpriteRenderer>().enabled = false;

        }
    }
}
