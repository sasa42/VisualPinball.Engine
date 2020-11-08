// Visual Pinball Engine
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
	public class GateApi : ItemApi<Engine.VPT.Gate.Gate, Engine.VPT.Gate.GateData>, IApiInitializable,
		IApiHittable, IApiRotatable, IApiSwitch, IColliderGenerator
	{
		/// <summary>
		/// Event emitted when the table is started.
		/// </summary>
		public event EventHandler Init;

		/// <summary>
		/// Event emitted when the ball hits the gate.
		/// </summary>
		///
		/// <remarks>
		/// For two-way gates, this is emitted twice, once when entering, and
		/// once when leaving. For one-way gates, it's emitted once when the
		/// ball rolls through it, but not when the gate blocks the ball. <p/>
		///
		/// Also note that the gate must be collidable.
		/// </remarks>
		public event EventHandler Hit;

		/// <summary>
		/// Event emitted when the gate passes its parked position. Only
		/// emitted for one-way gates.
		/// </summary>
		///
		/// <remarks>
		/// Can be emitted multiple times, as the gate bounces a few times
		/// before coming to a rest.<p/>
		///
		/// Note that the gate must be collidable.
		/// </remarks>
		public event EventHandler<RotationEventArgs> LimitBos;

		/// <summary>
		/// Event emitted when the gate rotates to its top position.
		/// </summary>
		///
		/// <remarks>
		/// The gate must be collidable.
		/// </remarks>
		public event EventHandler<RotationEventArgs> LimitEos;

		// todo
		public event EventHandler Timer;


		public GateApi(Engine.VPT.Gate.Gate item, Entity entity, Entity parentEntity, Player player) : base(item, entity, parentEntity, player)
		{
		}

		void IApiSwitch.AddSwitchId(string switchId, int pulseDelay) => AddSwitchId(switchId, Item.IsPulseSwitch, pulseDelay);

		#region Collider Generation

		internal override bool IsColliderEnabled => Data.IsCollidable;
		internal override PhysicsMaterialData GetPhysicsMaterial(Table table)=> new PhysicsMaterialData {
			Elasticity = Data.Elasticity,
			ElasticityFalloff = 0,
			Friction = Data.Friction,
			ScatterAngleRad = 0
		};

		void IColliderGenerator.CreateColliders(Table table, List<ICollider> colliders, ref int nextColliderId)
		{
			var colliderGenerator = new GateColliderGenerator(this);
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
			OnSwitch(true);
		}

		void IApiRotatable.OnRotate(float speed, bool direction)
		{
			if (direction) {
				LimitEos?.Invoke(this, new RotationEventArgs { AngleSpeed = speed });
			} else {
				LimitBos?.Invoke(this, new RotationEventArgs { AngleSpeed = speed });
			}
		}

		#endregion
	}
}
