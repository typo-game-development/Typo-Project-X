using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventController : SerializableMonoBehaviour
{
    [HideInInspector]
    public GameObject[] actualLetters;
    public GameObject eventCamera;

    [HideInInspector]
    public TextAsset eventListSourceFile;
    public GameObject apGain;
    public EventDataManager eventManager;
    public GameObject letterPrefab;
    public TMP_FontAsset font;
    public string spaceChar;
    public string lineFeedChar;
    public float spaceBetweenChars = 0.3f;
    public float spaceBetweenLines = 0.2f;
    public float spaceWidth = 0.4f;
    public LayerMask renderingLayer = 9;

    public override string FileExtension { get => ".smec"; protected set => base.FileExtension = value; }
    public override string FileExtensionName { get => "SMEC"; protected set => base.FileExtensionName = value; }

    [System.Serializable]
    public class Row
    {
        [HideInInspector] public string rowText;
        [HideInInspector] public GameObject[] letterObjs;
    }

    public Vector3 cameraOffset = Vector3.zero;

    public Row[] actualEventRows;

    // Start is called before the first frame update
    void Start()
    {
        eventManager = new EventDataManager();
        eventManager.Initialize(EventDataManager.FromJson<Tomba.TombaEvent>(eventListSourceFile.text));

        //WriteEventOnScreen(100);
        WriteEventOnScreen(100);
        eventCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) - cameraOffset;
        eventCamera.transform.LookAt(this.transform);

    }

    // Update is called once per frame
    void Update()
    {
        eventCamera.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) - cameraOffset;
    }

    public void WriteEventOnScreen(int id)
    {
        string eventName;
        eventName = eventManager.events[id].name;

        FindObjectOfType<AudioManager>().Play("Event Occurred");

        string[] eventRows =  eventName.Split(char.Parse("|"));

        if (eventRows.Length == 0)
        {
            return;
        }
        int rowCount = eventRows.Length;
        int i = 0;

        Vector3 centerPos = this.transform.position;

        actualEventRows = new Row[rowCount];
        float yOffset = 0f;

        foreach (Row r in actualEventRows)
        {
            int i2 = 0;

            string rowText = eventRows[i];

            yOffset = (float)(Math.Floor((float)(rowCount + 1 / 2))) - i - (i * 0.3f); 

            Debug.Log(yOffset);
            actualEventRows[i] = new Row();
            actualEventRows[i].rowText = rowText;
            actualEventRows[i].letterObjs = new GameObject[rowText.Length];

            foreach(GameObject l in actualEventRows[i].letterObjs)
            {
                if(actualEventRows[i].rowText[i2].ToString() != " ")
                {
                    float xOffset = 0f;
                    int letterCount = actualEventRows[i].letterObjs.Length;

                    xOffset = (float)(Math.Floor((float)(letterCount / 2))) - i2;

                    GameObject letter = actualEventRows[i].letterObjs[i2];
                    TextMeshPro textMesh;

                    letter = Instantiate(letterPrefab, this.gameObject.transform);
                    letter.name = actualEventRows[i].rowText[i2].ToString();
                    letter.transform.position = new Vector3(centerPos.x - xOffset, centerPos.y + yOffset , centerPos.z);
                    textMesh = letter.GetComponentInChildren<TextMeshPro>();
                    textMesh.text = letter.name;
                    textMesh.font = font;

                    actualEventRows[i].letterObjs[i2] = letter;
                }

                i2++;
            }
            i++;
        }
        int apGainCount = eventManager.events[id].occurrAPGain;

        if(apGainCount > 0)
        {
            apGain.transform.position = new Vector3(centerPos.x, centerPos.y + yOffset - 1.3f, centerPos.z);
            apGain.GetComponent<TextMeshPro>().text = apGainCount.ToString();
            apGain.SetActive(true);
        }
        else
        {
            apGain.SetActive(false);
        }
    }

    public override void Save(string path)
    {
        throw new NotImplementedException();
    }

    public override void Load()
    {
        throw new NotImplementedException();
    }

    //public void WriteEventOnScreen(int id)
    //{
    //    int spaceCount = 0;
    //    int lineFeedCount = 0;
    //    int carriageIndex = 0;
    //    float tempIndex = 0f;

    //    Array.Resize(ref actualLetters, eventManager.events[id].name.Length);

    //    apGain.SetActive(true);

    //    for (int i = 0; i < eventManager.events[id].name.Length; i++)
    //    {
    //        string letter = eventManager.events[id].name.ToCharArray()[i].ToString();

    //        if (letter != spaceChar)
    //        {
    //            if (letter != lineFeedChar)
    //            {
    //                GameObject newLetter = Instantiate(letterPrefab, this.gameObject.transform);
    //                TextMeshPro textMesh = newLetter.GetComponentInChildren<TextMeshPro>();

    //                newLetter.name = letter;
    //                //newLetter.layer = renderingLayer;

    //                int actualLetterIndex = i - carriageIndex;
    //                float actualYSpace = lineFeedCount + (spaceBetweenLines * lineFeedCount);

    //                if (spaceCount > 0)
    //                {
    //                    spaceCount = 0;
    //                }
    //                newLetter.transform.position = new Vector3((this.transform.position.x + ((actualLetterIndex + (actualLetterIndex * spaceBetweenChars) + (spaceWidth * spaceCount)))), this.transform.position.y - actualYSpace, this.transform.position.z);

    //                textMesh.text = newLetter.name;
    //                textMesh.font = font;
    //                actualLetters[i] = newLetter;
    //            }
    //            else
    //            {
    //                lineFeedCount += 1;
    //                carriageIndex = i + 1;
    //                spaceCount = 0;
    //            }
    //        }
    //        else
    //        {
    //            spaceCount += 1;
    //        }
    //    }
    //}
}
