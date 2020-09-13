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

#region ReSharper
// ReSharper disable UnassignedField.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using VisualPinball.Engine.IO;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Table;

namespace VisualPinball.Engine.VPT.Primitive
{
	[Serializable]
	[MessagePackObject]
	public class PrimitiveData : ItemData, IPhysicalData
	{
		public override string GetName() => Name;
		public override void SetName(string name) { Name = name; }

		[Key(15)]
		[BiffString("NAME", IsWideString = true, Pos = 15)]
		public string Name;

		[Key(1)]
		[BiffVertex("VPOS", IsPadded = true, Pos = 1)]
		public Vertex3D Position;

		[Key(2)]
		[BiffVertex("VSIZ", IsPadded = true, Pos = 2)]
		public Vertex3D Size = new Vertex3D(100, 100, 100);

		[Key(40)]
		[BiffInt("M3VN", Pos = 40)]
		public int NumVertices;

		[Key(41)]
		[BiffInt("M3CY", Pos = 41)]
		public int CompressedVertices;

		[Key(43)]
		[BiffInt("M3FN", Pos = 43)]
		public int NumIndices;

		[Key(44)]
		[BiffInt("M3CJ", Pos = 44)]
		public int CompressedIndices = 0;

		[Key(46)]
		[BiffInt("M3AY", Pos = 46)]
		public int CompressedAnimationVertices;

		[Key(42)]
		[BiffVertices("M3DX", SkipWrite = true)]
		[BiffVertices("M3CX", IsCompressed = true, Pos = 42)]
		[BiffIndices("M3DI", SkipWrite = true)]
		[BiffIndices("M3CI", IsCompressed = true, Pos = 45)]
		[BiffAnimation("M3AX", IsCompressed = true, Pos = 47)]
		public Mesh Mesh = new Mesh();

		[Key(3)]
		[BiffFloat("RTV0", Index = 0, Pos = 3)]
		[BiffFloat("RTV1", Index = 1, Pos = 4)]
		[BiffFloat("RTV2", Index = 2, Pos = 5)]
		[BiffFloat("RTV3", Index = 3, Pos = 6)]
		[BiffFloat("RTV4", Index = 4, Pos = 7)]
		[BiffFloat("RTV5", Index = 5, Pos = 8)]
		[BiffFloat("RTV6", Index = 6, Pos = 9)]
		[BiffFloat("RTV7", Index = 7, Pos = 10)]
		[BiffFloat("RTV8", Index = 8, Pos = 11)]
		public float[] RotAndTra = new float[9];

		[Key(12)]
		[TextureReference]
		[BiffString("IMAG", Pos = 12)]
		public string Image = string.Empty;

		[Key(13)]
		[TextureReference]
		[BiffString("NRMA", Pos = 13)]
		public string NormalMap = string.Empty;

		[Key(14)]
		[BiffInt("SIDS", Pos = 14)]
		public int Sides = 4;

		[Key(16)]
		[MaterialReference]
		[BiffString("MATR", Pos = 16)]
		public string Material = string.Empty;

		[Key(17)]
		[BiffColor("SCOL", Pos = 17)]
		public Color SideColor = new Color(0x0, ColorFormat.Bgr);

		[Key(18)]
		[BiffBool("TVIS", Pos = 18)]
		public bool IsVisible = true;

		[Key(34)]
		[BiffBool("REEN", Pos = 34)]
		public bool IsReflectionEnabled = true;

		[Key(19)]
		[BiffBool("DTXI", Pos = 19)]
		public bool DrawTexturesInside;

		[Key(20)]
		[BiffBool("HTEV", Pos = 20)]
		public bool HitEvent = true;

		[Key(21)]
		[BiffFloat("THRS", Pos = 21)]
		public float Threshold = 2f;

		[Key(22)]
		[BiffFloat("ELAS", Pos = 22)]
		public float Elasticity = 0.3f;

		[Key(23)]
		[BiffFloat("ELFO", Pos = 23)]
		public float ElasticityFalloff = 0.5f;

