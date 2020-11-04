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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VisualPinball.Engine.Physics;
using Debug = UnityEngine.Debug;
using Logger = NLog.Logger;

namespace VisualPinball.Unity
{
	internal static class QuadTreeCreationSystem
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static void Create(EntityManager entityManager)
		{
			var player = Object.FindObjectOfType<Player>();
			var itemApis = player.Collidables.ToArray();

			// 1. generate colliders
			var colliderId = 0;
			var colliderList = new List<ICollider>();
			var (playfieldCollider, glassCollider) = player.TableApi.CreateColliders(player.Table, ref colliderId);
			foreach (var itemApi in itemApis) {
				itemApi.CreateColliders(player.Table, colliderList, ref colliderId);
			}

			// 1. now we know how many there are, create a blob asset reference
			BlobAssetReference<ColliderBlob> colliderBlobAssetRef;
			using (var builder = new BlobBuilder(Allocator.TempJob)) {
				ref var root = ref builder.ConstructRoot<ColliderBlob>();
				var colliders = builder.Allocate(ref root.Colliders, colliderList.Count + 2); // plane colliders are not in this list

				playfieldCollider.Allocate(builder, ref colliders);
				glassCollider.Allocate(builder, ref colliders);

				root.PlayfieldColliderId = playfieldCollider.Id;
				root.GlassColliderId = glassCollider.Id;

				foreach (var collider in colliderList) {
					collider.Allocate(builder, ref colliders);
				}
				colliderBlobAssetRef = builder.CreateBlobAssetReference<ColliderBlob>(Allocator.Persistent);
			}

			// 3. Create quadtree blob (BlobAssetReference<QuadTreeBlob>) from AABBs
			BlobAssetReference<QuadTreeBlob> quadTreeBlobAssetRef;
			using (var builder = new BlobBuilder(Allocator.Temp)) {
				ref var rootQuadTree = ref builder.ConstructRoot<QuadTreeBlob>();
				QuadTree.Create(builder, ref colliderBlobAssetRef.Value.Colliders, ref rootQuadTree.QuadTree,
					player.Table.BoundingBox.ToAabb());

				quadTreeBlobAssetRef = builder.CreateBlobAssetReference<QuadTreeBlob>(Allocator.Persistent);
			}


			// save it to entity
			var collEntity = entityManager.CreateEntity(ComponentType.ReadOnly<QuadTreeData>(), ComponentType.ReadOnly<ColliderData>());
			//DstEntityManager.SetName(collEntity, "Collision Data Holder");
			entityManager.SetComponentData(collEntity, new QuadTreeData { Value = quadTreeBlobAssetRef });
			entityManager.SetComponentData(collEntity, new ColliderData { Value = colliderBlobAssetRef });

			Logger.Info("Static QuadTree initialized.");
		}

		public static void CreateLegacy(EntityManager entityManager)
		{
			var table = Object.FindObjectOfType<TableAuthoring>().Table;
			var stopWatch = new Stopwatch();

			stopWatch.Start();

			// 1. init playables - this already creates colliders for some items
			foreach (var playable in table.Playables) {
				playable.Init(table);
			}

			// 2. now collect all hit objects, resulting in creation of the remaining colliders. this also sets the IDs
			var hittables = table.Hittables.Where(hittable => hittable.IsCollidable).ToArray();
			var hitObjects = new List<HitObject>();
			var id = 0;
			var log = "";
			var c = 0;

			foreach (var item in hittables) {
				var hitShapes = item.GetHitShapes();
				log += item.Name + ": " + hitShapes.Length + "\n";
				c += hitShapes.Length;
				foreach (var hitObject in hitShapes) {
					hitObject.SetIndex(item.Index, item.Version, item.ParentIndex, item.ParentVersion);
					hitObject.Id = id++;
					hitObject.CalcHitBBox();
					hitObjects.Add(hitObject);
				}
			}
			stopWatch.Stop();
			Logger.Info("Collider Count:\n" + log + "\nTotal: " + c + " colliders in " + stopWatch.ElapsedMilliseconds + "ms");

			// 3. create the "ported" (class) quadtree
			var quadTree = new Engine.Physics.QuadTree(hitObjects, table.BoundingBox);

			// 4. convert the "ported" (class) quadtree to "runtime" (struct) quadtree
			var quadTreeBlobAssetRef = QuadTreeBlob.CreateBlobAssetReference(
				quadTree,
				table.GeneratePlayfieldHit(), // todo use `null` if separate playfield mesh exists
				table.GenerateGlassHit()
			);

			// playfield and glass need special treatment, since not part of the quad tree
			var playfieldHitObject = table.GeneratePlayfieldHit();
			var glassHitObject = table.GenerateGlassHit();
			playfieldHitObject.Id = id++;
			glassHitObject.Id = id;
			hitObjects.Add(playfieldHitObject);
			hitObjects.Add(glassHitObject);

			// 5. construct collider blob out of hit objects - this converts hit objects to structs
			var colliderBlob = ColliderBlob.CreateBlobAssetReference(hitObjects, playfieldHitObject.Id, glassHitObject.Id);

			// save it to entity
			var collEntity = entityManager.CreateEntity(ComponentType.ReadOnly<QuadTreeData>(), ComponentType.ReadOnly<ColliderData>());
			//DstEntityManager.SetName(collEntity, "Collision Data Holder");
			entityManager.SetComponentData(collEntity, new QuadTreeData { Value = quadTreeBlobAssetRef });
			entityManager.SetComponentData(collEntity, new ColliderData { Value = colliderBlob });
		}
	}
}
