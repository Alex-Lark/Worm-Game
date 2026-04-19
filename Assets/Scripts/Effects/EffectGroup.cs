using UnityEngine;

[System.Serializable]
public class EffectGroup
{
    [SerializeField] private Effect[] _effects;

    public void Play(Vector2 position, Vector2 direction = default)
    {
        foreach (Effect effect in _effects)
        {
            effect.Play(position, direction.normalized);
        }
    }

    [System.Serializable]
    public class Effect
    {
        public enum EffectType
        {
            ScreenShake,
            SoundEffect,
            PooledEffect
        }

        public enum Intensity
        {
            Low,
            Medium,
            High
        }

        [SerializeField] private EffectType _type;

        // ScreenShake
        [SerializeField] private Intensity _intensity;

        // Animation
        [SerializeField] private AnimationClip _animationClip;

        // Pooled
        [SerializeField] private GameObject _pooledEffectPrefab;

        public void Play(Vector2 position, Vector2 direction = default)
        {
            switch (_type)
            {
                case EffectType.ScreenShake:
                    float intensityValue;
                    float duration;
                    switch (_intensity)
                    {
                        case Intensity.Low:
                            intensityValue = 1f; duration = 0.15f; break;
                        case Intensity.Medium:
                            intensityValue = 1.75f; duration = 0.2f; break;
                        case Intensity.High:
                            intensityValue = 2.5f; duration = 0.3f; break;
                        default: throw new System.ArgumentOutOfRangeException();
                    }
                    // ScreenShake.Instance.PlayScreenShake(intensityValue, duration);
                    break;
                case EffectType.SoundEffect:
                    // _audioContainer.Play(position);
                    break;
                case EffectType.PooledEffect:
                    // EffectPooler.Instance.GetPooledEffect(_pooledEffectPrefab, position, direction.To2DQuaternion());
                    break;
            }
        }
    }
}