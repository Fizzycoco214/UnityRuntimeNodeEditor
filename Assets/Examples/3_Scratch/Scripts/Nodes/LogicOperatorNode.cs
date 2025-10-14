using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogicOperatorNode : Node
{
    public enum Operation
    {
        AND,
        OR
    }

    [Inspectable("Operation", typeof(Operation))]
    public TMP_Dropdown dropdown;
    public TMP_Text displayText;

    [Inspectable("Require input from all sockets")]
    public bool requireAllInputs = false;

    public SocketOutput outputSocket;
    public SocketInput inputSocket1;
    public SocketInput inputSocket2;

    List<IOutput> incomingOutputs;

    public override void Setup()
    {
        incomingOutputs = new List<IOutput>();

        Register(inputSocket1, typeof(bool));
        Register(inputSocket2, typeof(bool));
        Register(outputSocket, typeof(bool));

        SetHeader("Logic");
        outputSocket.SetValue(false);

        dropdown.options.Clear();
        foreach (Operation operation in Enum.GetValues(typeof(Operation)))
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(operation.ToString()));
        }

        dropdown.onValueChanged.AddListener(value => {
            OnConnectedValueUpdated();
        });

        OnConnectionEvent += OnConnection;
        OnDisconnectEvent += OnDisconnect;
    }

    public void OnConnection(SocketInput input, IOutput output)
    {
        output.ValueUpdated += OnConnectedValueUpdated;
        incomingOutputs.Add(output);

        OnConnectedValueUpdated();
    }

    public void OnDisconnect(SocketInput input, IOutput output)
    {
        output.ValueUpdated -= OnConnectedValueUpdated;
        incomingOutputs.Remove(output);

        OnConnectedValueUpdated();
    }

    public override void OnSerialize(Serializer serializer)
    {
        serializer.Add("operation", dropdown.value.ToString());
    }

    public override void OnDeserialize(Serializer serializer)
    {
        int opType = int.Parse(serializer.Get("operation"));
        dropdown.SetValueWithoutNotify(opType);

        OnConnectedValueUpdated();
    }

    private void OnConnectedValueUpdated()
    {
        if (requireAllInputs && incomingOutputs.Count != 2)
        {
            outputSocket.SetValue(false);
            displayText.text = false.ToString();
        }

        List<bool> incomingValues = new List<bool>();
        foreach (var c in incomingOutputs)
        {
            incomingValues.Add(c.GetValue<bool>());
        }

        bool result = Calculate(incomingValues);
        outputSocket.SetValue(result);
        displayText.text = result.ToString();
    }

    private bool Calculate(List<bool> values)
    {
        if (values.Count == 0)
        {
            return false;
        }

        switch ((Operation)dropdown.value)
        {
            case Operation.AND:
                return values.Aggregate((x, y) => x && y);
            case Operation.OR:
                return values.Aggregate((x, y) => x || y);
            default:
                return false;
        }
    }
}
