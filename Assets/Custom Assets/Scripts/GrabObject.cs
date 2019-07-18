using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabObject : MonoBehaviour
{
    public enum eType
    {
        Chest,
        Pig,
        Bird
    }

    public eType type;
    public bool throwable = false;
    public bool movesWithPlayer = false;
    public bool hasBeenThrown = false;
    public float throwDirection = 0f;

    public int hitCount = 0;

    private bool startCountForDestroy = false;
    private float elapsedTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    bool flag = false;

    // Update is called once per frame
    void Update()
    {
        if(hasBeenThrown)
        {
            if(!startCountForDestroy)
            {
                startCountForDestroy = true;
                elapsedTime = Time.time;
            }
            else
            {
                if (hitCount > 3 || Time.time - elapsedTime >= 2)
                {
                    GameObject obj = Resources.Load("Prefabs/CFX2_WWExplosion_C", typeof(GameObject)) as GameObject;
                    obj.transform.localScale = Vector3.one * 0.5f;

                    GameObject newObj = Instantiate(obj, this.transform.position, Quaternion.Euler(90,0,0));

                    FindObjectOfType<AudioManager>().Play("Pig_Dead");

                    Destroy(this.gameObject);
                }
            }

            if (!flag)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z);
                flag = true;
            }
            Vector3 rotation = -Vector3.forward * (((1000f * throwDirection) / hitCount) * Time.deltaTime);

            if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y) && !float.IsNaN(rotation.z))
            {
                //Do stuff
                transform.Rotate(rotation);

            }




        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(hasBeenThrown)
        {
            this.GetComponent<Rigidbody>().AddForce(collision.contacts[0].normal * 2f, ForceMode.Impulse);
            this.GetComponent<Rigidbody>().AddForce(-collision.contacts[0].normal, ForceMode.Acceleration);
            throwDirection *= -1;
            this.GetComponent<Rigidbody>().useGravity = true;

            //this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity / 2;
            hitCount++;
        }
    }
}
