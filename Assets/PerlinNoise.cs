using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinNoise : MonoBehaviour
{
    [Range(0f, 1f)]
    public float noiseSpeed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_BumpMap", Vector2.Lerp(new Vector2(0, 0), new Vector2(0, -Time.time * noiseSpeed), Time.deltaTime * 100000000f));
    }
}