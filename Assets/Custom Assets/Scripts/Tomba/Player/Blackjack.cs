using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackjack : MonoBehaviour
{

    bool go;//Will Be Used To Change Direction Of Weapon

    public GameObject player;//Reference To The Main Character



    Transform itemToRotate;//The Weapon That Is A Child Of The Empty Game Object

    Vector3 locationInFrontOfPlayer;//Location In Front Of Player To Travel To

    public BezierSpline spline;
    public GameObject obj;
    GameObject newObj;
    // Use this for initialization
    void Start()
    {
        //obj = Resources.Load("Prefabs/FXs/FX_RockHit", typeof(GameObject)) as GameObject;
        //obj.AddComponent<DontDestroyOnLoad>();
        //obj.SetActive(false);
        //newObj.SetActive(false);
    }

    public void Throw()
    {

        go = false; //Set To Not Return Yet
        this.gameObject.SetActive(true);

        //player = GameObject.Find("Akane");// The GameObject To Return To
        //sword = GameObject.Find("Sword");//The Weapon The Character Is Holding In The Scene

        //sword.GetComponent<MeshRenderer>().enabled = false; //Turn Off The Mesh Render To Make The Weapon Invisible

        itemToRotate = gameObject.transform.GetChild(0); //Find The Weapon That Is The Child Of The Empty Object      

        //Adjust The Location Of The Player Accordingly, Here I Add To The Y position So That The Object Doesn't Go Too Low ...Also Pick A Location In Front Of The Player
        locationInFrontOfPlayer = new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z) + player.transform.forward * 3f;
        //spline.points[1] = new Vector3(spline.points[0].x - 1f, spline.points[0].y + 2f, spline.points[0].z);
        //spline.points[3] = spline.transform.InverseTransformPoint(locationInFrontOfPlayer);

        //Boom();//Now Start The Coroutine
        StartCoroutine(Boom1());


    }

    void Boom()
    {

        go = true;
        progress = 1f;
        //yield return new WaitForSeconds(0.35f);//Any Amount Of Time You Want
    }

    IEnumerator Boom1()
    {
        Physics.IgnoreCollision(player.GetComponent<CapsuleCollider>(), gameObject.transform.GetComponent<SphereCollider>(), true);
        FindObjectOfType<AudioManager>().Play("Throw_Weapon");

        go = true;
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z);

        progress = 1f;
        yield return new WaitForSeconds(0.35f);//Any Amount Of Time You Want
        if(go)
        {
            spline.points[3] = spline.transform.InverseTransformPoint(transform.position);

        }
        go = false;
    }

    float progress = 1f;

    // Update is called once per frame
    void Update()
    {
        spline.points[0] = spline.transform.InverseTransformPoint(player.transform.position + (player.transform.up * 0.5f));

        if(go)
        {
            if (player.GetComponent<TombiCharacterController>().lastInputHorizontal == 1)
            {
                spline.points[1] = new Vector3(spline.points[0].x - 1.8f, spline.points[0].y + 0.8f, spline.points[0].z);

            }
            else if (player.GetComponent<TombiCharacterController>().lastInputHorizontal == -1)
            {
                spline.points[1] = new Vector3(spline.points[0].x + 1.8f, spline.points[0].y + 0.8f, spline.points[0].z);

            }
        }

        spline.points[2] = new Vector3(spline.points[3].x, spline.points[3].y + 0.3f, spline.points[3].z);

        if (itemToRotate != null)
        {
            itemToRotate.transform.Rotate(0, Time.deltaTime * 500, 0); //Rotate The Object

            if (go)
            {
                transform.position = Vector3.Lerp(transform.position, locationInFrontOfPlayer, Time.deltaTime * 5f); //Change The Position To The Location In Front Of The Player      


                //if (Vector3.Distance(this.transform.position, locationInFrontOfPlayer) < 0.1f)
                //{
                //    go = !go;


                //}
            }

            if (!go)
            {

                progress -= Time.deltaTime / 0.5f;

                if (progress < 0f)
                {
                    progress = 0f;
                }

                this.transform.position = spline.GetPoint(progress);
                //transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z), Time.deltaTime * 13); //Return To Player
            }

            if (!go && Vector3.Distance(player.transform.position, transform.position) < 0.6f)
            {
              
                FindObjectOfType<AudioManager>().Play("GroundHit");

                this.gameObject.SetActive(false);
                transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.5f, player.transform.position.z);
                Physics.IgnoreCollision(player.GetComponent<CapsuleCollider>(), gameObject.transform.GetComponent<SphereCollider>(), false);

            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer != 14 && other.gameObject.GetComponent<MeshRenderer>() != null)
        {
            if(go)
            {
                go = false;
                spline.points[3] = spline.transform.InverseTransformPoint(transform.position);
                newObj = Instantiate(obj);
                newObj.transform.position = transform.position;
                newObj.transform.localScale = Vector3.one * 0.5f;

                if(newObj.activeSelf)
                {
                    newObj.SetActive(false);
                }
                newObj.SetActive(true);

                switch (other.gameObject.tag)
                {
                    case "Ground":
                        FindObjectOfType<AudioManager>().Play("Hit_Ground");
                        break;

                    default:
                        FindObjectOfType<AudioManager>().Play("Hit_Pump_Rock");
                        break;

                }
            }
        }
    }
}