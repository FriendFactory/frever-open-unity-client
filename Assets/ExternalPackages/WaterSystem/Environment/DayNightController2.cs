using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Serialization;

namespace BoatAttack
{
    /// <summary>
    /// Simple day/night system
    /// </summary>
    [ExecuteInEditMode]
    public class DayNightController2 : MonoBehaviour
    {
        private const float NIGHT_TIME = 0.75f;
        private const float MID_DAY_TIME = 0.5f;
        private const float MORNING_TIME = 0.25f;

        private static readonly int ROTATION = Shader.PropertyToID("_Rotation");
        private static readonly int SKY_TINT = Shader.PropertyToID("_SkyTint");
        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
        private static readonly int CLOUD_COLOR = Shader.PropertyToID("_CloudColor");
        private static readonly int NIGHT_FADE = Shader.PropertyToID("_NightFade");

        // Serialized

        [Range(0, 1)]
        [SerializeField]
        private float _time = MID_DAY_TIME;

        [Range(0,1)]
        public float StartTime = 0.255f;

        [SerializeField]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private bool _autoIcrement;

        [SerializeField]
        private float _speed = 1f;
        
        [Header("Skybox Settings")]

            [SerializeField]
            private Material _skybox; // skybox reference

            [SerializeField]
            private Gradient _skyboxColour; // skybox tint over time

            [SerializeField]
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            private Transform clouds;

            [Range(-180, 180)]
            [SerializeField]
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            private float cloudOffset;

            [SerializeField]
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            private ReflectionProbe[] reflections;

        [Header("Sun Settings")]

            [SerializeField]
            private Light _sun;

            [SerializeField]
            private Gradient _sunColour;

            [Range(0, 360)]
            [SerializeField]
            private float _northHeading = 136;

            [Range(0, 90)]
            [SerializeField]
            private float _tilt = 60f;

        [Header("Ambient Lighting")]

            [SerializeField]
            private Gradient _ambientColour; // ambient light colour (not used in LWRP correctly) over time

        [Header("Fog Settings")]

            [SerializeField]
            [GradientUsage(true)]
            private Gradient _fogColour; // fog colour over time

        [Header("Cloud Settings")]

            [GradientUsage(true)]
            [SerializeField] private Gradient _cloudColor;
        
        [Header("Particle systems")]

            [SerializeField] private ParticleSystemRenderer[] _particleSystems;

        [Tooltip("Time: time value from this class. Value: Alpha of the Base Color material, that is being used by Particle Systems")]
        [SerializeField] private AnimationCurve _timeParticlesAlphaCurve;

        // Internal

        private float _prevTime;
        private float _deltaTime;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<float> TimeUpdated;
        public event Action<DayStatus> DayStatusChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float Time => _time;
        public Gradient FogColour => _fogColour;

        public DayStatus CurrentDayStatus { get; private set; }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public bool AutoIncrement
        {
            get => _autoIcrement;
            set => _autoIcrement = value;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _time = StartTime;
            _prevTime = _time;
        }
        
        private void Update()
        {
            if (UnityEngine.Time.deltaTime == 0) return;
            
            if (AutoIncrement && Application.isPlaying)
            {
                _deltaTime += UnityEngine.Time.deltaTime;
                var t = Mathf.Repeat(_deltaTime * _speed, 1f);
                _time = t;
            }
            
            if (Math.Abs(_time - _prevTime) > 0.0001f) // if time has changed
            {
                SetTimeOfDay(_time);
            }

            var newStatus = GetDayStatus();
            if (CurrentDayStatus == newStatus) return;

            CurrentDayStatus = newStatus;
            DayStatusChanged?.Invoke(newStatus);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Run()
        {
            //force update
            SetTimeOfDay(_time);
        }

        public void SetTime(float time)
        {
            SetTimeOfDay(time);
        }
        
        public void SetSpeed(float speed)
        {
            _speed = speed;
            _autoIcrement = speed != 0;
            _deltaTime = (AutoIncrement) ? _time / speed : _time;
        }
        
        public float GetMidDayTime()
        {
            return MID_DAY_TIME;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        /// <summary>
        /// Sets the time of day
        /// </summary>
        /// <param name="time">Time in linear 0-1</param>
        /// <param name="reflectionUpdate">Update reflections at the same time</param>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void SetTimeOfDay(float time, bool reflectionUpdate = false)
        {
            _time = time;
            _prevTime = time;

            if (reflections != null && reflections.Length > 0 && reflectionUpdate)
            {
                foreach (var probe in reflections)
                {
                    probe.RenderProbe();
                }
            }
            
            // do update
            if (_sun)
            {
                // update sun
                _sun.transform.forward = Vector3.down;
                _sun.transform.rotation *= Quaternion.AngleAxis(_northHeading, Vector3.forward); // north facing
                _sun.transform.rotation *= Quaternion.AngleAxis(_tilt, Vector3.up);
                _sun.transform.rotation *= Quaternion.AngleAxis((_time * 360f) - 180f, Vector3.right); // time of day

                _sun.color = _sunColour.Evaluate(Mathf.Clamp01(_time * 2f - 0.5f));
            }
            if (_skybox)
            {
                // update skybox
                _skybox.SetFloat(ROTATION, 85 + ((_time - 0.5f) * 20f)); // rotate slightly for cheap moving cloud eefect
                _skybox.SetColor(SKY_TINT, _skyboxColour.Evaluate(_time));
            }

            if (clouds)
            {
                clouds.eulerAngles = new Vector3(0f, _time * 45f + cloudOffset, 0f);
            }
            
            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    var color = _particleSystems[i].sharedMaterial.GetColor(BASE_COLOR);
                    color.a = _timeParticlesAlphaCurve.Evaluate(_time);//Mathf.Clamp01(nightFade * _starsFadeMultiplier);
                    _particleSystems[i].sharedMaterial.SetColor(BASE_COLOR, color);
                }
            }

            Shader.SetGlobalColor(CLOUD_COLOR, _cloudColor.Evaluate(_time));
            Shader.SetGlobalFloat(NIGHT_FADE, Mathf.Clamp01(Mathf.Abs(_time * 2f - 1f) * 3f - 1f));
            RenderSettings.fogColor = _fogColour.Evaluate(_time); // update fog colour
            RenderSettings.ambientSkyColor = _ambientColour.Evaluate(_time); // update ambient light colour
            TimeUpdated?.Invoke(_time);
        }

        private DayStatus GetDayStatus()
        {
            if (MORNING_TIME <= _time && MID_DAY_TIME > _time) return DayStatus.Morning;
            if (MID_DAY_TIME <= _time && NIGHT_TIME > _time) return DayStatus.Day;
            if (NIGHT_TIME <= _time || MORNING_TIME > _time) return DayStatus.Night;

            return DayStatus.None;
        }
    }
    
    public enum DayStatus
    {
        None,
        Night,
        Day,
        Morning
    }
}