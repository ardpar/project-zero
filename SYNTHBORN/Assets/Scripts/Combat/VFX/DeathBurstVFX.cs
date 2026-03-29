using UnityEngine;

namespace Synthborn.Combat.VFX
{
    /// <summary>
    /// Simple death burst VFX: spawns a burst of colored particles, then self-destructs.
    /// Configurable via Inspector for per-enemy-type color/size variety.
    /// </summary>
    public class DeathBurstVFX : MonoBehaviour
    {
        [SerializeField] private Color _burstColor = Color.white;
        [SerializeField] private float _burstSize = 0.2f;
        [SerializeField] private int _particleCount = 8;
        [SerializeField] private float _lifetime = 0.5f;
        [SerializeField] private Material _burstMaterial;

        private void Start()
        {
            var ps = GetComponent<ParticleSystem>();
            if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = _lifetime;
            main.startSpeed = 3f;
            main.startSize = _burstSize;
            main.startColor = _burstColor;
            main.maxParticles = _particleCount;
            main.loop = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, _particleCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null && _burstMaterial != null)
            {
                renderer.material = _burstMaterial;
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", _burstColor);
                renderer.SetPropertyBlock(mpb);
            }

            ps.Play();
            Destroy(gameObject, _lifetime + 0.1f);
        }
    }
}
