using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GloveUI : MonoBehaviour
{
    private Image abilityIcon;
    
    private List<Upgrade> _unlockedUpgrades = new List<Upgrade>();
    private int _activeIndex = 0;
    public Upgrade ActiveUpgrade => _unlockedUpgrades.Count > 0 ? _unlockedUpgrades[_activeIndex] : null;

    private void Awake()
    {
        abilityIcon = transform.GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {
        RefreshIcon();
    }

    void OnEnable() => UpgradeManager.OnUpgradeUnlocked += AddAbility;
    void OnDisable() => UpgradeManager.OnUpgradeUnlocked -= AddAbility;

    void AddAbility(Upgrade upgrade)
    {
        Debug.Log($"GloveUI received upgrade: {upgrade.name}");
        if (upgrade is not IModifierUpgrade) return;
        _unlockedUpgrades.Add(upgrade);
        RefreshIcon();
    }

    public void CycleNext()
    {
        if (_unlockedUpgrades.Count == 0) return;
        _activeIndex = (_activeIndex + 1) % _unlockedUpgrades.Count;
        RefreshIcon();
    }

    void RefreshIcon()
    {
        Debug.Log($"Refreshing GloveUI icon for active upgrade: {ActiveUpgrade?.name ?? "None"}");
        if (ActiveUpgrade is IModifierUpgrade active)
        {
            abilityIcon.sprite = active.Icon;
            abilityIcon.gameObject.SetActive(true);
        }
        else
        {
            abilityIcon.gameObject.SetActive(false);
        }
    }
}
