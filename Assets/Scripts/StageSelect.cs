using UnityEngine;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    [SerializeField]
    private int _buttonStage;

    private Button button;
    private GameObject activeImage;
    private Image stageImage;
    private TMPro.TMP_Text countText;

    private void Awake()
    {
        button = GetComponent<Button>();
        activeImage = transform.GetChild(0).gameObject;
        stageImage = activeImage.GetComponent<Image>();
        countText = GetComponentInChildren<TMPro.TMP_Text>();
    }

    private void Start()
    {
        button.onClick.AddListener(UpdateStage);
    }

    private void OnEnable()
    {
        countText.text = _buttonStage.ToString();

        stageImage.color = MainMenuManager.instance._colors[_buttonStage - 1];
        string currentStageName = Constants.DATA.CURRENT_STAGE + "_" + _buttonStage.ToString();
        int stageActive = PlayerPrefs.HasKey(currentStageName) ? PlayerPrefs.GetInt(currentStageName) : 0;
        if(_buttonStage == 1)
        {
            stageActive = 1;
            PlayerPrefs.SetInt(currentStageName, stageActive);
        }
        activeImage.SetActive(stageActive == 1);

    }

    private void UpdateStage()
    {
        if (!activeImage.activeInHierarchy) return;
        PlayerPrefs.SetInt(Constants.DATA.CURRENT_STAGE, _buttonStage);
        MainMenuManager.instance.ClickedStage();
    }
}
