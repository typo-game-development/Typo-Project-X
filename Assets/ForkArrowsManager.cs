using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkArrowsManager : MonoBehaviour
{
    public GameObject forkTop;
    public GameObject forkForward;
    public GameObject forkBack;
    public GameObject forkBottom;
    private TombiCharacterController charScript;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    charScript = FindObjectOfType<TombiCharacterController>();
    //    this.transform.position = new Vector3(charScript.transform.position.x, this.transform.position.y, charScript.transform.position.z);
    //    this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x, charScript.transform.rotation.eulerAngles.y - 90, this.transform.rotation.eulerAngles.z));
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    this.transform.position = new Vector3(charScript.transform.position.x, this.transform.position.y, charScript.transform.position.z);
    //    this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x, charScript.transform.rotation.eulerAngles.y - 90, this.transform.rotation.eulerAngles.z));
    //}
}
