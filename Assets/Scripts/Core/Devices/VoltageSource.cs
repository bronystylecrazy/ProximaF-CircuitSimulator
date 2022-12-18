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
using SpiceSharp.Components;
using SpiceSharp.Entities;
using UnityEngine;
using Simulation = Core.Simulator;
using Core.Managers;

namespace Core.Devices {
    public class VoltageSource : Node {

        [SerializeField] private LineRenderer _negLineRenderer;
        [SerializeField] private LineRenderer _posLineRenderer;
        [SerializeField] private Transform _model;
        [SerializeField] private Transform _pos;
        [SerializeField] private Transform _neg;

        protected override void Awake() {
            base.Awake();
            _posLineRenderer = _pos.GetComponent<LineRenderer>();
            _negLineRenderer = _neg.GetComponent<LineRenderer>();
        }

        protected override void Update()
        {
            base.Update();
            _posLineRenderer.startWidth = Simulation.Simulator.GetLineHeight();
            _posLineRenderer.endWidth = Simulation.Simulator.GetLineHeight();
            _negLineRenderer.startWidth = Simulation.Simulator.GetLineHeight();
            _negLineRenderer.endWidth = Simulation.Simulator.GetLineHeight();

            _posLineRenderer.startColor = Simulation.Simulator.GetLineColor();
            _posLineRenderer.endColor = Simulation.Simulator.GetLineColor();
            _negLineRenderer.startColor = Simulation.Simulator.GetLineColor();
            _negLineRenderer.endColor = Simulation.Simulator.GetLineColor();
        }

        public override IEntity GetEntity() {
            return new SpiceSharp.Components.VoltageSource("V"+id, GetPositive(), GetNegative(), GetValue());
        }

        public override void UpdateLine(Vector3 pos, Vector3 neg) {
            base.UpdateLine(pos, neg);
            _negLineRenderer.SetPosition(0, _neg.transform.position);
            _negLineRenderer.SetPosition(1, pos);


            _posLineRenderer.SetPosition(0, _pos.transform.position);
            _posLineRenderer.SetPosition(1, neg);

            _model.transform.position = (pos + neg) / 2;

            //float angleModelRotate = Vector3.Angle(neg, pos);

            // _model.transform.localRotation = Quaternion.Euler(0, angleModelRotate, 0);
            _model.transform.LookAt(pos);
        }

        public void OnGUI()
        {
            if (Simulation.Simulator.IsShowingInformation())
            {
                Vector3 object2screenposition = CameraManager.GetMainCamera().WorldToScreenPoint(_model.transform.position);
                GUI.Label(new Rect(object2screenposition.x, Screen.height - (object2screenposition.y + 25), 250, 50), GetGUIText(), new GUIStyle()
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold
                });
            }
        }
    }
}