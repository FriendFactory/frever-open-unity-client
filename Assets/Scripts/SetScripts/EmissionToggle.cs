using BoatAttack;
using UnityEngine;

namespace SetScripts
{
    public class EmissionToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _emissionObject;
        [SerializeField] private DayNightController2 _dayNightController;
        [SerializeField] private float _emissionIntensity;
        [SerializeField] private Color _emissionColor;

        private static readonly int EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");
        private Renderer _renderer;
        private int? _materialIndex;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private Renderer Renderer => _renderer
            ? _renderer
            : _renderer = _emissionObject.GetComponent<Renderer>();

        private int? MaterialIndex => _materialIndex ?? (_materialIndex = GetEmissionMaterialIndex());

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _dayNightController.DayStatusChanged += OnDayStatusChanged;
        }

        private void OnDisable()
        {
            _dayNightController.DayStatusChanged -= OnDayStatusChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnDayStatusChanged(DayStatus status)
        {
            if (status == DayStatus.Night)
            {
                ChangeEmission(_emissionColor * _emissionIntensity);
            }
            else
            {
                ChangeEmission(Color.black);
            }
        }

        private void ChangeEmission(Color emissionColor)
        {
            if (!MaterialIndex.HasValue) return;
            var emissionMaterial = Renderer.materials[MaterialIndex.Value];
            emissionMaterial.SetColor(EMISSION_COLOR, emissionColor);
            Renderer.materials[MaterialIndex.Value] = emissionMaterial;
        }

        private int? GetEmissionMaterialIndex()
        {
            for (var i = 0; i < Renderer.materials.Length; i++)
            {
                var material = Renderer.materials[i];
                if (material.name.StartsWith("Emission")) return i;
            }

            return null;
        }
    }
}