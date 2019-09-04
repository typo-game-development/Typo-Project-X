using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ForkArrowAnimator : MonoBehaviour
{
    public bool _selected = false;

    private bool isAnimating = false;
    private Vector3 oldPosition;
    private TombiCharacterController charScript;
    public bool followTargetRotation;
    public Transform referenceForward;
    public bool Selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;

        }

    }
    private void OnEnable()
    {
        isAnimating = false;
        this.transform.DOComplete();
        StartCoroutine(_AnimateArrow());
    }

    private void OnDisable()
    {
        this.transform.DOComplete();
    }

    // Start is called before the first frame update
    void Awake()
    {
        oldPosition = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (charScript == null)
        {
            charScript = GetComponentInParent<TombiCharacterController>();

        }

    }

    IEnumerator _AnimateArrow()
    {
        isAnimating = true;

        while (this.isActiveAndEnabled)
        {
            if (followTargetRotation && charScript != null)
            {
                this.transform.DOComplete();

                this.transform.DOLocalMove(oldPosition - this.transform.right * 0.05f * charScript.lastInputHorizontal, 0.2f, false);
                yield return new WaitForSeconds(0.2f);

                this.transform.DOComplete();
                this.transform.DOLocalMove(oldPosition + this.transform.right * 0.05f * charScript.lastInputHorizontal, 0.35f, false);

                yield return new WaitForSeconds(0.35f);
            }
            else
            {
                this.transform.DOComplete();

                this.transform.DOLocalMove(oldPosition - this.transform.right * 0.05f, 0.2f, false);
                yield return new WaitForSeconds(0.2f);

                this.transform.DOComplete();
                this.transform.DOLocalMove(oldPosition + this.transform.right * 0.05f, 0.35f, false);

                yield return new WaitForSeconds(0.35f);
            }

        }

        this.transform.DOComplete();
        this.transform.DOLocalMove(oldPosition, 0.1f, false);
        yield return new WaitForSeconds(0.1f);

        isAnimating = false;

    }
}
