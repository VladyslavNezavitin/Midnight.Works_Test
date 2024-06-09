using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuItemView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _selectionMark;

    public Button Button => _button;
    public void SetSprite(Sprite sprite) => _icon.sprite = sprite;
    public void SetNameText(string text) => _nameText.text = text;
    public void SetSelected(bool isSelected) => _selectionMark.SetActive(isSelected);
}