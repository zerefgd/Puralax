using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public CellData _cellData;
    public Vector2Int CurrentPos => new((int)transform.position.x, (int)transform.position.y);

    [SerializeField]
    private GameObject _first, _second;

    [SerializeField]
    private List<GameObject> _cirlces;

    private Square frontSquare, backSquare;

    private void Awake()
    {
        _cellData = new();
        _cellData.color = -1;
        _cellData.moves = 0;
        _cellData.gridPos = CurrentPos;

        _first.SetActive(false);
        _second.SetActive(false);
        frontSquare = _first.GetComponent<Square>();
        backSquare = _second.GetComponent<Square>();

        for (int i = 0; i < _cirlces.Count; i++)
        {
            _cirlces[i].SetActive(false);
        }
    }

    public void InitializeCell(CellData data)
    {
        _cellData = data;
        frontSquare.gameObject.SetActive(true);
        frontSquare.InitializeSquare(_cellData.color);
        StartCoroutine(UpdateMoves());
    }

    public IEnumerator UpdateMoves()
    {
        yield return new WaitForSeconds(Constants.Values.ANIMATION_TIME);

        for (int i = 0; i < _cirlces.Count; i++)
        {
            _cirlces[i].SetActive(i < _cellData.moves);
        }
    }

    public IEnumerator MoveToPos()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new(_cellData.gridPos.x + 0.5f, _cellData.gridPos.y + 0.5f);
        float animationTime = Constants.Values.ANIMATION_TIME;
        Vector3 speed = (endPos - startPos) / animationTime;
        float endTime = 0f;
        while(endTime < animationTime)
        {
            transform.position += speed * Time.deltaTime;
            endTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    public IEnumerator ChangeColor(float delay)
    {
        yield return new WaitForSeconds(delay);

        backSquare.gameObject.SetActive(true);
        backSquare.StartColorSettings(_cellData.color, true);
        frontSquare.StartColorSettings(_cellData.color, false);

        var temp = frontSquare;
        frontSquare = backSquare;
        backSquare = temp;

        yield return frontSquare.StartAnimation();

        backSquare.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct CellData
{
    public int moves;
    public int color;
    public Vector2Int gridPos;
}