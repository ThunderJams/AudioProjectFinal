using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is attached to the player, and it is used to control the player's movement.
// If player movement is disabled, this mostly just controls opening and closing the GBA
// Many of the variables are self explanitory for player movement and/or redundant with the Drone AI script
public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    public float speed = 6.0f;
    public bool movementEnabled = false;

    private FloorType floorBelow = FloorType.None;
    
    // capulse collider
    [SerializeField] private CapsuleCollider feet;

    // reference to the gba
    [SerializeField] public Gameboy gba;

    // RTPC for interfacing with the volume of the GameBoyBus in Wwise
    [SerializeField] private AK.Wwise.RTPC gameboyAudio;
    [SerializeField] public float gameboyAudioVolume = 0;

    bool firstTimePlay = true;
    public float score = 0;

    // Wwise events for the footsteps
    public AK.Wwise.Event footsteps = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Switch carpetSwitch;
    [SerializeField] private AK.Wwise.Switch woodSwitch;

    //	Used to determine when to trigger footstep sounds.
    private float walkCount = 0.0f;

    // The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // update
    void Update()
    {
        // if Escape is pressed, close the application
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // if Q pressed, toggle movement
        if (Input.GetKeyDown(KeyCode.Q))
        {
            movementEnabled = !movementEnabled;
        }

        // call function for interfacing with the gameboy
        // this is called regardless of the movementEnabled variable
        GameBoy();

        // set the volume of the GBBus in Wwise to the gameboyAudioVolume variable
        gameboyAudio.SetGlobalValue(gameboyAudioVolume);

        if (movementEnabled)
        {
            // get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // move respective of camera direction
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            Vector3 desiredMoveDirection = forward * vertical + right * horizontal;
            characterController.Move(desiredMoveDirection * Time.deltaTime * speed);

            // check which surface we are currently on
            FloorCheck();

            // if we are walking, trigger footstep sounds
            if (horizontal != 0 || vertical != 0)
            {
                walkCount += Time.deltaTime;

                if (walkCount > footstepRate)
                {
                    footsteps.Post(gameObject);

                    walkCount = 0.0f;
                }
            }            

            // if floor below is none, get affected by gravity
            if (floorBelow == FloorType.None)
            {
                characterController.Move(Vector3.down * Time.deltaTime * 9.8f);
            }
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

    void GameBoy()
    {
        // if G is pressed, enable or disable the gameboy
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (gba.isActiveAndEnabled)
            {
                // pause music
                gba.pauseEvent.Post(gba.gameObject);
                gba.gameObject.SetActive(false);

                
            }
            else
            {
                gba.gameObject.SetActive(true);

                // opn first play, this posts the GBA music - every other time it simply resumes the existing music
                if (firstTimePlay)
                {                   
                    gba.StartMusic();
                    Debug.Log("music started");
                    
                    firstTimePlay = false;

                }

                gba.resumeEvent.Post(gba.gameObject);
            }
        }

        // if up arrow key is pressed, increase game boy volume, and vice versa
        // a range is used so that the player cannot completely mute the GBA, and/or turn it up too loud
        if (Input.GetKey("up") && gameboyAudioVolume <= -12)
        {
            gameboyAudioVolume += 10 * Time.deltaTime;
        }
        else if (Input.GetKey("down") && gameboyAudioVolume >= -36)
        {
            gameboyAudioVolume -= 10 * Time.deltaTime;
        }

        // increase the players score if the GBA is open. This is called at the end of the Game
        if (gba.isActiveAndEnabled)
        {
            if (gameboyAudioVolume > -35)
            {
                score += Time.deltaTime * (gameboyAudioVolume + 35);
            }
            
        }
    }
}
