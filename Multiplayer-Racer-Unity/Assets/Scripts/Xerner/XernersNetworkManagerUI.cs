using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class XernersNetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button hostBtn;
    [SerializeField] Button serverBtn;
    [SerializeField] Button clientBtn;
    [SerializeField] TMPro.TMP_InputField inputField;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            var localIP = GetLocalIPv4();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.ServerListenAddress = localIP;
            transport.ConnectionData.Port = 7777;
            NetworkManager.Singleton.StartHost();
        });
        serverBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); });
        clientBtn.onClick.AddListener(() => {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = inputField.text;
            NetworkManager.Singleton.StartClient();
        });
    }

    string GetLocalIPv4() {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}
