using System;
using UnityEngine;

namespace ProcGen
{
	[Serializable]
	public struct Size
	{
		public Size(int width, int height)
		{
			Width = width;
			Height = height;
		}

		[field: SerializeField] public int Width { get; set; }
		[field: SerializeField] public int Height { get; set; }
	}
}