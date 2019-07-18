using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayPlatform : MonoBehaviour
{
    public Collider platformObject;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Player")
        {
            Physics.IgnoreCollision(other, platformObject, true);

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Physics.IgnoreCollision(other, platformObject, false);
        }
    }

    private void OnDrawGizmos()
    {
        //Utilities.Instance.DrawLabelIcon(this.gameObject, 5);

    }
}