		[Key(24)]
		[BiffFloat("RFCT", Pos = 24)]
		public float Friction = 0.3f;

		[Key(25)]
		[BiffFloat("RSCT", Pos = 25)]
		public float Scatter;

		[Key(26)]
		[BiffFloat("EFUI", Pos = 26)]
		public float EdgeFactorUi = 0.25f;

		[Key(27)]
		[BiffFloat("CORF", Pos = 27)]
		public float CollisionReductionFactor = 0;

		[Key(28)]
		[BiffBool("CLDR", Pos = 28)]
		public bool IsCollidable = true; // originally "CLDRP"

		[Key(29)]
		[BiffBool("ISTO", Pos = 29)]
		public bool IsToy;

		[Key(36)]
		[MaterialReference]
		[BiffString("MAPH", Pos = 36)]
		public string PhysicsMaterial = string.Empty;

		[Key(37)]
		[BiffBool("OVPH", Pos = 37)]
		public bool OverwritePhysics = true;

		[Key(31)]
		[BiffBool("STRE", Pos = 31)]
		public bool StaticRendering = true;

		[Key(32)]
		[BiffFloat("DILI", QuantizedUnsignedBits = 8, Pos = 32)]
		public float DisableLightingTop; // m_d.m_fDisableLightingTop = (tmp == 1) ? 1.f : dequantizeUnsigned<8>(tmp); // backwards compatible hacky loading!

		[Key(33)]
		[BiffFloat("DILB", Pos = 33)]
		public float DisableLightingBelow;

		[Key(30)]
		[BiffBool("U3DM", Pos = 30)]
		public bool Use3DMesh;

		[Key(35)]
		[BiffBool("EBFC", Pos = 35)]
		public bool BackfacesEnabled;

		[Key(38)]
		[BiffBool("DIPT", Pos = 38)]
		public bool DisplayTexture;

		[Key(385)]
		[BiffBool("OSNM", Pos = 38.5)]
		public bool ObjectSpaceNormalMap;

		[Key(39)]
		[BiffString("M3DN", Pos = 39)]
		public string MeshFileName = string.Empty;

		[Key(48)]
		[BiffFloat("PIDB", Pos = 48)]
		public float DepthBias = 0;

		// IPhysicalData
		public float GetElasticity() => Elasticity;
		public float GetElasticityFalloff() => 0;
		public float GetFriction() => Friction;
		public float GetScatter() => Scatter;
		public bool GetOverwritePhysics() => OverwritePhysics;
		public bool GetIsCollidable() => IsCollidable;
		public string GetPhysicsMaterial() => PhysicsMaterial;

		protected override bool SkipWrite(BiffAttribute attr)
		{
			if (!Use3DMesh) {
				switch (attr.Name) {
					case "M3VN":
					case "M3CY":
					case "M3FN":
					case "M3CJ":
						return true;
				}
			}
			return false;
		}

		#region BIFF

		static PrimitiveData()
		{
			Init(typeof(PrimitiveData), Attributes);
		}

		[SerializationConstructor]
		public PrimitiveData() : base(StoragePrefix.GameItem)
		{
		}

		public PrimitiveData(BinaryReader reader, string storageName) : base(storageName)
		{
			Load(this, reader, Attributes);
			Mesh.Name = Name;
		}

		public PrimitiveData(string name, float x, float y) : base(StoragePrefix.GameItem)
		{
			Name = name;
			Position = new Vertex3D(x, y, 0f);
		}

		public override void Write(BinaryWriter writer, HashWriter hashWriter)
		{
			writer.Write((int)ItemType.Primitive);
			WriteRecord(writer, Attributes, hashWriter);
			WriteEnd(writer, hashWriter);
		}

		private static readonly Dictionary<string, List<BiffAttribute>> Attributes = new Dictionary<string, List<BiffAttribute>>();

		#endregion
	}

