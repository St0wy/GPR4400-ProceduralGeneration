using MyBox;
using UnityEngine;

namespace ProcGen.Player
{
	public class CameraFollow : MonoBehaviour
	{
		[SerializeField] private Camera cam;
		[MustBeAssigned] [SerializeField] private Transform toFollow;

		private void Awake()
		{
			if (cam == null)
			{
				cam = Camera.main;
			}
		}

		private void Update()
		{
			Vector3 pos = toFollow.position;
			Transform trans = cam.transform;
			trans.position = new Vector3(pos.x, pos.y, trans.position.z);
		}
	}
}