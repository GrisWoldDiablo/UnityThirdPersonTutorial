using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BarsEffect))]
public class ThirdPersonCamer : MonoBehaviour {

	#region Variables (private)

	[SerializeField]
	private float distanceAway;
	[SerializeField]
	private float distanceUp;
    [SerializeField]
	private float smooth;
	[SerializeField]
	private Transform followXForm;
    [SerializeField]
    private float distanceFromWall;
    [SerializeField]
    private float widescreen = 0.2f;
    [SerializeField]
    private float targetingTime = 0.5f;




    // Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float camSmoothDampTime = 0.1f;

    // Private global only
    private Vector3 lookDir;
	private Vector3 targetPosition;
    private BarsEffect barsEffect;
    private CamStates camState = CamStates.Behind;
    #endregion

    #region Properties (public)
    
    public enum CamStates
    {
        Behind,
        FirstPerson,
        Target,
        Free
    } 

    #endregion


    #region Unity event functions
    // Use this for initialization
    void Start ()
	{
		followXForm = GameObject.FindWithTag("Player").transform;

        barsEffect = GetComponent<BarsEffect>();
        if (barsEffect == null)
        {
            Debug.LogError("Attach a widescreen BarsEffet script to the camera.", this);
        }

		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void LateUpdate()
	{
        Vector3 characterOffset = followXForm.position + new Vector3(0.0f, distanceUp, 0.0f);

        // Determine camera state
        if (Input.GetAxis("Target") > 0.01f)
        {
            barsEffect.coverage = Mathf.SmoothStep(barsEffect.coverage, widescreen, targetingTime);

            camState = CamStates.Target;
        }
        else
        {
            barsEffect.coverage = Mathf.SmoothStep(barsEffect.coverage, 0.0f, targetingTime);

            camState = CamStates.Behind;
        }

        // Execute camera state
        switch (camState)
        {
            case CamStates.Behind:
                // Calculate direction from camera to player, kill Y, and normalize to give a valid direction with unity magnitude
                lookDir = characterOffset - this.transform.position;
                lookDir.y = 0.0f;
                lookDir.Normalize();
                Debug.DrawRay(this.transform.position, lookDir, Color.green);

                // Setting the target position to be the correct ofset from the hovercraft
                targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
                Debug.DrawLine(followXForm.position, targetPosition, Color.magenta);
                break;

            case CamStates.FirstPerson:
                break;

            case CamStates.Target:
                lookDir = followXForm.forward;
                break;

            case CamStates.Free:
                break;

            default:
                break;
        }


        targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
        // Making a smooth trasition between its current position and the position it wants to be in
        CompensateForWalls(characterOffset, ref targetPosition);
        SmoothPosition(this.transform.position, targetPosition);
        
		// Make sure the camera is looking the right way
		transform.LookAt(characterOffset);

	}

	#endregion

	#region Methods

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        // Making a smooth transition between camera's current position and the position it wants to be in

        this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        Debug.DrawLine(fromObject, toTarget, Color.cyan);
        // Compensate for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject,toTarget,out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);// + wallHit.normal * distanceFromWall;
        }
    }
	#endregion
}
