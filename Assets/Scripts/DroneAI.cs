using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private FloorType floorBelow = FloorType.None;

    // capulse collider
    [SerializeField] private CapsuleCollider feet;

    // wwise variables for the footsteps
    public AK.Wwise.Event footsteps = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Switch carpetSwitch;
    [SerializeField] private AK.Wwise.Switch woodSwitch;

    // wwise event for the door knocking
    public AK.Wwise.Event doorKnock = new AK.Wwise.Event();

    // The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;

    // walking speed
    [Range(0.01f, 5.0f)]    
    public float walkSpeed = 1.0f;

    //	Used to determine when to trigger footstep sounds.
	private bool walking = true;
    private float walkCount = 0.0f;

    // This collection of variables is used to determine the simple path that the AI follows
    // The AI is given a list of locations it can go to, and back and forth
    public int currentLocation;
    // public list of transforms
    public List<Transform> locations;
    // int move threshold for each location
    public List<int> moveThreshold;
    public Transform roomLocation;
    bool directionOfTravel = true;

    // timer for the AI to wait - not moving or performing events
    float waitCounter = 0;

    // Variables for the AI walking into the player's room
    // This is very messy and there are definetly better ways to implement this.
    // I just wanted a quick solution to get it working, to be cleaned up if this was a larger scale project
    bool inRoom = false;
    bool walkingInRoom = false;
    bool leavingRoom = false;
    float roomTimer = 0;
    bool justLeftRoom = true;

    // variables for the ending screen
    public GameObject hudImage;
    public GameObject endingImage;
    public TextMeshProUGUI endingText;
    bool gameRunning = true;

    // reference to the player
    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        // if we are going to a new location and are not there yet, move towards the current location
        // the additional conditions are exceptions for if the drone is walking into the player's room
        if (locations[currentLocation].position != transform.position && !walkingInRoom && !inRoom && !leavingRoom)
        {
            transform.position = Vector3.MoveTowards(transform.position, locations[currentLocation].position, walkSpeed * Time.deltaTime);
            walking = true;
        }
        else if (!walkingInRoom && !inRoom && !leavingRoom)
        {
            // call a new event or move elsewhere
            walking = false;

            // Each event has a "wait counter", that represents how long the AI should wait before calling a new event
            // If we are waiting, simply decremenmt the counter
            if (waitCounter <= 0)
            {
                // calls a new event (either moving to a new location, or playing a sound at the current location)
                EventAI();
            }
            else
            {
                waitCounter -= Time.deltaTime;
            }
            
        }

        // code for moving the player into the room - determined by the OutsideDoorAI() function
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

        // code for leaving the room
        // the "justLeftRoom" variable is used to prevent the AI from immediately moving back into the room - to make the movement patterns feel more realistic
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
        
        // if the AI is in the room and the player has thier GBA open, fail the game
        if ((inRoom || leavingRoom) && gameRunning && player.movementEnabled)
        {
            if (player.gba.isActiveAndEnabled)
            {
                // fail
                hudImage.SetActive(false);
                endingImage.SetActive(true);
                endingText.text = "You were caught!" + "\n" + "You kept the GBA open for " + player.gba.GetTimer() + " seconds." + "\n" + "You earned " + player.score + " points." + "\n" + "Press Esc to Close Application.";
                gameRunning = false;
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

        // check the floor below each frame - used for playing the appropraite footsteps
        FloorCheck();
        
        // if floor below is none, get affected by gravity
        if (floorBelow == FloorType.None)
        {
            cc.Move(Vector3.down * Time.deltaTime * 9.8f);
        }

    }

    // Floor Check function - this is implemented through the use of an invisible collider below the player/drone called the "feet"
    // If the "feet" overlap with the floor, and that floor is carpet, we switch to the carpet footsteps in Wwise
    // If the "feet" overlap with the floor, and that floor is wood, we switch to the wood footsteps in Wwise
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
        // generate a random number between 1 and 100
        int random = Random.Range(1, 100);

        // use the gameboy audio level to influence the AI - the louder the gameboy, the more likely the drone will move
        if (player.gba.isActiveAndEnabled)
        {
            int vol = (int)player.gameboyAudioVolume;
            vol += 50;
            random = Random.Range(1, vol);
        }

        // if the randomly generated number is within the locations "move threshold", then we move to the new location
        // the move threshold is a value between 1 and 100 that represents the likelihood of the AI moving to a new location at the corrosponding location
        // for example, this number is 100% in the hallway, meaing the AI will never stop in the hallway
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
                    // if the drone has just left the room, they will not re-enter it until they go back upstairs
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

    // Function for determining the AI's actions directly outside the door
    private void OutsideDoorAI()
    {
        // Actions are randomised - with said random value being influenced by the gameboy audio level
        int random = Random.Range(1, 100);        
        if (player.gba.isActiveAndEnabled)
        {
            int vol = (int)player.gameboyAudioVolume;
            vol += 50;
            random = Random.Range(vol, 100);
        }

        if (random > 80)
        {
            // low chance of entering room
            walkingInRoom = true;

        }
        else if (random > 50)
        {
            // medium chance of knocking on door
            doorKnock.Post(gameObject);
            waitCounter = 3;
        }
        else
        {
            // low chance for drone to do nothing - to increase suspense
            waitCounter = 2;
        }
    }

    // Function for determining the AI's actions in the living room
    // The AI just switches on the TV and flicks through the channels
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


