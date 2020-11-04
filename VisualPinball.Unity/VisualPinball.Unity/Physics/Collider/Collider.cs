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
using NLog;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Physics;
using VisualPinball.Engine.VPT;
using VisualPinball.Engine.VPT.Bumper;
using VisualPinball.Engine.VPT.Flipper;
using VisualPinball.Engine.VPT.Gate;
using VisualPinball.Engine.VPT.Kicker;
using VisualPinball.Engine.VPT.Plunger;
using VisualPinball.Engine.VPT.Spinner;
using VisualPinball.Engine.VPT.Trigger;
using Random = Unity.Mathematics.Random;

namespace VisualPinball.Unity
{
	/// <summary>
	/// Base struct common to all colliders.
	/// Dispatches the interface methods to appropriate implementations for the collider type.
	/// </summary>
	internal struct Collider : IComponentData
	{
		public ColliderHeader Header;

		public int Id => Header.Id;
		public Entity Entity => Header.Entity;
		public Entity ParentEntity => Header.ParentEntity;
		public ColliderType Type => Header.Type;
		public PhysicsMaterialData Material => Header.Material;
		public float Threshold => Header.Threshold;
		public bool FireEvents => Header.FireEvents;

		public static Collider None => new Collider {
			Header = { Type = ColliderType.None }
		};

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public unsafe Aabb Aabb {
			get {
				fixed (Collider* collider = &this) {
					switch (collider->Type) {
						case ColliderType.Bumper:
						case ColliderType.Circle:
							return ((CircleCollider*) collider)->Aabb;
					}
				}
				return default;
			}
		}

		public static unsafe float HitTest(ref Collider coll, ref CollisionEventData collEvent,
			ref DynamicBuffer<BallInsideOfBufferElement> insideOf, in BallData ball, float dTime)
		{
			fixed (Collider* collider = &coll)
			{
				switch (collider->Type)
				{
					case ColliderType.Bumper:
					case ColliderType.Circle:
						return ((CircleCollider*) collider)->HitTest(ref collEvent, ref insideOf, in ball, dTime);
					case ColliderType.Gate:
						return ((GateCollider*) collider)->HitTest(ref collEvent, ref insideOf, in ball, dTime);
					case ColliderType.Line:
						return ((LineCollider*) collider)->HitTest(ref collEvent, ref insideOf, in ball, dTime);
					case ColliderType.LineZ:
						return ((LineZCollider*) collider)->HitTest(ref collEvent, in ball, dTime);
					case ColliderType.Line3D:
						return ((Line3DCollider*) collider)->HitTest(ref collEvent, in ball, dTime);
					case ColliderType.Point:
						return ((PointCollider*) collider)->HitTest(ref collEvent, in ball, dTime);
					case ColliderType.Plane:
						return ((PlaneCollider*) collider)->HitTest(ref collEvent, in ball, dTime);
					case ColliderType.Spinner:
						return ((SpinnerCollider*) collider)->HitTest(ref collEvent, ref insideOf, in ball, dTime);
					case ColliderType.Triangle:
						return ((TriangleCollider*) collider)->HitTest(ref collEvent, in insideOf, in ball, dTime);
					case ColliderType.KickerCircle:
					case ColliderType.TriggerCircle:
						return ((CircleCollider*) collider)->HitTestBasicRadius(ref collEvent, ref insideOf, in ball, dTime, false, false, false);
					case ColliderType.TriggerLine:
						return ((LineCollider*) collider)->HitTestBasic(ref collEvent, ref insideOf, in ball, dTime, false, false, false);

					case ColliderType.Plunger:
						throw new InvalidOperationException("ColliderType.Plunger must be hit-tested separately!");
					case ColliderType.Flipper:
						throw new InvalidOperationException("ColliderType.Flipper must be hit-tested separately!");
					case ColliderType.LineSlingShot:
						throw new InvalidOperationException("ColliderType.LineSlingShot must be hit-tested separately!");

					default:
						return -1;
				}
			}
		}

		/// <summary>
		/// Most colliders use the standard Collide3DWall routine, only overrides
		/// are cast and dispatched to their respective implementation.
		/// </summary>
		public static unsafe void Collide(ref Collider coll, ref BallData ballData,
			ref NativeQueue<EventData>.ParallelWriter events, in CollisionEventData collEvent,
			ref Random random)
		{
			fixed (Collider* collider = &coll)
			{
				switch (collider->Type)
				{
					case ColliderType.Circle:
						((CircleCollider*) collider)->Collide(ref ballData, in collEvent, ref random);
						break;
					case ColliderType.Line:
						((LineCollider*) collider)->Collide(ref ballData, ref events, in collEvent, ref random);
						break;
					case ColliderType.Line3D:
						((Line3DCollider*) collider)->Collide(ref ballData, ref events, in collEvent, ref random);
						break;
					case ColliderType.LineZ:
						((LineZCollider*) collider)->Collide(ref ballData, ref events, in collEvent, ref random);
						break;
					case ColliderType.Plane:
						((PlaneCollider*) collider)->Collide(ref ballData, in collEvent, ref random);
						break;
					case ColliderType.Point:
						((PointCollider*) collider)->Collide(ref ballData, ref events, in collEvent, ref random);
						break;
					case ColliderType.Triangle:
						((TriangleCollider*) collider)->Collide(ref ballData, ref events, in collEvent, ref random);
						break;

					default:
						break;
				}
			}
		}

		public static void FireHitEvent(ref BallData ball, ref NativeQueue<EventData>.ParallelWriter events, in ColliderHeader collHeader)
		{
			if (collHeader.FireEvents/* && collHeader.IsEnabled*/) { // todo enabled

				// is this the same place as last event? if same then ignore it
				var posDiff = ball.EventPosition - ball.Position;
				var distLs = math.lengthsq(posDiff);

				// remember last collide position
				ball.EventPosition = ball.Position;

				// hit targets when used with a captured ball have always a too small distance
				var normalDist = collHeader.ItemType == ItemType.HitTarget ? 0.0f : 0.25f; // magic distance

				// must be a new place if only by a little
				if (distLs > normalDist) {
					events.Enqueue(new EventData(EventId.HitEventsHit, collHeader.ParentEntity, true));
				}
			}
		}

		public static void Contact(ref Collider coll, ref BallData ball, in CollisionEventData collEvent, double hitTime, in float3 gravity)
		{
			BallCollider.HandleStaticContact(ref ball, in collEvent, coll.Header.Material.Friction, (float)hitTime, gravity);
		}
	}
}
