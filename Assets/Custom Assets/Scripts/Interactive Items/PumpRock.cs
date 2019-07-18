using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpRock : MonoBehaviour
{
    [ShowOnly] public bool pressed;

    public GameObject springObject;
    public GameObject baseObject;
    public GameObject topObject;
    public GameObject splashEffect;

    public Material unpressedMaterial;
    public Material pressedMaterial;

    float scale;
    bool rescale;
    bool repos;
    Vector3 oldpos;
    float oldScale;
    float newScale;
    [HideInInspector] public bool cancollapse2 = false;
    Vector3 hitpos;
    [HideInInspector] public bool canCollapse = false;
    bool playedPressSound = false;
    bool playedReleaseSound = false;
    float time = 0f;
    float pressedScale = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        if(springObject != null)
        {
            oldScale = this.transform.localScale.y;
            newScale = oldScale;
            oldpos = this.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        scale = this.transform.localScale.y;

        if(rescale)
        {
            newScale += Time.deltaTime * 20f;

            if (this.transform.localScale.y < oldScale)
            {
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(this.transform.localScale.x, oldScale, this.transform.localScale.z), Time.deltaTime * 2f);

                if(this.transform.localScale.y > pressedScale)
                {
                    if (!playedReleaseSound && !cancollapse2)
                    {
                        FindObjectOfType<AudioManager>().Play("PumpRock_Release");
                        playedReleaseSound = true;
                        playedPressSound = false;
                    }
                }
            }
            else
            {
                rescale = false;
            }
        }

        if(cancollapse2)
        {
            newScale -= Time.deltaTime * 50f;

            if (this.transform.localScale.y > pressedScale)
            {
                this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, new Vector3(this.transform.localScale.x, pressedScale, this.transform.localScale.z), Time.deltaTime * 2f);

                if (!playedPressSound)
                {
                    FindObjectOfType<AudioManager>().Play("PumpRock_Press");
                    playedPressSound = true;
                    playedReleaseSound = false;
                }
            }
            else
            {
                newScale = pressedScale;
                cancollapse2 = false;
            }
        }

        if(this.transform.localScale.y <= 0.6f)
        {
            splashEffect.transform.GetChild(0).gameObject.SetActive(true);
            splashEffect.transform.GetChild(1).gameObject.SetActive(true);
            splashEffect.transform.GetChild(2).gameObject.SetActive(true);

        }

        if (this.transform.localScale.y <= 0.478f)
        {
            pressed = true;
        }

        if(pressed && springObject.GetComponent<MeshRenderer>().materials.Length == 1)
        {
            pressed = false;
            springObject.GetComponent<MeshRenderer>().materials = new Material[2] { unpressedMaterial, pressedMaterial };
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.gameObject.tag == "Player")
        {
            if (canCollapse)
            {
                cancollapse2 = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.collider.gameObject.tag =="Player")
        {
            rescale = true;
            repos = true;
            canCollapse = false;
        }
    }
}
