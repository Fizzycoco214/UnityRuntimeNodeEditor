using System;
using System.Collections.Generic;
using RuntimeNodeEditor;
using UnityEngine;

public class Connection : Node
{
    public SocketOutput outputSocket;
    public SocketInput inputSocket;
    public IOutput incomingData;

    public Type dataType;

    public override void Setup()
    {
        Register(inputSocket, typeof(object));
        Register(outputSocket, typeof(object));

        OnConnectionEvent += OnConnection;
        OnDisconnectEvent += OnDisconnect;
    }

    public void OnConnection(SocketInput input, IOutput output)
    {
        output.ValueUpdated += OnConnectedValueUpdated;
        incomingData = output;
        dataType = inputSocket.Connections[0].output.socketType;

        OnConnectedValueUpdated();
    }

    public void OnDisconnect(SocketInput input, IOutput output)
    {
        output.ValueUpdated -= OnConnectedValueUpdated;
        incomingData = null;
        dataType = typeof(object);
        OnConnectedValueUpdated();
    }

    private void OnConnectedValueUpdated()
    {
        inputSocket.SetColor(dataType);
        outputSocket.SetType(dataType);
        if (incomingData != null)
        {
            outputSocket.SetValue(incomingData.GetValue<object>());
        }
        else
        {
            print("nulled");
            outputSocket.SetValue(null);
        }
    }
}
