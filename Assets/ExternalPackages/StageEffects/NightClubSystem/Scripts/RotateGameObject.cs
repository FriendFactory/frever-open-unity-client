using UnityEngine;

public class RotateGameObject : MonoBehaviour
{
    public enum RotateState
    {
        Free, Controllable
    }

	public float rot_speed_x=0;
	public float rot_speed_y=0;
	public float rot_speed_z=0;
	public bool local=false;

    [SerializeField] private RotateState rotateState = RotateState.Free;

    public bool IsRotate { get; set; }

	private void Update ()
    {
        if (rotateState == RotateState.Controllable)
        {
            if(IsRotate)
            {
                Rotate();
            }
        }
        else
        {
            Rotate();
        }
	}

    private void Rotate()
    {
        if (local)
        {
            transform.Rotate(Time.fixedDeltaTime * new Vector3(rot_speed_x, rot_speed_y, rot_speed_z), Space.Self);
        }
        else
        {
            transform.Rotate(Time.fixedDeltaTime * new Vector3(rot_speed_x, rot_speed_y, rot_speed_z), Space.World);
        }
    }
}
