using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enum for floor types
enum FloorType
{
    None,
    Floor,
    Carpet
}



public class DroneAI : MonoBehaviour
{
    CharacterController cc;
    private float internalTimer = 0;
    private bool goingLeft = true;
    private FloorType floorBelow = FloorType.None;

    /// The Wwise event to trigger a footstep sound.
	public AK.Wwise.Event woodenFootstep = new AK.Wwise.Event();
    
	public AK.Wwise.Event carpetFootstep = new AK.Wwise.Event();

    /// The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;

    ///	Used to determine when to trigger footstep sounds.
	private bool walking = true;
    ///	Used to determine when to trigger footstep sounds.
    private float walkCount = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        internalTimer += Time.deltaTime;
        // alternate moving left and right on a cycle
        if (internalTimer > 5)
        {
            internalTimer = 0;
            goingLeft = !goingLeft;
        }

        if (goingLeft)
        {
            cc.Move(Vector3.left * Time.deltaTime);
        }
        else
        {
            cc.Move(Vector3.right * Time.deltaTime);
        }

        // if we are walking, trigger footstep sounds
        if (walking)
        {
            walkCount += Time.deltaTime;

            if (walkCount > footstepRate)
            {
                Footstep();

                walkCount = 0.0f;
            }
        }

        // debug log what terrain we are standing on
        Debug.Log(floorBelow);
        
    }


    // function for triggering footstep sounds
    void Footstep()
    {
        // find out what is below us
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // if it is a floor, we are walking
            if (hit.collider.gameObject.tag == "Floor")
            {
                floorBelow = FloorType.Floor;
            }
            else if (hit.collider.gameObject.tag == "Carpet")
            {
                floorBelow = FloorType.Carpet;
            }
            else
            {
                floorBelow = FloorType.None;
            }

        }

        // if it is a floor, do floor footsteps
        if (floorBelow == FloorType.Floor)
        {
            woodenFootstep.Post(gameObject);
        }
        // if it is a carpet, do carpet footsteps
        else if (floorBelow == FloorType.Carpet)
        {
            carpetFootstep.Post(gameObject);
        }
    }
}


