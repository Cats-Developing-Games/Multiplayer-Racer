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

    private Rect? initializerRect = null;

    public void SetInitializerPosition(Rect sourceRect)
    {
        initializerRect = sourceRect;
    }

    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (isInitialPosition && time != 0)
        {
            if (initializerRect != null)
            {
                controlledCamera.rect = initializerRect.Value;
            }
            else
            {
                controlledCamera.rect = DetermineInitialRectPosition(cameraRect);
            }
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
        int xScalar = 0;
        if (rect.x == 0) xScalar = -1;
        else if (rect.x + rect.width == 1) xScalar = 1;

        int yScalar = 0;
        if (rect.y == 0) yScalar = -1;
        else if (rect.y + rect.height == 1) yScalar = 1;


        Vector2 horizontalOffsetRect = new Vector2(rect.x + xScalar * rect.width, rect.y);
        Vector2 verticalOffsetRect = new Vector2(rect.x, rect.y + yScalar * rect.height);
        Vector2 offset;

        if (yScalar == -1)
        {
            // Prefer to slide in from bottom
            offset = verticalOffsetRect;
        }
        else if (xScalar == 1)
        {
            // Second preference is to slide in from right side
            offset = horizontalOffsetRect;
        }
        else if (xScalar != 0 && yScalar != 0)
        {
            // Need to determine whichever distance would be shorted and slid in based on that direction
            var actualWidth = Screen.width * rect.width;
            var actualHeight = Screen.height * rect.height;

            if (actualWidth < actualHeight)
            {
                // Scroll in horizontally
                offset = horizontalOffsetRect;
            }
            else
            {
                // Scroll in vertically
                offset = verticalOffsetRect;
            }
        }
        else if (xScalar != 0) offset = horizontalOffsetRect;
        else if (yScalar != 0) offset = verticalOffsetRect;
        else
        {
            // The position of the cell is in the center of the grid. No good choice, so slide in from wherever
            offset = new Vector2(rect.x < .5f ? -rect.width : 1f, rect.y);
        }

        return new Rect(offset, new Vector2(rect.width, rect.height));
    }

    public override void OnDestroy()
    {
        OnCameraDestroy?.Invoke(this);
    }
}
