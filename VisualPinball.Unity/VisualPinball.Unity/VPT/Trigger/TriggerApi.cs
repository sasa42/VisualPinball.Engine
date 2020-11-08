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
	public class TriggerApi : ItemApi<Engine.VPT.Trigger.Trigger, Engine.VPT.Trigger.TriggerData>,
		IApiInitializable, IApiHittable, IApiSwitch, IColliderGenerator
	{
		/// <summary>
		/// Event emitted when the table is started.
		/// </summary>
		public event EventHandler Init;

		/// <summary>
		/// Event emitted when the ball glides on the trigger.
		/// </summary>
		public event EventHandler Hit;

		/// <summary>
		/// Event emitted when the ball leaves the trigger.
		/// </summary>
		public event EventHandler UnHit;

		internal TriggerApi(Engine.VPT.Trigger.Trigger item, Entity entity, Entity parentEntity, Player player) : base(item, entity, parentEntity, player)
		{
		}

		void IApiSwitch.AddSwitchId(string switchId, int pulseDelay) => AddSwitchId(switchId, Item.IsPulseSwitch, pulseDelay);

		#region Collider Generation

		internal override bool FireHitEvents { get; } = true;

		void IColliderGenerator.CreateColliders(Table table, List<ICollider> colliders, ref int nextColliderId)
		{
			var colliderGenerator = new TriggerColliderGenerator(this);
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

		void IApiHittable.OnHit(bool isUnHit)
		{
			if (isUnHit) {
				UnHit?.Invoke(this, EventArgs.Empty);
				OnSwitch(false);

			} else {
				Hit?.Invoke(this, EventArgs.Empty);
				OnSwitch(true);
			}
		}

		#endregion
	}
}
