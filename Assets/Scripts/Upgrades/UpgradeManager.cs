using System.Collections.Generic;
using UnityEngine;
using Game.Resources;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] List<Upgrade> defaultUpgrades; 
    [SerializeField] GameObject player;

    private void Start()
    {
        foreach (var upgrade in defaultUpgrades)
        {
            UnlockUpgrade(upgrade, player);
        }
    }

    public static event Action<Upgrade> OnUpgradeUnlocked;
    readonly HashSet<UpgradeType> _unlockedUpgrades = new HashSet<UpgradeType>();

    public bool isUnlocked(UpgradeType upgradeType)
    {
        return _unlockedUpgrades.Contains(upgradeType);
    }

    public void UnlockUpgrade(Upgrade upgrade, GameObject player)
    {
        if (isUnlocked(upgrade.Type))
        {
            return;
        }

        _unlockedUpgrades.Add(upgrade.Type);
        upgrade.ApplyUpgrade(player);

        Debug.Log($"Unlocked upgrade: {upgrade.name}");
        OnUpgradeUnlocked?.Invoke(upgrade);
    }
}
