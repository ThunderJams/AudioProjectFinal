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
    private bool goingLeft = false;
    private FloorType floorBelow = FloorType.None;

    // capulse collider
    [SerializeField] private CapsuleCollider feet;

    /// The Wwise event to trigger a footstep sound.
	public AK.Wwise.Event woodenFootstep = new AK.Wwise.Event();
    
	public AK.Wwise.Event carpetFootstep = new AK.Wwise.Event();

    /// The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;

    // walking speed
    [Range(0.01f, 5.0f)]    
    public float walkSpeed = 1.0f;

    ///	Used to determine when to trigger footstep sounds.
	private bool walking = true;
    ///	Used to determine when to trigger footstep sounds.
    private float walkCount = 0.0f;

    public int currentLocation;
    // public list of transforms
    public List<Transform> locations;
    // int move threshold for each location
    public List<int> moveThreshold;

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

        // if we are going to a new location and are not there yet
        if (locations[currentLocation].position != transform.position)
        {
            // move towards the current location
            transform.position = Vector3.MoveTowards(transform.position, locations[currentLocation].position, walkSpeed * Time.deltaTime);
            walking = true;
        }
        else
        {
            // call a new event or move elsewhere
            walking = false;
            EventAI();
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

        FloorCheck();
        
        // if floor below is none, get affected by gravity
        if (floorBelow == FloorType.None)
        {
            cc.Move(Vector3.down * Time.deltaTime * 9.8f);
        }

        // debug log what terrain we are standing on
        //Debug.Log(floorBelow);

    }

    void FloorCheck()
    {
        floorBelow = FloorType.None;

        // feet overlap circle
        Collider[] hitColliders = Physics.OverlapCapsule(feet.bounds.center, feet.bounds.center, feet.radius, LayerMask.GetMask("Floor"));
        // if hit colliders is not empty
        if (hitColliders.Length > 0)
        {
            floorBelow = FloorType.Floor;
        }

        // feet overlap circle
        Collider[] hitColliders2 = Physics.OverlapCapsule(feet.bounds.center, feet.bounds.center, feet.radius, LayerMask.GetMask("Carpet"));
        // if hit colliders is not empty
        if (hitColliders2.Length > 0)
        {
            floorBelow = FloorType.Carpet;
        }
    }
    
    // function for triggering footstep sounds
    void Footstep()
    {
        // if we are on the floor, play wooden footstep
        if (floorBelow == FloorType.Floor)
        {
            woodenFootstep.Post(gameObject);
        }
        // if we are on the carpet, play carpet footstep
        else if (floorBelow == FloorType.Carpet)
        {
            carpetFootstep.Post(gameObject);
        }

    }

    // run this event AI after every event (aka sound effect)
    // or after we have moved to a new location
    void EventAI()
    {
        // generate a move threshold based on the current location
        // generate a random number between 1 and 100
        int random = Random.Range(1, 100);

        if (random < moveThreshold[currentLocation])
        {
            // move to the next location
            currentLocation++;
            
            // if we are at the end of the list, go back to the beginning
            if (currentLocation >= locations.Count)
            {
                currentLocation = 0;
            }
        }
        else
        {
            // do event
            
        }




    }
}


