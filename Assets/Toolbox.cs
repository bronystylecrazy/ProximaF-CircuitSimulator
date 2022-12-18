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
using Core;
using Simulation = Core.Simulator;
using Core.Devices;

public class Toolbox : MonoBehaviour
{
    [SerializeField] public Node node;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        if (node == null) return;
        Debug.Log("Pressed on " + node.Name);
        if(Simulation.Simulator.GetDevice() == "Selection" && node.NodeType == "Switch")
        {
            (node as Switch).Toggle();
            Simulation.Simulator.PerformSimulation();
        }
    }
}
