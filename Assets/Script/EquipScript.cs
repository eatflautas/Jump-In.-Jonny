using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipScript : MonoBehaviour
{
    public Transform PlayerTransform;
    public Camera Camera;
    public float range = 3f;

    [Header("Throw Settings")]
    public float throwForce = 15f;
    public float extraUpwardForce = 2f;

    // This now tracks whatever object we are currently holding
    private GameObject currentItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // If we are already holding something, drop/throw it first
            if (currentItem != null)
            {
                UnequipObject();
            }
            // If we aren't holding anything, try to pick something up
            else
            {
                CheckForPickup();
            }
        }
    }

    void CheckForPickup()
    {
        RaycastHit hit;
        // Search for objects in front of the camera
        if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, range))
        {
            // IMPORTANT: Make sure the pickable objects have the Tag "Item" 
            // and a Rigidbody component
            if (hit.transform.CompareTag("Item"))
            {
                EquipObject(hit.transform.gameObject);
            }
        }
    }

    void EquipObject(GameObject obj)
    {
        currentItem = obj;
        
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Stop physics while holding
            rb.detectCollisions = false; // Prevent it from bumping into you
        }

        // Snap to hand position
        currentItem.transform.SetParent(PlayerTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
    }

    void UnequipObject()
    {
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;

            // Throw logic
            Vector3 forceToApply = Camera.transform.forward * throwForce + transform.up * extraUpwardForce;
            rb.AddForce(forceToApply, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-5, 5), 5, Random.Range(-5, 5)));
        }

        currentItem.transform.SetParent(null);
        currentItem = null; // We are no longer holding anything
    }
}