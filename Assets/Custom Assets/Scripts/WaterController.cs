using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaterController : MonoBehaviour
{
    public static Dictionary<string, WaterController> _waterControllers = new Dictionary<string, WaterController>();

    public float scale = 0.1f;
    public float speed = 1.0f;
    public float waveDistance = 1f;
    public Vector3 WaveDirection;
    public Vector3 WaveInterference;
    public float WaveDirectionWeight;
    public float NoiseStrength;
    public float NoiseWalk;
    public Vector3 Obstacle;

    Renderer _renderer;

    float _yAdjustFromCollider;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        var collider = GetComponent<BoxCollider>();

        if (collider != null)
        {
            _yAdjustFromCollider = collider.center.y;
        }

        if (Application.isPlaying)
        {
            _waterControllers.Add(this.gameObject.name, this);
        }
    }

    public static WaterController GetController(string id)
    {
        if (_waterControllers.ContainsKey(id))
            return _waterControllers[id];

        return null;
    }

    //Get the y coordinate from whatever wavetype we are using
    public float GetWaveYPos(Vector3 pos)
    {
        float noiseSample = Mathf.PerlinNoise(pos.x + NoiseWalk, pos.y + Mathf.Sin(LastTime * 0.1f)) * NoiseStrength;
        float y = (Mathf.Sin((LastTime * speed + ((pos.x + noiseSample) * WaveDirection.x) + ((pos.z + noiseSample) * WaveDirection.z)) / waveDistance) * scale) * WaveDirectionWeight;

        y += (Mathf.Sin((LastTime * speed + ((pos.x + noiseSample) * WaveDirection.x)) / waveDistance) * scale) * WaveInterference.x;
        y += (Mathf.Sin((LastTime * speed + ((pos.z + noiseSample) * WaveDirection.z)) / waveDistance) * scale) * WaveInterference.z;

        return y;
    }

    //Find the distance from a vertice to water
    //Make sure the position is in global coordinates
    //Positive if above water
    //Negative if below water
    public float DistanceToWater(Vector3 position)
    {
        float waterHeight = GetWaveYPos(position);

        return (transform.position.y + _yAdjustFromCollider + waterHeight) - position.y;
    }

    public float WorldWaterHeight(Vector3 position)
    {
        float waterHeight = GetWaveYPos(position);

        return (transform.position.y + _yAdjustFromCollider + waterHeight);
    }

    public static float LastTime;


    void Update()
    {
        LastTime = Time.time;

        _renderer.sharedMaterial.SetFloat("_WaterScale", scale);
        _renderer.sharedMaterial.SetFloat("_WaterSpeed", speed);
        _renderer.sharedMaterial.SetFloat("_WaterDistance", waveDistance);
        _renderer.sharedMaterial.SetFloat("_WaterTime", LastTime);
        _renderer.sharedMaterial.SetFloat("_WaveDirectionWeight", WaveDirectionWeight);
        _renderer.sharedMaterial.SetFloat("_NoiseStrength", NoiseStrength);
        _renderer.sharedMaterial.SetFloat("_NoiseWalk", NoiseWalk);
        _renderer.sharedMaterial.SetVector("_WaveDirection", WaveDirection);
        _renderer.sharedMaterial.SetVector("_WaveInterference", WaveInterference);
        _renderer.sharedMaterial.SetVector("_Obstacle", Obstacle);
    }
}