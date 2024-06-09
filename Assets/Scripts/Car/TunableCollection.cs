using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
public class TunableCollection
{
    [SerializeField] private TunableGroup[] _groups;

    private Dictionary<CarEntityType, TunableGroup> _groupMap;

    public TunableGroup this[CarEntityType key] => _groupMap[key];
    public TunableGroup[] Groups => _groups;
    public bool ContainsGroup(CarEntityType key) => _groupMap.ContainsKey(key);

    public void Initialize()
    {
        _groupMap = new Dictionary<CarEntityType, TunableGroup>();

        foreach (var group in _groups)
            _groupMap.Add(group.type, group);
    }

    public void ValidateGroups()
    {
        foreach (var group in _groups.GroupBy(g => g.type))
        {
            if (group.Count() > 1)
                Debug.LogWarning("There are several tuning groups of the same type");
        }

        foreach (var group in _groups)
        {
            group.Validate();
        }
    }
}

[Serializable]
public struct TunableGroup
{
    public string name;
    public CarEntityType type;
    public List<CarEntity> entities;
    public bool canBeTuned;

    public void Validate()
    {
        if (entities == null || entities.Count == 0)
            return;

        foreach (var entity in entities.ToArray())
        {
            if (entity == null || entity.Type != type)
                entities.Remove(entity);
        }
    }
}