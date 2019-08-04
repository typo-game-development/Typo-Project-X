using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetPositionLerper : MonoBehaviour
{
    public GameObject player = null;
    public Vector3 offset = Vector3.zero;
    public float smoothing = 2f;
    public float magnitude = 1f;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            Vector3 test = Vector3.MoveTowards(this.transform.position, ((player.transform.position + player.transform.forward) * magnitude) + offset, smoothing * Time.deltaTime);
            test.y = 0;

            this.transform.position = test;

            //this.transform.position = Vector3.MoveTowards(this.transform.position, ((player.transform.position + player.transform.forward) * magnitude) + offset, smoothing * Time.deltaTime);

        }
    }
}
