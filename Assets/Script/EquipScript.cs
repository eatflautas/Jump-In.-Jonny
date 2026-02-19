using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipScript : MonoBehaviour
{
    public Transform PlayerTransform;
    public GameObject Gun;
    public Camera Camera;
    public float range = 2f;
    
    [Header("Throw Settings")]
    public float throwForce = 15f;
    public float extraUpwardForce = 2f;

    // This tracks whether the gun is in our hands
    private bool isHolding = true; 

    void Start()
    {
        // Start the game with the gun dropped/kinematic off 
        // OR set this based on your initial scene setup
        Gun.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isHolding)
            {
                UnequipObject();
            }
            else
            {
                Shoot(); // This is your "Pick Up" raycast logic
            }
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, range))
        {
            // Check if we hit the Gun or a Target
            if (hit.transform.gameObject == Gun || hit.transform.GetComponent<Target>() != null)
            {
                EquipObject();
            }
        }
    }

    void UnequipObject()
    {
        isHolding = false; // Update our state

        Gun.transform.SetParent(null); 
        Rigidbody rb = Gun.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        Vector3 forceToApply = Camera.transform.forward * throwForce + transform.up * extraUpwardForce;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        
        rb.AddTorque(new Vector3(Random.Range(-10, 10), 10, Random.Range(-10, 10)));
    }

    void EquipObject()
    {
        isHolding = true; // Update our state

        Gun.GetComponent<Rigidbody>().isKinematic = true;
        Gun.transform.position = PlayerTransform.transform.position;
        Gun.transform.rotation = PlayerTransform.transform.rotation;
        Gun.transform.SetParent(PlayerTransform);
    }
}