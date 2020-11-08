﻿// Visual Pinball Engine
// Copyright (C) 2020 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Unity.Entities;
using VisualPinball.Engine.VPT.Table;

namespace VisualPinball.Unity
{
	public class SurfaceApi : ItemApi<Engine.VPT.Surface.Surface, Engine.VPT.Surface.SurfaceData>,
		IApiInitializable, IApiHittable, IApiSlingshot, IColliderGenerator
	{
		/// <summary>
		/// Event emitted when the table is started.
		/// </summary>
		public event EventHandler Init;

		/// <summary>
		/// Event emitted when the ball hits the surface.
		/// </summary>
		public event EventHandler Hit;

		/// <summary>
		/// Event emitted when a slingshot segment was hit.
		/// </summary>
		public event EventHandler Slingshot;

		internal SurfaceApi(Engine.VPT.Surface.Surface item, Entity entity, Entity parentEntity, Player player) : base(item, entity, parentEntity, player)
		{
		}
		#region Collider Generation

		internal override bool FireHitEvents { get; } = true;
		internal override float HitThreshold => Data.Threshold;

		void IColliderGenerator.CreateColliders(Table table, List<ICollider> colliders, ref int nextColliderId)
		{
			var colliderGenerator = new SurfaceColliderGenerator(this);
			colliderGenerator.GenerateColliders(table, colliders, ref nextColliderId);
		}

		ColliderInfo IColliderGenerator.GetNextColliderInfo(Table table, ref int nextColliderId) =>
			GetNextColliderInfo(table, ref nextColliderId);

		#endregion

		#region Events

		void IApiInitializable.OnInit(BallManager ballManager)
		{
			Init?.Invoke(this, EventArgs.Empty);
		}

		void IApiHittable.OnHit(bool _)
		{
			Hit?.Invoke(this, EventArgs.Empty);
		}

		public void OnSlingshot()
		{
			Slingshot?.Invoke(this, EventArgs.Empty);
		}

		#endregion
	}
}
