using RuntimeNodeEditor;
using RuntimeNodeEditor.Examples;
using UnityEngine;

public class LogicEditorStarter : MonoBehaviour
{
    public RectTransform editorHolder;
    public LogicNodeEditor logicEditor;

    private void Start()
    {
        var graph = logicEditor.CreateGraph<NodeGraph>(editorHolder);
        logicEditor.StartEditor(graph);
    }
}
