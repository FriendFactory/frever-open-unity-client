using UnityEngine;

public class RotationObject : MonoBehaviour
{
    public enum RotateAxis
    {
        X, Y, Z
    }

    private float minQuat;
    private float maxQuat;
    private float playerQuat;
    private Vector3 rotVector;
    private float angleDistance;
    private float halfDistance;
    private float curSpeed;
    private float min;
    private float max;

    [Header("Main Settings")]
    [Tooltip("Choose a rotation angle / Выбрать угол поворота")][SerializeField] private RotateAxis rotateAxis = RotateAxis.X;
    [Tooltip("Local coordinates / Локальные координаты")] [SerializeField] private bool local;
    [Tooltip("Choose which way to start to rotate / Выбрать в какую сторону начать вращать")][SerializeField] private bool anotherDirection;
    [Tooltip("Rotation speed / Скорость поворота")][SerializeField] private float speed;
    [Tooltip("The speed of acceleration of the gun when it changes the side / Скорость разгона пушки, когда меняет сторону вращения")][SerializeField] private float overclocking;
    [Tooltip("Slowdown (the more, the less slowdown) / Замедление (чем больше, тем меньше замедление - как-то так)")][SerializeField] private float smoothly;

    [Header("Clamp of X Axis")]
    [Tooltip("Minimum angle X / Минимальный угол по X")][SerializeField] private float minX;
    [Tooltip("X maximum angle / Максимальный угол по X")][SerializeField] private float maxX;

    [Header("Clamp of Y Axis")]
    [Tooltip("Min Y angle / Минимальный угол по Y")][SerializeField] private float minY;
    [Tooltip("Maximum angle of Y / Максимальный угол по Y")][SerializeField] private float maxY;

    [Header("Clamp of Z Axis")]
    [Tooltip("Z Minimum Angle / Минимальный угол по Z")][SerializeField] private float minZ;
    [Tooltip("Z maximum angle / Максимальный угол по Z")][SerializeField] private float maxZ;


    private void Awake()
    {
        SetClampAngles();
        curSpeed = speed;
    }

    private void Update()
    {
        playerQuat = GetPlayerRotation();

        Rotation();
    }

    private void Rotation()
    {
        curSpeed = SpeedCalculate();
        transform.Rotate((anotherDirection ? -rotVector : rotVector) * curSpeed * Time.deltaTime, local ? Space.Self : Space.World);

        if (anotherDirection)
        {
            if (playerQuat <= minQuat)
            {
                anotherDirection = false;
            }
        }
        else
        {
            if (playerQuat >= maxQuat)
            {
                anotherDirection = true;
            }
        }
    }

    private float SpeedCalculate()
    {
        float newSpeed = 0;
        float curDist = Mathf.Abs((playerQuat - (anotherDirection ? minQuat - smoothly : maxQuat + smoothly)) * 100);

        if(curDist <= halfDistance)
        {
            newSpeed = Mathf.Lerp(0, speed, curDist / halfDistance);
        }
        else
        {
            newSpeed = curSpeed + overclocking * Time.deltaTime;
        }

        newSpeed = Mathf.Clamp(newSpeed, 0, speed);

        return newSpeed;
    }

    private void SetClampAngles()
    {
        switch (rotateAxis)
        {
            case RotateAxis.X:
                min = minX;
                max = maxX;
                rotVector = Vector3.right;

                if(local)
                {
                    minQuat = Quaternion.Euler(min, transform.localRotation.y, transform.localRotation.z).x;
                    maxQuat = Quaternion.Euler(max, transform.localRotation.y, transform.localRotation.z).x;
                }
                else
                {
                    minQuat = Quaternion.Euler(min, transform.rotation.y, transform.rotation.z).x;
                    maxQuat = Quaternion.Euler(max, transform.rotation.y, transform.rotation.z).x;
                }

                break;

            case RotateAxis.Y:
                min = minY;
                max = maxY;
                rotVector = Vector3.up;

                if (local)
                {
                    minQuat = Quaternion.Euler(transform.localRotation.x, min, transform.localRotation.z).y;
                    maxQuat = Quaternion.Euler(transform.localRotation.x, max, transform.localRotation.z).y;
                }
                else
                {
                    minQuat = Quaternion.Euler(transform.rotation.x, min, transform.rotation.z).y;
                    maxQuat = Quaternion.Euler(transform.rotation.x, max, transform.rotation.z).y;
                }

                break;

            case RotateAxis.Z:
                min = minZ;
                max = maxZ;
                rotVector = Vector3.forward;

                if (local)
                {
                    minQuat = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, min).z;
                    maxQuat = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, max).z;
                }
                else
                {
                    minQuat = Quaternion.Euler(transform.rotation.x, transform.rotation.y, min).z;
                    maxQuat = Quaternion.Euler(transform.rotation.x, transform.rotation.y, max).z;
                }

                break;
        }

        angleDistance = Mathf.Abs((maxQuat + smoothly) - (minQuat - smoothly)) * 100;
        halfDistance = angleDistance / 2;
    }

    private float GetPlayerRotation()
    {
        switch (rotateAxis)
        {
            case RotateAxis.X:
                return local ? transform.localRotation.x : transform.rotation.x;

            case RotateAxis.Y:
                return local ? transform.localRotation.y : transform.rotation.y;

            case RotateAxis.Z:
                return local ? transform.localRotation.z : transform.rotation.z;

            default:
                return 0;
        }
    }
}