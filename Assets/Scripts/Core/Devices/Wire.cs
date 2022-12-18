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

namespace Core.Devices {
    public class Wire : Node {

        [SerializeField] private LineRenderer _lineRenderer;

        [SerializeField] private Transform _model;

        protected override void Awake() {
            base.Awake();
            _lineRenderer = GetComponent<LineRenderer>();
        }
        protected override void Update()
        {
            base.Update();
            _lineRenderer.startWidth = Simulation.Simulator.GetLineHeight();
            _lineRenderer.endWidth = Simulation.Simulator.GetLineHeight();
            _lineRenderer.startColor = Simulation.Simulator.GetLineColor();
            _lineRenderer.endColor = Simulation.Simulator.GetLineColor();
        }

        public override IEntity GetEntity() {
            return new SpiceSharp.Components.Resistor("R"+id, GetPositive(), GetNegative(), GetValue());
        }

        public override void UpdateLine(Vector3 pos, Vector3 neg) {
            base.UpdateLine(pos, neg);
            _lineRenderer.SetPosition(0, neg);
            _lineRenderer.SetPosition(1, pos);

            _model.transform.position = (pos + neg) / 2;
            float d = (pos - neg).magnitude;
            _model.localScale = new Vector3(_model.localScale.x, _model.localScale.y, d * 0.98f);
            _model.transform.LookAt(pos);
        }
    }
}