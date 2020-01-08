using UnityEngine;

namespace LightFramework.Resource.OfflineData
{
    public class EffectOfflineData : OfflineData
    {
        public ParticleSystem[] ParticleSystems;
        public TrailRenderer[] TrailRenderers;


        public override void ResetProp()
        {
            base.ResetProp();
            foreach (var particleSystem in ParticleSystems)
            {
                particleSystem.Clear(true);
                particleSystem.Play();
            }

            foreach (var trailRenderer in TrailRenderers)
            {
                trailRenderer.Clear();
            }
        }

        public override void BindData()
        {
            base.BindData();
            ParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            TrailRenderers = gameObject.GetComponentsInChildren<TrailRenderer>();
        }
    }
}