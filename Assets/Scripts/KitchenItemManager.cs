using Fusion;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "KitchenItemManager", menuName = "Kitchen/KitchenItemManager")]
public class KitchenItemManager : ScriptableObject
{
    [System.Serializable]
    public struct KitchenItemVariantMap
    {
        public ItemVariant variant;        // enum z KitchenItem.cs
        public NetworkPrefabRef prefab;    // prefab dla danego wariantu
    }

    public KitchenItemVariantMap[] variantMap;

    private Dictionary<ItemVariant, NetworkPrefabRef> _variantDict;

    public void Init()
    {
        _variantDict = new Dictionary<ItemVariant, NetworkPrefabRef>();
        foreach (var v in variantMap)
            _variantDict[v.variant] = v.prefab;
    }

    public NetworkPrefabRef GetPrefab(ItemVariant variant)
    {
        if (_variantDict == null) Init();
        _variantDict.TryGetValue(variant, out var prefab);
        return prefab;
    }
}