	/// <summary>
	/// Parses vertex data.<p/>
	///
	/// Since we additionally need <see cref="PrimitiveData.NumVertices"/> in
	/// order to know how many vertices to parse, we can't use the standard
	/// BiffAttribute.
	/// </summary>
	public class BiffVerticesAttribute : BiffAttribute
	{
		/// <summary>
		/// If set, the vertices are Zlib-compressed.
		/// </summary>
		public bool IsCompressed;

		public BiffVerticesAttribute(string name) : base(name) { }

		public override void Parse<T>(T obj, BinaryReader reader, int len)
		{
			if (obj is PrimitiveData primitiveData) {
				try {
					ParseVertices(primitiveData, IsCompressed
						? BiffZlib.Decompress(reader.ReadBytes(len))
						: reader.ReadBytes(len));
				} catch (Exception e) {
					throw new Exception($"Error parsing vertices for {primitiveData.Name} ({primitiveData.StorageName}).", e);
				}
			}
		}

		public override void Write<TItem>(TItem obj, BinaryWriter writer, HashWriter hashWriter)
		{
			if (obj is PrimitiveData primitiveData) {
				if (!primitiveData.Use3DMesh) {
					// don't write vertices if not using 3d mesh
					return;
				}
				var vertexData = SerializeVertices(primitiveData);
				var data = IsCompressed ? BiffZlib.Compress(vertexData) : vertexData;
				WriteStart(writer, data.Length, hashWriter);
				writer.Write(data);
				hashWriter?.Write(data);

			} else {
				throw new InvalidOperationException("Unknown type for [" + GetType().Name + "] on field \"" + Name + "\".");
			}
		}

		private void ParseVertices(PrimitiveData data, byte[] bytes)
		{
			if (data.NumVertices == 0) {
				throw new ArgumentOutOfRangeException(nameof(data), "Cannot add vertices when size is unknown.");
			}

			if (bytes.Length < data.NumVertices * Vertex3DNoTex2.Size) {
				throw new ArgumentOutOfRangeException($"Tried to read {data.NumVertices} vertices for primitive item \"${data.Name}\" (${data.StorageName}), but only ${bytes.Length} bytes available.");
			}

			if (!(GetValue(data) is Mesh mesh)) {
				throw new ArgumentException("BiffVertices attribute must sit on a Mesh object.");
			}

			var vertices = new Vertex3DNoTex2[data.NumVertices];
			using (var stream = new MemoryStream(bytes))
			using (var reader = new BinaryReader(stream)) {
				for (var i = 0; i < data.NumVertices; i++) {
					vertices[i] = new Vertex3DNoTex2(reader);
				}
			}
			mesh.Vertices = vertices;
		}

