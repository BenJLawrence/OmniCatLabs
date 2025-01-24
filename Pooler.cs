using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Pooler", menuName = "Pooling/Pooler", order = 1)]
public class Pooler : ScriptableObject
{
    [Serializable]
    public sealed class PrefabEntry
    {
        public string Name;
        public GameObject Prefab;
    }

    [SerializeField]
    public List<PrefabEntry> prefabPools = new List<PrefabEntry>();
    private static Pooler _instance;
    public readonly Dictionary<string, GameObjectPoolWrapper> originPrefabMap = new Dictionary<string, GameObjectPoolWrapper>();

    public static Pooler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.FindObjectsOfTypeAll<Pooler>().FirstOrDefault();
                if (_instance == null)
                {
                    Debug.LogError("No Pooler was found in the project. Ensure that a Pooler exists in some location of the project files before continuing");
                }
                _instance.prefabPools.ForEach(entry =>
                {
                    GameObjectPoolWrapper wrappedPrefab = new GameObjectPoolWrapper(entry.Prefab);
                    _instance.originPrefabMap.Add(entry.Name, wrappedPrefab);
                    _instance.pools.Add(wrappedPrefab, new ObjectPool<GameObjectPoolWrapper>(10, wrappedPrefab));
                });
            }
            return _instance;
        }
    }

    //public Dictionary<Type, IPool> pools = new Dictionary<Type, IPool>();
    public Dictionary<object, IPool> pools = new Dictionary<object, IPool>();
}

[CustomEditor(typeof(Pooler))]
public class PoolerEditor : Editor
{
    private SerializedProperty pools;
    private Pooler pooler;

    private void OnEnable()
    {
        pooler = (Pooler)target;
        pools = serializedObject.FindProperty("prefabPools");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(pools, true);

        serializedObject.ApplyModifiedProperties();
    }
}
