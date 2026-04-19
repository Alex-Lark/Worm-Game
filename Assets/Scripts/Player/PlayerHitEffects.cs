using System.Collections;
using CreatureParts;
using UnityEngine;

namespace Player
{
    public class PlayerHitEffects : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float hitEffectDuration;
        [SerializeField] private WormRenderer wormRenderer;
        [SerializeField] private Player player;
        [SerializeField] private MeshRenderer headMeshRenderer;
        [SerializeField] private ParticleSystem hitParticles;

        private Coroutine hitEffectCoroutine;

        private IEnumerator Start()
        {
            // Wait a frame for the material to be initialized
            yield return null;
            SetEmissionColor(Color.black);
            player.OnTakeDamage += OnTakeDamage;
            player.OnWormHeadbutHitBall += OnHitBall;
        }

        private void OnHitBall(Vector3 point)
        {
            PlayParticlesAtPoint(point);
        }

        private void OnDestroy()
        {
            player.OnTakeDamage -= OnTakeDamage;
        }

        private void OnTakeDamage(HitInfo hitInfo)
        {
            if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);

            PlayParticlesAtPoint(hitInfo.contactPoint);

            hitEffectCoroutine = StartCoroutine(HitEffectCoroutine());
        }

        private void PlayParticlesAtPoint(Vector3 point)
        {
            hitParticles.transform.position = point;
            hitParticles.Play();
        }

        private IEnumerator HitEffectCoroutine()
        {
            float elapsed = 0f;
            while (elapsed < hitEffectDuration)
            {
                float t = elapsed / hitEffectDuration;
                
                Color color = Color.Lerp(hitColor, Color.black, t);
                SetEmissionColor(color);
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetEmissionColor(Color.black);
            hitEffectCoroutine = null;
        }

        private void SetEmissionColor(Color color)
        {
            wormRenderer.wormMaterial.SetColor(EmissionColor, color);
            headMeshRenderer.material.SetColor(EmissionColor, color);
        }
    }
}
