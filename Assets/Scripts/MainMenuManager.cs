using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;

    [SerializeField]
    private GameObject _mainPanel, _stagePanel, _levelPanel, _activeSoundImage;

    [SerializeField]
    private TMP_Text _stageText;

    [SerializeField]
    private Image _stageColorInLevelPanel;

    public List<Color> _colors;

    private void Awake()
    {
        instance = this;

        _mainPanel.SetActive(true);
        _stagePanel.SetActive(false);

    }

    private void Start()
    {
        AudioManager.instance.AddButtonSound();
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void ClickedPlay()
    {
        _mainPanel.SetActive(false);
        _stagePanel.SetActive(true);
    }

    public void BackToMain()
    {
        _mainPanel.SetActive(true);
        _stagePanel.SetActive(false);
    }

    public void BackToStage()
    {
        _levelPanel.SetActive(false);
        _stagePanel.SetActive(true);
    }

    public void ClickedStage()
    {
        _stagePanel.SetActive(false);

        int currentStage = PlayerPrefs.GetInt(Constants.DATA.CURRENT_STAGE);
        _stageText.text = "STAGE " + currentStage.ToString();


        _levelPanel.SetActive(true);
        _stageColorInLevelPanel.color = _colors[currentStage - 1];
    }

    public void ToggleSound()
    {
        bool sound = PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND) == 0;
        PlayerPrefs.SetInt(Constants.DATA.SETTINGS_SOUND, sound ? 1 : 0);
        _activeSoundImage.SetActive(!sound);

        AudioManager.instance.ToggleSound();
    }
}
