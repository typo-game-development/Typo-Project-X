using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_MainMenu : MonoBehaviour
{
    public enum eButtonType
    {
        NewGame,
        LoadGame,
        Options,
        Credits,
        ExitGame,
        Back
    }


    public GameObject backgroundPanel;
    public GameObject backButton;
    public GameObject mainMenuObjects;

    public bool optionsOpened = false;
    public bool creditsOpened = false;
    public bool loadGameOpened = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleButtonPressed(eButtonType buttonType)
    {
        switch (buttonType)
        {
            case eButtonType.NewGame:
                break;

            case eButtonType.LoadGame:
                break;

            case eButtonType.Options:
                break;

            case eButtonType.Credits:
                break;

            case eButtonType.ExitGame:
                HandleApplicationExit();
                break;

            case eButtonType.Back:
                backgroundPanel.GetComponent<Animator>().ResetTrigger("OptionsOpen");
                backgroundPanel.GetComponent<Animator>().SetTrigger("OptionsClose");
                break;

            default:
                break;
        }
    }


    public void NewGameButtonPress()
    {
        HandleButtonPressed(eButtonType.NewGame);
    }

    public void LoadGameButtonPress()
    {
        HandleButtonPressed(eButtonType.LoadGame);
    }

    public void OptionsButtonPress()
    {
        HandleButtonPressed(eButtonType.Options);
    }

    public void CreditsButtonPress()
    {
        HandleButtonPressed(eButtonType.Credits);
    }

    public void ExitGameButtonPress()
    {
        HandleButtonPressed(eButtonType.ExitGame);
    }

    public void BackButtonPress()
    {
        HandleButtonPressed(eButtonType.Back);
    }

    private void HandleApplicationExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }
}
