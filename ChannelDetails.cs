using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An Attribute that designates a method as a receiver for the specified channel.
/// <para>Notes</para>
/// <list type="bullet">
/// <item>
/// <description>This Attribute must be the first Attribute on your method</description>
/// </item>
/// <item>
/// <description>
/// Your method can accept any number of parameters of any type.
/// <para>
/// However it will only receive messages that are sent with the exact same signature
/// </para>
/// </description>
/// </item>
/// <item>
/// <description>You can bind any amount of methods to the same channel, including methods of varying types and methods with the same signature</description>
/// </item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ChannelReceiver : Attribute {
    public string boundChannelName;

    public ChannelReceiver(string channelToBind) {
        boundChannelName = channelToBind;
    }
}

/// <summary>
/// A custom collection specifically for channel receivers. 
/// <para>Functions in a similar way to a HashSet wherein it only allows unique entries.</para>
/// </summary>
[System.Serializable]
public class ReceiverList : ICollection<Delegate> {
    private readonly List<Delegate> _innerHashSet = new List<Delegate>();

    public int Count => ((ICollection<Delegate>)_innerHashSet).Count;

    public bool IsReadOnly => ((ICollection<Delegate>)_innerHashSet).IsReadOnly;

    public bool Add(Delegate item) {
        if (!_innerHashSet.Contains(item)) {
            _innerHashSet.Add(item);
            return true;
        }

        return false;
    }

    public void Clear() {
        ((ICollection<Delegate>)_innerHashSet).Clear();
    }

    public bool Contains(Delegate item) {
        return ((ICollection<Delegate>)_innerHashSet).Contains(item);
    }

    public void CopyTo(Delegate[] array, int arrayIndex) {
        ((ICollection<Delegate>)_innerHashSet).CopyTo(array, arrayIndex);
    }

    public IEnumerator<Delegate> GetEnumerator() {
        return ((IEnumerable<Delegate>)_innerHashSet).GetEnumerator();
    }

    public bool Remove(Delegate item) {
        return ((ICollection<Delegate>)_innerHashSet).Remove(item);
    }

    void ICollection<Delegate>.Add(Delegate item) {
        Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)_innerHashSet).GetEnumerator();
    }
}

[System.Serializable]
public class Channel {
    /// <summary>
    /// The name of the channel that Senders and Receivers refer to
    /// </summary>
    public string name;

    [HideInInspector]
    /// <summary>
    /// Collection of receivers listening to this channel.
    /// </summary>
    public ReceiverList receivers = new ReceiverList();
}

[CustomEditor(typeof(ChannelHub))]
public class ChannelHubEditor : Editor {
    private SerializedProperty channelHubProperty;
    private ChannelHub channelHub;

    private void OnEnable() {
        ChannelHub[] instances = Resources.FindObjectsOfTypeAll<ChannelHub>();
        if (instances.Length > 1) {
            Debug.LogError("Multiple Channel Hubs are not supported. Ensure that there is only one Channel Hub in the project");
            for (int i = 1; i < instances.Length; i++) {
                DestroyImmediate(instances[i], true);
            }
        }
        channelHub = (ChannelHub)target;
        channelHubProperty = serializedObject.FindProperty("channels");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(channelHubProperty, true);

        if (GUILayout.Button("Add Channel")) {
            channelHub.channels.Add(new Channel() { name = "New Channel" });
        }

        if (GUILayout.Button("Remove Last Channel")) {
            if (channelHub.channels.Count > 0) {
                channelHub.channels.RemoveAt(channelHub.channels.Count - 1);
            }
        }


        if (GUILayout.Button("Validate Hub")) {
            ChannelHub[] instances = Resources.FindObjectsOfTypeAll<ChannelHub>();
            if (instances.Length > 1) {
                Debug.Log("Multiple Channel Hubs found. Removing extras.");
                for (int i = 1; i < instances.Length; i++) {
                    DestroyImmediate(instances[i], true);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(channelHub);
    }
}
