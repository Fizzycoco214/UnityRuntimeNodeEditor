using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RuntimeNodeEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class LogicNodeEditor : NodeEditor
{
    private string savePath;

    public static LogicNodeEditor Instance { private set; get; }

    public override void StartEditor(NodeGraph graph)
    {
        Instance = this;

        base.StartEditor(graph);

        savePath = Application.dataPath + "/Examples/3_Scratch/Resources/graph.json";
    }

    private Action CreateNode(string prefabName)
    {
        return () =>
        {
            CreateNodeFromPrefab("Nodes/" + prefabName);
        };
    }

    protected override ContextItemData GetGraphContextMenu()
    {
        return new ContextMenuBuilder()
                .Add("Create Node/Boolean", CreateNode("Boolean Node"))
                .Add("Create Node/Logic", CreateNode("Logic Operator Node"))
                .Add("Create Node/JSON", CreateNode("JSON Node"))
                .Add("Create Node/Display", CreateNode("Display Node"))
                .Add("Create Node/Connector", CreateNode("Connection"))
                .Add("Create Group", CreateNode("Collection"))
                .Add("Load", () => LoadGraph(savePath))
                .Add("Save", () => SaveGraph(savePath))
                .Build();
    }

    protected override ContextItemData GetNodeContextMenu(Node node)
    {
        return new ContextMenuBuilder()
                    .Add("Duplicate", () => DuplicateNode(node))
                    .Add("Clear Connections", () => ClearNodeConnections(node))
                    .Add("Delete", () => DeleteNode(node))
                    .Build();
    }
}
