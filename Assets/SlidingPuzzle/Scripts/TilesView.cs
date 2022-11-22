using System;
using System.Collections;
using UnityEngine;

public class TilesView : MonoBehaviour
{
    public event Action<TilesView> OnBlockPressed;
    public event Action OnFinishedMoving;

    public Vector2Int coord;
    Vector2Int startingCoord;

    public void Init(Vector2Int startingCoord, Texture2D _texture2D, Material material)
    {
        this.startingCoord = startingCoord;
        coord = startingCoord;

        GetComponent<MeshRenderer>().material = new Material(material);
        GetComponent<MeshRenderer>().material.mainTexture = _texture2D;
    }

    public void MoveToPosition(Vector2 target, float duration)
    {
        StartCoroutine(AnimateMove(target, duration));
    }

    void OnMouseDown()
    {
        if (OnBlockPressed != null)
        {
            OnBlockPressed(this);
        }
    }

    IEnumerator AnimateMove(Vector2 target, float duration)
    {
        Vector2 initialPos = transform.position;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime / duration;
            transform.position = Vector2.Lerp(initialPos, target, percent);
            yield return null;
        }

        if (OnFinishedMoving != null)
        {
            OnFinishedMoving();
        }
    }

    public bool IsAtStartingCoord()
    {
        return coord == startingCoord;
    }
}