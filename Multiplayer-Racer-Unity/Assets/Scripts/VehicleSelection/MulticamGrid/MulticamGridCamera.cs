using System.Xml.Linq;
using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MulticamGridCamera : NetworkBehaviour
{
    private bool isInitialPosition = true;
    public bool IsInitialPosition => isInitialPosition;

    private IEnumerator animateToPosition;

    public Action<MulticamGridCamera> OnCameraDestroy;
    [SerializeField] private Camera controlledCamera;

    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (isInitialPosition && time != 0)
        {
            controlledCamera.rect = DetermineInitialRectPosition(cameraRect);
        }
        isInitialPosition = false;

        if (animateToPosition != null) StopCoroutine(animateToPosition);
        if (time != 0)
        {
            StartCoroutine(AnimateToPosition(cameraRect, time));
        }
        else
        {
            controlledCamera.rect = cameraRect;
        }
    }

    private IEnumerator AnimateToPosition(Rect target, float time)
    {
        var initialRect = controlledCamera.rect;
        if (time <= 0)
        {
            controlledCamera.rect = target;
            yield break;
        }

        var elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            var tStep = elapsedTime / time;

            Rect newPosition = new Rect(
                Vector2.Lerp(initialRect.position, target.position, tStep),
                Vector2.Lerp(initialRect.size, target.size, tStep)
            );

            controlledCamera.rect = newPosition;
            yield return null;
        }

        controlledCamera.rect = target;
        yield break;
    }

    private Rect DetermineInitialRectPosition(Rect rect)
    {
        // Determine initial position to use
        int horizontalSlideDirection = 0;
        if (rect.x == 0) horizontalSlideDirection = -1;
        else if (rect.x + rect.width == 1) horizontalSlideDirection = 1;

        int verticalSlideDirection = 0;
        if (rect.y == 0) verticalSlideDirection = -1;
        else if (rect.y + rect.height == 1) verticalSlideDirection = 1;

        bool hasMultiRows = rect.height != 1f;
        bool hasMultiCols = rect.width != 1f;

        Vector2 horizontalOffsetRect = new Vector2(rect.x + horizontalSlideDirection * rect.width, rect.y);
        Vector2 verticalOffsetRect = new Vector2(rect.x, rect.y + verticalSlideDirection * rect.height);
        Vector2 offset;

        // Priority is multiple rows
        if (hasMultiRows)
        {
            if (verticalSlideDirection != 0)
            {
                // Prefer to slide in from top / bottom
                offset = verticalOffsetRect;
            }
            else if (horizontalSlideDirection != 0)
            {
                offset = horizontalOffsetRect;
            }
            else
            {
                // The position of the cell is in the center of the grid. No good choice, so slide in from wherever
                offset = new Vector2(rect.x < .5f ? -rect.width : 1f, rect.y);
            }
        }
        /// Priority is multiple columns
        else if (hasMultiCols)
        {
            if (horizontalSlideDirection != 0)
            {
                offset = horizontalOffsetRect;
            }
            else if (verticalSlideDirection != 0)
            {
                // Prefer to slide in from top / bottom
                offset = verticalOffsetRect;
            }
            else
            {
                // The position of the cell is in the center of the grid. No good choice, so slide in from wherever
                offset = new Vector2(rect.x < .5f ? -rect.width : 1f, rect.y);
            }
        }
        else offset = Vector2.zero;
        
        return new Rect(offset, new Vector2(rect.width, rect.height));
    }

    public override void OnDestroy()
    {
        OnCameraDestroy?.Invoke(this);
    }
}
