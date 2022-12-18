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
using Core.Managers;
using Core.Interface;
using Simulation = Core.Simulator;

public class ElectronMove : MonoBehaviour, ITick
{
    public Node node;
    public Transform pos;
    public Transform neg;
    public List<Electron> electrons = new List<Electron>();
    public float electronDensity = 0.1f;
    private float step = 0;
    public float current = -0.1f;

    public Vector3 direction;

    [SerializeField] private float _currentTime;
    [SerializeField] private float _currentStep;


    void Start()
    {
        Simulation.Simulator.AddMovement(this);
    }


    public void MountedElectrons(bool isReverseCurrent)
    {
        Debug.Log("Electron movement has been mounted!");
        if (node == null)
        {
            Debug.LogError("Node has not been found!");
            return;
        }
        current = Mathf.Abs(node.GetCurrent());
        Debug.Log("Current: " + current);
        if (current == 0) return;
        electronDensity = Simulation.Simulator.GetElectronDensity();
        if (isReverseCurrent)
        {
            (pos, neg) = (neg, pos);
        }
        GenerateElectrons();
    }

    private void UpdateElectronPositions()
    {
        foreach (var electron in electrons)
        {
            UpdatePosition(electron);
        }
    }


    void Update()
    {
        UpdateElectronPositions();
    }
    

    public void CleanUp()
    {
        foreach (var e in electrons) Destroy(e.gameObject);
        electrons.Clear();
    }

    public void Tick(float steps)
    {
        _currentStep = step = steps;
        //UpdateElectronPositions();
    }

    private void GenerateElectrons()
    {
        CleanUp();

        if (current == 0) return;

        direction = (neg.position - pos.position).normalized;
        float d = (neg.position - pos.position).magnitude; // 5
        float i;

        Electron electron = null;

        for (i = 0; i < d; i += electronDensity)
        {
            Vector3 defaultPos = pos.position + direction * i + direction * (electronDensity * 0);// 9 - 8.5
            Vector3 maxPos = pos.position + direction * i + direction * (electronDensity * 1f);
            electron = GenerateElectron(defaultPos);
            electron.SetDefaultPosition(pos.position + direction * i);
            electron.SetMaxPosition(maxPos);
            electron.SetTravelDistance(((pos.position + direction * i) - maxPos).magnitude);
            electrons.Add(electron);
        }

        if (electron != null)
        {
            Vector3 lastMaxPos = neg.position;
            electron.SetMaxPosition(lastMaxPos);
            electron.RecalculateTravelDistance();
            electron.SetIsLast(true);
        }
    } 

    private Electron GenerateElectron(Vector3 position)
    {
        return Instantiate(ResourceManager.GetPrefab("Electron").GetComponent<Electron>(), position, Quaternion.identity);
    }

    private float _goneDistance;
    private bool isHidden;

    private void UpdatePosition(Electron electron)
    {

        Vector3 defaultPos = electron.GetDefaultPosition();
        Vector3 maxPos = electron.GetMaxPosition();
        float d = electron.GetTravelDistance();



        float dPos = (electron.transform.position - maxPos).magnitude;
        float dNeg = (electron.transform.position - defaultPos).magnitude;

        bool isExceed = (dPos + dNeg > d + 0.001f);

        if (isExceed)
        {
            if (electron.GetIsLast())
            {
                if (!isHidden)
                {
                    electron.SetVisible(false);
                    isHidden = true;
                    _goneDistance = electron.GetTravelDistance();
                }
                else
                {
                    if (_goneDistance >= electronDensity)
                    {
                        isHidden = false;
                        _goneDistance = electron.GetTravelDistance();
                        electron.SetVisible(true);
                        electron.transform.position = defaultPos;
                    }
                    _goneDistance += (current) * Simulation.Simulator.GetSpeed() * Time.deltaTime;
                }
            }
            else electron.transform.position = defaultPos;
        }
        else
        {
            if (electron.GetIsLast()) electron.SetVisible(true);
        }

        electron.transform.position += direction * (current) * Simulation.Simulator.GetSpeed() * Time.deltaTime;
    }
}
