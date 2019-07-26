using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Tomba.Utilities;
using UnityEditor;

namespace Tomba
{
    [System.Serializable]
    public class GameManager : MonoBehaviour
    {
        public SaveState currentSaveState = new SaveState();
        public SaveState[] saveStates = new SaveState[10];
        private void Awake()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {
            //EditorUtility.SetDirty(eventListFile);
            //eventList = FromJson<Event>(eventListFile.text);

            Initialize();


        }

        public SaveState[] GetSaveStates()
        {
            SaveState[] tempSaveStates = new SaveState[11];

            for (int i = 0; i< tempSaveStates.Length; i++)
            {
                tempSaveStates[i] = new SaveState();
            }
            return tempSaveStates;

        }

        public void Initialize()
        {
            Debug.Log("Initializing GameManager...");

            string gameDataFilePath = string.Format("{0}/Game Data/bin", Application.dataPath);
            string gameDataFileName = "data.bin";

            if (!System.IO.Directory.Exists(gameDataFilePath))
            {
                Debug.Log("Creating " + gameDataFilePath + " path...");
                System.IO.Directory.CreateDirectory(gameDataFilePath);
            }

            if (!System.IO.File.Exists(gameDataFilePath + "/" + gameDataFileName))
            {
                Debug.Log("Creating " + string.Format("{0}/Game Data/bin/{1}", Application.dataPath, gameDataFileName) + " file...");
                string str = Serializer.SerializeToXML<GameManager, GameManager>(this, false, System.Text.Encoding.ASCII, typeof(SaveState));

                //System.IO.File.Create(gameDataFilePath + "/" + gameDataFileName);
                System.IO.File.WriteAllText(gameDataFilePath + "/" + gameDataFileName, str, System.Text.Encoding.ASCII);
            }
            else
            {
                //LOAD 
            }
            Debug.Log("GameManager initialized successfully!");

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LoadGameState()
        {
            
        }

        public void SaveGameState()
        {

        }
    }
}
