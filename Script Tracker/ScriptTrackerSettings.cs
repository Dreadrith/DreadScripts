#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace DreadScripts.ScriptTracker
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Tracker Settings", menuName = "DreadScripts/Script Tracker Settings")]
    internal sealed class ScriptTrackerSettings : ScriptableObject
    {
        public List<string> HighRisk;
        public List<string> NormalRisk;
        public List<string> LowRisk;
        public bool promptDLL;
        public bool alwaysDLL;

        ScriptTrackerSettings()
        {
            HighRisk = new List<string>(){
            "UnitypackageRussianRoulett",
            "System.Security",
            "Environment.SpecialFolder.DesktopDirectory",
            "GenerateRandomSalt",
            "Cryptography",
            "DownloadRansomware",
            "ScriptTracker",
            "ransomware",
            "discord"
            };

            NormalRisk = new List<string>(){
            "Networking",
            "System.Net",
            "UnityWebRequest",
            "WebClient",
            "http"
            };
            LowRisk = new List<string>()
            {

            };
            promptDLL = true;
            alwaysDLL = false;
        }
    
    }
}
#endif