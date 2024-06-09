using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarConfigurator : MonoBehaviour
{
    [SerializeField] private CarConfig _baseData;
    [SerializeField] private Transform _enginePoint;
    [SerializeField] private Transform _spoilerPoint;
    [SerializeField] private Transform[] _wheelPoints;

    private List<CarEntity> _appliedEntities;
    private List<CarEntity> _previewedEntities;

    public CarStats Stats { get; private set; }
    public List<CarEntity> AppliedEntities => _appliedEntities.ToList();

    public void Initialize(CarData data)
    {
        _appliedEntities = new List<CarEntity>();
        _previewedEntities = new List<CarEntity>();

        foreach (var entity in data.availableEntities)
            PreviewEntity(entity);

        ApplyPreviewed();
        RecalculateStats();
    }

    public void PreviewEntity(CarEntity entity)
    {
        switch (entity.Type)
        {
            case CarEntityType.Engine:
                ReplacePartModel(entity as CarPart, _enginePoint);
                break;
            case CarEntityType.Spoiler:
                ReplacePartModel(entity as CarPart, _spoilerPoint);
                break;
            case CarEntityType.Wheels:
                foreach (var point in _wheelPoints)
                    ReplacePartModel(entity as CarPart, point);
                break;
            case CarEntityType.Color:
                SetColor(entity as CarColor);
                break;
        }

        _previewedEntities.Add(entity);
    }

    public void ApplyPreviewed()
    {
        foreach (var entity in _previewedEntities)
        {
            _appliedEntities.RemoveAll(a => a.Type == entity.Type);
            _appliedEntities.Add(entity);
        }

        _previewedEntities.Clear();
    }

    public void RevertPreviewed()
    {
        foreach (var entity in _appliedEntities)
            PreviewEntity(entity);

        _previewedEntities.Clear();
    }

    private void ReplacePartModel(CarPart part, Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        if (part == null || parent == null)
            return;

        if (part.Model != null)
        {
            GameObject go = Instantiate(part.Model, transform);
            go.name = part.name;
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    private void SetColor(CarColor color)
    {
        // replace car body material (current car models don't support multiple materials (sharing single mat))
    }

    private void RecalculateStats()
    {
        CarStats stats = _baseData.BaseStats;

        foreach (var entity in _appliedEntities)
        {
            CarPart part = entity as CarPart;

            if (part != null)
                stats += part.Stats;
        }

        Stats = stats;
    }
}