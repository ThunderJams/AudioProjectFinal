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

    float score = 0;

    // wwise switch
    public AK.Wwise.Switch mainSwitch = new AK.Wwise.Switch();
    public AK.Wwise.Switch drumsSwitch = new AK.Wwise.Switch();
    bool drumsSwitchedOn = false;

    



    // start
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // update
    void Update()
    {
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

            FloorCheck();

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
        }

        // feet overlap circle
        Collider[] hitColliders2 = Physics.OverlapCapsule(feet.bounds.center, feet.bounds.center, feet.radius, LayerMask.GetMask("Carpet"));
        // if hit colliders is not empty
        if (hitColliders2.Length > 0)
        {
            floorBelow = FloorType.Carpet;
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
