using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;
using System.Linq;

public class VehicleStatsViewer : MonoBehaviour
{
    [SerializeField] private TMP_Text vehicleNameLabel;
    [SerializeField] private RectTransform vehicleStatsBody;
    [SerializeField] private GameObject statsRowPrefab;

    private List<(MemberInfo info, VehicleStatRow statsRow)> statsRows = new List<(MemberInfo info, VehicleStatRow statsRow)>();

    private VehicleSO currentVehicle = null;

    // Start is called before the first frame update
    void Start()
    {
        InitializeStatRows();
        // Disable it on start
        ToggleVehicleStatsBody();

        if (currentVehicle != null) DisplayStats(currentVehicle);
    }

    public void DisplayStats(VehicleSO vehicle)
    {
        currentVehicle = vehicle;
        vehicleNameLabel.SetText(vehicle.VehicleName);

        //Debug.Log("Display Stats for: " + vehicle.name);

        foreach (var (memberInfo, statsRow) in statsRows)
        {
            var rating = GetStatValue(vehicle, memberInfo);

            statsRow.SetRating(rating);
        }
    }

    private float GetStatValue(VehicleSO vehicle, MemberInfo info)
    {
        var attributeData = info.GetCustomAttribute<DisplayStatAttribute>();
        var fieldValue = (float)GetUnderlyingValue(info, vehicle);
        return CalculateRating(fieldValue, attributeData.MinValue, attributeData.MaxValue);
    }

    private float CalculateRating(float value, float min, float max)
    {

        return Mathf.Lerp(0, VehicleStatRow.MaxRating, Mathf.InverseLerp(min, max, value));
    }

    private object GetUnderlyingValue(MemberInfo member, VehicleSO vehicle)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)member).GetValue(vehicle);
            case MemberTypes.Method:
                return ((MethodInfo)member).Invoke(vehicle, null);
            case MemberTypes.Property:
                return ((PropertyInfo)member).GetValue(vehicle);
            default:
                throw new ArgumentException
                (
                 "Unsupported Case"
                );
        }
    }

    private void InitializeStatRows()
    {
        Type vehicleType = typeof(VehicleSO);

        List<(int orderNumber, string name, MemberInfo info)> statRatings = new List<(int orderNumber, string name, MemberInfo info)>();

        foreach (var memberInfo in vehicleType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(member => member.GetCustomAttribute<DisplayStatAttribute>() != null))
        {
            statRatings.Add(ExtractStatMetadata(memberInfo));
        }

        //Debug.Log($"Found {statRatings.Count} stats");

        foreach (var (_, name, info) in statRatings.OrderBy(sr => sr.orderNumber))
        {
            var statsRowGO = Instantiate(statsRowPrefab, vehicleStatsBody.position, vehicleStatsBody.rotation, vehicleStatsBody.transform);

            //Debug.Log($"Creating {name} stat");

            var statsRow = statsRowGO.GetComponent<VehicleStatRow>();
            statsRow.SetStatData(name, 0);

            statsRows.Add((info, statsRow));
        }
    }

    private (int orderNumber, string name, MemberInfo info) ExtractStatMetadata(MemberInfo info)
    {
        var attributeData = info.GetCustomAttribute<DisplayStatAttribute>();

        var name = attributeData.StatName ?? info.Name;
        var orderId = attributeData.Order;

        return (orderId, name, info);
    }


    public void ToggleVehicleStatsBody() => vehicleStatsBody.gameObject.SetActive(!vehicleStatsBody.gameObject.activeInHierarchy);
}
