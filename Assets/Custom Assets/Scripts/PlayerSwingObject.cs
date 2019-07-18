using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerSwingObject : MonoBehaviour
{
    public HingeJoint hinge;
    public GameObject swinger;
    public GameObject rightHandIK;
    public GameObject leftHandIK;
    public GameObject subCollider1;
    public GameObject subCollider2;

    TombiCharacterController charScript;

    // Start is called before the first frame update
    void Start()
    {
        charScript = FindObjectOfType<TombiCharacterController>();

        if (hinge == null)
        {
            Debug.LogError("Hinge not assigned to swing object!");
        }

        if (swinger == null)
        {
            Debug.LogError("Swinger not found in swing object!");
        }

        if(subCollider1 != null)
        {
            subCollider1.transform.position = new Vector3(this.transform.position.x - 1.1f, this.transform.position.y - 0.08f, hinge.transform.position.z);
        }

        if (subCollider2 != null)
        {
            subCollider2.transform.position = new Vector3(this.transform.position.x + 1.1f, this.transform.position.y - 0.08f, hinge.transform.position.z);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if(charScript != null)
        {
            if ((charScript.railFirstPoint != null) && (charScript.railLastPoint != null))
            {
                Vector3 AB = charScript.railLastPoint - charScript.railFirstPoint;
                Vector3 AC = transform.position - charScript.railFirstPoint;
                Vector3 AX = Vector3.Project(AC, AB);
                Vector3 X = AX + charScript.railFirstPoint;

                hinge.transform.position = new Vector3(hinge.transform.position.x, hinge.transform.position.y, X.z);
                swinger.transform.position = new Vector3(swinger.transform.position.x, swinger.transform.position.y, X.z);

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(hinge.transform.position, hinge.transform.right);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(hinge.transform.position, -hinge.transform.right);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(hinge.transform.position, hinge.transform.up);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(hinge.transform.position, hinge.transform.forward);


    }

    private void OnDrawGizmos()
    {
        //Utilities.Instance.DrawLabelIcon(this.gameObject, 5);
        //DrawDotBigIcon(this.gameObject,5);
        //Utilities.Instance.DrawDotBigIcon(hinge.gameObject, 7);
    }




}
