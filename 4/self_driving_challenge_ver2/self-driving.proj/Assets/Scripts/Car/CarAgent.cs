// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class CarAgent : Agent
{
    [SerializeField] private int currentStep = 0;
    private int CurrentStep { get { return currentStep; } set { currentStep = value; } }

    [SerializeField] private int currentStepMax = 5000;
    private int CurrentStepMax => currentStepMax;

    [SerializeField] private int localStep = 0;
    private int LocalStep { get { return localStep; } set { localStep = value; } }

    [SerializeField] private int localStepMax = 200;
    private int LocalStepMax => localStepMax;

    [SerializeField] private bool allowPlusReward = true;
    private bool AllowPlusReward => allowPlusReward;

    [SerializeField] private bool isLearning = true;
    private bool IsLearning => isLearning;

    private Sensor[] Sensors { get; set; }
    private CarController Controller { get; set; }
    private Rigidbody CarRb { get; set; }
    private float IndReward = 0;
    private float minDistance = 0;
    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }
    private Vector3 LastPosition { get; set; }
    private Vector3 LastVelocity { get; set;}
    private Vector3 LastAcceleration { get; set; }
    private Vector3 Acceleration {get; set;}
    private Vector3 JerkAccum;
    private int bumpCount = 0;
    private int bumperCount = 0;
    private double impact = 0.0;
    private float jerkScore = 0.0f;
    private float currentReward = 0.0f;
    private float TotalDistance { get; set; }
    private int WaypointIndex { get; set; }
    private float maximpact = 0.0f;

    private GameObject[] walls;

    private void Awake() {
        CarRb = GetComponent<Rigidbody>();
        Controller = GetComponent<CarController>();
        Sensors = GetComponentsInChildren<Sensor>();
    }

    public void Start() {
        StartPosition = transform.position;
        StartRotation = transform.rotation;
        CurrentStep = 0;
        LocalStep = 0;
        LastPosition = StartPosition;
        TotalDistance = 0;
        bumpCount = 0;
    }

    public override void AgentReset() {
        transform.position = StartPosition;
        transform.rotation = StartRotation;

        Controller.GasInput = 0;
        Controller.SteerInput = 0;
        Controller.BrakeInput = 0;

        gameObject.SetActive(false);
        gameObject.SetActive(true);

        CurrentStep = 0;
        LocalStep = 0;
        TotalDistance = 0;
        bumpCount = 0;
        bumperCount = 0;
        LastPosition = StartPosition;
        LastVelocity = new Vector3(0.0f,0.0f,0.0f);
        LastAcceleration = new Vector3(0.0f,0.0f,0.0f);
        JerkAccum = new Vector3(0.0f,0.0f,0.0f);

        WaypointIndex = 0;
    }

    public override int GetState() {
        var stateDivide = 3;//11
        var results = new List<double>();
        var r = 0;
        Array.ForEach(Sensors, sensor => {
            results.AddRange(sensor.Hits());
        });
        for(int i = 0; i < results.Count; i++) {
            var v = Mathf.FloorToInt(Mathf.Lerp(0, stateDivide - 1, (float)results[i]));
            if(results[i] >= 0.99f) {
                v = stateDivide - 1;
            }
            r += (int)(v * Mathf.Pow(stateDivide, i));
        }
        var numStates = (int)Mathf.Pow(stateDivide, results.Count);
        int n;
        if(CarRb.velocity.magnitude < 10) { n = 0; }
        else if(CarRb.velocity.magnitude < 15) { n = 1; }
        else { n = 2; }
        r += numStates * n;
        return r;
    }

    public override List<double> CollectObservations() {
        // センサーの距離をリストに追加する
        var results = new List<double>();
        Array.ForEach(Sensors, sensor => {
            results.AddRange(sensor.Hits());
        });
        Vector3 local_v = CarRb.transform.InverseTransformDirection(CarRb.velocity);
        results.Add(local_v.x / 5.0f);
        results.Add(local_v.z / 5.0f);
        return results;
    }

    public override double[] ActionNumberToVectorAction(int ActionNumber) {
        var action = new double[3];
        var steering = 0.0d;
        var braking = 0.0d;
        if(ActionNumber % 6 == 1) {
            steering = 1d;
        }
        else if(ActionNumber % 6 == 2) {
            steering = -1d;
        }
        else if(ActionNumber % 6 == 3) {
            steering = 0.5d;
        }
        else if(ActionNumber % 6 == 4) {
            steering = -0.5d;
        }
        else if(ActionNumber % 6 == 5) {
            braking = 0.5d;
        }

        var gasInput = 0.5d;
        action[0] = steering;
        action[1] = gasInput;
        action[2] = braking;
        return action;
    }

    public override void AgentAction(double[] vectorAction) {
        CurrentStep++;
        LocalStep++;
        TotalDistance += (transform.position - LastPosition).magnitude;
        
        Acceleration = CarRb.velocity-LastVelocity;

        JerkAccum += Acceleration - LastAcceleration;

        //if ((Acceleration - LastAcceleration).magnitude > 0.3f){
            //Debug.Log((Acceleration - LastAcceleration).magnitude);
        //}
        //walls = GameObject.FindGameObjectsWithTag("wall");

        //minDistance = GetClosestWall(walls);

        if (CurrentStep % 10 == 0){
            minDistance = findClosestWall();
            if (minDistance > 5){
                minDistance = 5;
            } else if (minDistance < 0){
                minDistance = -1.0f * (minDistance * minDistance);
            }
            currentReward += (minDistance / 5.0f) * (CarRb.velocity.magnitude / 13.0f);
        }

        if (CurrentStep % 50 == 0){
            if (JerkAccum.magnitude > 0.1f){
                bumpCount++;
            }
            if (JerkAccum.magnitude > 0.15f){
                //Debug.Log("Bumpy ride! : " + currentStep.ToString());
            }
            IndReward += currentReward;
            currentReward = 0.0f;
            JerkAccum = new Vector3(0.0f,0.0f,0.0f);
        }

        if (this.transform.GetChild(2).transform.childCount > 1){
            Destroy(this.transform.GetChild(2).transform.GetChild(1).gameObject);
            Destroy(this.transform.GetChild(3).transform.GetChild(1).gameObject);
            Destroy(this.transform.GetChild(4).transform.GetChild(1).gameObject);
            Destroy(this.transform.GetChild(5).transform.GetChild(1).gameObject);
        }

        //Debug.Log(minDistance.ToString() + " " + IndReward.ToString());

        //IndReward += GetClosestWall(walls)/8.0f;

        if(IsLearning) {
            if(CurrentStep > CurrentStepMax) { 
                //Debug.Log("CurrentStep Maxed");
                DoneWithReward(TotalDistance); //#####
                //DoneWithReward(IndReward);
                return;
            }

            if(LocalStep > LocalStepMax) {
                //Debug.Log("LocalStep Maxed");
                DoneWithReward(-1.0f/TotalDistance); //#####
                //DoneWithReward(IndReward - 200.0f);
                return;
            }
        }

        var steering = Mathf.Clamp((float)vectorAction[0], -1.0f, 1.0f);
        var gasInput = Mathf.Clamp((float)vectorAction[1], 0.5f, 1.0f);
        var braking = Mathf.Clamp((float)vectorAction[2], 0.0f, 0.5f);

        Controller.SteerInput = steering;
        Controller.GasInput = gasInput;
        Controller.BrakeInput = braking;

        //Debug.Log(braking);

        if (braking > 0.1f){
            this.transform.GetChild(1).GetComponent<Renderer>().materials[3].EnableKeyword("_EMISSION");
        } else {
            this.transform.GetChild(1).GetComponent<Renderer>().materials[3].DisableKeyword("_EMISSION");
        }

        LastPosition = transform.position;
        LastVelocity = CarRb.velocity;
        LastAcceleration = Acceleration;
    }

    /// <summary>
    /// 衝突時に呼び出されるコールバック 
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionEnter(Collision collision) {
        walls = GameObject.FindGameObjectsWithTag("wall");
        var collisionDir = collision.contacts[0].point-CarRb.position;
        //Debug.Log("Hit Distance: " + findClosestWall());// +  GetClosestWall(walls));
        impact = Math.Abs(Math.Sin(Vector3.Angle(collisionDir,CarRb.velocity)*(3.14159265/180.0)))*CarRb.velocity.magnitude;
        //Debug.Log(maximpact);
        if(collision.gameObject.tag == "wall") {
            //DoneWithReward(-1.0f * (float)impact / TotalDistance);
            //DoneWithReward(-1.0f / TotalDistance);
            //DoneWithReward(IndReward);
            DoneWithReward(-1.0f / TotalDistance);
            //Debug.Log("Collision");
            //Debug.Log("Collision! :" + impact.ToString());
            //DoneWithReward(IndReward - 200.0f);// - (float)impact * 100.0f); //#####
        }
    }

    public void OnTriggerEnter(Collider other) {

        var waypoint = other.GetComponent<Waypoint>();
        if(waypoint == null) {
            return;
        }

        if(WaypointIndex >= waypoint.Index) {
            SetReward(-1.0f / TotalDistance);
            return;
        }
        
        WaypointIndex = waypoint.Index;
        if(waypoint.IsLast) {
            WaypointIndex = 0;
        }
        LocalStep = 0;
    }

    public override void Stop() {
        CarRb.velocity = Vector3.zero;
        CarRb.angularVelocity = Vector3.zero;
        Controller.Stop();
    }

    private void DoneWithReward(float reward) {
        if(bumpCount/TotalDistance > 0.1){
           reward -= -Mathf.NegativeInfinity;
        }
        try
        {
            File.AppendAllLines("/Users/masayukifujita/file.txt", new[] { TotalDistance.ToString() + ", " + reward.ToString() + ", " +bumpCount.ToString() + "," + impact.ToString() });
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Executing finally block.");
        }
        //Debug.Log(reward);
        //---------------------------------------------
        //---------------------------------------------
        IndReward = 0;
        if(reward > 0 && !AllowPlusReward) {
            reward = 0;
        }
        SetReward(reward);
        Done();
    }

    float findClosestWall(){
        Vector3 currentPos = CarRb.transform.position;
        //for (i = 0; i < 1000; i++){
        float minDist = 0.0f;
        Collider[] hitColliders;
        int breakFlag = 0;
        for (int j = 0; j < 1000; j++){
            minDist += 0.1f;
            float centerDist = 0.0f;
            //Center
            for (int k = 0; k < 6; k++){
                //Debug.Log(centerDist);
                hitColliders = Physics.OverlapSphere(currentPos + centerDist * CarRb.transform.forward, minDist/(float)(Math.Pow(2.0f, k)));
                for (int i = 0; i < hitColliders.Length; i++){
                    if (hitColliders[i].tag.Contains("wall")){
                        breakFlag = 1;
                        break;
                    }
                }
                centerDist += 1.0f/(float)(Math.Pow(2.0f, k));
            }

            if(breakFlag == 1){
                break;
            }
        }
        //}
        return minDist - 2.5f;

    }

    /*float GetClosestWall(GameObject[] walls) {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = CarRb.transform.position;
        Vector3 wallCenterPos;
        Vector3 tMinWallCenterPos = new Vector3(0.0f,0.0f,0.0f);
        Mesh mesh;
        Vector3[] vertices;
        float dist;
        foreach (GameObject t in walls)
        {
            //Vector3 closestPoint = GetComponent<Collider>().ClosestPoint(t.transform.position);
            //Vector3 toCenterVector = t.transform.position - closestPoint;
            //float wallAngle = (float)Math.Abs(Math.Sin(Vector3.Angle(t.transform.position - currentPos, t.transform.forward)*(3.14159265/180.0)));
            //float dist = Vector3.Distance(closestPoint, t.transform.position) * wallAngle;
            //Debug.Log("Distance:" + Math.Abs(Math.Cos(Vector3.Angle(toCenterVector, t.transform.forward))).ToString());
            if (t.name.Contains("Rock")){
                wallCenterPos = t.transform.position; //Rockなら中央
            } else {
                Vector3 averageVec = new Vector3(0.0f,0.0f,0.0f); //壁の中心を探す
                mesh = t.GetComponent<MeshFilter>().mesh;
                vertices = mesh.vertices;
                for (var i = 0; i < vertices.Length; i++){
                    averageVec += vertices[i];
                }
                averageVec /= vertices.Length;
                //Debug.Log(t.transform.parent.name + " " + t.name + " avg: " + (t.transform.position + t.transform.rotation*averageVec).ToString());
                wallCenterPos = t.transform.position + t.transform.rotation*averageVec;
            }
            dist = Vector3.Distance(currentPos, wallCenterPos);

            if (dist < minDist)
            {
                tMin = t;
                tMinWallCenterPos = wallCenterPos;
                minDist = dist;
            }
        }
        //Debug.Log("Distance: " + 
        //    (minDist*Math.Abs(Math.Cos(Vector3.Angle(tMinWallCenterPos - currentPos,tMin.transform.forward))*(3.1415f/180.0f))));  
        float theAngle = Vector3.Angle(tMinWallCenterPos - currentPos, tMin.transform.forward);
    // Debug.Log(minDist * Math.Abs(Math.Cos(theAngle*(3.141592f/180.0f))));

        // tMin.transform.forward)*(3.1415f/180.0f)))).ToString() );
        //Debug.Log(closestWallPoint);
        //Debug.Log("car size:" + Vector3.Distance(closestPoint, currentPos));
    // Debug.Log(tMin.transform.parent.name + " " + tMin.transform.name);

        return minDist * (float)Math.Abs(Math.Cos(theAngle*(3.141592f/180.0f))) - 2.5f;
    }*/
}
