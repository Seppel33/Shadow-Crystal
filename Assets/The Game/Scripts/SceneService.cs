using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService : MonoBehaviour
{
    AsyncOperation multiplayerSceneLoading;
    AsyncOperation multiplayerSceneUnloading;
    bool sceneIsLoading = false;
    
    bool sceneIsUnloading = false;
    public MenuService menuService;
    private void Awake()
    {
        multiplayerSceneLoading = SceneManager.LoadSceneAsync("Shadow Crystal Map", LoadSceneMode.Additive);
        sceneIsLoading = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (multiplayerSceneLoading!=null&&multiplayerSceneLoading.isDone&& sceneIsLoading)
        {
            sceneIsLoading = false;
            menuService.MultiplayerSceneLoaded();
        }

        if(multiplayerSceneUnloading!=null&&multiplayerSceneUnloading.isDone&& sceneIsUnloading)
        {
            sceneIsUnloading = false;
            multiplayerSceneLoading = SceneManager.LoadSceneAsync("Shadow Crystal Map", LoadSceneMode.Additive);
            sceneIsLoading = true;
        }
    }

    public void ReloadScene()
    {
        multiplayerSceneUnloading = SceneManager.UnloadSceneAsync("Shadow Crystal Map");
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopHost();
        sceneIsUnloading = true;
        menuService.MultiplayerSceneUnloaded();
    }


    public void RemoveAuthorityAndReloadScene()
    {
        try
        {
            AbstractController abstractController = GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayer.GetComponent<AbstractController>();
            if(abstractController.Type == 0)
            {
                GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayerManager.GetComponent<PlayerManager>().CmdTriggerEndGame();
            }
            else
            {
                ((MagesController)abstractController).CmdAddDeadPlayerAfterLeavingGame();
            }



            
            
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            ReloadScene();
        }
        
    }

    public void RemoveAuthorityAndQuitApp()
    {
        try
        {
            GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayer.GetComponent<AbstractController>().CmdRemoveClientAuthority();

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopHost();
            Application.Quit();
        }
    }


}
