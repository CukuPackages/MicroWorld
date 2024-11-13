using UnityEngine;

namespace Cuku.MicroWorld
{
	[CreateAssetMenu(fileName = nameof(WorldSettings), menuName = "Settings/World")]
	public class WorldSettings : ScriptableObject
	{
        [SerializeField, Tooltip("Scale applied to the whole world.")]
        public float WorldScale = 1f;
    }
}