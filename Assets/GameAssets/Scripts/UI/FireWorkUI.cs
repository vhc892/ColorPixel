using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FireworkUI : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private int lastAliveCount;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void Update()
    {
        if (!ps.IsAlive()) return;
        // lấy số hạt còn sống hiện tại
        int alive = ps.GetParticles(particles);

        // === Khi hạt vừa chết (nổ) ===
        if (alive < lastAliveCount)
        {
            int died = lastAliveCount - alive;
            for (int i = 0; i < died; i++)
            {
                AudioManager.Instance.FireWorkExplodeSfx();
            }
        }

        // lưu lại số hạt cho frame tiếp theo
        lastAliveCount = alive;
    }
}
