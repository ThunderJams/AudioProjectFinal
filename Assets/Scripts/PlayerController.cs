using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    public float speed = 6.0f;
    public bool movementEnabled = false;

    private FloorType floorBelow = FloorType.None;
    
    // capulse collider
    [SerializeField] private CapsuleCollider feet;

    [SerializeField] public Gameboy gba;

    // wwise bus
    [SerializeField] private AK.Wwise.RTPC gameboyAudio;

    [SerializeField]
    //[Range(-12, 12)]
    public float gameboyAudioVolume = 0;

    bool firstTimePlay = true;

    public float score = 0;

    public AK.Wwise.Event footsteps = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Switch carpetSwitch;
    [SerializeField] private AK.Wwise.Switch woodSwitch;

    ///	Used to determine when to trigger footstep sounds.
    private float walkCount = 0.0f;

    /// The speed at which footstep sounds are triggered.
    [Range(0.01f, 1.0f)]
    public float footstepRate = 0.3f;



    // start
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // update
    void Update()
    {
        // IF ESCAPE, CLOSE APPLICATION
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // if Q pressed, toggle movement
        if (Input.GetKeyDown(KeyCode.Q))
        {
            movementEnabled = !movementEnabled;
        }
        
        

        GameBoy();

        gameboyAudio.SetGlobalValue(gameboyAudioVolume);

        if (movementEnabled)
        {
            // get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            //// move
            //Vector3 direction = new Vector3(horizontal, 0, vertical);
            //characterController.Move(-direction * Time.deltaTime * speed);

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
        // if G is pressed
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (gba.isActiveAndEnabled)
            {
                // stop music
                
                gba.pauseEvent.Post(gba.gameObject);
                gba.gameObject.SetActive(false);

                
            }
            else
            {
                //gameboyAudioVolume = prevGBvol;
                gba.gameObject.SetActive(true);
                
                
                
                if (firstTimePlay)
                {
                    //mainSwitch.SetValue(gameObject);                    
                    gba.StartMusic();
                    Debug.Log("music started");
                    
                    firstTimePlay = false;

                }

                gba.resumeEvent.Post(gba.gameObject);
            }
        }

        // if up arrow key is pressed, increase game boy volume
        if (Input.GetKey("up") && gameboyAudioVolume <= -12)
        {
            gameboyAudioVolume += 10 * Time.deltaTime;
        }
        else if (Input.GetKey("down") && gameboyAudioVolume >= -36)
        {
            gameboyAudioVolume -= 10 * Time.deltaTime;
        }

        if (gba.isActiveAndEnabled)
        {
            if (gameboyAudioVolume > -35)
            {
                score += Time.deltaTime * (gameboyAudioVolume + 35);
            }
            
        }

        //if (score > 500 && !drumsSwitchedOn)
        //{
        //    Debug.Log("music switched");
        //    drumsSwitchedOn = true;
        //    drumsSwitch.SetValue(gameObject);
        //}
    }
}
