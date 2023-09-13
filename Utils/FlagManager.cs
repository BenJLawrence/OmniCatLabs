using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace OmnicatLabs.Utils
{
    [System.Serializable]
    public class Flag   //Serializable class to hold data of a flag
    {
        public string name;
        public bool isSet = false;
        //this event is invoked when a specific flag is set
        public UnityEvent onThisFlagSet = new UnityEvent();
    }

    public class FlagManager : MonoBehaviour
    {
        public List<Flag> flags;
        //this event is will be invoked whenever any flag is set
        public UnityEvent onFlagSet = new UnityEvent();

        private void Start()
        {
            //way to initialize lists that is more consistent than doing it on the line the variable is declared
            if (flags == null)
            {
                flags = new List<Flag>();
            }
        }

        /// <summary>
        /// Sets a given flag which matches the passed name to the passed value
        /// </summary>
        /// <param name="name">Name of the flag</param>
        /// <param name="listener">Method that should listen for this particular flag being set.</param>
        /// <param name="value">Defaults to true if no argument given. T/F value</param>
        public void SetFlag(string name, UnityAction listener, bool value = true)
        {
            //lambda to find the flag that matches the name given
            Flag flag = flags.Find(flag => flag.name == name);
            if (flag == null)   //checks to throw an error when the name could not be found in the list
            {
                Debug.LogWarning("Flag: " + name + " not found");
                return;
            }

            //set the flag to on or off to match the given value
            flag.isSet = value;

            //in this version of the function we add the passed method as a listener of this specific event and immediately invoke it too since the flag has just been set
            flag.onThisFlagSet.AddListener(listener);
            flag.onThisFlagSet.Invoke();

            //invokes the general event that happens when any flag is set
            onFlagSet.Invoke();
        }

        /// <summary>
        /// Set a given flag which matches the passed name to the passed value
        /// </summary>
        /// <param name="name">Name of the flag</param>
        /// <param name="value">Defaults to true if no argument given. T/F value</param>
        public void SetFlag(string name, bool value = true) //same as the other version but with no method to add as a specific flags listener
        {
            Flag flag = flags.Find(flag => flag.name == name);
            if (flag == null)
            {
                Debug.LogWarning("Flag: " + name + " not found");
                return;
            }

            flag.isSet = value;

            onFlagSet.Invoke();
        }
    }
}
