using System;
using RuntimeNodeEditor;
using UnityEngine;
using UnityEngine.UI;

public class BooleanNode : Node
{
    [Inspectable("Value")]
    public Toggle valueField;
    public SocketOutput outputSocket;

    public override void Setup()
    {
        Register(outputSocket, typeof(bool));
        SetHeader("Bool");

        valueField.onValueChanged.AddListener(HandleValueField);
        valueField.isOn = false;
    }

    public void HandleValueField(bool value)
    {
        outputSocket.SetValue(value);
    }

    public override void OnSerialize(Serializer serializer)
    {
        serializer.Add("boolValue", valueField.isOn.ToString());
    }

    public override void OnDeserialize(Serializer serializer)
    {
        string value = serializer.Get("boolValue");
        valueField.SetIsOnWithoutNotify(bool.Parse(value));

        HandleValueField(valueField.isOn);
    }
}
