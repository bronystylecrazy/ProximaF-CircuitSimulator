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
using System.Collections.Generic;
using System.Linq;
using Core.Managers;
using Core.Devices;
using UnityEngine;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Documentation;
using SpiceSharp.Simulations;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Core.Simulator {
    public class Simulator : Singleton<Simulator> {

        [SerializeField] private string _deviceName = "Resistor";
        [SerializeField] private Node currentNode;
        [Range(0, 1)]
        [SerializeField] private float _lineHeight;
        [SerializeField] private Color _lineColor = Color.white;
        [SerializeField] private List<Node> nodeList = new List<Node>();
        [SerializeField] private List<ElectronMove> moveList = new List<ElectronMove>();

        [SerializeField] private Vector3 cameraPositionOrigin;
        [SerializeField] private Quaternion cameraRotationOrigin;

        [Range(0, 100)]
        [SerializeField] private float speed = 0.025f;

        [Range(0.01f, 128f)]
        [SerializeField] private float fps = 128f;

        [SerializeField] private float step = 0;
        [SerializeField] private float _currentTime = 0;
        [SerializeField] private float _electronDensity = 0.05f;

        [SerializeField] private Slider speedSlider;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button resetButton;

        [SerializeField]
        private float defaultSpeed = 40f;
        [SerializeField] private float defaultTimeScale = 1.0f;

        [SerializeField] private TMP_Text monitor;

        [SerializeField] private List<Button> itemButtons = new List<Button>();
        [SerializeField] private Toggle showInfoToggle;
        [SerializeField] private GameObject ValueEditor;
        [SerializeField] private TMP_Text EditorNodeType;
        [SerializeField] private Button DeleteChanged;
        [SerializeField] private Button SubmitChanged;
        [SerializeField] private Button CancelChanged;
        [SerializeField] private TMP_InputField EditorInputField;
        [SerializeField] private Toolbox targetToolBox;

        [SerializeField] private bool _showInformation;


        public static bool IsShowingInformation() => Instance._showInformation;

        [SerializeField] private List<VirtualGrid> activeGrids = new List<VirtualGrid>();

        public static void AddActiveGrid(VirtualGrid grid)
        {
            var context = Instance.activeGrids;
            if (!context.Contains(grid)) context.Add(grid);
        }

        public static void RemoveActiveGrid(VirtualGrid grid)
        {
            var context = Instance.activeGrids;
            context.Remove(grid);
        }

        protected override void Awake()
        {
            base.Awake();
            speedSlider.maxValue = 200;
            speedSlider.minValue = 0;
            speedSlider.value = defaultSpeed;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            timeSlider.maxValue = 2.5f;
            timeSlider.minValue = 0;
            timeSlider.value = defaultTimeScale;
            timeSlider.onValueChanged.AddListener(OnTimeScaleChanged);

            resetButton.onClick.AddListener(OnReset);
            DeleteChanged.onClick.AddListener(OnDeleteNode);

            SubmitChanged.onClick.AddListener(OnValueChanged);
            CancelChanged.onClick.AddListener(OnValueCancel);

            foreach (var button in itemButtons)
            {
                button.onClick.AddListener(() => SetNodeTypeByButton(button));
            }
            _showInformation = showInfoToggle.isOn;
            showInfoToggle.onValueChanged.AddListener((value) => _showInformation = value);

            ClearColorFromButtons();
        }

        public void OnDeleteNode()
        {
            if (targetToolBox == null || targetToolBox.node == null) return;

            Node node = targetToolBox.node;
            Node nextNode = node.GetNextNode();
            Node previousNode = node.GetPreviousNode();
            
            if(nextNode != null)
            {
                nextNode.SetPreviousNode(null);
            }

            if (previousNode != null)
            {
                previousNode.SetNextNode(null);
            }

            nodeList.Remove(node);
            Destroy(node.gameObject);

            PerformSimulation();
            ValueEditor.SetActive(false);
        }

        public void OnValueChanged()
        {
            float value;
            if (float.TryParse(EditorInputField.text, out value)) {
                targetToolBox.node.SetValue(value);
                ValueEditor.SetActive(false);
                PerformSimulation();
                chooseToolBox = null;

            }
        }

        public void OnValueCancel()
        {
            EditorInputField.text = "0";
            ValueEditor.SetActive(false);
            chooseToolBox = null;
        }

        public static void SetMonitor(string text)
        {
            Instance.monitor.text = text;
        }

        public void OnReset()
        {
            foreach (var movement in moveList)
            {
                movement.CleanUp();
                Destroy(movement.gameObject);
            }
            foreach (var node in nodeList)
            {
                Destroy(node.gameObject);
            }

            nodeList.Clear();
            moveList.Clear();
            
        }

        protected void Update() {
            CreateDevice();
            HandleNodeSwitch();
            HandleSelectNode();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (activeGrids.Count > 0) {
                    TraverseGrid(activeGrids[0], 0);
                }
            }
        }

        protected void LateUpdate() {
            CreateDeviceLate();
            TimeStepize();
            HandleSelectionNodeLate();
        }

        private void OnSpeedChanged(float value)
        {
            speed = value;
        }

        private void OnTimeScaleChanged(float value)
        {
            Time.timeScale = value;
        }

        private void TimeStepize()
        {
            if (step >= 1) step = 0;

            if (Time.time > _currentTime)
            {
                UpdateTimeOnceElectronMoves();
                step += 0.01f;
                _currentTime = Time.time + 1f / GetFPS();
            }
        }




        private void UpdateTimeOnceElectronMoves()
        {
            foreach (var move in moveList) move.Tick(step);
        }

        private bool isDragged;
        private Vector3 originalPosition;
        [SerializeField] private Vertex hoveringVertex;

        public void HandleNodeSwitch() {
            if (Input.GetKeyDown(KeyCode.Keypad1)) _deviceName = "Wire";
            if (Input.GetKeyDown(KeyCode.Keypad2)) _deviceName = "Resistor";
            if (Input.GetKeyDown(KeyCode.Keypad3)) _deviceName = "VoltageSource";
        }

        public Toolbox chooseToolBox;

        [SerializeField] private TMP_Text InfoBox;

        public void HandleSelectNode()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (_deviceName == "Selection")
            {
                Ray ray = CameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Device")))
                {
                    if (hit.collider.CompareTag("Device"))
                    {
                        chooseToolBox = hit.collider.GetComponent<Toolbox>();
                        Node node = chooseToolBox.node;
                        if (node != null) {
                            float V = (node.posVoltage - node.negVoltage);
                            float R = node.GetValue();
                            float I = Mathf.Abs(V / R);
                            float P = I * I * R;
                            string text = "";
                            if (node.NodeType == "VoltageSource") text += "Vd = {2}v\nV+ = "+ node.posVoltage +"v\nV- = "+ node.negVoltage +"v";
                            if (node.NodeType == "Resistor") text += "I = {0}A\nR = {2}Ω\nV = {2}v\nV+ = " + node.posVoltage + "v\nV- = " + node.negVoltage + "v\nP = {3} watt";
                            if (node.NodeType == "Switch") text += "State = " + (node.state ? "ON" : "OFF");
                            if (node.NodeType == "Wire") text += "Very nice wire!\nI = {0}A\nV = {2}v\nV+ = " + node.posVoltage + "v\nV- = " + node.negVoltage + "v\nP = {3} watt";
                            if (node.NodeType == "Diode") text += "Genius bulb!\nI = {0}A\nV = {2}v\nV+ = " + node.posVoltage + "v\nV- = " + node.negVoltage + "v\nP = {3} watt";

                            InfoBox.text = string.Format("<b>Information</b>\n"+ text, I, V, R, P);
                        }
                    }
                    else
                    {
                        SetMonitor("");
                        InfoBox.text = "No information..";
                        chooseToolBox = null;
                    }
                }
                else
                {
                    chooseToolBox = null;
                }
            }


        }
        public static string GetDevice()
        {
            return Instance._deviceName;
        }

        public void HandleSelectionNodeLate()
        {
            if (chooseToolBox == null || chooseToolBox.node == null || EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("Clicked on " + chooseToolBox.name);
                if (chooseToolBox.node.NodeType == "Switch")
                {
                    (chooseToolBox.node as Switch).Toggle();
                    PerformSimulation();
                }
                else
                {
                    EditorInputField.text = chooseToolBox.node.GetValue().ToString();
                    EditorNodeType.text = chooseToolBox.node.NodeType;
                    targetToolBox = chooseToolBox;
                    ValueEditor.SetActive(true);
                }
            }
        }

        [SerializeField] GameObject ErrorPanel;
        [SerializeField] TMP_Text ErrorText;
        public static void PerformSimulation()
        {
            Instance.ErrorPanel.gameObject.SetActive(false);
            try
            {
                Instance.Simulate();
            }
            catch (ValidationFailedException e)
            {
                int i = 0;   
                foreach (var violation in e.Rules.Violations)
                {
                    Debug.LogError("Simulation Error: " + violation.Rule.ToString());
                    i++;
                }

                foreach (var node in Instance.nodeList) node.RecieveError();
                Instance.ErrorPanel.gameObject.SetActive(true);
                Instance.ErrorText.text = string.Format("{0} simulation errors take place!", i);
            }
        }


        public void OnGUI()
        {
            if (chooseToolBox == null || EventSystem.current.IsPointerOverGameObject() || _showInformation) return;
            Node node = chooseToolBox.node;
            if (node == null) return;
            string text = chooseToolBox.node.GetGUIText();

            GUI.Label(new Rect(Input.mousePosition.x, Screen.height - (Input.mousePosition.y + 25), 250, 50), text, new GUIStyle()
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            });


        }

        public void CreateDevice() {
            if (_deviceName == "Selection" || EventSystem.current.IsPointerOverGameObject() && currentGrid == null) return;
            if (Input.GetMouseButtonDown(0) ) {
                isDragged = true;
                currentNode = ResourceManager.CreateDevice(_deviceName);
                if (hoveringVertex != null) {
                    if (hoveringVertex != currentNode.posVertex) {
                        originalPosition = hoveringVertex.transform.position;
                        currentNode.UpdateNegVert(originalPosition);
                        currentNode.SetPreviousNode(hoveringVertex.Parent);
                        if (hoveringVertex.isPositively) {
                            hoveringVertex.Parent.SetNextNode(currentNode);
                        }
                        else {
                            hoveringVertex.Parent.SetPreviousNode(currentNode);
                        }
                    }
                }
                else {
                    currentNode.UpdateNegVert(currentGrid.transform.position);
                    originalPosition = currentGrid.transform.position;
                    currentGrid.Connect(currentNode);
                }
                currentNode.UpdatePosVert(currentGrid.transform.position);
            }

            if (isDragged && Input.GetMouseButtonDown(1)) {
                Destroy(currentNode.gameObject);
                currentGrid.Disconnect(currentNode);
                isDragged = false;
            }

            FindHoveringVertex();
        }

        public void CreateDeviceLate() {
            if (_deviceName == "Selection" || EventSystem.current.IsPointerOverGameObject() || currentGrid == null) return;
            if (isDragged && Input.GetMouseButton(0)) {
                currentNode.UpdateNegVert(originalPosition);
                if (hoveringVertex != null) {
                    if (hoveringVertex != currentNode.posVertex) {
                        currentNode.UpdatePosVert(hoveringVertex.transform.position);
                    }
                }
                else {
                    if(currentGrid != null)
                    {
                        currentNode.UpdatePosVert(currentGrid.transform.position);
                    }
                    else currentNode.UpdatePosVert(MousePosition);
                }
            }

            if (isDragged && Input.GetMouseButtonUp(0)) {
                if (hoveringVertex != null) {
                    if (hoveringVertex != currentNode.posVertex) {
                        currentNode.UpdatePosVert(hoveringVertex.transform.position);
                        currentNode.SetNextNode(hoveringVertex.Parent);
                        
                        if (hoveringVertex.isPositively) {
                            hoveringVertex.Parent.SetNextNode(currentNode);
                        }
                        else {
                            hoveringVertex.Parent.SetPreviousNode(currentNode);
                        }
                        hoveringVertex.SetVisible(false);
                    }
                }
                else {
                    if (currentGrid != null)
                    {
                        currentNode.UpdatePosVert(currentGrid.transform.position);
                        currentGrid.Connect(currentNode);
                    }
                    else currentNode.UpdatePosVert(MousePosition);
                }

                if (Vector3.Distance(currentNode.posVertex.transform.position, currentNode.negVertex.transform.position) < 0.05f)
                {
                    Destroy(currentNode.gameObject);
                }
                else
                {

                    currentNode.OnPlaced();
                    if (!nodeList.Contains(currentNode))
                    {
                        nodeList.Add(currentNode);
                    }

                    PerformSimulation();

                    currentNode = null;
                    isDragged = false;
                }
            }

        }

        public static float GetLineHeight()
        {
            return Instance._lineHeight;
        }

        public static Color GetLineColor()
        {
            return Instance._lineColor;
        }

        public Vector3 MousePosition => CameraManager.GetWorldMousePosition();

        private void CleanupHoveringVortex() {
            if (hoveringVertex != null) {
                hoveringVertex.SetVisible(false);
                hoveringVertex = null;
            }
        }

        public static float GetSpeed() => Instance.speed;
        public static float GetFPS() => Instance.fps;

        [SerializeField] private VirtualGrid currentGrid;

        private void FindHoveringVertex() {
            Ray ray = CameraManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit0;

            if (Physics.Raycast(ray, out hit0, 1000f, LayerMask.GetMask("Grid")))
            {
                if (hit0.collider.CompareTag("Grid"))
                {
                    if (currentGrid != null) currentGrid.SetVisible(false);
                    currentGrid = hit0.collider.GetComponent<VirtualGrid>();
                    currentGrid.SetVisible(true);
                }
                else
                {
                    if (currentGrid != null)
                    {
                        currentGrid.SetVisible(false);
                    }
                }
            }
            else
            {
                if (currentGrid != null)
                {
                    currentGrid.SetVisible(false);
                }
                currentGrid = null;
            }

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f)) {
                
                //*
                if (hit.collider.CompareTag("Vertex")) {
                    CleanupHoveringVortex();
                    hoveringVertex = hit.collider.GetComponent<Vertex>();
                    if (hoveringVertex != null)
                    {
                        hoveringVertex.SetVisible(true);
                        SetMonitor(string.Format("V = {0}{1}", hoveringVertex.Parent.posVoltage, "v"));
                    }
                }
                else {
                    CleanupHoveringVortex();
                    SetMonitor("");
                }
            }
            else {
                hoveringVertex = null;
            }
        }

        private Circuit circuit;
        public void Simulate() {
            circuit = new Circuit(); // wtf is this not support Parallel mode???
            for (int i = 0; i < nodeList.Count; i++) {
                if (nodeList[i].GetPreviousNode() != null && nodeList[i].NodeType == "VoltageSource" && nodeList[i].GetPreviousNode().NodeType != "VoltageSource") {
                    Debug.Log(nodeList[i].GetPreviousNode().pos + " / " + nodeList[i].pos);
                    Debug.Log(nodeList[i].GetPreviousNode().pos + " / " + nodeList[i].neg);
                    Debug.Log(nodeList[i].GetPreviousNode().neg + " / " + nodeList[i].pos);
                    Debug.Log(nodeList[i].GetPreviousNode().neg + " / " + nodeList[i].neg);
                    if (nodeList[i].GetPreviousNode().pos == nodeList[i].pos) nodeList[i].GetPreviousNode().pos = nodeList[i].pos = "0";
                    if (nodeList[i].GetPreviousNode().pos == nodeList[i].neg) nodeList[i].GetPreviousNode().pos = nodeList[i].neg = "0";
                    if (nodeList[i].GetPreviousNode().neg == nodeList[i].pos) nodeList[i].GetPreviousNode().neg = nodeList[i].pos = "0";
                    if (nodeList[i].GetPreviousNode().neg == nodeList[i].neg) nodeList[i].GetPreviousNode().neg = nodeList[i].neg = "0";
                }
            }

            foreach (var node in nodeList) {
                Debug.Log(node.GetEntity());
                circuit.Add(node.GetEntity());
            }

            OP op = new OP("Simulation");
            op.ExportSimulationData += OnSimulated;
            op.Run(circuit);
        }

        [SerializeField] private List<VNode> connection = new List<VNode>();

        private void OnSimulated(object sender, ExportDataEventArgs e) {
            foreach (var node in nodeList) {
                Debug.LogWarning("Voltage: " + e.GetVoltage(node.GetPositive()));
                node.SetVoltage((float)e.GetVoltage(node.GetPositive()), (float)e.GetVoltage(node.GetNegative()));
                Debug.LogWarning($@"Voltage at: {node.GetPositive()} -> {node.GetNegative()} is " + e.GetVoltage(node.GetPositive(), node.GetNegative()));
                node.OnUpdated();
            }
        }

        public static void AddMovement(ElectronMove movement)
        {
            if (Instance.moveList.Contains(movement)) return;
            Instance.moveList.Add(movement);
        }

        public static float GetElectronDensity() => Instance._electronDensity;

        public void ClearColorFromButtons()
        {
            foreach (var itemButton in itemButtons)
            {
                Image image = itemButton.GetComponentInChildren<Image>();
                if (image != null)
                {
                    if (_deviceName == itemButton.gameObject.name) {
                        image.color = new Color(1, 1, 1, .2f);
                    }
                    else
                    {
                        image.color = new Color(0, 0, 0, .2f);
                    }
                }
            }
        }

        public void SetNodeTypeByButton(Button button)
        {

            _deviceName = button.gameObject.name;
            ClearColorFromButtons();

        }

        //[SerializeField] private List<string> pathKeys = new List<string>();
        [SerializeField] private Dictionary<string, List<Node>> vnodes = new Dictionary<string, List<Node>>();
        [SerializeField] private List<VNode> vnodes_list = new List<VNode>();

        public static void AddPathKey(string path, Node node)
        {
            var context = Instance.vnodes;
            if (context.ContainsKey(path))
            {
                if (!context[path].Contains(node))
                    context[path].Add(node);
            }
            else
            {
                context.Add(path, new List<Node>() { node });
            }

            var context0 = Instance.vnodes_list;
            var vNodeFound = context0.Find(vnode => vnode.name == path);
            if(vNodeFound != null)
            {
                if (!vNodeFound.connections.Contains(node))
                    vNodeFound.connections.Add(node);
            }
            else
            {
                context0.Add(new VNode(path, new List<Node>() { node }));
            }
        }

        public static void RemovePathKey(string path, Node node)
        {
            var context = Instance.vnodes;
            if (!context.ContainsKey(path)) return;

            context[path].Remove(node);
        }

        public void TraverseGrid(VirtualGrid grid, int depth)
        {
            if (grid.isVisited) return;
            Debug.Log("Hiii!");
            grid.isVisited = true;
            grid.depth = depth;

            foreach(var connection in grid.connections)
            {
                foreach(var childGrid in connection.virtualGrids)
                {
                    if (grid.connections.Count > 2)
                    {
                        TraverseGrid(childGrid, depth + 1);
                    }
                    else
                    {
                        TraverseGrid(childGrid, depth);
                    }
                }
            }
        }
    }

    [Serializable]
    public class VNode
    {
        public string name;
        public List<Node> connections;

        public VNode(string name)
        {
            this.name = name;
        }

        public VNode(string name, List<Node> connections) : this(name)
        {
            this.connections = connections;
        }
    }
}