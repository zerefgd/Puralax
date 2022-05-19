using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    [SerializeField]
    private int _currentLevel;

    private GameObject activeImage, lockedImage;
    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        activeImage = transform.GetChild(1).gameObject;
        lockedImage = transform.GetChild(0).gameObject;
        TMP_Text countText = GetComponentInChildren<TMP_Text>();
        countText.text = _currentLevel.ToString();
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        button.onClick.AddListener(UpdateLevel);
    }

    private void OnEnable()
    {
        int currentStage = PlayerPrefs.GetInt(Constants.DATA.CURRENT_STAGE);
        string currentStageName = Constants.DATA.CURRENT_STAGE + "_" + currentStage.ToString();
        string currentButtonLevelName = currentStageName + "_" + _currentLevel.ToString();
        int levelActive = PlayerPrefs.HasKey(currentButtonLevelName) ? PlayerPrefs.GetInt(currentButtonLevelName) : 0;
        if(_currentLevel <= 5 && levelActive == 0)
        {
            levelActive = 1;
            PlayerPrefs.SetInt(currentButtonLevelName, levelActive);
        }

        lockedImage.SetActive(levelActive == 0);
        activeImage.SetActive(levelActive == 1);
        buttonImage.color = MainMenuManager.instance._colors[currentStage - 1];
    }

    private void UpdateLevel()
    {
        if (lockedImage.activeInHierarchy) return;
        PlayerPrefs.SetInt(Constants.DATA.CURRENT_LEVEL, _currentLevel);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
