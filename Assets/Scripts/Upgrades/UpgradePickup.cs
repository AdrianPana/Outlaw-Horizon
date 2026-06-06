using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UpgradePickup : MonoBehaviour
{
    public Upgrade upgrade;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UpgradeManager.Instance.UnlockUpgrade(upgrade, other.gameObject);
            Destroy(gameObject);
        }
    }
}
