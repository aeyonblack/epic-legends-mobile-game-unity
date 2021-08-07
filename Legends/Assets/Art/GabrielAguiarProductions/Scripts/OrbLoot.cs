using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider))]
public class OrbLootBox : MonoBehaviour
{
    public Animator cameraAnimator;
    public GameObject lootBox;
    public GameObject lootBoxFractured;
    public GameObject lootReward;

    private GameObject fracturedObject;
    private GameObject loot;
    private Animator animator;    
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) 
        {
            if (hit.transform.name == gameObject.name)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Hover", true);

                cameraAnimator.SetBool("Idle", false);
                cameraAnimator.SetBool("Hover", true);

                if (Input.GetMouseButtonDown(0)) 
                {            
                    animator.SetBool("Open", true);
                    cameraAnimator.SetBool("Open", true);
                }
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Hover", false);
                
                cameraAnimator.SetBool("Idle", true);
                cameraAnimator.SetBool("Hover", false);
            } 
        }      
    }

    //called via animation
    public void LootReward ()
    {  
        loot = Instantiate (lootReward) as GameObject;

        lootBox.SetActive (false);

        if(lootBoxFractured != null)
        {
            fracturedObject = Instantiate (lootBoxFractured) as GameObject;
        }
    }

    public void Restart ()
    {        
        animator.Rebind();
        animator.SetBool("Idle", true);
        animator.SetBool("Open", false);
        animator.SetBool("Hover", false);

        cameraAnimator.Rebind();
        cameraAnimator.SetBool("Idle", true);
        cameraAnimator.SetBool("Open", false);
        cameraAnimator.SetBool("Hover", false);

        Destroy (loot);
        Destroy (fracturedObject);

        lootBox.SetActive (true);
    }

    IEnumerator RestartCo ()
    {
        yield return new WaitForFixedUpdate();
    }
}
