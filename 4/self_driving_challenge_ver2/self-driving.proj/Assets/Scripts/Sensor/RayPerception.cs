// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using UnityEngine;
 
public class RayPerception : MonoBehaviour
{
    public enum CastType
    {
        Ray = 0,
        Sphere,
    }
 
//    private Color HitColor { get; } = new Color(1, 0, 0, 0.5f);
//    private Color NoHitoColor { get; } = new Color(0, 1, 0, 0.5f);
 
    private Dictionary<Type, object> ListContainer { get; } = new Dictionary<Type, object>();
 
    /// <summary>
    /// 衝突距離の取得
    /// </summary>
    /// <param name="distance">距離</param>
    /// <param name="angles">角度</param>
    /// <param name="layer">衝突レイヤー</param>
    /// <param name="isNormalized">正規化指定</param>
    /// <param name="cast">キャスト指定</param>
    /// <returns>角度に対応した衝突距離のリスト</returns>
    public List<double> Perceive(float distance, float[] angles, int layer, bool isNormalized, CastType cast = CastType.Ray) {
        var results = Perceive<double>(distance, angles, layer, cast, (hit, b) => {
            if(b) {
                return isNormalized ? hit.distance / distance : hit.distance;
            }
 
            return isNormalized ? 1 : distance;
        });
 
        return results;
    }
 
    /// <summary>
    /// 衝突座標の取得
    /// </summary>
    /// <param name="distance">距離</param>
    /// <param name="angles">角度</param>
    /// <param name="layer">衝突レイヤー</param>
    /// <param name="cast">キャスト指定</param>
    /// <returns>角度に対応した衝突座標のリスト</returns>
    public List<Vector3> PerceivePosition(float distance, float[] angles, int layer, CastType cast = CastType.Ray) {
        var results = Perceive<Vector3>(distance, angles, layer, cast, (hit, b) => {
            return b ? transform.InverseTransformPoint(hit.point) : Vector3.zero;
        });
 
        return results;
    }
 
    public List<T> Perceive<T>(float distance, float[] angles, int layer, CastType cast, Func<RaycastHit, bool, T> onHit) {
        var type = typeof(T);
        if(!ListContainer.ContainsKey(type)) {
            ListContainer.Add(type, new List<T>());
        }
 
        var buffer = ListContainer[type] as List<T>;
        buffer.Clear();
 
        var start = transform.position;
        for(int i = 0; i < angles.Length; i++) {
            var angle = angles[i];
            var direction = transform.TransformDirection(PolarToCartesian(angle));
 
            RaycastHit hit;
            var result = false;
            if(cast == CastType.Ray) {
                result = Physics.Raycast(start, direction, out hit, distance, 1 << layer);
            }
            else {
                result = Physics.SphereCast(start, 0.5f, direction, out hit, distance, 1 << layer);
            }
 
            if(result) {
                buffer.Add(onHit(hit, true));
#if UNITY_EDITOR
                Debug.DrawRay(start, direction * hit.distance, new Color(1, 0, 0, 0.5f), 0.01f, true);
#endif
            }
            else {
                buffer.Add(onHit(hit, false));
#if UNITY_EDITOR
                Debug.DrawRay(start, direction * distance, new Color(0, 1, 0, 0.5f), 0.01f, true);
#endif
            }
        }
        return buffer;
    }
 
    /// <summary>
    /// 円周上の座標を取得
    /// </summary>
    /// <param name="angle">角度</param>
    /// <returns>指定した角度に対する円周上の座標</returns>
    public static Vector3 PolarToCartesian(float angle) {
        float x = Mathf.Cos(Mathf.Deg2Rad * angle);
        float z = Mathf.Sin(Mathf.Deg2Rad * angle);
        return new Vector3(x, 0f, z);
    }
}