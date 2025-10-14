using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

public class Collection : Node
{
    RectTransform nodeBody;
    public override void Setup()
    {
        SetHeader("Group");
        SetAsFirstSibling();
        nodeBody = GetComponent<RectTransform>();
    }

    public override void OnSelect(NodeGraph graph)
    {
        LogicNodeEditor nodeEditor = FindFirstObjectByType<LogicNodeEditor>();
        foreach (Node node in graph.nodes)
        {
            if (node.gameObject == gameObject)
            {
                continue;
            }

            if (nodeEditor.highlightedNodes.Contains(node))
            {
                continue;
            }

            if (IsInsideNodeArea(node.Position))
            {
                nodeEditor.HighlightNode(node);
            }
        }
    }

    private bool IsInsideNodeArea(Vector2 position)
    {
        Vector3 middleOfBox = Position + (PanelRect.sizeDelta / 2f * new Vector2(1, -1));

        if (Mathf.Abs(middleOfBox.x - position.x) < PanelRect.sizeDelta.x / 2f)
        {
            if (Mathf.Abs(middleOfBox.y - position.y) < PanelRect.sizeDelta.y / 2f)
            {
                return true;
            }
        }

        return false;
    }

    public override void OnSerialize(Serializer serializer)
    {
        serializer.Add("title", headerText.text);
        serializer.Add("sizeX", nodeBody.sizeDelta.x.ToString());
        serializer.Add("sizeY", nodeBody.sizeDelta.y.ToString());
    }

    public override void OnDeserialize(Serializer serializer)
    {
        GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(serializer.Get("title"));
        nodeBody.sizeDelta = new Vector2(float.Parse(serializer.Get("sizeX")), float.Parse(serializer.Get("sizeY")));
    }
}
