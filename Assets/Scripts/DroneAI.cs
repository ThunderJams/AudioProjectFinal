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
    public Television tv;
    private float internalTimer = 0;
    private bool goingLeft = false;
    private FloorType floorBelow = FloorType.None;

    // capulse collider
    [SerializeField] private CapsuleCollider feet;

    public AK.Wwise.Event footsteps = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Switch carpetSwitch;
    [SerializeField] private AK.Wwise.Switch woodSwitch;


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

    bool directionOfTravel = true;

    float waitCounter = 0;

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
        // and if wwise sound is not playing
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

            if (waitCounter <= 0)
            {
                EventAI();
            }
            else
            {
                waitCounter -= Time.deltaTime;
            }
            
        }

        // if we are walking, trigger footstep sounds
        if (walking)
        {
            walkCount += Time.deltaTime;

            if (walkCount > footstepRate)
            {
                footsteps.Post(gameObject);

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
            woodSwitch.SetValue(gameObject);
        }

        // feet overlap circle
        Collider[] hitColliders2 = Physics.OverlapCapsule(feet.bounds.center, feet.bounds.center, feet.radius, LayerMask.GetMask("Carpet"));
        // if hit colliders is not empty
        if (hitColliders2.Length > 0)
        {
            floorBelow = FloorType.Carpet;
            carpetSwitch.SetValue(gameObject);
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
            if (directionOfTravel)
            {
                currentLocation++;
            }
            else
            {
                currentLocation--;
            }

            // flip direction of travel if we go out of the list
            if (currentLocation >= locations.Count)
            {
                currentLocation = locations.Count - 2;
                directionOfTravel = false;
            }
            else if (currentLocation < 0)
            {
                currentLocation = 1;
                directionOfTravel = true;
            }
        }
        else
        {
            // do event
            // if we are in the living room
            if (currentLocation == 4)
            {
                

                // if the tv is on
                if (tv.isTVon)
                {
                    int random2 = Random.Range(1, 100);
                    
                    if (random2 <= 10)
                    {
                        // if tv is on - turn tv off
                        tv.tvOff.Post(tv.gameObject);
                        tv.isTVon = false;
                        waitCounter = 4;
                    }
                    else if (tv.currentChannel != TVChannel.News)
                    {
                        // if static is on, switch to news
                        
                        // stop playing the existing sound
                        tv.tvOff.Post(tv.gameObject);
                        tv.newsSwitch.SetValue(tv.gameObject);
                        tv.tv.Post(tv.gameObject);
                        tv.currentChannel = TVChannel.News;
                        waitCounter = 4;
                    }

                }
                else
                {
                    tv.tv.Post(tv.gameObject);
                    tv.isTVon = true;
                    waitCounter = 3;
                }
                

                

                

                
            }
        }




    }

    // when the drone stops colliding with door
    private void OnTriggerExit(Collider other)
    {
        // if we are colliding with a door
        if (other.gameObject.tag == "Door")
        {
            // play the door sound
            other.gameObject.GetComponent<Door>().doorEvent.Post(other.gameObject);
        }
    }
}


