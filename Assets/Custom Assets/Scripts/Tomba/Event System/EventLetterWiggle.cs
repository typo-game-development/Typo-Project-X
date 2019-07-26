using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLetterWiggle : MonoBehaviour
{
   


    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.transform.Rotate(this.transform.forward, Random.Range(-15, 15));
        this.gameObject.transform.position -= new Vector3(0f, Random.Range(-0.15f, 0.15f), 0f);


    }
    public float rotSign = 1f;
    public float actRotZ = 0f;
    // Update is called once per frame
    void Update()
    {
        actRotZ = this.gameObject.transform.eulerAngles.z;

        this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(1, Random.Range(0.8f, 1f), 1), Time.deltaTime * 5f);

        if (rotSign == 1f)
        {
            this.gameObject.transform.Rotate(this.transform.forward, Time.deltaTime * 50f * rotSign);

            if (actRotZ >= 10f && actRotZ <= 40f)
            {
                rotSign = -1f;

            }
        }

        else if (rotSign == -1f)
        {
            this.gameObject.transform.Rotate(this.transform.forward, Time.deltaTime * 50f * rotSign);

            if(actRotZ <= (355f) && actRotZ > (310f))
            {
                rotSign = 1f;
            }            
        }
    }
}
