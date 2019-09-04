using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class BaseJobObjectExample : MonoBehaviour
{
    [SerializeField]
    protected int m_ObjectCount = 10000;

    [SerializeField]
    protected float m_ObjectPlacementRadius = 100f;

    protected GameObject[] m_Objects;
    protected Transform[] m_Transforms;
    protected Renderer[] m_Renderers;

    protected void Awake()
    {
        m_Objects = new GameObject[m_ObjectCount];
        m_Transforms = new Transform[m_ObjectCount];
        m_Renderers = new Renderer[m_ObjectCount];
    }
}

public class TestJobSystem : BaseJobObjectExample
{
    NativeArray<Vector3> m_Positions;
    NativeArray<Bounds> m_NativeBounds;

    BoundsContainsPointJob m_Job;
    BoundsIntersectionJob m_IntersectionJob;

    JobHandle m_JobHandle;
    JobHandle m_IntersectionJobHandle;

    public void Start()
    {
        m_Positions = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);
        m_NativeBounds = new NativeArray<Bounds>(m_ObjectCount, Allocator.Persistent);

        for (int i = 0; i < m_ObjectCount; i++)
        {
            m_Renderers[i] = m_Objects[i].GetComponent<Renderer>();
            m_NativeBounds[i] = m_Renderers[i].bounds;
        }
    }

    // this job only checks to see if any bounds intersected instead of giving a list
    struct BoundsContainsPointJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        public Vector3 point;

        public void Execute(int i)
        {
            Bounds testAgainst = boundsArray[i];
            if (testAgainst.Contains(point))
            {
                Debug.Log("point " + point + " is in Bounds: " + testAgainst);
            }
        }
    }

    // right now this just logs when we detect intersection
    // TODO - demonstrate processing a results list
    struct BoundsIntersectionJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        public Bounds boundsToCheck;

        public void Execute(int i)
        {
            Bounds testAgainst = boundsArray[i];
            if (boundsToCheck.Intersects(testAgainst))
            {
                Debug.Log(boundsToCheck + " intersects with: " + testAgainst);
            }
        }
    }

    public void Update()
    {
        var point = Random.insideUnitSphere * 100f;

        // check if a point intersects any of the cube's bounds
        m_Job = new BoundsContainsPointJob()
        {
            point = point,
            boundsArray = m_NativeBounds,
        };

        // check if a bounding box outside that point intersects any cubes
        m_IntersectionJob = new BoundsIntersectionJob()
        {
            boundsToCheck = new Bounds(point, Vector3.one),
            boundsArray = m_NativeBounds
        };

        m_JobHandle = m_Job.Schedule(m_Positions.Length, 64);
        m_IntersectionJobHandle = m_IntersectionJob.Schedule(m_NativeBounds.Length, 64);
    }

    public void LateUpdate()
    {
        m_JobHandle.Complete();
        m_IntersectionJobHandle.Complete();
    }

    private void OnDestroy()
    {
        m_Positions.Dispose();
        m_NativeBounds.Dispose();
    }
}