using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UITrackerObject is for handling and positioning the Icons of the given GameObjects on the right place.<para/>
/// This script should be placed on the GameObject that should be tracked.
/// 
/// Transforms the coordinates of the GameObject to camera coordinates.<para/>
/// Is able to display the icon and the direction of the GameObject on the screen.<para/>
/// If the GameObject is on screen it only shows the Icons and not the direction.<para/>
/// Different flags determine the visibility of the Icon.<para/>
/// <see cref="visible"/> is for the visbility of the icon if the GameObject is outside of the screen.<para/>
/// <see cref="visibleOnScreen"/> is for the visibility of the icon if the GameObject is inside of the screen.<para/>
/// 
/// The UITracker can also show the progress of something.<para/>
/// <see cref="progressionVisible"/> is for the visibility of the overlaying progression icon.<para/>
/// <see cref="progression"/> determines how much of the progression icon should be filled.<para/>
/// </summary>
public class UITracker : NetworkBehaviour
{
    public float testVar;
    public float testVarPlus;

    public float iconSize = 50f;
    [HideInInspector]
    float scaleRes = Screen.width / 200;
    [HideInInspector]
    public RectTransform directionElement;
    [HideInInspector]
    public RectTransform iconElement;
    [HideInInspector]
    public RectTransform progressionElement;
    [HideInInspector]
    public RectTransform xElement;
    private Image progressionImage;



    public GameObject directionPrefab;
    public GameObject iconPrefab;
    public GameObject progressionPrefab;
    public GameObject xIconPrefab;

    private Transform parent;

    public Camera cam;
    public bool visible = false; //Whether or not the object is visible in the camera.

    public float offSet = 0.05f;
    private float horizontalOffset;
    private float verticalOffset;

    public float offSetForArrow = 54f;

    private bool isLocalPlayerMage;

    public bool isVisibleToMages;


    public bool isXIconVisible;

    public bool visibleOnScreen = false;

    public bool progressionVisible = false;

    [SyncVar]
    private float progression;

    public float Progression { get => progression; set
        {

            if (value < 0)
            {
                value = 0;
            }
             if(progression > 1)
            {
                value = 1;
            }
            if (value != progression)
            {
                progression = value;
                CmdSetProgressionServer(value);
            }
                     
        }
    }
    [Command]
    private void CmdSetProgressionServer(float progression)
    {
        this.progression = progression;
    }

    void Start()
    {
        GameObject serviceManager = GameObject.Find("ServiceManager");
        parent = serviceManager.GetComponent<UITrackerService>().parent;
        cam = serviceManager.GetComponent<UITrackerService>().cam;

        directionElement =  Instantiate(directionPrefab,parent.transform).GetComponent<RectTransform>();
        iconElement = Instantiate(iconPrefab, parent.transform).GetComponent<RectTransform>();
        progressionElement = Instantiate(progressionPrefab, parent.transform).GetComponent<RectTransform>();
        xElement = Instantiate(xIconPrefab, parent.transform).GetComponent<RectTransform>();

        progressionImage = progressionElement.GetComponentInChildren<Image>();
        serviceManager.GetComponent<PlayerService>().EventInitialisedMage += Initialized;

        iconElement.gameObject.SetActive(false);
        directionElement.gameObject.SetActive(false);
        progressionElement.gameObject.SetActive(false);
        xElement.gameObject.SetActive(false);

    }

    private void Initialized()
    {
        if (GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster)
        {
            isLocalPlayerMage = false;

        }
        else
        {
            isLocalPlayerMage = true;
        }
    }

