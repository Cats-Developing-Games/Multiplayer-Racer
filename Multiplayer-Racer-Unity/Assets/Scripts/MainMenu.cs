using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Linq;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private TMP_InputField clientIPInputField;
    [SerializeField] private Button clientBtn;

    private string publicIPAddress = null;

    void Awake()
    {
        StartCoroutine(SetUpPublicIPv4());

        hostBtn.onClick.AddListener(() =>
        {
            if (publicIPAddress == null) {
                Debug.Log("The public IP Address has not been loaded yet");
                return;
            }

            var localIP = GetLocalIPv4();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = publicIPAddress;
            transport.ConnectionData.ServerListenAddress = localIP;

            NetworkManager.Singleton.StartHost();
            Debug.Log("Hosting Server on: " + transport.ConnectionData.ServerEndPoint);
            NetworkManager.Singleton.SceneManager.LoadScene("Level 1", UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = clientIPInputField.text;
            NetworkManager.Singleton.StartClient();
        });
    }

    private string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    private class IPRequest{ public string ip; }

    private IEnumerator SetUpPublicIPv4()
    {
        // Make request to endpoint that returns JSON of { ip: [YOUR IP] }
        var request = new UnityWebRequest("https://api.ipify.org?format=json", "GET");
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        // Unpack JSON
        var requestedIP = JsonUtility.FromJson<IPRequest>(request.downloadHandler.text);

        // Assigns public IP
        publicIPAddress = requestedIP.ip;
    }
}
