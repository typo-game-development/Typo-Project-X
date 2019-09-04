using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMovementRailController : MonoBehaviour
{
    [HideInInspector]
    public List<PlayerMovementRailV1> rails = new List<PlayerMovementRailV1>(5);

    [HideInInspector]
    public PlayerMovementRailV1 currentEditedRail = null;

    [HideInInspector]
    public bool isEditing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddRail()
    {
        PlayerMovementRailV1 newObj = new GameObject("Rail "+ this.rails.Count).AddComponent<PlayerMovementRailV1>();
        newObj.transform.parent = this.gameObject.transform;
        //newObj.gameObject.hideFlags = HideFlags.HideInHierarchy;

        this.rails.Add(newObj);

    }

    public void RemoveRail(PlayerMovementRailV1 rail)
    {
        this.rails.Remove(rail);
        DestroyImmediate(rail.gameObject);
    }

    public void RefreshRails()
    {
        PlayerMovementRailV1[] tempRails = FindObjectsOfType<PlayerMovementRailV1>();

        rails = new List<PlayerMovementRailV1>(tempRails);
    }

    public void EditRail(PlayerMovementRailV1 railToEdit)
    {
        railToEdit.editorSelected = true;

    }
    
}
