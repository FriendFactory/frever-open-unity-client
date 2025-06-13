using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class AntiGravity : MonoBehaviour
{
    public float MaxFloatingPosition = 0f;              //the maximum float position (Y-axis)
    public float AntiGravityStrength = 10f;             //the strength of the force pull
    public Vector3 AntiGravityDirection = new Vector3(0f, 1f, 0f);                //direction of antigravity force
    
    public float RandomRotationStrength = 3f;           //the random rotation strength which makes the object rotate
    public bool AntiGravitate = true;                   //enable or disable the anti gravity
    
    public float DownwardStrength = 3f;                 //the weak force that will make the object gradually land after reaching the max point
    public Vector3 DownwardDirection = new Vector3(0f, -1f, 0f);                   //direction of downward force
    public bool OnMaxPointStay = false;                 //make the object float and stay there when it reaches the max position
    
    private Rigidbody rb;                               //instantiate the RB variable up top for performance
    public bool VerticalLock = true;                    //make the object go up and down only no other axis movement

    public float PushForceAmount = 1f;                  //the amount of push force
    public float PushForceIterations = 3f;              //how many loops of push to take effect

    float pushI;                                        //save the main amount of push force iterations
    float floatingPos;                                  //save the max floating position

    bool _push = false;                                 //property modifier
    public bool StartPush {                             //trigger the push force property
        get {
            return _push;
        }

        set {
            if(!_push){
                floatingPos = MaxFloatingPosition;
                MaxFloatingPosition = Mathf.Infinity;
                pushI = PushForceIterations;
                _push = value;
            }
        }
    }

    void Awake(){
        //cache the rigidbody for performance
        rb = transform.GetComponent<Rigidbody>();

        //GRAVITY MUST BE ENABLED
        GetComponent<Rigidbody>().useGravity = true;

        floatingPos = MaxFloatingPosition;
    }

    //physics update
    void FixedUpdate() {
        //only when the anti gravitate checkbox is checked
        //trigger the method
        if(AntiGravitate){
            PullUpwards();
        }

        if(_push && pushI > 0f){
            PushForce();
            pushI--;
        }else{
            _push = false;
            MaxFloatingPosition = floatingPos;
        }
    }

    //the method responsible for the anti gravity
    void PullUpwards(){
        //if vertical lock is enabled
        //lock the rigidbody
        if(VerticalLock){
            //check if max point stay is checked
            //if so lock all 3 axis
            if(OnMaxPointStay && (transform.position.y >= MaxFloatingPosition)){
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
            }else{
                //if not lock the only two (X, Z)
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
            }
        }else{
            // vertical lock not enabled, disable the X and Z constraint
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX;
        }

        //if object is lower than the maximum floating position
        if (transform.position.y < MaxFloatingPosition) {
            //add force upwards
            rb.AddForce(AntiGravityDirection * AntiGravityStrength);
            //apply random rotation to all axis
            transform.Rotate(RandomRotationStrength, RandomRotationStrength, RandomRotationStrength);
        }
        
        //if object is more or equal to the max floating position
        //time to go down or stay afloat
        if (transform.position.y >= MaxFloatingPosition) {
            //if stay afloat is checked
            if (OnMaxPointStay) {
                //if vertical lock is checked
                //lock the 3 axis
                if(VerticalLock){
                    rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
                }else{
                    //freeze the Y axis on the rigidbody
                    rb.constraints = RigidbodyConstraints.FreezePositionY;
                }
                //keep rotating
                transform.Rotate(RandomRotationStrength, RandomRotationStrength, RandomRotationStrength);
            } else {
                //if not stay afloat
                //add a downward force that'll make it go down gradually
                rb.AddForce(DownwardDirection * DownwardStrength);
                transform.Rotate(RandomRotationStrength, RandomRotationStrength, RandomRotationStrength);
            }
        }
    }

    //add the push force
    public void PushForce(){
        rb.AddForce(AntiGravityDirection * PushForceAmount, ForceMode.Impulse);
    }
}