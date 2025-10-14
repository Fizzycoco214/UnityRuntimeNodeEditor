using RuntimeNodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayNode : Node
{
    public SocketInput inputSocket;
    public IOutput incomingData;

    public TMP_Text displayText;
    public override void Setup()
    {
        Register(inputSocket, typeof(object));

        SetHeader("Display");

        OnConnectionEvent += OnConnection;
        OnDisconnectEvent += OnDisconnect;
    }

    public void Display()
    {
        displayText.text = incomingData?.GetValue<object>()?.ToString();
    }

    public void OnConnection(SocketInput input, IOutput output)
    {
        output.ValueUpdated += Display;
        incomingData = output;
        Display();
    }

    public void OnDisconnect(SocketInput input, IOutput output)
    {
        output.ValueUpdated -= Display;
        incomingData = null;

        displayText.text = "";
    }
}
