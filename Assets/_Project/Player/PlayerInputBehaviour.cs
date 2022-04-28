using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProcGen.Player
{
	public class PlayerInputBehaviour : MonoBehaviour
	{
		public Vector2 Movement { get; private set; }

		[UsedImplicitly]
		private void OnMove(InputValue value)
		{
			Movement = value.Get<Vector2>();
		}
	}
}
