using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuScreen;
    public GameObject HelpMenuScreen;
    public GameObject CreditsScreen;
    public GameObject FirstNameSet;
    public GameObject PlayMenuScreen;

    public string playerName;

    private void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        if(playerName == null || playerName.Equals(""))
        {
            FirstNameSet.SetActive(true);
            MainMenuScreen.SetActive(false);
        }
    }
    public void OpenHelpMenu()
    {
        MainMenuScreen.SetActive(false);
        HelpMenuScreen.SetActive(true);
    }
    public void OpenNameInput()
    {
        HelpMenuScreen.transform.Find("Menu").gameObject.SetActive(false);
        HelpMenuScreen.transform.Find("SelectNameMenu").gameObject.SetActive(true);
        HelpMenuScreen.transform.Find("SelectNameMenu").Find("NameInputField").GetComponent<TMP_InputField>().text = "";
        HelpMenuScreen.transform.Find("SelectNameMenu").Find("NameInputField").GetComponent<TMP_InputField>().transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = playerName;
    }
    public void BackToMainMenu(GameObject oldMenu)
    {
        oldMenu.SetActive(false);
        MainMenuScreen.SetActive(true);
    }

    public void OpenCredits()
    {
        CreditsScreen.SetActive(true);
        MainMenuScreen.SetActive(false);
    }

    public void OpenControls()
    {
        HelpMenuScreen.transform.Find("Menu").gameObject.SetActive(false);
        HelpMenuScreen.transform.Find("Controls").gameObject.SetActive(true);
    }
    public void SetPlayerName(TMP_InputField inputField)
    {
        if (!playerName.Equals(""))
        {
            playerName = inputField.GetComponent<TMP_InputField>().text;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }
    }

    public void BackToHelpMenu(GameObject oldMenu)
    {
        oldMenu.SetActive(false);
        HelpMenuScreen.SetActive(true);
        HelpMenuScreen.transform.Find("Menu").gameObject.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void BackToPlayMenu()
    {
        PlayMenuScreen.transform.Find("Main Menu").gameObject.SetActive(true);
        PlayMenuScreen.transform.Find("JoinMenu").gameObject.SetActive(false);
    }

    public void OpenPlayMenu()
    {
        PlayMenuScreen.SetActive(true);
        MainMenuScreen.SetActive(false);
    }
    public void OpenJoinMenu()
    {
        PlayMenuScreen.transform.Find("Main Menu").gameObject.SetActive(false);
        PlayMenuScreen.transform.Find("JoinMenu").gameObject.SetActive(true);
    }
}
