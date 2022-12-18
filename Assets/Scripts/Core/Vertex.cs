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
using UnityEngine;

namespace Core {
    public class Vertex : MonoBehaviour {
        public Node Parent;
        public bool isPositively = true;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private SphereCollider _collider;
        protected void Awake() {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<SphereCollider>();
            SetCollider(false);
        }

        public void SetParent(Node parent) {
            this.Parent = parent;
        }

        public void SetVisible(bool visible) {
            _renderer.enabled = false;
        }

        public void SetCollider(bool collider) {
            _collider.enabled = collider;
        }
    }
}