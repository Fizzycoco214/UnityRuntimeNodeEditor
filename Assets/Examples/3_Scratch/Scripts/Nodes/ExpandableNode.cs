using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net.Sockets;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExpandableNode : Node
{
    public RectTransform nodeBody;
    public VerticalLayoutGroup inputSocketGroup;
    public VerticalLayoutGroup outputSocketGroup;

    public GameObject labledInputSocket;
    public GameObject labledOutputSocket;

    protected Dictionary<string, object> inputData = new();
    protected Dictionary<string, object> outputData = new();

    protected Dictionary<string, object> detailData = new();

    protected void AddInputSocket(string title, string dataType)
    {
        SocketInput socket = Instantiate(labledInputSocket, inputSocketGroup.transform).GetComponent<SocketInput>();
        Register(socket, ParseStringType(dataType));
        socket.GetComponentInChildren<TMP_Text>().text = title;

        inputData.Add(title, dataType);

        if (inputData.Count > outputData.Count)
        {
            nodeBody.sizeDelta += Vector2.up * 30;
        }
    }

    protected void AddOutputSocket(string title, string dataType, string defaultValue)
    {
        SocketOutput socket = Instantiate(labledOutputSocket, outputSocketGroup.transform).GetComponent<SocketOutput>();
        Register(socket, ParseStringType(dataType));
        socket.GetComponentInChildren<TMP_Text>().text = title;
        socket.SetValue(ParseString(defaultValue, dataType));

        outputData.Add(title, ParseString(defaultValue, dataType));
        


        if (inputData.Count < outputData.Count)
        {
            nodeBody.sizeDelta += Vector2.up * 30;
        }
    }

    public override void OnSerialize(Serializer serializer)
    {
        foreach (string key in outputData.Keys)
        {
            outputData.TryGetValue(key, out object value);
            serializer.Add(key, value.ToString());
            serializer.Add(key + " Data Type", value.GetType().Name);
        }
    }

    public override void OnDeserialize(Serializer serializer)
    {
        foreach (string key in outputData.Keys)
        {
            string value = serializer.Get(key);
            string dataType = serializer.Get(key + " Data Type");
            AddOutputSocket(key, dataType, value);
        }
    }

    protected Type ParseStringType(string dataType)
    {
        switch (dataType)
        {
            case "bool":
                return typeof(bool);
            case "Single":
            case "float":
                return typeof(float);
            case "string":
                return typeof(string);
            default:
                return default;
        }
    }

    protected object ParseString(string value, Type dataType) => ParseString(value, dataType.Name);

    protected object ParseString(string value = "", string dataType = "")
    {
        switch (dataType)
        {
            case "bool":
                return bool.Parse(value); 
            case "Single":
            case "float":
                return float.Parse(value);
            case "string":
                return value;
            default:
                return default;
        }
    }

    public override void ShowDetails(DetailsPanel panel)
    {
        SocketOutput[] sockets = GetComponentsInChildren<SocketOutput>();

        foreach (SocketOutput socket in sockets)
        {
            switch (socket.socketType.Name)
            {
                case "Boolean":
                    BooleanDetail booleanDetail = panel.LoadDetailPrefab<BooleanDetail>("Boolean Detail");
                    string socketTitle = socket.GetComponentInChildren<TMP_Text>().text;

                    booleanDetail.name = socketTitle;
                    booleanDetail.title.text = socketTitle;
                    booleanDetail.toggle.SetIsOnWithoutNotify(socket.GetValue<bool>());

                    booleanDetail.toggle.onValueChanged.AddListener(
                        (bool value) => socket.SetValue(value)
                    );
                    break;
                case "Single":
                    TextDetail floatDetail = panel.LoadDetailPrefab<TextDetail>("Text Detail");
                    socketTitle = socket.GetComponentInChildren<TMP_Text>().text;

                    floatDetail.name = socketTitle;
                    floatDetail.title.text = socketTitle;
                    floatDetail.inputField.SetTextWithoutNotify(socket.GetValue<float>().ToString());

                    floatDetail.inputField.onEndEdit.AddListener(
                        (string value) => socket.SetValue(float.Parse(value))
                    );
                    break;
                case "String":
                    TextDetail textDetail = panel.LoadDetailPrefab<TextDetail>("Text Detail");
                    socketTitle = socket.GetComponentInChildren<TMP_Text>().text;

                    textDetail.name = socketTitle;
                    textDetail.title.text = socketTitle;
                    textDetail.inputField.SetTextWithoutNotify(socket.GetValue<string>());

                    textDetail.inputField.onEndEdit.AddListener(
                        (string value) => socket.SetValue(value)
                    );
                    break;

            }
        }
    }

    public void ShowDetail(string title, string dataType, string value)
    {
        if (!detailData.ContainsKey(title))
        {
            detailData.Add(title, ParseString(value, dataType));
        }
        else
        {
            value = detailData[title].ToString();
        }

        switch (dataType)
        {
            case "bool":
                BooleanDetail booleanDetail = DetailsPanel.Instance.LoadDetailPrefab<BooleanDetail>("Boolean Detail");

                booleanDetail.name = title;
                booleanDetail.title.text = title;
                booleanDetail.toggle.SetIsOnWithoutNotify((bool)ParseString(value, dataType));

                booleanDetail.toggle.onValueChanged.AddListener(
                    (bool data) =>
                    {
                        detailData[title] = data;
                    }
                );
                break;
            case "float":
                TextDetail floatDetail = DetailsPanel.Instance.LoadDetailPrefab<TextDetail>("Text Detail");

                floatDetail.name = title;
                floatDetail.title.text = title;
                floatDetail.inputField.SetTextWithoutNotify(value);

                floatDetail.inputField.onEndEdit.AddListener(
                    (string data) =>
                    {
                        detailData[title] = (float)ParseString(data, dataType);
                    }
                );
                break;
            case "string":
                TextDetail textDetail = DetailsPanel.Instance.LoadDetailPrefab<TextDetail>("Text Detail");

                textDetail.name = title;
                textDetail.title.text = title;
                textDetail.inputField.SetTextWithoutNotify(value);

                textDetail.inputField.onEndEdit.AddListener(
                    (string data) =>
                    {
                        detailData[title] = data;
                    }
                );
                break;

        }
    }
}
