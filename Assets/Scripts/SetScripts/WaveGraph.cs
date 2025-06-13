using UnityEngine;

namespace SetScripts
{
    public class WaveGraph : MonoBehaviour
    {
        public Transform prefab;
        [Range(0,100)] public int resolution = 10;
        [Range(0, 2)]
        public int function;
        [Range(0, 1)]
        public float speed;
        Transform [] cubes; // it's called cubes because in the beginning I tested with cubes

        static Functions[] graphs = { SineFunction, Sine2DFunction, MultiSineFunction };

        void Awake()
        {
            var step = 2f / resolution;
            var scale = Vector3.one * step;
            Vector3 position;
            position.y = 0;
            position.z = 0;
            cubes = new Transform[resolution * resolution];
            for (int i = 0, z = 0; z < resolution; z++)
            {
                position.z = (z + 0.5f) * step - 1f;
                for (int x = 0; x < resolution; x++, i++)
                {
                    var cube = Instantiate(prefab, transform, false);
                    position.x = (x + 0.5f) * step - 1f;

                    cube.localPosition = position;
                    cube.localScale = scale;
                    cubes[i] = cube;
                }
            }
        }

        private const float PI = Mathf.PI;

        private static float SineFunction(float x, float z, float t)
        {
            return Mathf.Sin(PI * (x + t));
        }

        private static float MultiSineFunction(float x, float z, float t)
        {
            float y = Mathf.Sin(PI * (x + t));
            y += Mathf.Sin(2f * PI * (x + 2f * t)) / 2f;
            y *= 2f / 3f;
            return y;
        }
        private static float Sine2DFunction(float x, float z, float t)
        {
            return Mathf.Sin(PI * (x + z + t));
        }
        private void Update()
        {
            var time = Time.time * speed;
            var graph = graphs[function];
            for (int i = 0; i < cubes.Length; i++)
            {
                var point = cubes[i];
                var position = point.localPosition;
                position.y = graph(position.x, position.z, time);
                point.localPosition = position;
            }
        }
    }
}
