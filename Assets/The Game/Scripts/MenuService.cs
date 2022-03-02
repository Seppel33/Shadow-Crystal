using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuService : MonoBehaviour
{

    public GameObject MainMenuScreen;
    public GameObject HelpMenuScreen;
    public GameObject CreditsScreen;
    public GameObject FirstNameSet;
    public GameObject PlayMenuScreen;
    public GameObject MenuCrystal;
    [SerializeField]
    private string localPlayerName = "";

    public GameObject canvas;
    
    public GameObject overlayCanvas;

    public GameObject ingameUICanvas;

    public TMP_Text quSpellCooldownText;

    public TMP_Text otherSpellCooldownText;

    public Image[] skills;
    public Sprite[] skillIcons;

    public GameObject endScreen;
    
    public TMP_Text[] playerNameTexts;
   
    public TMP_Text timer;

    public TMP_Text ShadowVisionCooldownText;

    public Image ShadowVisionImage;

    private NetworkManager networkManager;

    private bool checkConnecting;

    public bool inLobby;

    private PlayerService playerService;
    private GameManagerService gameService;

    private bool ready;

    private bool timerStarted;
    private float lobbyTimerFloat;

    private bool ingameUIActive;

    private MagesController localMage;
    private ShadowMonsterController localMonster;

    public GameObject magesUI;
    public GameObject monsterUI;

    public GameObject buttonReturnToMenu;

    public Image buttonImage;

    private bool escMenuActive;
    public GameObject tabMenu;
    public GameObject escMenu;
    public GameObject overMainMenu;
    private string LastSteamID = "";
    private void Start()
    {
        if (PlayerPrefs.HasKey("localPlayerName"))
        {
            localPlayerName = PlayerPrefs.GetString("localPlayerName");
        }
        if (PlayerPrefs.HasKey("LastSteamID"))
        {
            LastSteamID = PlayerPrefs.GetString("LastSteamID");
        }
        if (localPlayerName == null || localPlayerName.Equals(""))
        {
            FirstNameSet.SetActive(true);
            MainMenuScreen.SetActive(false);
        }

        MultiplayerSceneUnloaded();
    }
    private void Update()
    {
        if (checkConnecting) CheckConnection();

        // CheckAvailableChars();

        if (inLobby) UpdateLobby();

        if (ingameUIActive)
        {
            UpdateCooldowns();
            UpdateTabMenu();
            EscMenu();
        }
        else
        {
            ingameUICanvas.SetActive(false);
        }

    }

    public void MultiplayerSceneUnloaded()
    {
        canvas.SetActive(true);

        overlayCanvas.SetActive(false);

        ingameUICanvas.SetActive(false);
        ingameUICanvas.transform.GetChild(0).gameObject.SetActive(false);
        ingameUICanvas.transform.GetChild(1).gameObject.SetActive(false);    

        ready = false;
    }

    public void MultiplayerSceneLoaded()
    {
        canvas.SetActive(true);

        overlayCanvas.SetActive(false);

        
        ingameUICanvas.SetActive(true);

        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        buttonImage.color = Color.white;

        lobbyTimerFloat = 10;
        timerStarted = false;

        ready = false;
    }
    #region MainMenu
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
        HelpMenuScreen.transform.Find("SelectNameMenu").Find("NameInputField").GetComponent<TMP_InputField>().transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = localPlayerName;
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
        if (!inputField.GetComponent<TMP_InputField>().text.Equals(""))
        {
            localPlayerName = inputField.GetComponent<TMP_InputField>().text;
            PlayerPrefs.SetString("localPlayerName", localPlayerName);
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
        PlayMenuScreen.transform.Find("JoinMenu").GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = LastSteamID;
    }
    public void BackToJoinMenu()
    {
        PlayMenuScreen.transform.Find("JoinMenu").gameObject.SetActive(true);
        PlayMenuScreen.transform.Find("JoinMenu").GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = LastSteamID;
        PlayMenuScreen.transform.Find("ConnectingMenu").gameObject.SetActive(false);
    }
    public void OpenConnectingMenu()
    {
        
        PlayMenuScreen.transform.Find("JoinMenu").gameObject.SetActive(false);
        PlayMenuScreen.transform.Find("ConnectingMenu").gameObject.SetActive(true);
    }
    public void OpenCInfo()
    {
        HelpMenuScreen.transform.Find("CInfos").gameObject.SetActive(true);
        HelpMenuScreen.transform.Find("Menu").gameObject.SetActive(false);
    }
    #endregion
    #region ConncectMenu

    public void HostOnClick()
    {
        networkManager.StartHost();
        ConnectedToServer();
    }

    public void ConnectOnClick(TMP_InputField inputField)
    {
        checkConnecting = true;
        //string has a " in the end. Why? I don't know
        //string wrongInput = inputField.text;
        //char[] charArray = wrongInput.ToCharArray();

        //networkManager.networkAddress = new string(charArray,0,charArray.Length-1);
        PlayerPrefs.SetString("LastSteamID", inputField.text);
        PlayerPrefs.Save();
        LastSteamID = inputField.text;

        networkManager.networkAddress = inputField.text;
        networkManager.StartClient();
    }

    public void CancelConnectingOnClick()
    {
        checkConnecting = false;
        networkManager.StopClient();
        BackToJoinMenu();
    }

    private void UpdateLobby()
    {
        for(int i = 0; i < 5; i++)
        {
            //playerService.LocalPlayerManager.GetComponent<PlayerManager>().CmdGetSelectedPlayers();
            if (playerService.connectedPlayersName[i]!=null) playerNameTexts[i].text = playerService.connectedPlayersName[i];

        }

        if (timerStarted)
        {
            lobbyTimerFloat = lobbyTimerFloat - Time.deltaTime;
            if (lobbyTimerFloat < 0) lobbyTimerFloat = 0;
        }
        else
        {
            lobbyTimerFloat = 10;
        }

        timer.text = ((int)lobbyTimerFloat).ToString();
        
        
    }

     private void CheckConnection()
    {
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (!NetworkClient.active)
            {
                networkManager.StopClient();
                checkConnecting = false;
            }
        }
        else
        {
            if (GameObject.Find("ServiceManager") != null && GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayerManager != null)
            {

                ConnectedToServer();

            }
        }       
    }


    private void ConnectedToServer()
    {
        GameObject serviceManager = GameObject.Find("ServiceManager");
        playerService = serviceManager.GetComponent<PlayerService>();
        gameService = serviceManager.GetComponent<GameManagerService>();

        gameService.EventLobbyTimerStart += LobbyTimerStartedHandler;
        gameService.EventLobbyTimerReset += LobbyTimerResettedHandler;

        gameService.EventGameStart += GameStartHandler;
        gameService.EventGameEndMagesWon += GameEndMagesWonHandler;
        gameService.EventGameEndMonsterWon += GameEndMonsterWonHandler;

        if (playerService.LocalPlayerManager != null)
        {
            Initialised();
        }
        else
        {
            playerService.EventInitialisedPlayerManager += Initialised;
        }
       
        

        checkConnecting = false;
    }

    private void Initialised()
    {
        //playerService.EventInitialisedPlayerManager -= Initialised;
        playerService.LocalPlayerManager.GetComponent<PlayerManager>().CmdGetSelectedPlayers();
        TransitionToLobby();
    }

    private void LobbyTimerStartedHandler()
    {
        timerStarted = true;
    }

    private void LobbyTimerResettedHandler()
    {
        timerStarted = false;
    }

    #endregion
    #region PlayerSelection
    
    private void TransitionToLobby()
    {
        for(int i = 0; i < 5; i++)
        {
            playerService.connectedPlayersName[i] = "Not selected...";
        }

        for (int i = 0; i < 5; i++)
        {
            //playerService.LocalPlayerManager.GetComponent<PlayerManager>().CmdGetSelectedPlayers();
            if (playerService.connectedPlayersName[i] != null) playerNameTexts[i].text = playerService.connectedPlayersName[i];

        }

        canvas.SetActive(false);
        overlayCanvas.SetActive(true);
        MenuCrystal.SetActive(false);
        GameObject.Find("Lobby").transform.GetChild(0).gameObject.SetActive(true);
        inLobby = true;

        for (int i = 0; i < 5; i++)
        {
            tabMenu.transform.GetChild(0).GetChild(i).GetComponent<Image>().color = Color.black;
        }
    }

    private void StartIngameUi()
    {
        ingameUICanvas.SetActive(true);
        int type;
        if (playerService.LocalPlayer.GetComponent<AbstractController>().Type != PlayerService.Characters.Monster)
        {
            localMage = playerService.LocalPlayer.GetComponent<MagesController>();
            magesUI.SetActive(true);
            monsterUI.SetActive(false);
            type = (int)playerService.LocalPlayer.GetComponent<MagesController>().Type;

        }
        else
        {
            localMonster = playerService.LocalPlayer.GetComponent<ShadowMonsterController>();
            magesUI.SetActive(false);
            monsterUI.SetActive(true);
            type = (int)playerService.LocalPlayer.GetComponent<ShadowMonsterController>().Type;
        }
       
        ingameUIActive = true;
        SetSkillIcons(type);
    }

    private void SetSkillIcons(int type)
    {
        switch (type)
        {
            case 1:
                skills[1].sprite = skillIcons[2];
                break;
            case 2:
                skills[1].sprite = skillIcons[3];
                break;
            case 3:
                skills[1].sprite = skillIcons[4];
                break;
            case 4:
                skills[1].sprite = skillIcons[5];
                break;
        }
    }

    private void UpdateCooldowns()
    {
        if (localMage != null)
        {
            switch (localMage.quState)
            {
                case AbstractController.SpellState.Up:
                    skills[0].color = new Color(200, 200, 200);
                    quSpellCooldownText.SetText("0");
                    break;
                case AbstractController.SpellState.InAction:
                    skills[0].color = new Color(255, 255, 255);
                    quSpellCooldownText.SetText("");
                    break;
                case AbstractController.SpellState.Cooldown:
                    skills[0].color = new Color(150, 150, 150);
                    quSpellCooldownText.SetText(((int)localMage.quCooldownTimer).ToString());
                    break;
            }

            switch (localMage.otherState)
            {
                case AbstractController.SpellState.Up:
                    skills[1].color = new Color(200, 200, 200);
                    otherSpellCooldownText.SetText("0");
                    break;
                case AbstractController.SpellState.InAction:
                    skills[1].color = new Color(255, 255, 255);
                    otherSpellCooldownText.SetText("");
                    break;
                case AbstractController.SpellState.Cooldown:
                    skills[1].color = new Color(150, 150, 150);
                    otherSpellCooldownText.SetText(((int)localMage.otherSpellCooldownTimer).ToString());
                    break;
            }

            if (localMage.isCharging)
            {
                skills[2].color = new Color(255, 255, 255);
            }
            else
            {
                skills[2].color = new Color(200, 200, 200);
            }

            if (localMage.anyOneInRange)
            {
                if(localMage.isHealing || localMage.isReviving)
                {
                    skills[3].color = new Color(255, 255, 255);
                }
                else
                {
                    skills[3].color = new Color(200, 200, 200);
                }
            }
            else
            {
                skills[3].color = new Color(150, 150, 150);
            }
          
        }
        else if(localMonster!=null)
        {
            switch (localMonster.shadowVisionState)
            {
                case AbstractController.SpellState.Up:
                    ShadowVisionImage.color = new Color(200, 200, 200); 
                    ShadowVisionCooldownText.SetText("0");
                    break;
                case AbstractController.SpellState.InAction:
                    ShadowVisionImage.color = new Color(255, 255, 255);
                    ShadowVisionCooldownText.SetText("");
                    break;
                case AbstractController.SpellState.Cooldown:
                    ShadowVisionImage.color = new Color(150, 150, 150);
                    ShadowVisionCooldownText.SetText(((int)localMonster.shadowVisionTimer).ToString());
                    break;
            }

        }
    }

    private void GameStartHandler()
    {
        inLobby = false;
        
        overlayCanvas.SetActive(false);
        StartIngameUi();
    }
    public SceneService sceneService;

    public void GameEndMagesWonHandler()
    {
       
        canvas.SetActive(true);
        overMainMenu.SetActive(false);
        endScreen.SetActive(true);
        if (playerService.LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster)
        {
            endScreen.transform.GetChild(1).GetComponent<TMP_Text>().text = "You got defeated. The Mages banished all darkness from this land once again.";
            endScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You lost!";
        }
        else
        {
            endScreen.transform.GetChild(1).GetComponent<TMP_Text>().text = "You defeated the shadows. For now you can rest easy in the daylight again.";
            endScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You won!";
        }
        
    }

    public void GameEndMonsterWonHandler()
    {

        
        canvas.SetActive(true);
        overMainMenu.SetActive(false);
        endScreen.SetActive(true);
        if (playerService.LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster)
        {
            endScreen.transform.GetChild(1).GetComponent<TMP_Text>().text = "You deafeated the mages of this land. The darkness will prevail and grow.";
            endScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You won!";
        }
        else
        {
            endScreen.transform.GetChild(1).GetComponent<TMP_Text>().text = "You got defeated by a monster out of the shadows. Hope for this lands are far gone now.";
            endScreen.transform.GetChild(0).GetComponent<TMP_Text>().text = "You lost!";
        }
    }

    public void RequestAuthority(int index)
    {
        playerService.LocalPlayerManager.GetComponent<PlayerManager>().SpawnPlayer(index, localPlayerName);
    }
    #endregion

    public void ReadyOnClick(Image buttonImage)
    {

        PlayerManager playerManager = playerService.LocalPlayerManager.GetComponent<PlayerManager>();
        if (!ready)
        {
            playerManager.CmdReadyClick((int)playerManager.player.GetComponent<AbstractController>().Type);
            buttonImage.color = Color.green;
            ready = true;
        }
        else
        {
            playerManager.CmdUnreadyClick((int)playerManager.player.GetComponent<AbstractController>().Type);
            buttonImage.color = Color.white;
            ready = false;
        }
    }
    #region TabMenu
    public void UpdateTabMenu()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            tabMenu.SetActive(true);

            for (int i =0; i< CheckCystals(); i++)
            {
                Debug.Log(CheckCystals());
                tabMenu.transform.GetChild(0).GetChild(i).GetComponent<Image>().color = Color.white;
            }
            string[] names = GetPlayerNames();
            GameObject[] allPlayerObjects = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                allPlayerObjects[i] = GetAllPlayers()[i].gameObject;
            }

            int missingPlayers = 0;
            for (int i = 0; i< 5; i++)
            {
                if(names[i] != null)
                {
                    tabMenu.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
                    tabMenu.transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = names[i];
                    if (i != 0)
                    {
                        float fill = 0;
                        switch (allPlayerObjects[i].GetComponent<MagesController>().lifePoints)
                        {
                            case 0:
                                fill = 1f;
                                break;
                            case 1:
                                fill = 0.5f;
                                break;
                            case 2:
                                fill = 0f;
                                break;
                        }
                        tabMenu.transform.GetChild(1).GetChild(i).GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = fill;

                        if (allPlayerObjects[i].GetComponent<MagesController>().GetIsDead())
                        {
                            tabMenu.transform.GetChild(1).GetChild(i).GetChild(1).GetChild(2).gameObject.SetActive(true);
                            tabMenu.transform.GetChild(1).GetChild(i).GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = 0f;
                        }
                    }
                }
                else
                {
                    missingPlayers++;
                }
            }
            tabMenu.transform.GetChild(1).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 750 - (missingPlayers * 160));
        }
        else
        {
            if (tabMenu.activeInHierarchy)
            {
                tabMenu.SetActive(false);
            }
        }
    }
    private int CheckCystals()
    {
        return GameObject.Find("ServiceManager").GetComponent<CrystalService>().GetChargedCrystals();
    }
    private string[] GetPlayerNames()
    {
        return GameObject.Find("ServiceManager").GetComponent<PlayerService>().connectedPlayersName;
    }
    private PlayerService.NetworkBondedPlayer[] GetAllPlayers()
    {
        return GameObject.Find("ServiceManager").GetComponent<PlayerService>()._allChars;
    }
    #endregion
    #region EscMenu
    private void EscMenu()
    {
        try
        {
            if (playerService.LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster || !playerService.LocalPlayer.GetComponent<MagesController>().GetIsDead())
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (escMenuActive)
                    {
                        escMenu.SetActive(false);
                        escMenuActive = false;
                    }
                    else
                    {
                        escMenu.SetActive(true);
                        escMenuActive = true;
                    }
                }
            }
        }catch(Exception e)
        {
            Debug.Log("Was monster");
        }
        
        
    }

    public void ReturnToGame()
    {
        escMenu.SetActive(false);
        escMenuActive = false;
    }
    public void OpenInGameHelp()
    {
        escMenu.transform.GetChild(1).gameObject.SetActive(true);
        escMenu.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void CloseInGameHelp()
    {
        escMenu.transform.GetChild(1).gameObject.SetActive(false);
        escMenu.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void ExitToMenu()
    {
        ingameUIActive = false;
        overMainMenu.SetActive(true);
        MenuCrystal.SetActive(true);
        ingameUICanvas.SetActive(false);
        overlayCanvas.SetActive(false);
        sceneService.RemoveAuthorityAndReloadScene();
    }
    public void ExitToDesktop()
    {
        sceneService.RemoveAuthorityAndQuitApp();
    }
    #endregion
    public void ExitFromEndToMenu()
    {
        ingameUIActive = false;
        overMainMenu.SetActive(true);
        endScreen.SetActive(false);
        MenuCrystal.SetActive(true);
        ingameUICanvas.SetActive(false);
        overlayCanvas.SetActive(false);
        sceneService.ReloadScene();
    }
}
