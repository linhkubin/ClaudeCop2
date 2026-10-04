using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using Unity.Mathematics;

namespace ClaudeCop.Camera.Editor
{
    /// <summary>Editor helper: dung spline tu cac Transform RailHint_*, va dung rig Cinemachine (Brain + Rail cam + PhaseDirector).</summary>
    public static class CameraRigBuilder
    {
        /// <summary>Tao GameObject chua SplineContainer (1 spline) di qua cac diem, theo thu tu ten. Knot dung Auto (Catmull-Rom).</summary>
        public static SplineContainer BuildSplineFromPoints(string name, IList<Transform> points, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var sc = go.AddComponent<SplineContainer>();
            var spline = sc.Spline;
            spline.Clear();
            foreach (var t in points)
            {
                var local = go.transform.InverseTransformPoint(t.position);
                spline.Add(new BezierKnot((float3)local), TangentMode.AutoSmooth);
            }
            return sc;
        }

        /// <summary>Lay cac Transform con (kieu hierarchy sau) co ten bat dau bang prefix, sap theo so cuoi ten.</summary>
        public static List<Transform> FindHints(Transform root, string prefix)
        {
            var list = new List<Transform>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith(prefix)) list.Add(t);
            list.Sort((a, b) => Number(a.name).CompareTo(Number(b.name)));
            return list;
        }

        static int Number(string n)
        {
            var m = Regex.Match(n, @"(\d+)$");
            return m.Success ? int.Parse(m.Value) : 0;
        }

        [MenuItem("ClaudeCop/Camera/Create Feel Profile Asset")]
        public static CameraFeelProfile CreateProfileAsset()
        {
            const string path = "Assets/_Game/Settings/CameraFeelProfile.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CameraFeelProfile>(path);
            if (existing != null) return existing;
            System.IO.Directory.CreateDirectory("Assets/_Game/Settings");
            var p = ScriptableObject.CreateInstance<CameraFeelProfile>();
            AssetDatabase.CreateAsset(p, path);
            AssetDatabase.SaveAssets();
            return p;
        }

        /// <summary>Dung rig trong scene hien tai: Main Camera + CinemachineBrain, RailCamera (SplineDolly), PhaseDirector.</summary>
        [MenuItem("ClaudeCop/Camera/Build Rig In Scene")]
        public static PhaseDirector BuildRig()
        {
            var profile = CreateProfileAsset();

            var brain = Object.FindFirstObjectByType<CinemachineBrain>();
            if (brain == null)
            {
                var cam = UnityEngine.Camera.main;
                GameObject camGo = cam != null ? cam.gameObject : new GameObject("Main Camera", typeof(UnityEngine.Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                brain = camGo.GetComponent<CinemachineBrain>() ?? camGo.AddComponent<CinemachineBrain>();
            }

            var rigRoot = GameObject.Find("CameraRig") ?? new GameObject("CameraRig");
            var rail = rigRoot.transform.Find("RailCamera");
            CinemachineCamera railCam;
            if (rail == null)
            {
                var g = new GameObject("RailCamera", typeof(CinemachineCamera), typeof(CinemachineSplineDolly));
                g.transform.SetParent(rigRoot.transform, false);
                railCam = g.GetComponent<CinemachineCamera>();
            }
            else railCam = rail.GetComponent<CinemachineCamera>();
            var dolly = railCam.GetComponent<CinemachineSplineDolly>();
            dolly.PositionUnits = PathIndexUnit.Distance;
            railCam.Priority.Enabled = true; railCam.Priority.Value = 0;
            if (railCam.GetComponent<CameraFeelApplier>() == null) railCam.gameObject.AddComponent<CameraFeelApplier>().profile = profile;

            var director = rigRoot.GetComponent<PhaseDirector>() ?? rigRoot.AddComponent<PhaseDirector>();
            director.profile = profile;
            director.brain = brain;
            director.railCamera = railCam;
            EditorUtility.SetDirty(director);
            return director;
        }
    }
}
