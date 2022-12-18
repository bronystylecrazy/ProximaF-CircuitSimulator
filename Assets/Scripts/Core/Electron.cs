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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Electron : MonoBehaviour
    {
        [SerializeField] private Vector3 defaultPosition = Vector3.zero;
        [SerializeField] private Vector3 maxPosition = Vector3.zero;
        [SerializeField] private float travelDistance;
        [SerializeField] private Renderer renderer;

        [SerializeField] private bool _isLast = false;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        public void SetDefaultPosition(Vector3 pos) => defaultPosition = pos;
        public Vector3 GetDefaultPosition() => defaultPosition;

        public void SetMaxPosition(Vector3 pos) => maxPosition = pos;
        public Vector3 GetMaxPosition() => maxPosition;


        public void SetVisible(bool value) => renderer.enabled = value;

        public float GetTravelDistance() => travelDistance;

        public void SetTravelDistance(float d) => travelDistance = d;

        public void SetIsLast(bool isLast) => _isLast = isLast;
        public bool GetIsLast() => _isLast;

        public void RecalculateTravelDistance() => travelDistance = Vector3.Distance(GetMaxPosition(), GetDefaultPosition());
    }

}