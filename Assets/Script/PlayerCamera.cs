using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public InputActionReference mouseLookAction;
    
    [Header("References")] 
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed = 10f;
    
    void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
        
        //rotate player obj
    }
    
    private void OnEnable()
    {
        mouseLookAction.action.Enable();
    }

    private void OnDisable()
    {
        mouseLookAction.action.Disable();
    }
}
