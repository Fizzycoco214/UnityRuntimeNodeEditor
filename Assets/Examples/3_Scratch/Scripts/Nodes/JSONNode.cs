using System;
using RuntimeNodeEditor;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class JSONNode : ExpandableNode
{
    public TextAsset jsonFile;

    [Serializable]
    public class InputSocket
    {
        public string label;
        public string dataType;
    }

    [Serializable]
    public class OutputSocket
    {
        public string label;
        public string dataType;
        public string defaultValue;
    }

    [Serializable]
    public class DetailData
    {
        public string label;
        public string dataType;
        public string defaultValue;
    }

    [Serializable]
    public class SocketData
    {
        public InputSocket[] inputSockets;
        public OutputSocket[] outputSockets;
        public DetailData[] details;
    }

    SocketData data;

    public override void Setup()
    {
        SetHeader("JSON");

        ParseJSON(jsonFile.text);

    }

    void ParseJSON(string file)
    {
        data = JsonUtility.FromJson<SocketData>(file);

        foreach (InputSocket socket in data.inputSockets)
        {
            AddInputSocket(socket.label, socket.dataType);
        }

        foreach (OutputSocket socket in data.outputSockets)
        {
            AddOutputSocket(socket.label, socket.dataType, socket.defaultValue);
        }

        foreach (DetailData detail in data.details)
        {
            detailData.Add(detail.label, detail.defaultValue);
        }
    }

    //Child JSON objects can implement logic for their node with this
    public virtual void ProcessData() { }

    public override void OnSerialize(Serializer serializer)
    {
        foreach (string key in detailData.Keys)
        {
            serializer.Add(key, detailData[key].ToString());
        }
    }

    public override void OnDeserialize(Serializer serializer)
    {
        foreach (DetailData detail in data.details)
        {
            string obj = serializer.Get(detail.label);
            detailData[detail.label] = ParseString(obj, detail.dataType);
        }
    }

    public override void ShowDetails(DetailsPanel panel)
    {
        foreach (DetailData detail in data.details)
        {
            ShowDetail(detail.label, detail.dataType, detail.defaultValue);
        }
    }
}