    /// <summary>
    /// Transforms the coordinates of the GameObject to camera coordinates.<para/>
    /// Is able to display the icon and the direction of the GameObject on the screen.<para/>
    /// If the GameObject is on screen it only shows the Icons and not the direction.<para/>
    /// Different flags determine the visibility of the Icon.<para/>
    /// <see cref="visible"/> is for the visbility of the icon if the GameObject is outside of the screen.<para/>
    /// <see cref="visibleOnScreen"/> is for the visibility of the icon if the GameObject is inside of the screen.<para/>
    /// 
    /// The UITracker can also show the progress of something.<para/>
    /// <see cref="progressionVisible"/> is for the visibility of the overlaying progression icon.<para/>
    /// <see cref="progression"/> determines how much of the progression icon should be filled.<para/>
    /// </summary>
    void Update()
    {     
        Vector3 position = cam.WorldToScreenPoint(transform.position);
        if (position.z < 1)
        {
            Vector3 tempPos = transform.position + (-position.z * 1.1f + 1) * (cam.transform.forward);
            Debug.DrawLine(transform.position, tempPos, Color.red);
            position = cam.WorldToScreenPoint(tempPos);
        }
        if (visible&&(isLocalPlayerMage==isVisibleToMages) &&(position.x<0||position.x > Screen.width || position.y < 0 || position.y > Screen.height))
        {
            directionElement.gameObject.SetActive(true);
            iconElement.gameObject.SetActive(true);


            horizontalOffset = Screen.width * offSet;
            verticalOffset = horizontalOffset;

            Vector2 screenPosition = new Vector2(position.x,position.y);

            if (screenPosition.x < 0 + horizontalOffset) screenPosition.x = 0 + horizontalOffset;
            if (screenPosition.x > Screen.width - horizontalOffset) screenPosition.x = Screen.width - horizontalOffset;
            if (screenPosition.y < 0 + verticalOffset) screenPosition.y = 0 + verticalOffset;
            if (screenPosition.y > Screen.height - verticalOffset) screenPosition.y =Screen.height - verticalOffset;
            

            Vector2 direction = new Vector2(position.x - (Screen.width / 2), position.y - (Screen.height / 2)).normalized;

            Quaternion rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(direction.x, direction.y, 0));

            Vector2 directionForArrow = new Vector2(position.x - screenPosition.x, position.y - screenPosition.y).normalized;
            Quaternion rotationForArrow = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(directionForArrow.x, directionForArrow.y, 0));
            //Vector2 positionArrow = screenPosition + direction * offSetForArrow;
            Vector2 positionArrow = screenPosition + directionForArrow * offSetForArrow;

            iconElement.position = new Vector3(screenPosition.x, screenPosition.y, 0);
            directionElement.position = new Vector3(positionArrow.x, positionArrow.y, 0);
           
            directionElement.rotation = rotationForArrow;

            ProgressionIcon(new Vector3(screenPosition.x, screenPosition.y, 0));
            DeathIcon(new Vector3(screenPosition.x, screenPosition.y, 0));
        }
        else if (visibleOnScreen && (isLocalPlayerMage == isVisibleToMages) &&(position.x >= 0 || position.x <= Screen.width || position.y >= 0 || position.y <= Screen.height))
        {
            //directionElement.gameObject.SetActive(true);
            iconElement.gameObject.SetActive(true);
            directionElement.gameObject.SetActive(false);
            // horizontalOffset = Screen.width * offSet;
            //verticalOffset = horizontalOffset;

            Vector2 screenPosition = new Vector2(position.x, position.y);                  

            iconElement.position = new Vector3(screenPosition.x, screenPosition.y, 0);

            ProgressionIcon(new Vector3(screenPosition.x, screenPosition.y, 0));
            DeathIcon(new Vector3(screenPosition.x, screenPosition.y, 0));
        }
        else
        {
            iconElement.gameObject.SetActive(false);
            directionElement.gameObject.SetActive(false);
            progressionElement.gameObject.SetActive(false);
            xElement.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the progression icon.
    /// </summary>
    /// <param name="position">Screenposition of the progression icon.</param>
    private void ProgressionIcon(Vector3 position)
    {
        //progression
        if (progressionVisible)
        {
            progressionElement.gameObject.SetActive(true);
            progressionElement.position = new Vector3(position.x, position.y, 0);

            progressionImage.fillAmount = progression;
        }
        else
        {
            progressionElement.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handle the X-Icon
    /// </summary>
    private void DeathIcon(Vector3 position)
    {
          //progression
        if (isXIconVisible)
        {
            xElement.gameObject.SetActive(true);
            xElement.position = new Vector3(position.x, position.y, 0);

           
        }
        else
        {
            xElement.gameObject.SetActive(false);
        }
    }


    public void OnBecameInvisible()
    {
        visible = false;
    }
    //Turns off the indicator if object is onscreen.
    public void OnBecameVisible()
    {
        visible = true;
    }
}
