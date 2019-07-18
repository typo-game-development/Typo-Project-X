using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRotationAtStart : MonoBehaviour
{
    public Vector3 rotation = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localEulerAngles = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
