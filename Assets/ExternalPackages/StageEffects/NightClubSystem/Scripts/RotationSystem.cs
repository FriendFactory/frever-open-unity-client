using Modules.LevelManaging.Assets.AssetHelpers;
using UnityEngine;

public class RotationSystem : MonoBehaviour, IVfxComponent
{
    private enum RotateAxis
    {
        X,
        Y,
        Z
    }

    private Vector3 curRotVector = Vector3.zero;
    private float vel = 0;

    [Header("Main Settings")] [Tooltip("Axis of rotation / Ось вращения")] [SerializeField]
    private RotateAxis axis = RotateAxis.X;

    [Tooltip("Starting direction of rotation / Стартовое направление вращения")] [SerializeField]
    private bool positiveDirection = true;

    [Tooltip("Rotate in local coordinates? / Вращать в локальных координатах?")] [SerializeField]
    private bool local = false;

    [Tooltip("Rotation without limits / Вращение без ограничений")] [SerializeField]
    private bool simpleRotation = false;

    [Tooltip(
        "The initial angle of rotation of the object (enter manually) / Начальный угол поворота объекта(вписать вручную)")]
    [SerializeField]
    private Vector3 startRotVector = Vector3.zero;

    [Tooltip("Rotation speed in unlimited mode / Скорость вращения в режиме без ограничения")] [SerializeField]
    private float speedSimpleRotation = 30;

    [Tooltip("Smoothing Time / Время сглаживания")] [SerializeField]
    private float smoothTime = 1;

    [Tooltip("Smoothing braking (the less the smoother) / Сглаживание торможения (чем меньше тем плавнее)")]
    [SerializeField]
    private float smoothlyStop = 1;

    [Tooltip("Min rotation angle / Мин угол вращения")] [SerializeField]
    private float minAngle = -180;

    [Tooltip("Max angle of rotation / Макс угол вращения")] [SerializeField]
    private float maxAngle = 180;

    private float _minTargetAngle;
    private float _maxTargetAngle;
    private float _anglePerSec;
    private float _startAngle;
    private bool _isPlaying;

    private void Awake()
    {
        curRotVector = startRotVector;
        _isPlaying = true;
    }

    private void Start()
    {
        switch (axis)
        {
            case RotateAxis.X:
                _startAngle = curRotVector.x;
                break;

            case RotateAxis.Y:
                _startAngle = curRotVector.y;
                break;

            case RotateAxis.Z:
                _startAngle = curRotVector.z;
                break;
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (simpleRotation)
        {
            SimpleRotation();
        }
        else
        {
            Rotation();
        }
    }

    private void SimpleRotation()
    {
        if (local)
        {
            transform.Rotate(
                Time.fixedDeltaTime * new Vector3(axis == RotateAxis.X ? speedSimpleRotation : 0,
                                                  axis == RotateAxis.Y ? speedSimpleRotation : 0,
                                                  axis == RotateAxis.Z ? speedSimpleRotation : 0),
                Space.Self);
        }
        else
        {
            transform.Rotate(
                Time.fixedDeltaTime * new Vector3(axis == RotateAxis.X ? speedSimpleRotation : 0,
                                                  axis == RotateAxis.Y ? speedSimpleRotation : 0,
                                                  axis == RotateAxis.Z ? speedSimpleRotation : 0),
                Space.World);
        }
    }

    private void Rotation()
    {
        if (local)
        {
            _minTargetAngle = minAngle - smoothlyStop;
            _maxTargetAngle = maxAngle + smoothlyStop;
            switch (axis)
            {
                case RotateAxis.X:
                    curRotVector.x = Mathf.SmoothDamp(curRotVector.x,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel,smoothTime);
                    break;

                case RotateAxis.Y:
                    curRotVector.y = Mathf.SmoothDamp(curRotVector.y,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel, smoothTime);
                    break;

                case RotateAxis.Z:
                    curRotVector.z = Mathf.SmoothDamp(curRotVector.z,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel, smoothTime);
                    break;
            }

            transform.localEulerAngles = curRotVector;
        }
        else
        {
            switch (axis)
            {
                case RotateAxis.X:
                    curRotVector.x = Mathf.SmoothDamp(curRotVector.x,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel, smoothTime);
                    break;

                case RotateAxis.Y:
                    curRotVector.y = Mathf.SmoothDamp(curRotVector.y,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel, smoothTime);
                    break;

                case RotateAxis.Z:
                    curRotVector.y = Mathf.SmoothDamp(curRotVector.y,
                                                      positiveDirection ? _maxTargetAngle : _minTargetAngle, ref vel, smoothTime);
                    break;
            }

            transform.eulerAngles = curRotVector;
        }

        if (positiveDirection)
        {
            switch (axis)
            {
                case RotateAxis.X:
                    if (curRotVector.x >= maxAngle)
                    {
                        positiveDirection = false;
                        _startAngle = curRotVector.x;
                    }

                    break;

                case RotateAxis.Y:
                    if (curRotVector.y >= maxAngle)
                    {
                        positiveDirection = false;
                        _startAngle = curRotVector.y;
                    }

                    break;

                case RotateAxis.Z:
                    if (curRotVector.z >= maxAngle)
                    {
                        positiveDirection = false;
                        _startAngle = curRotVector.z;
                    }

                    break;
            }
        }
        else
        {
            switch (axis)
            {
                case RotateAxis.X:
                    if (curRotVector.x <= minAngle)
                    {
                        positiveDirection = true;
                        _startAngle = curRotVector.x;
                    }

                    break;

                case RotateAxis.Y:
                    if (curRotVector.y <= minAngle)
                    {
                        positiveDirection = true;
                        _startAngle = curRotVector.y;
                    }

                    break;

                case RotateAxis.Z:
                    if (curRotVector.z <= minAngle)
                    {
                        positiveDirection = true;
                        _startAngle = curRotVector.z;
                    }

                    break;
            }
        }
    }

    private void SetAngleAtTime(float time)
    {
        float anglePosition;
        if (smoothTime > 0)
        {
            time = time % smoothTime;

            if (positiveDirection)
            {
                _anglePerSec = Mathf.Abs(_maxTargetAngle / smoothTime);
            }
            else
            {
                _anglePerSec = Mathf.Abs(_minTargetAngle / smoothTime);
            }

            anglePosition = _anglePerSec * time;
        }
        else
        {
            if (positiveDirection)
            {
                anglePosition = _maxTargetAngle;
            }
            else
            {
                anglePosition = _minTargetAngle;
            }
        }

        switch (axis)
        {
            case RotateAxis.X:
                curRotVector.x = anglePosition;
                break;

            case RotateAxis.Y:
                curRotVector.y = anglePosition;
                break;

            case RotateAxis.Z:
                curRotVector.z = anglePosition;
                break;
        }

        transform.localEulerAngles = curRotVector;
    }

    public void Simulate(float time)
    {
        SetAngleAtTime(time);
    }

    public void Play()
    {
        _isPlaying = true;
    }

    public void Resume()
    {
        _isPlaying = true;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Stop()
    {
        _isPlaying = false;
    }
}