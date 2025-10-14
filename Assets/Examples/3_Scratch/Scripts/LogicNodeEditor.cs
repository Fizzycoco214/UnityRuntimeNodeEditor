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

    public GameObject highlight;
    private List<Coroutine> highlightCoroutines = new();

    private List<RectTransform> activeHighlights = new();
    [HideInInspector]
    public HashSet<Node> highlightedNodes = new();
    private Dictionary<Node, Vector2> nodeDragOrigins = new();

    public RectTransform dragBox;
    private Vector2 dragOrigin;
    private bool hasDraggedNodes = false;

    public DetailsPanel detailsPanel;



    public override void StartEditor(NodeGraph graph)
    {

        base.StartEditor(graph);

        savePath = Application.dataPath + "/Examples/3_Scratch/Resources/graph.json";


        Events.OnGraphPointerClickEvent += OnGraphPointerClick;
        Events.OnGraphPointerDragEvent += OnGraphPointerDrag;


        Events.OnNodePointerClickEvent += OnNodePointerClick;
        Events.OnNodePointerDragEvent += OnNodeDrag;
        Events.OnConnectionPointerClickEvent += OnNodeConnectionPointerClick;

        Events.OnNodePointerDownEvent += OnNodePointerDown;
        Events.OnNodePointerUpEvent += OnNodePointerUp;

        Events.OnGraphPointerDown += OnGraphPointerDown;
        Events.OnGraphPointerUp += OnGraphPointerUp;

        Graph.SetSize(Vector2.one * 20000);

        ClearHighlights();

        dragBox.parent = Graph.background;
        dragBox.gameObject.SetActive(false);

        detailsPanel.HidePanel();
    }

    private void OnGraphPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                CloseContextMenu();
                detailsPanel.HidePanel();
                break;
            case PointerEventData.InputButton.Right:
                var contextMenu = new ContextMenuBuilder()
                .Add("Create Node/Boolean", CreateNode("Boolean Node"))
                .Add("Create Node/Logic", CreateNode("Logic Operator Node"))
                .Add("Create Node/JSON", CreateNode("JSON Node"))
                .Add("Create Node/Display", CreateNode("Display Node"))
                .Add("Create Node/Connector", CreateNode("Connection"))
                .Add("Create Group", CreateNode("Collection"))
                .Add("Load", LoadGraph)
                .Add("Save", SaveGraph)
                .Build();

                SetContextMenu(contextMenu);
                DisplayContextMenu();
                break;
        }
    }

    private void SaveGraph()
    {
        CloseContextMenu();
        Graph.SaveFile(savePath);
    }

    private void LoadGraph()
    {
        ClearHighlights();
        CloseContextMenu();
        Graph.Clear();
        Graph.LoadFile(savePath);
    }

    private Action CreateNode(string prefabName)
    {
        return () =>
        {
            Graph.Create("Nodes/" + prefabName);
            CloseContextMenu();
        };
    }


    private void OnGraphPointerDown(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                ClearHighlights();
                dragOrigin = Utility.GetMousePosition();
                dragBox.sizeDelta = new();
                break;
        }
    }

    private void OnGraphPointerUp(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                DragHighlight();
                dragBox.gameObject.SetActive(false);
                break;
        }
    }

    private void OnGraphPointerDrag(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                dragBox.gameObject.SetActive(true);
                Vector2 size = Utility.GetMousePosition() - dragOrigin;
                size.Scale(new Vector2(1, -1));

                dragBox.pivot = GetDragBoxPivot(size);
                dragBox.position = dragOrigin;
                dragBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
                break;
        }
    }

    private Vector2 GetDragBoxPivot(Vector2 size)
    {
        Vector2 pivot = new();
        if (size.x > 0)
        {
            pivot.x = 0;
        }
        else
        {
            pivot.x = 1;
        }

        if (size.y > 0)
        {
            pivot.y = 1;
        }
        else
        {
            pivot.y = 0;
        }
        return pivot;
    }

    private void DragHighlight()
    {
        bool isInsideBox(Vector3 node)
        {
            Vector3 offset = FindNodeCenterOffset(dragBox);
            Vector3 middleOfBox = dragBox.position + offset;

            if (Mathf.Abs(middleOfBox.x - node.x) < dragBox.sizeDelta.x / 2f)
            {
                if (Mathf.Abs(middleOfBox.y - node.y) < dragBox.sizeDelta.y / 2f)
                {
                    return true;
                }
            }

            return false;
        }


        foreach (Node node in Graph.nodes)
        {
            if (highlightedNodes.Contains(node))
            {
                continue;
            }

            if (isInsideBox(node.PanelRect.position + FindNodeCenterOffset(node.PanelRect)))
            {
                HighlightNode(node);
            }
        }
    }

    private void OnNodePointerClick(Node node, PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                CloseContextMenu();
                if (!(Input.GetKey(KeyCode.LeftControl) || hasDraggedNodes))
                {
                    ClearHighlights();
                }
                break;
            case PointerEventData.InputButton.Right:
                var ctx = new ContextMenuBuilder()
                .Add("Duplicate", () => DuplicateNode(node))
                .Add("Clear Connections", () => ClearNodeConnections(node))
                .Add("Delete", () => DeleteNode(node))
                .Build();

                SetContextMenu(ctx);
                DisplayContextMenu();
                break;
        }
    }

    private void OnNodePointerDown(Node node, PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                HighlightNode(node); 
                node.OnSelect(Graph);
                detailsPanel.ShowPanel(node);
                print("DOWN");

                foreach (Node selectedNode in highlightedNodes)
                {
                    nodeDragOrigins.Add(selectedNode, selectedNode.Position);
                }
                break;
        }
    }


    private void OnNodeDrag(Node node, PointerEventData eventData)
    {
        hasDraggedNodes = true;
        nodeDragOrigins.TryGetValue(node, out Vector2 nodeOrigin);
        Vector2 nodePosDiff = node.Position - nodeOrigin;

        foreach (Node selectedNode in highlightedNodes)
        {
            if (selectedNode == node)
            {
                continue;
            }

            nodeDragOrigins.TryGetValue(selectedNode, out Vector2 selectedNodeOrigin);
            selectedNode.SetPosition(selectedNodeOrigin + nodePosDiff);
        }
    }

    private void OnNodePointerUp(Node node, PointerEventData eventData)
    {
        IEnumerator DelayedReset()
        {
            yield return new WaitForEndOfFrame();
            hasDraggedNodes = false;
        }
        nodeDragOrigins.Clear();
        StartCoroutine(DelayedReset());
    }

    private void OnNodeConnectionPointerClick(string connID, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var contextMenu = new ContextMenuBuilder()
            .Add("Clear Connection", () => DisconnectConnection(connID))
            .Build();

            SetContextMenu(contextMenu);
            DisplayContextMenu();
        }
    }

    private void DeleteNode(Node node)
    {
        foreach (Node highlightedNode in highlightedNodes)
        {
            Graph.Delete(highlightedNode);
        }
        if (node != null)
        {
            Graph.Delete(node);
        }
        ClearHighlights();
        CloseContextMenu();
    }

    private void DuplicateNode(Node node)
    {
        Graph.Duplicate(node);
        CloseContextMenu();
    }

    private void DisconnectConnection(string connID)
    {
        Graph.Disconnect(connID);
        CloseContextMenu();
    }

    private void ClearNodeConnections(Node node)
    {
        Graph.ClearConnectionsOf(node);
        CloseContextMenu();
    }

    public void HighlightNode(Node node)
    {
        if (highlightedNodes.Contains(node))
        {
            if (activeHighlights.Count > 1) //Double check for the details panel being hidden
            {
                detailsPanel.HidePanel();
            }
            return;
        }

        RectTransform highlightRect = Instantiate(highlight, Graph.background).GetComponent<RectTransform>();
        highlightRect.gameObject.SetActive(true);

        highlightRect.sizeDelta = node.PanelRect.sizeDelta + new Vector2(10, 10);

        activeHighlights.Add(highlightRect);
        highlightedNodes.Add(node);
        highlightCoroutines.Add(StartCoroutine(HighlightFollow(node, highlightRect)));
        node.SetAsLastSibling();

        if (activeHighlights.Count > 1)
        {
            detailsPanel.HidePanel();
        }
    }

    IEnumerator HighlightFollow(Node node, RectTransform highlightRect)
    {
        while (true && highlight != null && node != null)
        {
            highlightRect.localPosition = node.PanelRect.localPosition + FindNodeCenterOffset(node.PanelRect);
            highlightRect.sizeDelta = node.PanelRect.sizeDelta + new Vector2(10, 10);
            yield return new WaitForEndOfFrame();
        }
        highlightRect.gameObject.SetActive(false);
    }

    private Vector3 FindNodeCenterOffset(RectTransform node)
    {
        float xMult = 0.5f - node.pivot.x;
        float yMult = 0.5f - node.pivot.y;


        return new Vector3(node.sizeDelta.x * xMult, node.sizeDelta.y * yMult);
    }

    private void ClearHighlights()
    {
        for (int i = 0; i < activeHighlights.Count; i++)
        {
            Destroy(activeHighlights[i].gameObject);
            StopCoroutine(highlightCoroutines[i]);
        }
        activeHighlights.Clear();
        highlightCoroutines.Clear();
        highlightedNodes.Clear();
        nodeDragOrigins.Clear();
    }
}
