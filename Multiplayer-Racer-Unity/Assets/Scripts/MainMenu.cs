using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private TMP_InputField clientIPInputField;
    [SerializeField] private Button clientBtn;
    
    void Awake()
    {
        hostBtn.onClick.AddListener(() => {
            
            var localIP = GetLocalIPv4();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.ServerListenAddress = localIP;

            NetworkManager.Singleton.StartHost();
            Debug.Log("Hosting Server on: " + transport.ConnectionData.ServerEndPoint);
            NetworkManager.Singleton.SceneManager.LoadScene("Level 1", UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        clientBtn.onClick.AddListener(() => {
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
}
