using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuGUI : GameGUI
{
    [SerializeField] private TMP_InputField _usernameInputField;
    private PlayerData _player;

    public void Initialize(PlayerData player)
    {
        _player = player;
        _usernameInputField.text = _player.Username;
    }

    public void OnApplyButtonPressed()
    {
        if (_usernameInputField.text.Length < 3 || _usernameInputField.text.Length > 16)
        {
            Debug.LogError("Username should be 3 to 16 characters long!");
            return;
        }

        _player.Username = _usernameInputField.text;
        SaveSystem.Save();

        ExitGUI();
    }
}

