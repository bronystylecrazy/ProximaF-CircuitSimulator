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
using System.Collections.Generic;
using Core;
using System.Linq;
using UnityEngine;

namespace Core.Managers {
    public class ResourceManager : Singleton<ResourceManager> {
        [SerializeField] private List<GameObject> prefabs;
        [SerializeField] private List<Node> devices;
        private Dictionary<string, GameObject> PrefabList = new Dictionary<string, GameObject>();
        private Dictionary<string, Node> DeviceList = new Dictionary<string, Node>();

        protected override void Awake() {
            base.Awake();
            AssembleResources();
        }

        public void AssembleResources() {
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/");
            Node[] devices = Resources.LoadAll<Node>("Devices/");
            PrefabList = prefabs.ToDictionary(prefab => prefab.name, prefab => prefab);
            DeviceList = devices.ToDictionary(device => device.NodeType, device => device);
            this.prefabs = prefabs.ToList();
            this.devices = devices.ToList();
        }

        public static GameObject GetPrefab(string name) {
            return Instance.PrefabList[name];
        }

        public static Node GetDevice(string name) {
            Debug.LogWarning("Getting device's prefab: " + name);
            Debug.LogWarning(Instance.DeviceList.ContainsKey(name));
            return Instance.DeviceList[name];
        }

        public static Node CreateDevice(string name) {
            Node newDevice = Instantiate(GetDevice(name), Vector3.zero, Quaternion.identity);
            return newDevice;
        }
    }
}