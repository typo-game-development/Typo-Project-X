using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMovementRailController : MonoBehaviour
{
    [HideInInspector]
    public List<PlayerMovementRail> rails = new List<PlayerMovementRail>(5);

    [HideInInspector]
    public PlayerMovementRail currentEditedRail = null;

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
        PlayerMovementRail newObj = new GameObject("Rail "+ this.rails.Count).AddComponent<PlayerMovementRail>();
        newObj.transform.parent = this.gameObject.transform;
        //newObj.gameObject.hideFlags = HideFlags.HideInHierarchy;

        this.rails.Add(newObj);

    }

    public void RemoveRail(PlayerMovementRail rail)
    {
        this.rails.Remove(rail);
        DestroyImmediate(rail.gameObject);
    }

    public void RefreshRails()
    {
        PlayerMovementRail[] tempRails = FindObjectsOfType<PlayerMovementRail>();

        rails = new List<PlayerMovementRail>(tempRails);
    }

    public void EditRail(PlayerMovementRail railToEdit)
    {
        railToEdit.editorSelected = true;

    }
    
}
