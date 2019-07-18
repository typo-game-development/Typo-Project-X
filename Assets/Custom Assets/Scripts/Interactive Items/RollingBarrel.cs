using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBarrel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, 90, this.transform.localEulerAngles.z);
    }
}
