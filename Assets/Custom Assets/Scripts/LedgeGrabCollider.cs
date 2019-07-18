using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabCollider : MonoBehaviour
{
    TombiCharacterController charScript;

    // Start is called before the first frame update
    void Start()
    {
        charScript = FindObjectOfType<TombiCharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            charScript.LedgeGrab(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            charScript.LedgeGrabReset();
        }
    }
}
