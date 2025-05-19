using UnityEngine;

namespace Utils.Common
{
    public static class Helper
    {
        public static T GetComponent_Helper<T>(GameObject go) where T : Component
        {
            if(!go) { Debug.LogError("GameObject is null!"); return null; }

            if (go.TryGetComponent(out T component)) return component;
            Debug.LogError($"{typeof(T)} is not found in {go.name}!"); 
            return null;
        }

        public static T GetComponentInChildren_Helper<T>(GameObject go, bool includeInActive = false) where T : Component
        {
            if(!go) { Debug.LogError("GameObject is null!"); return null; }
            var component = go.GetComponentInChildren<T>(includeInActive);
            if(component) return component;
            Debug.LogError($"{typeof(T)} is not found in {go.name} and children of {go.name}!"); 
            return null;
        }
        
        public static T GetComponentInParent_Helper<T>(GameObject go, bool includeInActive = false) where T : Component
        {
            if(!go) { Debug.LogError("GameObject is null!"); return null; }
            var component = go.GetComponentInParent<T>(includeInActive);
            if(component) return component;
            Debug.LogError($"{typeof(T)} is not found in {go.name} and parents of {go.name}!"); 
            return null;
        }
    }
}