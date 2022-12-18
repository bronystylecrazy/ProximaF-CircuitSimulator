/**
 * 
 * ProximaF-CircuitSimulator - Final Project
 * A simple 3D circuit simulator made with Unity3d using Spice#
 * 
 * 
 * Created by Proxima F
 * 63130500216 Patiphon Klangpraphan - Formula/Algorithm Researcher
 * 63130500223 Bhumjate Sudprasert - 3D Model / Unity3d User Interface Designer
 * 63130500227 Sirawit Pratoomsuwan - Mesh Analysis with Spice# /Unity3d Simulator Programmer
 * 63130500258 Yannakorn Rungphetwong - Formula/Algorithm Researcher
 * 
 * This project has been created for educational purposes only to present Dr.Stanislas Grare
 * 
 * WARNING: This project might contain unfixed/unresolved bugs. 
 * It still CANNOT simulate complex parallel-circuit right now. It will be updated so soon.
 * 
 * For more information: https://github.com/bronystylecrazy/ProximaF-CircuitSimulator
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Interface;
using SpiceSharp.Entities;
using UnityEngine;
using Core.Managers;
using Simulation = Core.Simulator;


namespace Core {
    public class Node : MonoBehaviour {
        public int id;
        public string Name = "Node";
        public string NodeType;
        
        [SerializeField] private Node nextNode;
        [SerializeField] private Node previousNode;
        [SerializeField] private List<Node> connections = new List<Node>();

        [SerializeField] public string pos;
        [SerializeField] public string neg;

        [SerializeField] private float value;
        
        private static int _id;

        [SerializeField] private Vector3 posPosition;
        [SerializeField] private Vector3 negPosition;

        public Vertex posVertex;
        public Vertex negVertex;

        public float posVoltage;
        public float negVoltage;

        [SerializeField] private float _current;

        private float _currentTime;

        [SerializeField] private List<ElectronMove> electronMovements = new List<ElectronMove>();

        [SerializeField] public List<VirtualGrid> virtualGrids = new List<VirtualGrid>();

        protected virtual void Start() {
            UpdateLine(Vector3.zero, Vector3.zero);
        }
        
        protected virtual void Awake() {
            id = _id++;
            Name = NodeType + "" + id;
        }

        protected virtual void Update()
        {
           // ComputeCurrent();
        }

        protected virtual void LateUpdate()
        {
            CalculateTick();
        }

        private void CalculateTick()
        {
            if(Time.time > _currentTime)
            {
                //SimulateElectron();
                _currentTime += Time.time + 1/ Simulation.Simulator.GetFPS();
            }
        }

        public virtual void Connect(Node node)
        {
            if (!connections.Contains(node))
            {
                connections.Add(node);
            }
        }

        public virtual void Disconnect(Node node)
        {
            connections.Remove(node);
        }

        public virtual void SetNextNode(Node node) {
            nextNode = node;
            if (nextNode == null)
            {
                Disconnect(node);
                node.Disconnect(this);
                return;
            }
            Connect(node);
            node.Connect(this);

            if (node.id > id) {
                pos = $@"{Name}_{node.Name}";
            }
            else {
                pos = $@"{node.Name}_{Name}";
            }

            Simulation.Simulator.AddPathKey(pos, node);
        }

        public void AddGrid(VirtualGrid grid)
        {
            if (!virtualGrids.Contains(grid)) virtualGrids.Add(grid);
        }

        public void RemoveGrid(VirtualGrid grid)
        {
            virtualGrids.Remove(grid);
        }

        public virtual void SetPreviousNode(Node node) {
            previousNode = node;

            if (previousNode == null)
            {
                Disconnect(node);
                node.Disconnect(this);
                return;
            }
            Connect(node);
            node.Connect(this);

            if (node.id > id) {
                neg = $@"{Name}_{node.Name}";
            }
            else {
                neg = $@"{node.Name}_{Name}";
            }

            Simulation.Simulator.AddPathKey(neg, node);
        }

        public virtual Node GetNextNode() {
            return nextNode;
        }
        
        public virtual Node GetPreviousNode() {
            return previousNode;
        }

        public virtual IEntity GetEntity() {
            return null;
        }

        public virtual VNode CreateVNode()
        {
            return null;
        }

        public string GetPositive() {
            return pos;
        }

        public virtual void SetPositive(string name) {
            pos = name;
        }
        
        public string GetNegative() {
            return neg;
        }

        public virtual void SetNegative(string name) {
            neg = name;
        }

        public float GetValue() {
            return value;
        }

        public void SetValue(float value) {
            this.value = value;
        }

        public virtual void ComputeCurrent()
        {
            if (nextNode == null || previousNode == null) return;

            float current = (NodeType == "VoltageSource") ? (nextNode.GetCurrent() + previousNode.GetCurrent()) / 2 : (posVoltage - negVoltage) / value;
            _current = current;
        }
        /*
        public virtual void SimulateElectron()
        {
            if (_current < 0)
            {
                float electronDensity = 0.5f;
                Vector3 basis = negPosition;
                Vector3 dPos = (posPosition - negPosition);
                Vector3 direction = dPos.normalized;
                float d = dPos.sqrMagnitude;

                
                for(float d = )

            }
        }*/
        /*
        private void SpawnElectrons()
        {
            Vector3 basis = negVertex.transform.position;
            Vector3 dPos = (posVertex.transform.position - negVertex.transform.position);
            Vector3 direction = dPos.normalized;
            float d = dPos.sqrMagnitude;
            int electronCount = Mathf.FloorToInt(d / electronDensity);

            for (float i = 0; i <= d; i += electronDensity)
            {
                Vector3 electronPos = basis + direction * i;
                Electron electron = GenerateElectron(electronPos);
            }
        }*/

        public virtual float GetCurrent()
        {
            ComputeCurrent();
            return _current;
        }

        public virtual float Current => _current;

        public virtual void UpdateLine(Vector3 pos, Vector3 neg) {
            negPosition = neg;
            posPosition = pos;
            posVertex.transform.position = posPosition;
            negVertex.transform.position = negPosition;
        }

        public virtual void UpdatePosVert(Vector3 pos) {
            posPosition = pos;
            posVertex.transform.position = posPosition;
            UpdateLine(posPosition, negPosition);
        }
        
        public virtual void UpdateNegVert(Vector3 neg) {
            negPosition = neg;
            negVertex.transform.position = negPosition;
            UpdateLine(posPosition, negPosition);
        }

        public virtual void OnPlaced() {
            posVertex.SetVisible(false);
            negVertex.SetVisible(false);
            posVertex.SetCollider(true);
            negVertex.SetCollider(true);
        }

        [SerializeField] private bool isReverseCurrent;

        public void SetVoltage(float pos, float neg) {
            this.posVoltage = pos;
            this.negVoltage = neg;
        }

        public virtual void OnUpdated()
        {
            Debug.Log("On updated!");
            //isReverseCurrent = posVoltage < negVoltage;
            foreach (var movement in electronMovements) movement.MountedElectrons(isReverseCurrent);
        }


        public bool state;

        public virtual string GetGUIText()
        {
            string text = "";
            if (NodeType == "Resistor") text = "Resistor " + GetValue() + "Ω";
            if (NodeType == "VoltageSource") text = "DC " + GetValue() + "v";
            if (NodeType == "Diode") text = "Bulb " + GetValue() + " watt";
            if (NodeType == "Switch") text = "Switch is " + (state ? "ON" : "OFF");
            if (NodeType == "Wire") text = " Wire ";

            return text;
        }

        public virtual void RecieveError()
        {
            state = false;
            _current = 0;
            negVoltage = 0;
            posVoltage = 0;
        }
    }

    

    [Serializable]
    public class VNode
    {
        public string type;
        public string name;
        public string[] pins;
        public object value;

        public VNode(string type, string name, object value, params string[] pins)
        {
            this.type = type;
            this.name = name;
            this.value = value;
            this.pins = pins;
        }
    }

    [Serializable]
    public class VNodeOption
    {
        public string name;
        public string[] pins;

        public VNodeOption(string name, string[] pins)
        {
            this.name = name;
            this.pins = pins;
        }
    }
}
