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

using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.VPT;
using VisualPinball.Engine.VPT.Table;

namespace VisualPinball.Unity
{
	public abstract class ItemApi<TItem, TData> : IApi where TItem : Item<TData> where TData : ItemData
	{
		public string Name => Item.Name;

		public readonly TItem Item;
		internal readonly Entity Entity;
		internal readonly Entity ParentEntity;

		internal TData Data => Item.Data;
		protected Table Table => _player.Table;

		protected readonly EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		internal VisualPinballSimulationSystemGroup SimulationSystemGroup => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<VisualPinballSimulationSystemGroup>();

		private readonly Player _player;

		protected ItemApi(TItem item, Entity entity, Entity parentEntity, Player player)
		{
			Item = item;
			Entity = entity;
			ParentEntity = parentEntity;
			_player = player;
			_gamelogicEngineWithSwitches = (IGamelogicEngineWithSwitches)player.GameEngine;
		}

		#region Collider

		internal virtual bool IsColliderEnabled  => Data is IPhysicalData physicalData && physicalData.GetIsCollidable();
		internal virtual bool FireHitEvents { get; } = false;
		internal virtual float HitThreshold { get; } = 0;
		internal virtual PhysicsMaterialData GetPhysicsMaterial(Table table)
		{
			if (Data is IPhysicalData physicalData) {
				var mat = table.GetMaterial(physicalData.GetPhysicsMaterial());
				var matData = new PhysicsMaterialData();
				if (mat != null && !physicalData.GetOverwritePhysics()) {
					matData.Elasticity = mat.Elasticity;
					matData.ElasticityFalloff = mat.ElasticityFalloff;
					matData.Friction = mat.Friction;
					matData.ScatterAngleRad = math.radians(mat.ScatterAngle);

				} else {
					matData.Elasticity = physicalData.GetElasticity();
					matData.ElasticityFalloff = physicalData.GetElasticityFalloff();
					matData.Friction = physicalData.GetFriction();
					matData.ScatterAngleRad = math.radians(physicalData.GetScatter());
				}
				return matData;
			}
			return default;
		}

		/// <summary>
		/// Returns returns collider info passed when creating the collider.
		///
		/// Use this for colliders that are part of the quad tree.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="nextColliderId">Reference to collider index</param>
		internal ColliderInfo GetNextColliderInfo(Table table, ref int nextColliderId)
		{
			var id = nextColliderId++;
			return GetColliderInfo(table, id);
		}

		/// <summary>
		/// Returns collider info.
		///
		/// Use this for colliders that are part of another collider and are
		/// not in the quad tree.
		/// </summary>
		/// <param name="table"></param>
		internal ColliderInfo GetColliderInfo(Table table)
		{
			return GetColliderInfo(table, -1);
		}

		private ColliderInfo GetColliderInfo(Table table, int id)
		{
			return new ColliderInfo {
				Id = id,
				ItemType = Item.ItemType,
				Entity = Entity,
				ParentEntity = ParentEntity,
				FireEvents = FireHitEvents,
				IsEnabled = IsColliderEnabled,
				Material = GetPhysicsMaterial(table),
				HitThreshold = HitThreshold,
			};
		}

		#endregion

		#region IApiSwitchable

		private List<SwitchConfig> _switchIds;
		private readonly IGamelogicEngineWithSwitches _gamelogicEngineWithSwitches;

		protected void AddSwitchId(string switchId, bool isPulseSwitch, int pulseDelay)
		{
			if (_switchIds == null) {
				_switchIds = new List<SwitchConfig>();
			}
			_switchIds.Add(new SwitchConfig(switchId, isPulseSwitch, pulseDelay));
		}

		protected void OnSwitch(bool normallyClosed)
		{
			if (_gamelogicEngineWithSwitches != null && _switchIds != null) {
				foreach (var switchConfig in _switchIds) {
					_gamelogicEngineWithSwitches.Switch(switchConfig.SwitchId, normallyClosed);

					// time switch opening if closed and pulse
					if (normallyClosed && switchConfig.IsPulseSwitch) {
						SimulationSystemGroup.ScheduleSwitch(switchConfig.PulseDelay,
							() => _gamelogicEngineWithSwitches.Switch(switchConfig.SwitchId, false));
					}
				}
			}
		}

		#endregion

		private readonly struct SwitchConfig
		{
			public readonly string SwitchId;
			public readonly int PulseDelay;
			public readonly bool IsPulseSwitch;

			public SwitchConfig(string switchId, bool isPulseSwitch, int pulseDelay)
			{
				SwitchId = switchId;
				PulseDelay = pulseDelay;
				IsPulseSwitch = isPulseSwitch;
			}
		}
	}
}
