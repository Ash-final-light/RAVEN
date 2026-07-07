using System.Collections.Generic;
using UnityEngine;

namespace Tools.Profile.Data
{
    public class ProfilePoolTest:MonoBehaviour,IProfilePoolProvider
    {
        [SerializeField] private ProfilePool profilePool;
        public ProfilePool ProfilePool => profilePool;
        public List<ProfileActionValue> ProfileValues = new();
    }
}