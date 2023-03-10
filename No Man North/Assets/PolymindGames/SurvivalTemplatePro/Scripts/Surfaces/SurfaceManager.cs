using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalTemplatePro.Surfaces
{
    /// <summary>
    /// Global surface effects system
    /// </summary>
    public class SurfaceManager : Singleton<SurfaceManager>
	{
		#region Internal
		public class TerrainInfo
		{
			public Vector3 Position { get; }
			public TerrainData Data { get; }

			private TerrainLayer[] m_Layers;


			public TerrainInfo(Terrain terrain)
			{
				Position = terrain.GetPosition();

				Data = terrain.terrainData;
				m_Layers = Data.terrainLayers;
			}

			public TerrainLayer GetSplatmapPrototypeId(int i)
			{
				if (m_Layers != null && m_Layers.Length > i)
					return m_Layers[i];

				return null;
			}
		}
		#endregion

		[SerializeField]
		private bool m_SpatializeAudio = false;

		[SerializeField, InLineEditor]
		private SurfaceInfo m_DefaultSurface;

		[Title("Pooling")]

		[SerializeField]
		private int m_MinPoolSize;

		[SerializeField]
		private int m_MaxPoolSize;

		[SerializeField]
		private float m_AutoReleaseDelay = 3f;

		private SurfaceInfo[] m_Surfaces;
		private Dictionary<Collider, TerrainInfo> m_SceneTerrains;


        public static void SpawnEffect(RaycastHit hitInfo, SurfaceEffects effectType, float audioVolume, bool parentEffect = false)
		{
			SpawnEffect(hitInfo, effectType, audioVolume, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), parentEffect);
		}

		public static void SpawnEffect(RaycastHit hitInfo, SurfaceEffects effectType, float audioVolume, Vector3 position, Quaternion rotation, bool parentEffect = false)
		{
			SurfaceInfo surfInfo = Instance.Internal_GetSurfaceInfo(hitInfo);

			if (surfInfo != null)
			{
				PoolableObject effect = PoolingManager.Instance.GetObject(surfInfo.GetInstanceID().ToString() + effectType.ToString());

				if (effect != null)
				{
					effect.transform.SetPositionAndRotation(position, rotation);

					if (parentEffect)
						effect.transform.SetParent(hitInfo.transform, true);

					effect.GetComponent<SurfaceEffect>().Play(audioVolume);
				}
			}
		}

		public static void SpawnEffect(int surfaceId, SurfaceEffects effectType, float audioVolume, Vector3 position, Quaternion rotation)
		{
			SurfaceInfo surfInfo = Instance.GetSurfaceWithId(surfaceId);

			PoolableObject effect = PoolingManager.Instance.GetObject(surfInfo.GetInstanceID().ToString() + effectType.ToString());

			if (effect != null)
			{
				effect.transform.SetPositionAndRotation(position, rotation);
				effect.GetComponent<SurfaceEffect>().Play(audioVolume);
			}
		}

		public static SurfaceInfo GetSurfaceInfo(RaycastHit hitInfo) => Instance.Internal_GetSurfaceInfo(hitInfo);
		public static SurfaceInfo GetSurfaceInfo(Material material) => Instance.GetSurfaceInfoWithMaterial(material);

		private SurfaceInfo Internal_GetSurfaceInfo(RaycastHit hitInfo)
		{
			Collider col = hitInfo.collider;

			// Check if the collider has a surface identity component beforehand, if not get the material from the renderer.
			if (col.TryGetComponent(out SurfaceIdentity surfId))
			{
				if (surfId.Surface == null)
					return m_DefaultSurface;

				foreach (var surfInfo in m_Surfaces)
				{
					if (surfInfo.name == surfId.Surface.name)
						return surfInfo;
				}
			}

			// Check if the collider is a terrain.
			if (m_SceneTerrains.TryGetValue(col, out TerrainInfo terrainInfo))
			{
				float[] textureMix = GetTerrainTextureMix(hitInfo.point, terrainInfo.Data, terrainInfo.Position);
				int textureIndex = GetTerrainLayerIndex(textureMix);

				var terrainLayer = terrainInfo.GetSplatmapPrototypeId(textureIndex);
				return GetSurfaceInfoWithTerrainLayer(terrainLayer);
			}

			var material = GetMaterialFromTriangle(col, hitInfo.triangleIndex);
			return GetSurfaceInfoWithMaterial(material);
		}

		private SurfaceInfo GetSurfaceInfoWithMaterial(Material material)
		{
			if (material == null)
				return null;

			for (int i = 0; i < m_Surfaces.Length; i++)
			{
				if (m_Surfaces[i].HasMaterial(material))
					return m_Surfaces[i];
			}

			return m_DefaultSurface;
		}

		private SurfaceInfo GetSurfaceInfoWithTerrainLayer(TerrainLayer terrainLayer)
		{
			if (terrainLayer == null)
				return null;

			for (int i = 0; i < m_Surfaces.Length; i++)
			{
				if (m_Surfaces[i].HasTerrainLayer(terrainLayer))
					return m_Surfaces[i];
			}

			return m_DefaultSurface;
		}

		private SurfaceInfo GetSurfaceWithId(int surfaceId)
		{
			if (m_Surfaces.Length > surfaceId && surfaceId >= 0)
				return m_Surfaces[surfaceId];

			return null;
		}

		protected override void OnAwake()
		{
			m_Surfaces = Resources.LoadAll<SurfaceInfo>("Surfaces/");

			for (int i = 0;i < m_Surfaces.Length;i++)
			{
				string surfName = m_Surfaces[i].name;

				m_Surfaces[i] = Instantiate(m_Surfaces[i]);
				m_Surfaces[i].name = surfName;

				m_Surfaces[i].CacheTextures();
			}

			SceneManager.sceneLoaded += CacheTerrains;
			CacheTerrains(new Scene(), LoadSceneMode.Single);

			CacheSurfaceEffects();
		}

        private void OnDestroy()
        {
			SceneManager.sceneLoaded -= CacheTerrains;
		}

        private void CacheSurfaceEffects()
		{
			foreach (var surfInfo in m_Surfaces)
			{
				string surfInstanceId = surfInfo.GetInstanceID().ToString();

				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.SoftFootstep}", surfInfo.SoftFootstepEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.SoftFootstep}");
				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.HardFootstep}", surfInfo.HardFootstepEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.HardFootstep}");
				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.FallImpact}", surfInfo.FallImpactEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.FallImpact}");

				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.BulletHit}", surfInfo.BulletHitEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.BulletHit}");
				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.Slash}", surfInfo.SlashEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.Slash}");
				CreatePoolForEffect($"{surfInfo.name}_{SurfaceEffects.Stab}", surfInfo.StabEffect, m_MinPoolSize, m_MaxPoolSize, true, $"{surfInstanceId}{SurfaceEffects.Stab}");

				if (m_DefaultSurface != null && m_DefaultSurface.name == surfInfo.name)
					m_DefaultSurface = surfInfo;
			}
		}

		private void CreatePoolForEffect(string name, SurfaceInfo.EffectPair effectPair, int poolSizeMin, int poolSizeMax, bool autoShrink, string poolId)
		{
			GameObject effectTemplate = new GameObject(name);
			SurfaceEffect effectComponent = effectTemplate.AddComponent<SurfaceEffect>();
			effectComponent.Init(effectPair.AudioEffect, effectPair.VisualEffect, m_SpatializeAudio);

			PoolingManager.Instance.CreatePool(effectTemplate, poolSizeMin, poolSizeMax, autoShrink, poolId, m_AutoReleaseDelay);

			Destroy(effectTemplate);
		}

		private void CacheTerrains(Scene scene, LoadSceneMode loadSceneMode)
		{
			m_SceneTerrains = new Dictionary<Collider, TerrainInfo>();
			Terrain[] sceneTerrains = FindObjectsOfType<Terrain>();

			for (int i = 0;i < sceneTerrains.Length;i++)
			{
				TerrainCollider terrainCollider = sceneTerrains[i].GetComponent<TerrainCollider>();

				if (terrainCollider == null)
					continue;

				m_SceneTerrains.Add(terrainCollider, new TerrainInfo(sceneTerrains[i]));
			}
		}

		private Material GetMaterialFromTriangle(Collider collider, int triangleIndex)
		{
			Renderer renderer = collider.GetComponent<Renderer>();
			if (renderer == null || renderer.sharedMaterials.Length == 0)
				return null;

			if (renderer.sharedMaterials.Length == 1)
				return renderer.sharedMaterials[0];

			MeshCollider meshCollider = collider as MeshCollider;
			if (meshCollider == null || meshCollider.convex)
				return renderer.sharedMaterials[0];

			Mesh mesh = meshCollider.sharedMesh;
			if (mesh.isReadable == false)
			{
				Debug.LogError("You need to mark model's mesh as Read/Write Enabled in Import Settings.", mesh);
				return null;
			}

			int materialIndex = -1;
			int lookupIndex1 = mesh.triangles[triangleIndex * 3];
			int lookupIndex2 = mesh.triangles[triangleIndex * 3 + 1];
			int lookupIndex3 = mesh.triangles[triangleIndex * 3 + 2];

			for (int i = 0;i < mesh.subMeshCount;i++)
			{
				int[] triangles = mesh.GetTriangles(i);

				for (int j = 0;j < triangles.Length;j += 3)
				{
					if (triangles[j] == lookupIndex1 && triangles[j + 1] == lookupIndex2 && triangles[j + 2] == lookupIndex3)
					{
						materialIndex = i;
						break;
					}
				}

				if (materialIndex != -1)
					break;
			}

			return renderer.sharedMaterials[materialIndex];
		}

		private float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
		{
			// Returns an array containing the relative mix of textures
			// on the terrain at this world position.

			// The number of values in the array will equal the number
			// of textures added to the terrain.

			// Calculate which splat map cell the worldPos falls within (ignoring y)
			int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
			int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

			// Get the splat data for this cell as a 1x1xN 3D array (where N = number of textures)
			float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

			// Extract the 3D array data to a 1D array:
			float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

			for(int n = 0;n < cellMix.Length;n++)
				cellMix[n] = splatmapData[0, 0, n];

			return cellMix;
		}

		private int GetTerrainLayerIndex(float[] textureMix)
		{
			// Returns the zero-based index of the most dominant texture
			float maxMix = 0;
			int maxIndex = 0;

			// Loop through each mix value and find the maximum.
			for (int n = 0;n < textureMix.Length;n++)
			{
				if (textureMix[n] > maxMix)
				{
					maxIndex = n;
					maxMix = textureMix[n];
				}
			}

			return maxIndex;
		}
	}
}