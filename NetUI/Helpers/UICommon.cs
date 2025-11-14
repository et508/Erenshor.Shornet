using System.Collections.Generic;
using UnityEngine;

namespace ShorNet
{
    public static class UICommon
    {
        public static Transform Find(GameObject root, string path)
        {
            return root != null ? root.transform.Find(path) : null;
        }
    }
}