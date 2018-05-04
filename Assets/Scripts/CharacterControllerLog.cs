using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerLog : MonoBehaviour {

    #region Variable (private)

    // Inspector serialized
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float directionDampTime = 0.25f;
    [SerializeField]
    private ThirdPersonCamer gamecam;
    [SerializeField]
    private float directionSpeed = 3.0f;
    [SerializeField]
    private float rotationDegreePerSecond = 120.0f;


    // Private global only
    private float speed = 0.0f;
    private float direction = 0.0f;
    private float horizontal = 0.0f;
    private float vertical = 0.0f;
    private AnimatorStateInfo stateInfo;

    // Hashes
    private int m_LocomotionId = 0;

    #endregion

    #region Unity event functions

    // Use this for initialization
    void Start ()
    {
        animator = GetComponent<Animator>();

        if (animator.layerCount >= 2)
        {
            animator.SetLayerWeight(1, 1);
        }

        // Hash all animation names for performance
        m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");


    }

    // Update is called once per frame
    void Update()
    {
        if (animator)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Pull values from controller/keyboard
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Translate controls stick coordinate into world/cam/character space;
            StickToWorldspace(this.transform, gamecam.transform, ref direction, ref speed);

            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);

            
        }
    }

    void FixedUpdate()
    {
        // Rotate character model if stick is tilted right or left, but only if character is moving in that direction
        if (IsInLocomotion() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0.0f, rotationDegreePerSecond * (horizontal < 0.0f ? -1.0f : 1.0f), 0.0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            this.transform.rotation = (this.transform.rotation * deltaRotation);
        }
        
    }

    #endregion

    #region Methods

    public void StickToWorldspace(Transform root, Transform camera, ref float directionOut, ref float speedOut)
    {
        Vector3 rootDirection = root.forward;

        Vector3 stickDirection = new Vector3(horizontal, 0.0f, vertical);

        speedOut = stickDirection.sqrMagnitude;

        // Get camera rotation
        Vector3 cameraDirection = camera.forward;
        cameraDirection.y = 0.0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, cameraDirection);

        // Convert joystick input in worldspace coordinate
        Vector3 moveDirection = referentialShift * stickDirection;
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

        //Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.0f, root.position.z), axisSign, Color.red);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.0f, root.position.z), moveDirection, Color.green);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.0f, root.position.z), rootDirection, Color.magenta);
        //Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.0f, root.position.z), stickDirection, Color.blue);

        float angleRootToMove = Vector3.Angle(rootDirection, moveDirection) * (axisSign.y >= 0 ? -1.0f : 1.0f);

        angleRootToMove /= 180;

        directionOut = angleRootToMove * directionSpeed;
    }

    public bool IsInLocomotion()
    {
        return stateInfo.fullPathHash == m_LocomotionId;
    }

    #endregion
}
