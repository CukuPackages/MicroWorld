using UnityEngine;
using System.Reflection;

namespace Cuku.MicroWorld
{
    public static class Unity
    {
        public static T CopyTo<T>(this T original, GameObject destination) where T : Component
        {
            T copy = destination.GetComponent<T>();
            if (copy == null)
                copy = destination.AddComponent<T>();

            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                field.SetValue(copy, field.GetValue(original));

            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                if (property.CanWrite)
                {
                    // Handle specific case for Renderer properties
                    if (typeof(T) == typeof(MeshRenderer) && (property.Name == "material" || property.Name == "materials"))
                    {
                        // In edit mode, use sharedMaterial and sharedMaterials to avoid material duplication
                        if (!Application.isPlaying)
                        {
                            if (property.Name == "material")
                                property.SetValue(copy, ((MeshRenderer)(object)original).sharedMaterial);
                            else if (property.Name == "materials")
                                property.SetValue(copy, ((MeshRenderer)(object)original).sharedMaterials);
                        }
                        else
                            property.SetValue(copy, property.GetValue(original));
                    }
                    else
                        property.SetValue(copy, property.GetValue(original));
                }
            return copy;
        }
    }
}