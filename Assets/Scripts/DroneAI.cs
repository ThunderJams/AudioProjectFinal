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


    public AK.Wwise.Event doorKnock = new AK.Wwise.Event();

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

    public Transform roomLocation;

    bool directionOfTravel = true;

    float waitCounter = 0;

    bool inRoom = false;
    bool walkingInRoom = false;
    bool leavingRoom = false;
    float roomTimer = 0;
    bool justLeftRoom = true;

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
        if (locations[currentLocation].position != transform.position && !walkingInRoom && !inRoom && !leavingRoom)
        {
            // move towards the current location
            transform.position = Vector3.MoveTowards(transform.position, locations[currentLocation].position, walkSpeed * Time.deltaTime);
            walking = true;
        }
        else if (!walkingInRoom && !inRoom && !leavingRoom)
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

        if (walkingInRoom && roomLocation.position != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, roomLocation.position, walkSpeed * Time.deltaTime);
            
        }
        else if (walkingInRoom)
        {
            inRoom = true;
            walkingInRoom = false;
            roomTimer = 3;
        }
        else if (roomTimer <= 0 && inRoom)
        {
            inRoom = false;
            leavingRoom = true;
        }

        if (leavingRoom && locations[currentLocation].position != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, locations[currentLocation].position, walkSpeed * Time.deltaTime);
        }
        else if (leavingRoom)
        {
            leavingRoom = false;
            justLeftRoom = true;
        }

        if (inRoom)
        {
            roomTimer -= Time.deltaTime;

            
        }
        
        if (inRoom || leavingRoom)
        {
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gba.isActiveAndEnabled)
            {
                // fail
            }
        }

        // if we are walking, trigger footstep sounds
        if (walking || (walkingInRoom && !inRoom) || leavingRoom)
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

        // use the gameboy audio level to influence the AI - the louder the gameboy, the more likely the drone will move
        // it is slightly inefficient to Find the Player each time but this is just for ease of access for this assignment
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gba.isActiveAndEnabled)
        {
            int vol = (int)GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gameboyAudioVolume;
            vol += 50;
            random = Random.Range(1, vol);
        }
        
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

            justLeftRoom = false;
        }

        else
        {
            // switch statement for current location
            switch (currentLocation)
            {
                // case 0: at player's door
                case 0:
                    if (!justLeftRoom)
                    {
                        OutsideDoorAI();
                    }
                    
                    break;

                // case 4: in living room
                case 4:
                    LivingRoomAI();
                    break;
            }
        }
    }

    // when the drone stops colliding with door, play a door closing sound
    private void OnTriggerExit(Collider other)
    {
        // if we are colliding with a door
        if (other.gameObject.tag == "Door")
        {
            // play the door sound
            other.gameObject.GetComponent<Door>().doorEvent.Post(other.gameObject);
        }
    }

    private void OutsideDoorAI()
    {
        int random = Random.Range(1, 100);        
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gba.isActiveAndEnabled)
        {
            int vol = (int)GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gameboyAudioVolume;
            vol += 50;
            random = Random.Range(vol, 100);
        }

        
        if (random > 50)
        {
            // medium chance of knocking on door
            doorKnock.Post(gameObject);
            waitCounter = 3;
        }
        else if (random < 80)
        {
            // low chance of entering room

            // go to the positon
            walkingInRoom = true;
            
        }
        else
        {
            // low chance for drone to do nothing
            waitCounter = 2;
        }
    }

    private void LivingRoomAI()
    {
        // if the tv is on
        if (tv.isTVon)
        {
            // random AI to switch between channels
            int random = Random.Range(1, 100);
            if (random <= 10)
            {
                // if tv is on - turn tv off
                tv.tvOff.Post(tv.gameObject);
                tv.isTVon = false;
                waitCounter = 4;
            }
            else if (tv.currentChannel != TVChannel.News)
            {
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


