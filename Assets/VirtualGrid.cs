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
using Core.Devices;
using Core;
using Simulation = Core.Simulator;

public class VirtualGrid : MonoBehaviour
{
    public bool isPositively = true;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private SphereCollider _collider;
    [SerializeField] public List<Node> connections = new List<Node>();
    [SerializeField] private Transform activeGridTransform;
    [SerializeField] private Transform inactiveGridTransform;

    public int depth = 0;

    protected void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<SphereCollider>();
        inactiveGridTransform = GameObject.Find("InactiveGrid").transform;
        activeGridTransform = GameObject.Find("ActiveGrid").transform;
    }

    public void SetVisible(bool visible)
    {
        _renderer.enabled = visible;
    }

    public void SetCollider(bool collider)
    {
        _collider.enabled = collider;
    }

    public void Connect(Node node)
    {
        if (!connections.Contains(node))
        {
            connections.Add(node);
            node.AddGrid(this);
        }
        Simulation.Simulator.AddActiveGrid(this);
        transform.SetParent(activeGridTransform);
    }

    public bool isVisited = false;

    public void ResetVisit()
    {
        isVisited = false;
    }

    public void Visit()
    {
        isVisited = true;
    }

    public void Disconnect(Node node)
    {
        connections.Remove(node);
        node.RemoveGrid(this);
        if(connections.Count <= 0)
        {
            Simulation.Simulator.RemoveActiveGrid(this);
            transform.SetParent(inactiveGridTransform);
        }
    }
}
