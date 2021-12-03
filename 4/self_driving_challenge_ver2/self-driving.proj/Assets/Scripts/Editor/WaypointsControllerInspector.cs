// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WaypointsController))]
public class WaypointsControllerInspector : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var controller = target as WaypointsController;

        if(GUILayout.Button("Set")) {
            Clear(controller);
            Set(controller);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        if(GUILayout.Button("Clear")) {
            Clear(controller);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private void Set(WaypointsController controller) {
        controller.Prefab.gameObject.SetActive(false);

        var position = controller.transform.position;
        var forward = controller.transform.forward;
        var waypoints = new List<Waypoint>();
        var obstacles = new List<Obstacle>();

        for(int i = 0; i < controller.Size; i++) {
            var waypoint = Instantiate<Waypoint>(controller.Prefab);
            waypoint.gameObject.SetActive(true);
            waypoint.transform.parent = controller.transform;
            waypoint.transform.position = position + Vector3.up * controller.Height;

            waypoint.transform.rotation = Quaternion.LookRotation(forward);
            waypoint.SetPosition(i + 1, i == (controller.Size - 1));

            forward = ProjectOnSide(forward, waypoint.transform);

            waypoint.transform.rotation = Quaternion.LookRotation(forward);
            var (l, r) = waypoint.SetScaleAndRotation();

            var distance = controller.Distance;
            RaycastHit hit;
            while(Physics.BoxCast(waypoint.transform.position, Vector3.one / 2, forward, out hit, waypoint.transform.rotation, distance, LayerMask.GetMask("Wall"))) {
                distance--;
            }

            position = waypoint.transform.position + (forward * distance);

            waypoints.Add(waypoint);

            if(controller.ObstaclePrefab != null) {
                if(i > 0 && (i % controller.ObstacleRate) == 0) {
                    var o = CreateObstacle(controller, waypoint, l, r);
                    o.SetBy(controller.MinScale, controller.MaxScale, controller.MinForce, controller.MaxForce);
                    obstacles.Add(o);

                    var firePosition = waypoints.Count + controller.ObstacleFirePosition;
                    if(firePosition >= 0) {
                        var observer = waypoints[firePosition].gameObject.AddComponent<TriggerObserver>();
                        observer.TargetObstacle = o;
                    }
                }
            }
        }

        obstacles.ForEach(o => Array.ForEach(o.GetComponentsInChildren<Collider>(), c => c.enabled = true));
    }

    private void Clear(WaypointsController controller) {
        var list = new List<GameObject>();
        var components = controller.GetComponentsInChildren<Transform>();
        foreach(var c in components) {
            if(c == controller.Prefab.transform) continue;
            if(c == controller.transform) continue;
            list.Add(c.gameObject);
        }
        list.ForEach(o => DestroyImmediate(o));
    }

    private Vector3 ProjectOnSide(Vector3 forward, Transform transform) {
        var distance = 0.0f;
        var temp = forward;

        RaycastHit hit;

        if(Physics.Raycast(transform.position, -transform.right, out hit, float.MaxValue, LayerMask.GetMask("Wall"))) {
            distance = hit.distance;
            temp = Vector3.ProjectOnPlane(forward, hit.normal);
        }

        if(Physics.Raycast(transform.position, transform.right, out hit, float.MaxValue, LayerMask.GetMask("Wall"))) {
            if(distance == 0 || hit.distance < distance) {
                temp = Vector3.ProjectOnPlane(forward, hit.normal);
            }
        }

        temp = new Vector3(temp.x, 0, temp.z);
        temp.Normalize();
        return temp;
    }

    private Obstacle CreateObstacle(WaypointsController controller, Waypoint waypoint, Vector3 left, Vector3 right) {
        var obstacle = Instantiate(controller.ObstaclePrefab);
        obstacle.transform.SetParent(controller.transform);
        obstacle.transform.position = waypoint.transform.position - waypoint.transform.up;
        obstacle.transform.rotation = waypoint.transform.rotation;
        obstacle.SetSide(left, right);
        Array.ForEach(obstacle.GetComponentsInChildren<Collider>(), c => c.enabled = false);
        return obstacle;
    }
}
