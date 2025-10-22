using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RuntimeNodeEditor
{
    public class NodeEditor : MonoBehaviour
    {
        public NodeGraph Graph { get { return _graph; } }
        public SignalSystem Events { get { return _signalSystem; } }
        public float minZoom = 0.3f;
        public float maxZoom = 2f;
        public GameObject contextMenuPrefab;

        private NodeGraph _graph;
        private ContextMenu _contextMenu;
        private ContextItemData _contextMenuData;
        private SignalSystem _signalSystem;

        [Header("Snapping Settings")]
        public float gridSnapUnitSize;


        [Header("Alignment Settings")]
        public float alignmentSnapSensitivity;
        public GameObject alignmentLinePrefab;

        [Header("Highlight Object")]
        public GameObject highlight;
        private List<Coroutine> highlightCoroutines = new();

        private List<RectTransform> activeHighlights = new();
        [HideInInspector]
        public HashSet<Node> highlightedNodes = new();
        private Dictionary<Node, Vector2> nodeDragOrigins = new();

        [Header("Drag Object")]
        public RectTransform dragBox;
        private Vector2 dragOrigin;
        private bool hasDraggedNodes = false;

        [Header("Details Panel Object")]
        public DetailsPanel detailsPanel;

        public virtual void StartEditor(NodeGraph graph)
        {
            _signalSystem = new SignalSystem();
            _graph = graph;
            _graph.Init(_signalSystem, minZoom, maxZoom);

            _graph.gridSnapUnitSize = gridSnapUnitSize;
            _graph.alignmentSnapSensitivity = alignmentSnapSensitivity;
            _graph.alignmentIndicator = alignmentLinePrefab;

            if (contextMenuPrefab != null)
            {
                _contextMenu = Instantiate(contextMenuPrefab, _graph.contextMenuContainer).GetComponent<ContextMenu>();
                _contextMenu.Init();
                CloseContextMenu();
            }

            Events.OnGraphPointerDown += OnGraphPointerDown;
            Events.OnGraphPointerUp += OnGraphPointerUp;
            
            Events.OnGraphPointerClickEvent += OnGraphPointerClick;
            Events.OnGraphPointerDragEvent += OnGraphPointerDrag;

            Events.OnNodePointerClickEvent += OnNodePointerClick;
            Events.OnNodePointerDragEvent += OnNodeDrag;
            Events.OnConnectionPointerClickEvent += OnNodeConnectionPointerClick;

            Events.OnNodePointerDownEvent += OnNodePointerDown;
            Events.OnNodePointerUpEvent += OnNodePointerUp;

            Graph.SetSize(Vector2.one * 20000);

            ClearHighlights();

            dragBox.SetParent(Graph.background);
            dragBox.gameObject.SetActive(false);

            detailsPanel.HidePanel();
        }

        //  context methods
        public void DisplayContextMenu()
        {
            _contextMenu.Clear();
            _contextMenu.Show(_contextMenuData, Utility.GetCtxMenuPointerPosition(Graph.contextMenuContainer));
        }

        public void CloseContextMenu()
        {
            _contextMenu.Hide();
            _contextMenu.Clear();
        }

        public void SetContextMenu(ContextItemData ctx)
        {
            _contextMenuData = ctx;
        }

        //  create graph in scene
        public TGraphComponent CreateGraph<TGraphComponent>(RectTransform holder) where TGraphComponent : NodeGraph
        {
            return CreateGraph<TGraphComponent>(holder, Color.gray, Color.yellow);
        }

        public TGraphComponent CreateGraph<TGraphComponent>(RectTransform holder, Color bgColor, Color connectionColor) where TGraphComponent : NodeGraph
        {
            //  Create a parent
            var parent = new GameObject("NodeGraph");
            parent.transform.SetParent(holder);
            parent.AddComponent<RectTransform>().Stretch();
            parent.AddComponent<Image>();
            parent.AddComponent<Mask>();

            //      - add background child, stretch
            var bg = new GameObject("Background");
            bg.transform.SetParent(parent.transform);
            var bgRect = bg.AddComponent<RectTransform>().Stretch();
            bg.AddComponent<Image>().color = bgColor;

            //      - add pointer listener child, stretch
            var pointerListener = new GameObject("PointerListener");
            pointerListener.transform.SetParent(parent.transform);
            pointerListener.AddComponent<RectTransform>().Stretch();
            pointerListener.AddComponent<Image>().color = Color.clear;

            //      - add graph child, center, with size
            var graph = new GameObject("Graph");
            graph.transform.SetParent(parent.transform);
            var graphRect = graph.AddComponent<RectTransform>();
            graphRect.sizeDelta = Vector2.one * 1000f;
            graphRect.anchoredPosition = Vector2.zero;


            //          - add line container child, stretch
            var lineContainer = new GameObject("LineContainer");
            lineContainer.transform.SetParent(graph.transform);
            var lineContainerRect = lineContainer.AddComponent<RectTransform>().Stretch();

            //          - add node container
            var nodeContainer = new GameObject("NodeContainer");
            nodeContainer.transform.SetParent(graph.transform);
            var nodeContainerRect = nodeContainer.AddComponent<RectTransform>().Stretch();

            //              - add pointer locator 
            var pointerLocator = new GameObject("PointerLocator");
            pointerLocator.transform.SetParent(nodeContainer.transform);
            var pLocatorRect = pointerLocator.AddComponent<RectTransform>();
            pLocatorRect.sizeDelta = Vector2.zero;
            pLocatorRect.anchoredPosition = Vector2.zero;


            //      - add ctx menu child, stretch
            var ctxMenuContainer = new GameObject("CtxMenuContainer");
            ctxMenuContainer.transform.SetParent(parent.transform);
            var ctxContainerRect = ctxMenuContainer.AddComponent<RectTransform>().Stretch();

            var bezierDrawer = graph.AddComponent<BezierCurveDrawer>();
            bezierDrawer.pointerLocator = pLocatorRect;
            bezierDrawer.lineContainer = lineContainerRect;
            bezierDrawer.connectionColor = connectionColor;

            var listener = pointerListener.AddComponent<GraphPointerListener>();

            var nodeGraph = graph.AddComponent<TGraphComponent>();
            nodeGraph.contextMenuContainer = ctxContainerRect;
            nodeGraph.nodeContainer = nodeContainerRect;
            nodeGraph.background = bgRect;
            nodeGraph.pointerListener = listener;
            nodeGraph.drawer = bezierDrawer;

            return nodeGraph;
        }

        protected void CreateNodeFromPrefab(string filePath)
        {
            Graph.Create(filePath);
            CloseContextMenu();
        }
        
        protected void SaveGraph(string filePath)
        {
            Graph.SaveFile(filePath);
            CloseContextMenu();
        }

        protected void LoadGraph(string filePath)
        {
            ClearHighlights();
            CloseContextMenu();
            Graph.Clear();
            Graph.LoadFile(filePath);
        }

        protected void OnGraphPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    CloseContextMenu();
                    detailsPanel.HidePanel();
                    break;
                case PointerEventData.InputButton.Right:
                    SetContextMenu(GetGraphContextMenu());
                    DisplayContextMenu();
                    break;
            }
        }

        protected void OnGraphPointerDown(PointerEventData eventData)
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

        protected void OnGraphPointerDrag(PointerEventData eventData)
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

        protected void OnGraphPointerUp(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    DragHighlight();
                    dragBox.gameObject.SetActive(false);
                    break;
            }
        }


        protected void OnNodePointerClick(Node node, PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    CloseContextMenu();
                    break;
                case PointerEventData.InputButton.Right:
                    SetContextMenu(GetNodeContextMenu(node));
                    DisplayContextMenu();
                    break;
            }
        }

        protected void OnNodePointerDown(Node node, PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    if (!Input.GetKey(KeyCode.LeftControl) && !highlightedNodes.Contains(node))
                    {
                        ClearHighlights();
                    }

                    HighlightNode(node);
                    node.OnSelect(Graph);
                    detailsPanel.ShowPanel(node);

                    foreach (Node selectedNode in highlightedNodes)
                    {
                        nodeDragOrigins.Add(selectedNode, selectedNode.Position);
                    }
                    break;
            }
        }

        protected void OnNodeDrag(Node node, PointerEventData eventData)
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

        protected void OnNodePointerUp(Node node, PointerEventData eventData)
        {
            IEnumerator DelayedReset()
            {
                yield return new WaitForEndOfFrame();
                hasDraggedNodes = false;
            }
            nodeDragOrigins.Clear();
            StartCoroutine(DelayedReset());
        }

        protected void OnNodeConnectionPointerClick(string connID, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                SetContextMenu(GetConnectionContextMenu(connID));
                DisplayContextMenu();
            }
        }


        protected virtual ContextItemData GetGraphContextMenu() => null;

        protected virtual ContextItemData GetNodeContextMenu(Node node) => null;

        protected ContextItemData GetConnectionContextMenu(string connID)
        {
            return new ContextMenuBuilder()
            .Add("Clear Connection", () => DisconnectConnection(connID))
            .Build();
        }

        protected void DeleteNode(Node node)
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

        protected void DuplicateNode(Node node)
        {
            Graph.Duplicate(node);
            CloseContextMenu();
        }

        protected void DisconnectConnection(string connID)
        {
            Graph.Disconnect(connID);
            CloseContextMenu();
        }

        protected void ClearNodeConnections(Node node)
        {
            Graph.ClearConnectionsOf(node);
            CloseContextMenu();
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

        //Node Pivot + Offset = Center of Node's RectTransform
        private Vector3 FindNodeCenterOffset(RectTransform node)
        {
            float xMult = 0.5f - node.pivot.x;
            float yMult = 0.5f - node.pivot.y;


            return new Vector3(node.sizeDelta.x * xMult, node.sizeDelta.y * yMult);
        }

        public void ClearHighlights()
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
}