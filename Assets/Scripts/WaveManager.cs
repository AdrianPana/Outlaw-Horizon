using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public Material oceanMaterial;

    public Vector4 waveA;
    public Vector4 waveC;
    public Vector4 waveB;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        waveA = oceanMaterial.GetVector("_WaveA");
        waveB = oceanMaterial.GetVector("_WaveB");
        waveC = oceanMaterial.GetVector("_WaveC");
    }

    public float GetWaveHeight(Vector3 worldPos)
    {
        float time = Time.time;
        Vector3 origin = transform.position;
        Vector3 local = worldPos - origin;
        Vector3 guess = local;

        // Vertices also move horizontally, so find the grid point that
        // ends up at this XZ. A few fixed-point iterations are enough.
        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = TotalOffset(guess, time);
            guess.x = local.x - offset.x;
            guess.z = local.z - offset.z;
        }

        return origin.y + TotalOffset(guess, time).y;
    }

    Vector3 GerstnerOffset(Vector4 wave, Vector3 gridPoint, float time)
    {
        float steepness = wave.z;
        float k = 2f * Mathf.PI / wave.w;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = new Vector2(wave.x, wave.y).normalized;
        float f = k * (Vector2.Dot(d, new Vector2(gridPoint.x, gridPoint.z)) - c * time);
        float a = steepness / k;
        return new Vector3(d.x * a * Mathf.Cos(f), a * Mathf.Sin(f), d.y * a * Mathf.Cos(f));
    }

    Vector3 TotalOffset(Vector3 gridPoint, float time)
    {
        return GerstnerOffset(waveA, gridPoint, time)
             + GerstnerOffset(waveB, gridPoint, time)
             + GerstnerOffset(waveC, gridPoint, time);
    }
}
