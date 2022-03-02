using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PickChar : MonoBehaviour
{
    public Light WassilyLight;
    public Light ReyLight;
    public Light HaraldLight;
    public Light FylaLight;
    public Camera characterChooserCam;
    public Camera mainCamera;
    private MenuService menu;
    public PlayerService playerService;

    public Volume monsterVolume;
    public Light directionalLight;

    public GameManagerService gameManagerService;

    private float[] lerpValue;
    private bool[] lerpActive;
    private float[] lerpSpeed;


    

    private int localIndex = -1;

    private bool isSelected = false;
    void Start()
    {
        menu = GameObject.Find("MenuService").GetComponent<MenuService>();
        monsterVolume.gameObject.SetActive(true);
        monsterVolume.weight = 0;

        gameManagerService.EventGameStart += GameStartHandler;



        characterChooserCam.gameObject.SetActive(true);

        characterChooserCam.enabled = true;

        mainCamera.enabled = false;

        lerpValue = new float[5];
        lerpActive = new bool[5];
        lerpSpeed = new float[]{1.2f,3,3,3,3};


        directionalLight.gameObject.SetActive(false);
        directionalLight.enabled = false;


    }

    private void GameStartHandler()
    {
        gameManagerService.EventGameStart -= GameStartHandler;
        mainCamera.enabled = true;
        Destroy(gameObject.transform.parent.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (menu.inLobby)
        {
            bool[] connectedPlayers = playerService.connectedPlayers;
            if (connectedPlayers == null) connectedPlayers = new bool[5];

            if (!isSelected)
            {

                for (int i = 0; i < 5; i++)
                {
                    lerpActive[i] = false;
                }

                int charakterIndex = -1;
                Ray screenRay = characterChooserCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit screenRayHit;
                //Moves the sphere to the given Position.If anything is in the way of the sphere, the sphere will be stopped there. The new position is calculated via raycast.
                if (Physics.Raycast(screenRay, out screenRayHit, 100))
                {
                    string tag = screenRayHit.collider.tag;
                    switch (tag)
                    {
                        //jewils den Helden noch auswählen
                        case "Monster":
                            lerpActive[0] = true;
                            if (!connectedPlayers[0])
                            {
                                charakterIndex = 0;
                            }
                            else
                            {
                                charakterIndex = -1;
                            }
                            break;
                        case "Harald":
                            lerpActive[1] = true;
                            HaraldLight.range = 3;
                            if (!connectedPlayers[1])
                            {
                                charakterIndex = 1;
                            }
                            else
                            {
                                charakterIndex = -1;
                            }
                            break;
                        case "Wassily":
                            lerpActive[2] = true;
                            WassilyLight.range = 3;
                            if (!connectedPlayers[2])
                            {
                                charakterIndex = 2;
                            }
                            else
                            {
                                charakterIndex = -1;
                            }

                            break;
                        case "Rey":
                            ReyLight.range = 3;
                            lerpActive[3] = true;
                            if (!connectedPlayers[3])
                            {
                                charakterIndex = 3;
                            }
                            else
                            {
                                charakterIndex = -1;
                            }


                            break;


                        case "Fyla":
                            lerpActive[4] = true;
                            FylaLight.range = 3;
                            if (!connectedPlayers[4])
                            {
                                charakterIndex = 4;
                            }
                            else
                            {
                                charakterIndex = -1;
                            }

                            break;

                        default:

                            ReyLight.range = 1.6f;
                            WassilyLight.range = 1.6f;
                            HaraldLight.range = 1.6f;
                            FylaLight.range = 1.6f;

                            charakterIndex = -1;
                            break;



                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (charakterIndex != -1)
                        {
                            if (charakterIndex > 0)
                            {
                                //directionalLight.gameObject.SetActive(true);
                                //directionalLight.enabled = true;
                            }
                            menu.RequestAuthority(charakterIndex);
                            localIndex = charakterIndex;
                            isSelected = true;
                        }
                    }
                }

            }
            if (connectedPlayers[0] && localIndex != 0)
            {
            }
            else if (localIndex == 0)
            {
                lerpActive[0] = true;
            }

            if (connectedPlayers[1] && localIndex != 1)
            {
                HaraldLight.color = Color.red;
                HaraldLight.range = 3;
            }
            else if (localIndex == 1)
            {
                HaraldLight.range = 3;

            }

            if (connectedPlayers[2] && localIndex != 2)
            {
                WassilyLight.color = Color.red;
                WassilyLight.range = 3;
            }
            else if (localIndex == 2)
            {
                WassilyLight.range = 3;

            }

            if (connectedPlayers[3] && localIndex != 3)
            {
                ReyLight.color = Color.red;
                ReyLight.range = 3;
            }
            else if (localIndex == 3)
            {
                ReyLight.range = 3;
            }

            if (connectedPlayers[4] && localIndex != 4)
            {
                FylaLight.color = Color.red;
                FylaLight.range = 3;
            }
            else if (localIndex == 4)
            {
                FylaLight.range = 3;
            }


            for (int i = 0; i < lerpActive.Length; i++)
            {
                if (!connectedPlayers[i])
                {
                    if (lerpActive[i])
                    {
                        lerpValue[i] += Time.deltaTime * lerpSpeed[i];
                        if (lerpValue[i] > 1) lerpValue[i] = 1;
                    }
                    else
                    {
                        lerpValue[i] -= Time.deltaTime * lerpSpeed[i];
                        if (lerpValue[i] < 0) lerpValue[i] = 0;
                    }
                    switch (i)
                    {
                        case 0:
                            monsterVolume.weight = lerpValue[i];
                            break;
                        case 1:
                            HaraldLight.range = Mathf.Lerp(1.6f, 3, lerpValue[i]);
                            break;
                        case 2:
                            WassilyLight.range = Mathf.Lerp(1.6f, 3, lerpValue[i]);
                            break;
                        case 3:
                            ReyLight.range = Mathf.Lerp(1.6f, 3, lerpValue[i]);
                            break;
                        case 4:
                            FylaLight.range = Mathf.Lerp(1.6f, 3, lerpValue[i]);
                            break;
                    }
                }
            }
        }
    }
}
