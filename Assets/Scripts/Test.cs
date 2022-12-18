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
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpCircuit;
public class Test : MonoBehaviour {
    public static double Round(double val, int places)
    {
        if (places < 0) throw new ArgumentException("places");
        return Math.Round(val - (0.5 / Math.Pow(10, places)), places);
    }

	void Start()
	{
		Circuit sim = new Circuit();

		var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
		volt0.maxVoltage = 600;
		var res0 = sim.Create<Resistor>();
		res0.resistance = 50;
		var ground0 = sim.Create<Ground>();

		sim.Connect(volt0.leadPos, res0.leadIn);
		sim.Connect(res0.leadOut, ground0.leadIn);

		for (int x = 1; x <= 100; x++)
		{
			sim.doTick();
			// Ohm's Law
			Debug.Log(res0.getVoltageDelta() + "," + res0.resistance * res0.getCurrent()); // V = I x R
			Debug.Log(res0.getCurrent() + "," + res0.getVoltageDelta() / res0.resistance); // I = V / R
			Debug.Log(res0.resistance + "," +  res0.getVoltageDelta() / res0.getCurrent()); // R = V / I
		}
	}
}
