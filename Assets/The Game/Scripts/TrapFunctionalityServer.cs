using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFunctionalityServer : NetworkBehaviour
{

    private ShadowMonsterController monstersController;

   // Start is called before the first frame update
    private void Start()
    {
        GameObject monsterGameObject = GameObject.Find("ServiceManager").GetComponent<PlayerService>().monster;
        monstersController = monsterGameObject.GetComponent<ShadowMonsterController>();

        GetComponent<CapsuleCollider>().enabled = monstersController._isLocalPlayer;

        if(GameObject.Find("ServiceManager").GetComponent<PlayerService>().LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the trap collision with the monster.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Monster")
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            monstersController.TrapRoot(gameObject);
            Animator animator = gameObject.GetComponent<Animator>();
            animator.SetBool("Trap Trigger", true);
        }
    }
}
