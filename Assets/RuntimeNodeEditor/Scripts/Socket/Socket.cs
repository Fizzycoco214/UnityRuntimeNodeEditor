using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeNodeEditor
{
    public abstract class Socket : MonoBehaviour
    {
        public Node OwnerNode { get { return _ownerNode; } }
        public ISocketEvents Events { get { return _socketEvents; } }

        public static Dictionary<Type, Color> socketColors = new Dictionary<Type, Color>
        {
            {typeof(bool), Color.red},
            {typeof(string), new Color(1, 0.65f, 0.23f)},
            {typeof(float), Color.green},
            {typeof(object), Color.white}
        };


        public string socketId;
        public SocketHandle handle;
        public Type socketType;
        public ConnectionType connectionType;
        public Image sprite;
        private Node _ownerNode;
        private ISocketEvents _socketEvents;



        public void SetOwner(Node owner, ISocketEvents events)
        {
            _ownerNode = owner;
            _socketEvents = events;
            Setup();
        }

        public virtual void Setup() { }
        public abstract bool HasConnection();


        public void SetType(Type type)
        {
            socketType = type;
            SetColor(type);
        }

        public void SetColor(Type type)
        {
            sprite.color = socketColors.GetValueOrDefault(type, Color.white);
        }
    }
}