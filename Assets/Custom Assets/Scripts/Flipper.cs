using UnityEngine;
using System.Collections;

public class Flipper : MonoBehaviour
{
    public float speed = 1.0f;
    public float maxRotation = 30.0f;

    void Update()
    {
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, maxRotation * Mathf.Sin(Time.time * speed));
        //transform.rotation = Quaternion.Euler(0.0f, 0.0f, maxRotation * (Mathf.PingPong(Time.time * speed, 2.0f)-1.0f));
    }
}