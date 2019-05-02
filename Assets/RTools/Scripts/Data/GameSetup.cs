using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTools
{
    public class GameSetup : ScriptableData<GameSetup>
    {

        public string appID;
        public string appName;
        public string appVersion;
        public int appVersionCode;
        public string companyName;

        public Texture2D icon;
        public Texture2D roundIcon;
        public Texture2D adaptiveIconForeground;
        public Texture2D adaptiveIconBackground;
    }
}