		private static byte[] SerializeVertices(PrimitiveData data)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream)) {
				for (var i = 0; i < data.NumVertices; i++) {
					data.Mesh.Vertices[i].Write(writer);
				}
				return stream.ToArray();
			}
		}
	}

	/// <summary>
	/// Parses index data.<p/>
	///
	/// Since we additionally need <see cref="PrimitiveData.NumIndices"/> in
	/// order to know how many indices to parse, we can't use the standard
	/// BiffAttribute.
	/// </summary>
	public class BiffIndicesAttribute : BiffAttribute
	{
		public bool IsCompressed;

		public BiffIndicesAttribute(string name) : base(name) { }

		public override void Parse<T>(T obj, BinaryReader reader, int len)
		{
			if (obj is PrimitiveData tableData) {
				ParseIndices(tableData, IsCompressed
					? BiffZlib.Decompress(reader.ReadBytes(len))
					: reader.ReadBytes(len));
			}
		}

		public override void Write<TItem>(TItem obj, BinaryWriter writer, HashWriter hashWriter)
		{
			if (obj is PrimitiveData primitiveData) {
				if (!primitiveData.Use3DMesh) {
					return;
				}
				var indexData = SerializeIndices(primitiveData);
				var data = IsCompressed ? BiffZlib.Compress(indexData) : indexData;
				WriteStart(writer, data.Length, hashWriter);
				writer.Write(data);
				hashWriter?.Write(data);

			} else {
				throw new InvalidOperationException("Unknown type for [" + GetType().Name + "] on field \"" + Name + "\".");
			}
		}

		private void ParseIndices(PrimitiveData data, byte[] bytes)
		{
			if (data.NumIndices == 0) {
				throw new ArgumentOutOfRangeException($"Cannot add indices when size is unknown.");
			}

			if (!(GetValue(data) is Mesh mesh)) {
				throw new ArgumentException("BiffIndices attribute must sit on a Mesh object.");
			}

			var indices = new int[data.NumIndices];
			using (var stream = new MemoryStream(bytes))
			using (var reader = new BinaryReader(stream)) {
				for (var i = 0; i < data.NumIndices; i++) {
					indices[i] = data.NumVertices > 65535 ? (int)reader.ReadUInt32() : reader.ReadUInt16();
				}
			}
			mesh.Indices = indices;
		}

		private static byte[] SerializeIndices(PrimitiveData data)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream)) {
				for (var i = 0; i < data.NumIndices; i++) {
					if (data.NumVertices > 65535) {
						writer.Write((uint) data.Mesh.Indices[i]);

					} else {
						writer.Write((ushort) data.Mesh.Indices[i]);
					}
				}
				return stream.ToArray();
			}
		}
	}

	/// <summary>
	/// Parses animated vertex data.<p/>
	///
	/// </summary>
	public class BiffAnimationAttribute : BiffAttribute
	{
		/// <summary>
		/// If set, the vertices are Zlib-compressed.
		/// </summary>
		public bool IsCompressed;

		public BiffAnimationAttribute(string name) : base(name) { }

		public override void Parse<T>(T obj, BinaryReader reader, int len)
		{
			if (obj is PrimitiveData primitiveData)
			{
				try
				{
					ParseAnimation(primitiveData, IsCompressed
						? BiffZlib.Decompress(reader.ReadBytes(len))
						: reader.ReadBytes(len));
				}
				catch (Exception e)
				{
					throw new Exception($"Error parsing animation data for {primitiveData.Name} ({primitiveData.StorageName}).", e);
				}
			}
		}

		public override void Write<TItem>(TItem obj, BinaryWriter writer, HashWriter hashWriter)
		{
			if (obj is PrimitiveData primitiveData)
			{
				if (!primitiveData.Use3DMesh)
				{
					// don't write animation if not using 3d mesh
					return;
				}

				for (var i = 0; i < primitiveData.Mesh.AnimationFrames.Count; i++) {
					var animationData = SerializeAnimation(primitiveData.Mesh.AnimationFrames[i]);
					var data = IsCompressed ? BiffZlib.Compress(animationData) : animationData;
					WriteStart(writer, data.Length, hashWriter);
					writer.Write(data);
					hashWriter?.Write(data);
				}

			}
			else
			{
				throw new InvalidOperationException("Unknown type for [" + GetType().Name + "] on field \"" + Name + "\".");
			}
		}

		private void ParseAnimation(PrimitiveData data, byte[] bytes)
		{
			if (data.NumVertices == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(data), "Cannot create animation when size is unknown.");
			}

			if (bytes.Length != data.NumVertices * Mesh.VertData.Size)
			{
				throw new ArgumentOutOfRangeException($"Tried to read {data.NumVertices} vertex animations for primitive item \"${data.Name}\" (${data.StorageName}), but had ${bytes.Length} bytes available.");
			}

			if (!(GetValue(data) is Mesh mesh))
			{
				throw new ArgumentException("BiffAnimationAttribute attribute must sit on a Mesh object.");
			}

			var vertices = new Mesh.VertData[data.NumVertices];
			using (var stream = new MemoryStream(bytes))
			using (var reader = new BinaryReader(stream))
			{
				for (var i = 0; i < data.NumVertices; i++)
				{
					vertices[i] = new Mesh.VertData(reader);
				}
			}
			mesh.AnimationFrames.Add(vertices);
		}

		private static byte[] SerializeAnimation(Mesh.VertData[] data)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				for (var i = 0; i < data.Length; i++)
				{
					data[i].Write(writer);
				}
				return stream.ToArray();
			}
		}
	}
}
