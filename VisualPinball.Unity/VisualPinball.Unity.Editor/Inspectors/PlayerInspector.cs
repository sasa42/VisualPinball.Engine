﻿using UnityEditor;
using UnityEngine;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Ball;
using VisualPinball.Engine.VPT.Table;
using Player = VisualPinball.Unity.Game.Player;

namespace VisualPinball.Unity.Editor.Inspectors
{
	[CustomEditor(typeof(Player))]
	[CanEditMultipleObjects]
	public class PlayerInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			if (EditorApplication.isPlaying) {
				if (GUILayout.Button("Spawn Ball")) {
					var player = (Player) target;
					player.CreateBall(new DebugBallCreator());
				}
			}
		}
	}

	internal class DebugBallCreator : IBallCreationPosition
	{
		public Vertex3D GetBallCreationPosition(Table table)
		{
			return new Vertex3D(table.Width / 3f, table.Height / 5f, 10f);
		}

		public Vertex3D GetBallCreationVelocity(Table table)
		{
			// no velocity
			return Vertex3D.Zero;
		}

		public void OnBallCreated(PlayerPhysics physics, Ball ball)
		{
			// nothing to do
		}
	}
}