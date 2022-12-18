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
using UnityEngine;
using System.Collections.Generic;
namespace Core.Managers {
    public class CameraManager : Singleton<CameraManager> {
        public Camera MainCamera;
        [SerializeField] private Vector3 _mousePosition;
        [SerializeField] private LayerMask ignoreVertex;
        [SerializeField] private float density = 0.1f;
        [SerializeField] private float width = 10f;
        [SerializeField] private Transform gridTransform;
        [SerializeField] private List<VirtualGrid> vertices;


        protected void Start() {
            MainCamera = Camera.main;
            GenerateGrid();
        }

        protected void Update(){
            CalculateWorldMousePosition();
            
        }

        public void GenerateGrid()
        {

            var topLeft = CalculateWorldPosition(new Vector2(0, 0));
            var bottomRight = CalculateWorldPosition(new Vector2(1, 1));

            for (var x = topLeft.x; x < bottomRight.x; x += density)
            {
                for (var z = topLeft.z; z < bottomRight.z; z += density)
                {
                    Debug.DrawRay(new Vector3(x, topLeft.y, z), Vector3.up, Color.red);
                    var vertex = Instantiate(ResourceManager.GetPrefab("Grid"), GetWorldMousePosition(), Quaternion.identity, gridTransform).GetComponent<VirtualGrid>();
                    vertex.transform.position = new Vector3(x - width, topLeft.y, z);
                    vertex.SetVisible(false);
                    vertices.Add(vertex);
                }
            }

        }

        private void CalculateWorldMousePosition() {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Space"))) {
                _mousePosition = hit.point;
            }
            Debug.DrawRay(_mousePosition, Vector3.up, Color.green);
        }

        private Vector3 CalculateWorldPosition(Vector3 input)
        {
            Ray ray = MainCamera.ViewportPointToRay(input);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Space")))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        public static Vector3 GetWorldMousePosition() {
            return Instance._mousePosition;
        }

        public static Camera GetMainCamera() {
            return Instance.MainCamera;
        }
    }
}