using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SporeHeight : MonoBehaviour
{
    [Range(0f, 1f)]
    public float height;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //height = gameObject.GetComponent<Renderer>().sharedMaterial.GetFloat("_Parallax");
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Parallax", height);
    }
}
