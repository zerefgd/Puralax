using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{

    #region VARIABLES
    public static GameManager instance;

    [SerializeField]
    TMP_Text _stageText, _levelText;

    [SerializeField]
    private Image _titleImage, _winImage;

    [SerializeField]
    private AudioClip _moveClip, _updateClip, _winClip, _loseClip;

    public List<Color> colors;

    [SerializeField]
    private LevelDictionary _levelDictionary;

    [SerializeField]
    private Cell _cellPrefab;

    private Dictionary<Vector2Int, Cell> cellsDictionary = new();

    private Level currentLevelObject;
    private string currentLevelName;
    private int winColor;

    private GameState currentGameState;
    private Vector2Int startClickGrid, endClickGrid;
    private float stateDelay;

    private List<Cell> neighbours = new();
    private List<Cell> newNeighbours = new();
    private Dictionary<Vector2Int, bool> visited = new();

    private readonly List<Vector2Int> directions = new()
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0,- 1)
    };

    #endregion

    #region GAMEOBJECT_METHODS
    private void Awake()
    {
        instance = this;

        _stageText.text = "STAGE " + PlayerPrefs.GetInt(Constants.DATA.CURRENT_STAGE).ToString();
        _levelText.text = "level " + PlayerPrefs.GetInt(Constants.DATA.CURRENT_LEVEL).ToString();

        _winImage.gameObject.SetActive(false);
        currentGameState = GameState.INPUT;
        SpawnLevel();

        AudioManager.instance.AddButtonSound();
    }

    private void Update()
    {
        if (currentGameState != GameState.INPUT) return;

        Vector3 inputPos;
        Vector2Int currentClickedPos;

        if(Input.GetMouseButtonDown(0))
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentClickedPos = new((int)inputPos.x, (int)inputPos.y);
            startClickGrid = currentClickedPos;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentClickedPos = new((int)inputPos.x, (int)inputPos.y);
            endClickGrid  = currentClickedPos;
            endClickGrid = GetDirection(endClickGrid - startClickGrid);

            currentGameState = GameState.ANIMATION;
            CalculateMoves();
        }
    }

    #endregion

    #region MOVES
    private void CalculateMoves()
    {
        AudioManager.instance.PlaySound(_moveClip);

        //VALID STARTPOS
        if(!IsValidPos(startClickGrid))
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }

        Cell currentClickedCell = cellsDictionary[startClickGrid];


        //VALID ENDPOS AND HASMOVES
        if(!IsValidPos(startClickGrid + endClickGrid) 
            || !(currentClickedCell._cellData.moves > 0))
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }

        //INVALID SAME COLOR
        Cell endClickedCell = cellsDictionary[startClickGrid + endClickGrid];
        if(currentClickedCell._cellData.color == endClickedCell._cellData.color)
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }

        //MOVE FOR EMPTY CELL
        if(endClickedCell._cellData.color == -1)
        {
            currentClickedCell._cellData.moves -= 1;
            StartCoroutine(currentClickedCell.UpdateMoves());

            var temp = endClickedCell._cellData.gridPos;
            endClickedCell._cellData.gridPos = currentClickedCell._cellData.gridPos;
            currentClickedCell._cellData.gridPos = temp;

            StartCoroutine(currentClickedCell.MoveToPos());
            StartCoroutine(endClickedCell.MoveToPos());

            cellsDictionary[startClickGrid] = endClickedCell;
            cellsDictionary[startClickGrid + endClickGrid] = currentClickedCell;

            stateDelay = Constants.Values.ANIMATION_TIME;
            StartCoroutine(SwitchStateAfterDelay());

            CheckResult();

            return;
        }

        //UPDATE THE FIRST COLLIDED CELL
        int updateColor = endClickedCell._cellData.color;
        endClickedCell._cellData.color = currentClickedCell._cellData.color;

        StartCoroutine(endClickedCell.ChangeColor(0f));
        currentClickedCell._cellData.moves--;

        stateDelay = Constants.Values.ANIMATION_TIME;
        StartCoroutine(currentClickedCell.UpdateMoves());
        StartCoroutine(SwitchStateAfterDelay());

        //CHECK FOR NEIGHBOURING CELLS
        newNeighbours.Clear();
        neighbours.Clear();
        visited.Clear();
        neighbours.Add(endClickedCell);

        while(neighbours.Count > 0)
        {
            newNeighbours.Clear();
            for (int i = 0; i < neighbours.Count; i++)
            {
                for (int j = 0; j < directions.Count; j++)
                {
                    if(IsValidPos(neighbours[i].CurrentPos + directions[j]))
                    {
                        endClickedCell = cellsDictionary[neighbours[i].CurrentPos + directions[j]];
                        if(!visited.ContainsKey(endClickedCell.CurrentPos))
                        {
                            if(endClickedCell._cellData.color == updateColor)
                            {
                                endClickedCell._cellData.color = currentClickedCell._cellData.color;
                                StartCoroutine(endClickedCell.ChangeColor(stateDelay));
                                newNeighbours.Add(endClickedCell);
                                visited[endClickedCell.CurrentPos] = true;
                            }
                        }
                    }
                }
            }

            Invoke("PlayUpdateSound", stateDelay);
            stateDelay += (newNeighbours.Count > 0 ? Constants.Values.ANIMATION_TIME : 0);
            neighbours.Clear();
            foreach (var item in newNeighbours)
            {
                neighbours.Add(item);
            }

        }

        CheckResult();
    }

    private void CheckResult()
    {
        int lose = 0;
        bool win = true;

        foreach (var item in cellsDictionary)
        {
            lose += item.Value._cellData.moves;
            win = win && (item.Value._cellData.color == -1 || item.Value._cellData.color == winColor);
        }

        if(win)
        {
            Invoke("ShowWin", stateDelay + 0.5f);
            Invoke("GameWin", stateDelay + 1.5f);
            return; 
        }
        else if(lose == 0)
        {
            AudioManager.instance.PlaySound(_loseClip);
            Invoke("GameLose", stateDelay + 1f);
            return;
        }
    }
    #endregion

    #region SPAWNER
    private void SpawnLevel()
    {
        int currentStage = PlayerPrefs.GetInt(Constants.DATA.CURRENT_STAGE);
        string currentStageName = Constants.DATA.CURRENT_STAGE + "_" + currentStage.ToString();
        int currentLevel = PlayerPrefs.GetInt(Constants.DATA.CURRENT_LEVEL);
        currentLevelName = currentStageName + "_" + currentLevel.ToString();

        currentLevelObject = _levelDictionary.GetLevel(currentLevelName);
        winColor = currentLevelObject.winColor;

        //SPAWN ALL CELLS
        for (int i = 0; i < currentLevelObject.row; i++)
        {
            for (int j = 0; j < currentLevelObject.col; j++)
            {
                Vector3 spawnPos = new(i + 0.5f, j + 0.5f);
                Cell temp = Instantiate(_cellPrefab, spawnPos, Quaternion.identity);
                cellsDictionary[new(i, j)] = temp;
            }
        }

        //SPAWM COLORED CELLS
        foreach (var item in currentLevelObject.cellData)
        {
            cellsDictionary[item.gridPos].InitializeCell(item);
        }

        //SET UP THE CAMERA
        float size = 0f;
        if(currentLevelObject.col <= currentLevelObject.row)
        {
            size = (currentLevelObject.row / 2) + 3.5f;
        }
        else
        {
            size = (currentLevelObject.col / 2) + 4.5f;
        }
        Camera.main.orthographicSize = size;
        Camera.main.transform.Translate(currentLevelObject.row / 2f, currentLevelObject.col / 2f, 0);

        //CHANGE TITLE COLOR
        _titleImage.color = colors[winColor];
    }
    #endregion

    #region GAME_FUNCTIONS

    private void GameWin()
    {
        int currentStage = PlayerPrefs.GetInt(Constants.DATA.CURRENT_STAGE);
        string currentStageName = Constants.DATA.CURRENT_STAGE + "_" + currentStage.ToString();
        int currentLevel = PlayerPrefs.GetInt(Constants.DATA.CURRENT_LEVEL);

        //SET THE LEVEL TO WON
        PlayerPrefs.SetInt(currentLevelName, 2);

        //UNLOCK THE NEXT LEVEL
        int updateLevel = currentLevel + 5;
        if(updateLevel <= 20)
        {
            PlayerPrefs.SetInt(currentStageName + "_" + updateLevel.ToString(), 1);
        }
        else
        {
            int updateStage = currentStage + 1;
            PlayerPrefs.SetInt(Constants.DATA.CURRENT_STAGE + "_" + updateStage.ToString(), 1);
        }

        //SET THE CURRENT LEVEL
        int playLevel = currentLevel + 1;
        if(playLevel > 20)
        {
            currentStage++;
            playLevel = 1;
        }
        PlayerPrefs.SetInt(Constants.DATA.CURRENT_STAGE, currentStage);
        PlayerPrefs.SetInt(Constants.DATA.CURRENT_LEVEL, playLevel);

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);

    }

    private void GameLose()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void ShowWin()
    {
        _winImage.gameObject.SetActive(true);
        _winImage.color = colors[winColor];
        AudioManager.instance.PlaySound(_winClip);
    }

    public void GameRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    #endregion

    #region HELPER_FUNCTION
    private Vector2Int GetDirection(Vector2Int offset)
    {
        Vector2Int result;
        if(Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            result = new(offset.x > 0 ? 1 : -1, 0);
        }
        else
        {
            result = new(0,offset.y > 0 ? 1 : -1);
        }
        return result;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return !(pos.x >= currentLevelObject.row || pos.x < 0 || pos.y < 0 || pos.y >= currentLevelObject.col);
    }

    private void PlayUpdateSound()
    {
        AudioManager.instance.PlaySound(_updateClip);
    }

    private IEnumerator SwitchStateAfterDelay()
    {
        while(stateDelay > 0f)
        {
            stateDelay -= Time.deltaTime;
            yield return null;
        }

        currentGameState = GameState.INPUT;
    }


    #endregion
}

public enum GameState
{
    INPUT,ANIMATION
}
