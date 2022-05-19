using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    private SpriteRenderer sr;

    private static Vector3 startScale = Vector3.one * 0.7f;
    private static Vector3 endScale = Vector3.one;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitializeSquare(int colorValue)
    {
        Color color = GameManager.instance.colors[colorValue];
        sr.color = color;
        sr.sortingOrder = Constants.Values.FRONT;
        StartCoroutine(StartAnimation());
    }

    public void StartColorSettings(int colorValue,bool front)
    {
        Color color = front ? GameManager.instance.colors[colorValue] : sr.color;
        sr.color = color;
        sr.sortingOrder = front ? Constants.Values.FRONT : Constants.Values.BACK;
    }

    public IEnumerator StartAnimation()
    {
        transform.localScale = startScale;

        float animationTime = Constants.Values.ANIMATION_TIME;
        Vector3 speed = (endScale - startScale) / animationTime;
        float endTime = 0f;
        while(endTime < animationTime)
        {
            transform.localScale += speed * Time.deltaTime;
            endTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
    }
}
