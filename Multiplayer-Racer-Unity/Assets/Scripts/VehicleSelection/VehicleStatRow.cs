using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VehicleStatRow : MonoBehaviour
{
    public static readonly int MaxRating = 6;
    private const float STAR_WIDTH = 54f;
    private static readonly float MAX_STAR_WIDTH = MaxRating * STAR_WIDTH;

    [SerializeField] private TMP_Text Label;
    [SerializeField] private RectTransform StarTransform;

    public void SetStatData(string statName, float starRating)
    {
        Label.SetText(statName + ":");
        SetRating(starRating);
    }

    public void SetRating(float starRating)
    {
        //Debug.Log("Set rating: " + starRating);

        var clampedRating = Mathf.Clamp(starRating, 0, MaxRating);
        var steppedRating = Mathf.Round(clampedRating * 2) / 2;

        var tStep = steppedRating / MaxRating;

        float width = Mathf.Lerp(0, MAX_STAR_WIDTH, tStep);
        StarTransform.sizeDelta = MakeSizeDelta(width);

        //Debug.Log("Set width to: " + width);
    }

    private Vector2 MakeSizeDelta(float width) => new Vector2(width, StarTransform.sizeDelta.y);
}
