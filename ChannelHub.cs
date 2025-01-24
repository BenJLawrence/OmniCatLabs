using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ChannelHub", menuName = "Channels/ChannelHub", order = 1)]
public class ChannelHub : ScriptableObject {
    private static ChannelHub _instance;

    public static ChannelHub Instance {
        get {
            if (_instance == null) {
                _instance = Resources.FindObjectsOfTypeAll<ChannelHub>().FirstOrDefault();
                if (_instance == null) {
                    Debug.LogError("No Channel Hub was found in the project. Ensure that a Channel Hub exists in some location of the project files before continuing");
                }
            }
            return _instance;
        }
    }

    public List<Channel> channels = new List<Channel>();
}
