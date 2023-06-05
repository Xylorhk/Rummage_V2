using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.TerrainTools;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
#if NM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace WorldStreamer2
{
    /// <summary>
    /// Split terrain.
    /// </summary>
    public class TerrainManager : Editor
    {
        string currentScene;
        TerrainManagerSettings terrainManagerSettings;

        List<TerrainData> terrainData = new List<TerrainData>();
        List<GameObject> terrainGo = new List<GameObject>();
        Terrain parentTerrain;
        Scene previewScene;
        Scene lastScene;
        private float minHeightTerrain;
        private float maxHeightTerrain;

        /// <summary>
        /// The scroll position.
        /// </summary>
        private Vector2 scrollPos;

        private List<MeshFilter> meshFiltersToFix = new List<MeshFilter>();


        public readonly GUIContent s_basicTerrain = EditorGUIUtility.TrTextContent("Basic Terrain");

        public readonly GUIContent s_groupingID =
            EditorGUIUtility.TrTextContent("Grouping ID", "Grouping ID for auto connection");

        public readonly GUIContent s_allowAutoConnect = EditorGUIUtility.TrTextContent("Auto Connect",
            "Allow the current terrain tile to automatically connect to neighboring tiles sharing the same grouping ID.");

        public readonly GUIContent s_attemptReconnect =
            EditorGUIUtility.TrTextContent("Reconnect", "Will attempt to re-run auto connection");

        public readonly GUIContent s_drawTerrain =
            EditorGUIUtility.TrTextContent("Draw", "Toggle the rendering of terrain");

        public readonly GUIContent s_drawInstancedTerrain =
            EditorGUIUtility.TrTextContent("Draw Instanced", "Toggle terrain instancing rendering");

        public readonly GUIContent s_pixelError = EditorGUIUtility.TrTextContent("Pixel Error",
            "The accuracy of the mapping between the terrain maps (heightmap, textures, etc.) and the generated terrain; higher values indicate lower accuracy but lower rendering overhead.");

        public readonly GUIContent s_baseMapDist = EditorGUIUtility.TrTextContent("Base Map Dist.",
            "The maximum distance at which terrain textures will be displayed at full resolution. Beyond this distance, a lower resolution composite image will be used for efficiency.");

        public readonly GUIContent s_castShadows =
            EditorGUIUtility.TrTextContent("Cast Shadows", "Does the terrain cast shadows?");

        public readonly GUIContent s_createMaterial = EditorGUIUtility.TrTextContent("Create...",
            "Create a new Material asset to be used by the terrain by duplicating the current default Terrain material.");

        public readonly GUIContent s_reflectionProbes = EditorGUIUtility.TrTextContent("Reflection Probes",
            "How reflection probes are used on terrain. Only effective when using built-in standard material or a custom material which supports rendering with reflection.");

        public readonly GUIContent s_preserveTreePrototypeLayers = EditorGUIUtility.TrTextContent(
            "Preserve Tree Prototype Layers",
            "Enable this option if you want your tree instances to take on the layer values of their prototype prefabs, rather than the terrain GameObject's layer.");

        public readonly GUIContent s_treeAndDetails = EditorGUIUtility.TrTextContent("Tree & Detail Objects");

        public readonly GUIContent s_drawTrees =
            EditorGUIUtility.TrTextContent("Draw", "Should trees, grass and details be drawn?");

        public readonly GUIContent s_detailObjectDistance = EditorGUIUtility.TrTextContent("Detail Distance",
            "The distance (from camera) beyond which details will be culled.");

        public readonly GUIContent s_detailObjectDensity = EditorGUIUtility.TrTextContent("Detail Density",
            "The number of detail/grass objects in a given unit of area. The value can be set lower to reduce rendering overhead.");

        public readonly GUIContent s_treeDistance = EditorGUIUtility.TrTextContent("Tree Distance",
            "The distance (from camera) beyond which trees will be culled. For SpeedTree trees this parameter is controlled by the LOD group settings.");

        public readonly GUIContent s_treeBillboardDistance = EditorGUIUtility.TrTextContent("Billboard Start",
            "The distance (from camera) at which 3D tree objects will be replaced by billboard images. For SpeedTree trees this parameter is controlled by the LOD group settings.");

        public readonly GUIContent s_treeCrossFadeLength = EditorGUIUtility.TrTextContent("Fade Length",
            "Distance over which trees will transition between 3D objects and billboards. For SpeedTree trees this parameter is controlled by the LOD group settings.");

        public readonly GUIContent s_treeMaximumFullLODCount = EditorGUIUtility.TrTextContent("Max Mesh Trees",
            "The maximum number of visible trees that will be represented as solid 3D meshes. Beyond this limit, trees will be replaced with billboards. For SpeedTree trees this parameter is controlled by the LOD group settings.");

        public readonly GUIContent s_grassWindSettings =
            EditorGUIUtility.TrTextContent("Wind Settings for Grass (On Terrain Data)");

        public readonly GUIContent s_wavingGrassStrength =
            EditorGUIUtility.TrTextContent("Speed", "The speed of the wind as it blows grass.");

        public readonly GUIContent s_wavingGrassSpeed = EditorGUIUtility.TrTextContent("Size",
            "The size of the 'ripples' on grassy areas as the wind blows over them.");

        public readonly GUIContent s_wavingGrassAmount =
            EditorGUIUtility.TrTextContent("Bending", "The degree to which grass objects are bent over by the wind.");

        public readonly GUIContent s_wavingGrassTint =
            EditorGUIUtility.TrTextContent("Grass Tint", "Overall color tint applied to grass objects.");

        public readonly GUIContent s_bakeLightProbesForTrees = EditorGUIUtility.TrTextContent(
            "Bake Light Probes For Trees",
            "If the option is enabled, Unity will create internal light probes at the position of each tree (these probes are internal and will not affect other renderers in the scene) and apply them to tree renderers for lighting. Otherwise trees are still affected by LightProbeGroups. The option is only effective for trees that have LightProbe enabled on their prototype prefab.");

        public readonly GUIContent s_deringLightProbesForTrees = EditorGUIUtility.TrTextContent(
            "Remove Light Probe Ringing",
            "When enabled, removes visible overshooting often observed as ringing on objects affected by intense lighting at the expense of reduced contrast.");

        public void OnEnable()
        {
            if (RenderPipelineManager.currentPipeline == null)
            {
            }
            else
            {
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    string defineHDRP = "NM_HDRP";
                    string define =
                        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings
                            .selectedBuildTargetGroup);
                    if (!define.Contains(defineHDRP))
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, define + " " + defineHDRP);
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                         srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    string defineURP = "NM_URP";
                    string define =
                        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings
                            .selectedBuildTargetGroup);
                    if (!define.Contains(defineURP))
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, define + " " + defineURP);
                }
            }
        }

        public void OnGUI()
        {
            if (currentScene != EditorSceneManager.GetActiveScene().path || terrainManagerSettings == null)
            {
                SceneChanged();
            }

            if (terrainManagerSettings == null)
                return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (terrainManagerSettings.colorSpaceLast == ColorSpace.Uninitialized)
            {
                if (PlayerSettings.colorSpace == ColorSpace.Gamma)
                {
                    terrainManagerSettings.ambientLightColor = new Color(1.23f, 1.23f, 1.23f);
                }
                else
                {
                    terrainManagerSettings.ambientLightColor = new Color(1f, 1f, 1f);
                }

                terrainManagerSettings.colorSpaceLast = PlayerSettings.colorSpace;
            }
            else if (terrainManagerSettings.colorSpaceLast != PlayerSettings.colorSpace)
            {
                if (terrainManagerSettings.colorSpaceLast == ColorSpace.Gamma)
                {
                    terrainManagerSettings.ambientLightColor.r = terrainManagerSettings.ambientLightColor.r / 1.23f;
                    terrainManagerSettings.ambientLightColor.g = terrainManagerSettings.ambientLightColor.g / 1.23f;
                    terrainManagerSettings.ambientLightColor.b = terrainManagerSettings.ambientLightColor.b / 1.23f;
                }
                else
                {
                    terrainManagerSettings.ambientLightColor.r = terrainManagerSettings.ambientLightColor.r * 1.23f;
                    terrainManagerSettings.ambientLightColor.g = terrainManagerSettings.ambientLightColor.g * 1.23f;
                    terrainManagerSettings.ambientLightColor.b = terrainManagerSettings.ambientLightColor.b * 1.23f;
                }

                terrainManagerSettings.colorSpaceLast = PlayerSettings.colorSpace;
            }


            Terrain[] terrains = Terrain.activeTerrains;


            SerializedObject serializedObject = new SerializedObject(terrainManagerSettings);

            SerializedProperty listTerrainTrees = serializedObject.FindProperty("terrainTrees");

            for (int i = 0; i < terrains.Length; i++)
            {
                if (terrains[i] != null && terrains[i].terrainData != null)
                {
                    TreePrototype[] prototypes = terrains[i].terrainData.treePrototypes;
                    foreach (var treePrototype in prototypes)
                    {
                        bool toAdd = true;
                        foreach (var terrainTree in terrainManagerSettings.terrainTrees)
                        {
                            if (treePrototype.prefab == terrainTree.tree)
                            {
                                toAdd = false;
                                break;
                            }
                        }

                        if (toAdd)
                        {
                            terrainManagerSettings.terrainTrees.Add(new TerrainTrees() {tree = treePrototype.prefab});
                        }
                    }
                }
            }

            EditorGUILayout.Space();
#if NM_HDRP
            GUILayout.Label("[HDRP]", EditorStyles.boldLabel);
            EditorGUILayout.Space();
#endif


#if NM_URP
            GUILayout.Label("[URP]", EditorStyles.boldLabel);
            EditorGUILayout.Space();


#endif
            ////Vertical
            GUILayout.Label("Terrains settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            TerrainSettingsGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Set settings for selected terrains"))
            {
                SetTerrainSettings();
            }

            if (GUILayout.Button("Set settings for all terrains"))
            {
                SetTerrainSettings(true);
            }

            EditorGUILayout.Space();


            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            ////Vertical
            GUILayout.Label("Terrain splitter", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            terrainManagerSettings.terrainsDataPath =
                EditorGUILayout.TextField("Terrain create folder", terrainManagerSettings.terrainsDataPath);
            terrainManagerSettings.splitSize =
                Mathf.NextPowerOfTwo(EditorGUILayout.IntSlider("Split size", terrainManagerSettings.splitSize, 2, 32));
            terrainManagerSettings.addTerrainCulling =
                EditorGUILayout.Toggle("Add terrain culling", terrainManagerSettings.addTerrainCulling);

            if (GUILayout.Button("Split selected terrains"))
            {
                SplitTerrain();
            }

            if (GUILayout.Button("Split all terrains"))
            {
                SplitTerrain(true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();


            ////Vertical
            GUILayout.Label("Terrain Low Poly Mesh Generator", EditorStyles.boldLabel);


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            terrainManagerSettings.terrainPath =
                EditorGUILayout.TextField("Terrain create folder", terrainManagerSettings.terrainPath);
            terrainManagerSettings.ambientLightColor = EditorGUILayout.ColorField(new GUIContent("Ambient color"),
                terrainManagerSettings.ambientLightColor, true, false, true);
            EditorGUILayout.Space();

            terrainManagerSettings.terrainPrefixName =
                EditorGUILayout.TextField("Terrain Mesh Name", terrainManagerSettings.terrainPrefixName);
            terrainManagerSettings.terrainLod =
                EditorGUILayout.IntSlider("Terrain lod", terrainManagerSettings.terrainLod, 0, 8);
            EditorGUILayout.Space();
            terrainManagerSettings.basemapResolution = Mathf.NextPowerOfTwo(
                EditorGUILayout.IntSlider("Basemap Resolution", terrainManagerSettings.basemapResolution, 128, 4096));
            terrainManagerSettings.useBaseMap =
                EditorGUILayout.Toggle("Use base map", terrainManagerSettings.useBaseMap);
            terrainManagerSettings.useSmoothness =
                EditorGUILayout.Toggle("Use smoothness map", terrainManagerSettings.useSmoothness);
#if NM_URP
            terrainManagerSettings.useMaskSmoothnessURP =
 EditorGUILayout.Toggle("Use mask map", terrainManagerSettings.useMaskSmoothnessURP);

#endif


            terrainManagerSettings.terrainNormalDetails = Mathf.NextPowerOfTwo(
                EditorGUILayout.IntSlider("Normal Details", terrainManagerSettings.terrainNormalDetails, 1, 32));
            terrainManagerSettings.terrainNormalStrength = EditorGUILayout.Slider("Normal strength",
                terrainManagerSettings.terrainNormalStrength, 1, 10);
            terrainManagerSettings.useTerrainNormal = EditorGUILayout.Toggle("Create normal from shape",
                terrainManagerSettings.useTerrainNormal);
            terrainManagerSettings.useTextureNormal = EditorGUILayout.Toggle("Create normal from textures",
                terrainManagerSettings.useTextureNormal);


            EditorGUILayout.Space();
            terrainManagerSettings.yOffset = EditorGUILayout.FloatField("Y offset", terrainManagerSettings.yOffset);
            terrainManagerSettings.addVerticesDown =
                EditorGUILayout.Toggle("Add vertices down", terrainManagerSettings.addVerticesDown);
            if (terrainManagerSettings.addVerticesDown)
                terrainManagerSettings.verticesDownDistance = EditorGUILayout.Slider("Vertices down distance",
                    terrainManagerSettings.verticesDownDistance, 1, 100);


            EditorGUILayout.Space();


            SerializedProperty trainglesTreesMax = serializedObject.FindProperty("trainglesTreesMax");

            EditorGUILayout.PropertyField(trainglesTreesMax,
                new GUIContent("Trees Max Traingles", "Trees Max Traingles to Export"));

            listTerrainTrees.isExpanded = EditorGUILayout.Foldout(listTerrainTrees.isExpanded,
                new GUIContent("Tree Prototypes", "Tree Prototypes to generate mesh lod"));
            EditorGUI.indentLevel++;
            if (listTerrainTrees.isExpanded)
            {
                //EditorGUILayout.PropertyField(listTerrainTrees.FindPropertyRelative("Array.size"));
                for (int i = 0; i < listTerrainTrees.arraySize; i++)
                {
                    SerializedProperty scriptable = listTerrainTrees.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                    scriptable.isExpanded = EditorGUILayout.Foldout(scriptable.isExpanded,
                        new GUIContent(scriptable.FindPropertyRelative("tree").objectReferenceValue.name), false);
                    EditorGUILayout.PropertyField(scriptable.FindPropertyRelative("active"), new GUIContent(""),
                        GUILayout.ExpandWidth(false), GUILayout.MaxWidth(40));

                    //EditorGUILayout.LabelField();

                    EditorGUILayout.EndHorizontal();
                    if (scriptable.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(scriptable.FindPropertyRelative("tree"));
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                    }
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Export selected Terrains data: shape and trees into LP Mesh"))
            {
                TerrainToMesh(true);
            }

            if (GUILayout.Button("Export selected Terrains data: shape into LP Mesh"))
            {
                TerrainToMesh();
            }


            if (GUILayout.Button("Export selected Terrains data: trees into LP Mesh"))
            {
                ExportTreesForSelectedTerrain();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Export all Terrains data: shape and trees into LP Mesh"))
            {
                TerrainToMesh(true, true);
            }

            if (GUILayout.Button("Export all Terrains data: shape into LP Mesh"))
            {
                TerrainToMesh(false, true);
            }


            if (GUILayout.Button("Export all Terrains data: trees into LP Mesh"))
            {
                ExportTreesForSelectedTerrain(true);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Export all Terrains Rim"))
            {
                TerrainToMeshRim(true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Creates the settings gameObject.
        /// </summary>
        private void SceneChanged()
        {
            currentScene = EditorSceneManager.GetActiveScene().path;

            terrainManagerSettings = FindObjectOfType(typeof(TerrainManagerSettings)) as TerrainManagerSettings;

            if (terrainManagerSettings == null)
            {
                GameObject gameObject = new GameObject("_TerrainManagerSettings");
                terrainManagerSettings = gameObject.AddComponent<TerrainManagerSettings>();
                terrainManagerSettings.materialTemplate =
                    AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat");
                gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild;
                gameObject.tag = "EditorOnly";
            }
        }


        private void TerrainSettingsGUI()
        {
            /*EditorApplication.SetSceneRepaintDirty();
                EditorUtility.SetDirty(terrain);
              if (!EditorApplication.isPlaying)
                    SceneManagement.EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);   
             */

            terrainManagerSettings.showBasicTerrainSettings =
                EditorGUILayout.BeginFoldoutHeaderGroup(terrainManagerSettings.showBasicTerrainSettings,
                    s_basicTerrain);

            if (terrainManagerSettings.showBasicTerrainSettings)
            {
                ++EditorGUI.indentLevel;

                terrainManagerSettings.groupingID =
                    EditorGUILayout.IntField(s_groupingID, terrainManagerSettings.groupingID);
                terrainManagerSettings.allowAutoConnect =
                    EditorGUILayout.Toggle(s_allowAutoConnect, terrainManagerSettings.allowAutoConnect);


                terrainManagerSettings.drawHeightmap =
                    EditorGUILayout.Toggle(s_drawTerrain, terrainManagerSettings.drawHeightmap);
                terrainManagerSettings.drawInstanced =
                    EditorGUILayout.Toggle(s_drawInstancedTerrain, terrainManagerSettings.drawInstanced);
                terrainManagerSettings.heightmapPixelError = EditorGUILayout.Slider(s_pixelError,
                    terrainManagerSettings.heightmapPixelError, 1, 200); // former string formatting: ""
                terrainManagerSettings.basemapDistance = EditorGUILayout.Slider(s_baseMapDist,
                    terrainManagerSettings.basemapDistance, 0.0f, 20000.0f);
                terrainManagerSettings.shadowCastingMode =
                    (ShadowCastingMode) EditorGUILayout.EnumPopup(s_castShadows,
                        terrainManagerSettings.shadowCastingMode);
                terrainManagerSettings.reflectionProbeUsage =
                    (ReflectionProbeUsage) EditorGUILayout.EnumPopup(s_reflectionProbes,
                        terrainManagerSettings.reflectionProbeUsage);
                terrainManagerSettings.materialTemplate = EditorGUILayout.ObjectField("Material",
                    terrainManagerSettings.materialTemplate, typeof(Material), false) as Material;


                --EditorGUI.indentLevel;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();

            terrainManagerSettings.showTreeAndDetailSettings =
                EditorGUILayout.BeginFoldoutHeaderGroup(terrainManagerSettings.showTreeAndDetailSettings,
                    s_treeAndDetails);

            if (terrainManagerSettings.showTreeAndDetailSettings)
            {
                ++EditorGUI.indentLevel;

                terrainManagerSettings.drawTreesAndFoliage =
                    EditorGUILayout.Toggle(s_drawTrees, terrainManagerSettings.drawTreesAndFoliage);
                terrainManagerSettings.bakeLightProbesForTrees = EditorGUILayout.Toggle(s_bakeLightProbesForTrees,
                    terrainManagerSettings.bakeLightProbesForTrees);
                terrainManagerSettings.deringLightProbesForTrees = EditorGUILayout.Toggle(s_deringLightProbesForTrees,
                    terrainManagerSettings.deringLightProbesForTrees);

                terrainManagerSettings.preserveTreePrototypeLayers =
                    EditorGUILayout.Toggle(s_preserveTreePrototypeLayers,
                        terrainManagerSettings.preserveTreePrototypeLayers);
                terrainManagerSettings.detailObjectDistance = EditorGUILayout.Slider(s_detailObjectDistance,
                    terrainManagerSettings.detailObjectDistance, 0, 250);
                terrainManagerSettings.detailObjectDensity = EditorGUILayout.Slider(s_detailObjectDensity,
                    terrainManagerSettings.detailObjectDensity, 0.0f, 1.0f);
                terrainManagerSettings.treeDistance =
                    EditorGUILayout.Slider(s_treeDistance, terrainManagerSettings.treeDistance, 0, 5000);
                terrainManagerSettings.treeBillboardDistance = EditorGUILayout.Slider(s_treeBillboardDistance,
                    terrainManagerSettings.treeBillboardDistance, 5, 2000);
                terrainManagerSettings.treeCrossFadeLength = EditorGUILayout.Slider(s_treeCrossFadeLength,
                    terrainManagerSettings.treeCrossFadeLength, 0, 200);
                terrainManagerSettings.treeMaximumFullLODCount = EditorGUILayout.IntSlider(s_treeMaximumFullLODCount,
                    terrainManagerSettings.treeMaximumFullLODCount, 0, 10000);


                --EditorGUI.indentLevel;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();

            terrainManagerSettings.showGrassWindSettings =
                EditorGUILayout.BeginFoldoutHeaderGroup(terrainManagerSettings.showGrassWindSettings,
                    s_grassWindSettings);

            if (terrainManagerSettings.showGrassWindSettings)
            {
                ++EditorGUI.indentLevel;

                terrainManagerSettings.wavingGrassStrength = EditorGUILayout.Slider(s_wavingGrassStrength,
                    terrainManagerSettings.wavingGrassStrength, 0, 1);
                terrainManagerSettings.wavingGrassSpeed = EditorGUILayout.Slider(s_wavingGrassSpeed,
                    terrainManagerSettings.wavingGrassSpeed, 0, 1);
                terrainManagerSettings.wavingGrassAmount = EditorGUILayout.Slider(s_wavingGrassAmount,
                    terrainManagerSettings.wavingGrassAmount, 0, 1);
                terrainManagerSettings.wavingGrassTint =
                    EditorGUILayout.ColorField(s_wavingGrassTint, terrainManagerSettings.wavingGrassTint);

                --EditorGUI.indentLevel;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private Texture ExportBaseMap(Terrain terrainBase, string terrainName, out Texture mask)
        {
            mask = null;
            float baseMapDistance = terrainBase.basemapDistance;

            if (terrainManagerSettings.useBaseMap)
            {
                terrainBase.basemapDistance = 20000;
            }
            else
            {
                terrainBase.basemapDistance = 0;
            }

            previewScene = EditorSceneManager.NewPreviewScene();
            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;
            if (SceneView.lastActiveSceneView.camera != null)
            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }


            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;

            
#if !NM_URP && !NM_HDRP
            terrain.materialTemplate = new Material(Shader.Find("NatureManufacture Shaders/Terrain/StandardAlbedo"));
            Debug.Log(terrainManagerSettings.ambientLightColor);
            terrain.materialTemplate.SetColor("_AmbientColor", terrainManagerSettings.ambientLightColor);
#endif

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();


            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center + Vector3.up * currentBounds.max.y + new Vector3(0, 1, 0);


            Selection.activeGameObject = cam.gameObject;


            cam.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 1000.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;

            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            DestroyImmediate(cameraGo);

            // EditorSceneManager.MoveGameObjectToScene(cameraGo, EditorSceneManager.GetActiveScene());
            DestroyImmediate(go);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.basemapDistance = baseMapDistance;

#if NM_URP
            screenShot = ExportTextureSmoothness(terrainBase, terrainName, 4);

            if (terrainManagerSettings.useMaskSmoothnessURP)
            {

                Texture2D smoothnessTexture = ExportTextureSmoothness(terrainBase, terrainName, 1);
                Texture2D aoTexture = ExportTextureSmoothness(terrainBase, terrainName, 2);
                Texture2D metalicTexture = ExportTextureSmoothness(terrainBase, terrainName, 3);


                for (int x = 0; x < metalicTexture.width; x++)
                {
                    for (int y = 0; y < metalicTexture.height; y++)
                    {

                        Color metalicColor = metalicTexture.GetPixel(x, y);
                        Color aoColor = aoTexture.GetPixel(x, y);
                        Color smoothnesColor = smoothnessTexture.GetPixel(x, y);

                        metalicColor.g = aoColor.r;
                        metalicColor.b = 0;
                        metalicColor.a = smoothnesColor.r;

                        metalicTexture.SetPixel(x, y, metalicColor);

                    }
                }

                byte[] byteMask = metalicTexture.EncodeToPNG();

                string nameMask = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_M.png";
                System.IO.File.WriteAllBytes(Application.dataPath + nameMask, byteMask);
                AssetDatabase.Refresh();


                TextureImporter importerMask = (TextureImporter)AssetImporter.GetAtPath("Assets" + nameMask);
                importerMask.wrapMode = TextureWrapMode.Clamp;
                importerMask.streamingMipmaps = true;
                importerMask.sRGBTexture = false;
                importerMask.anisoLevel = 8;
                importerMask.SaveAndReimport();
                mask = AssetDatabase.LoadAssetAtPath<Texture>("Assets" + nameMask);

            }
            else
                mask = null;
#endif

            if (terrainManagerSettings.useSmoothness)
            {
                Texture2D smoothnessTexture = ExportTextureSmoothness(terrainBase, terrainName);

                for (int x = 0; x < screenShot.width; x++)
                {
                    for (int y = 0; y < screenShot.height; y++)
                    {
                        Color smoothnesColor = smoothnessTexture.GetPixel(x, y);
                        Color basemapColor = screenShot.GetPixel(x, y);
                        basemapColor.a = smoothnesColor.r;

                        // screenShot.SetPixel(x, y, smoothnesColor);
                        screenShot.SetPixel(x, y, basemapColor);
                    }
                }
            }


            byte[] bytesAtlas = screenShot.EncodeToPNG();

            string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_BC.png";
            System.IO.File.WriteAllBytes(Application.dataPath + name, bytesAtlas);
            AssetDatabase.Refresh();

            //Debug.Log("Assets" + name);
            TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath("Assets" + name);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.streamingMipmaps = true;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture>("Assets" + name);
        }


        private Texture ExportBaseMapHDRP(Terrain terrainBase, string terrainName, out Texture mask)
        {
            float baseMapDistance = terrainBase.basemapDistance;

            if (terrainManagerSettings.useBaseMap)
            {
                terrainBase.basemapDistance = 20000;
            }
            else
            {
                terrainBase.basemapDistance = 0;
            }


            previewScene = EditorSceneManager.NewPreviewScene();

            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;

            if (SceneView.lastActiveSceneView.camera != null)
            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }

            GameObject light = new GameObject("lightNM");
            EditorSceneManager.MoveGameObjectToScene(light, previewScene);
            light.transform.eulerAngles = new Vector3(90, 0, 0);

#if NM_HDRP
            var hdLight = light.AddHDLight(HDLightTypeAndShape.Directional);
            //hdLight.intensity = 6.283186f;
            hdLight.intensity = 3.141593f;

            hdLight.EnableColorTemperature(false);
            // hdLight.SetColor(Color.white, 6500);

#endif

            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            bool drawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = false;


            bool blend = terrain.materialTemplate.HasProperty("_EnableHeightBlend") &&
                         terrain.materialTemplate.GetFloat("_EnableHeightBlend") > 0;
            float heightTransition = 0;
            if (blend)
            {
                heightTransition = terrain.materialTemplate.GetFloat("_HeightTransition");
            }

            terrain.materialTemplate = new Material(Shader.Find("HDRP/TerrainLitNM Normal"));

            terrain.materialTemplate.SetFloat("_metallicMap", -1);

#if NM_HDRP
            if (blend)
            {
                CoreUtils.SetKeyword(terrain.materialTemplate, "_TERRAIN_BLEND_HEIGHT", blend);
                terrain.materialTemplate.SetFloat("_HeightTransition", heightTransition);
            }
#endif


            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();

#if NM_HDRP
            HDAdditionalCameraData cameraData = cameraGo.AddComponent<HDAdditionalCameraData>();
            cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;

            FrameSettings frameSettings = cameraData.renderingPathCustomFrameSettings;
            cameraData.customRenderingSettings = true;

            FrameSettingsOverrideMask frameSettingsOverrideMask =
 cameraData.renderingPathCustomFrameSettingsOverrideMask;


            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Postprocess] = true;
            frameSettings.SetEnabled(FrameSettingsField.Postprocess, false);

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.ExposureControl] = true;
            frameSettings.SetEnabled(FrameSettingsField.ExposureControl, false);

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.DirectSpecularLighting] = true;
            frameSettings.SetEnabled(FrameSettingsField.DirectSpecularLighting, false);

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.SkyReflection] = true;
            frameSettings.SetEnabled(FrameSettingsField.SkyReflection, false);

            cameraData.renderingPathCustomFrameSettings = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
#endif


            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center - cam.transform.forward * (currentBounds.max.y + 300);

            GameObject centerObject = new GameObject("Center");
            EditorSceneManager.MoveGameObjectToScene(centerObject, previewScene);

            centerObject.transform.position = currentBounds.center;
            cam.transform.parent = centerObject.transform;
            centerObject.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 300.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;


            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            cam.transform.parent = null;
            DestroyImmediate(centerObject);
            DestroyImmediate(terrain);
            DestroyImmediate(cameraGo);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.drawInstanced = drawInstanced;
            terrainBase.basemapDistance = baseMapDistance;

            byte[] bytesAtlas = screenShot.EncodeToPNG();

            string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_BC.png";
            System.IO.File.WriteAllBytes(Application.dataPath + name, bytesAtlas);
            AssetDatabase.Refresh();

            //Debug.Log("Assets" + name);
            TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath("Assets" + name);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.streamingMipmaps = true;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();

            GC.Collect();


            if (terrainManagerSettings.useSmoothness)
            {
                Texture2D metalicTexture = ExportTextureSmoothnessHDRP(terrainBase, terrainName, 1);
                Texture2D aoTexture = ExportTextureSmoothnessHDRP(terrainBase, terrainName, 2);
                Texture2D smoothnessTexture = ExportTextureSmoothnessHDRP(terrainBase, terrainName, 3);


                for (int x = 0; x < metalicTexture.width; x++)
                {
                    for (int y = 0; y < metalicTexture.height; y++)
                    {
                        Color metalicColor = metalicTexture.GetPixel(x, y);
                        Color aoColor = aoTexture.GetPixel(x, y);
                        Color smoothnesColor = smoothnessTexture.GetPixel(x, y);

                        metalicColor.g = aoColor.r;
                        metalicColor.b = 0;
                        metalicColor.a = smoothnesColor.r;

                        metalicTexture.SetPixel(x, y, metalicColor);
                    }
                }

                byte[] byteMask = metalicTexture.EncodeToPNG();

                string nameMask = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_M.png";
                System.IO.File.WriteAllBytes(Application.dataPath + nameMask, byteMask);
                AssetDatabase.Refresh();


                TextureImporter importerMask = (TextureImporter) AssetImporter.GetAtPath("Assets" + nameMask);
                importerMask.wrapMode = TextureWrapMode.Clamp;
                importerMask.streamingMipmaps = true;
                importerMask.sRGBTexture = false;
                importerMask.anisoLevel = 8;
                importerMask.SaveAndReimport();

                mask = AssetDatabase.LoadAssetAtPath<Texture>("Assets" + nameMask);
            }
            else
                mask = null;

            return AssetDatabase.LoadAssetAtPath<Texture>("Assets" + name);
        }

        private Texture2D ExportTextureNormalmapHDRP(Terrain terrainBase, string terrainName)
        {
            previewScene = EditorSceneManager.NewPreviewScene();
            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;
            if (SceneView.lastActiveSceneView.camera != null)
            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }


            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            bool drawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = false;

            bool blend = terrain.materialTemplate.HasProperty("_EnableHeightBlend") &&
                         terrain.materialTemplate.GetFloat("_EnableHeightBlend") > 0;
            float heightTransition = 0;
            if (blend)
            {
                heightTransition = terrain.materialTemplate.GetFloat("_HeightTransition");
            }

            terrain.materialTemplate = new Material(Shader.Find("HDRP/TerrainLitNM Normal"));

#if NM_HDRP
            if (blend)
            {
                CoreUtils.SetKeyword(terrain.materialTemplate, "_TERRAIN_BLEND_HEIGHT", blend);
                terrain.materialTemplate.SetFloat("_HeightTransition", heightTransition);
            }
#endif

            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;


            GameObject light = new GameObject("light");
            EditorSceneManager.MoveGameObjectToScene(light, previewScene);
            light.transform.eulerAngles = new Vector3(90, 0, 0);

#if NM_HDRP
            var hdLight = light.AddHDLight(HDLightTypeAndShape.Directional);
            hdLight.intensity = 3.141593f;
#endif

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();
#if NM_HDRP
            HDAdditionalCameraData cameraData = cameraGo.AddComponent<HDAdditionalCameraData>();
            cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.None;
            FrameSettings frameSettings = cameraData.renderingPathCustomFrameSettings;

            FrameSettingsOverrideMask frameSettingsOverrideMask =
 cameraData.renderingPathCustomFrameSettingsOverrideMask;
            cameraData.customRenderingSettings = true;

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Postprocess] = true;
            frameSettings.SetEnabled(FrameSettingsField.Postprocess, false);

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.ExposureControl] = true;
            frameSettings.SetEnabled(FrameSettingsField.ExposureControl, false);

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.DirectSpecularLighting] = true;
            frameSettings.SetEnabled(FrameSettingsField.DirectSpecularLighting, false);

            cameraData.renderingPathCustomFrameSettings = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
#endif

            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center - cam.transform.forward * currentBounds.max.y;

            GameObject centerObject = new GameObject("Center");
            centerObject.transform.position = currentBounds.center;
            cam.transform.parent = centerObject.transform;
            centerObject.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 10.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;

            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            //byte[] bytesAtlas = screenShot.EncodeToPNG();

            //string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_BC.png";
            //System.IO.File.WriteAllBytes(Application.dataPath + name, bytesAtlas);

            cam.transform.parent = null;
            DestroyImmediate(centerObject);
            DestroyImmediate(cameraGo);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.drawInstanced = drawInstanced;


            return screenShot;
        }

        private Texture2D ExportTextureNormalmap(Terrain terrainBase, string terrainName)
        {
            previewScene = EditorSceneManager.NewPreviewScene();
            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;
            if (SceneView.lastActiveSceneView.camera != null)
            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }


            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            bool drawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = false;

#if NM_URP
            terrain.materialTemplate = new Material(Shader.Find("NatureManufacture Shaders/Terrain Lit Normal"));

            terrain.materialTemplate.SetFloat("_NMNormal", 0);
#else
            terrain.materialTemplate = new Material(Shader.Find("NatureManufacture Shaders/Terrain/Standard"));
#endif

            // Debug.Log(terrain.drawInstanced);


            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();


            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center - cam.transform.forward * currentBounds.max.y;

            GameObject centerObject = new GameObject("Center");
            centerObject.transform.position = currentBounds.center;
            cam.transform.parent = centerObject.transform;
            centerObject.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 10.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;

            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            ////
            //byte[] bytesAtlas = screenShot.EncodeToPNG();

            //string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_BCRob.png";
            //System.IO.File.WriteAllBytes(Application.dataPath + name, bytesAtlas);
            ////

            cam.transform.parent = null;
            DestroyImmediate(centerObject);
            DestroyImmediate(cameraGo);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.drawInstanced = drawInstanced;

            return screenShot;
        }

        private Texture2D ExportTextureSmoothness(Terrain terrainBase, string terrainName, int type = 1)
        {
            previewScene = EditorSceneManager.NewPreviewScene();
            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;
            if (SceneView.lastActiveSceneView.camera != null)

            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }


            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            bool drawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = false;


#if NM_URP
            terrain.materialTemplate = new Material(Shader.Find("NatureManufacture Shaders/Terrain Lit Normal"));

            terrain.materialTemplate.SetFloat("_NMNormal", type);
#else
            terrain.materialTemplate =
                new Material(Shader.Find("NatureManufacture Shaders/Terrain/StandardSmoothness"));
#endif


            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();


            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center - cam.transform.forward * currentBounds.max.y;

            GameObject centerObject = new GameObject("Center");
            centerObject.transform.position = currentBounds.center;
            cam.transform.parent = centerObject.transform;
            centerObject.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 10.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;

            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            //byte[] bytesAtlas = screenShot.EncodeToPNG();

            //string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_BC.png";
            //System.IO.File.WriteAllBytes(Application.dataPath + name, bytesAtlas);

            cam.transform.parent = null;
            DestroyImmediate(centerObject);
            DestroyImmediate(cameraGo);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.drawInstanced = drawInstanced;

            return screenShot;
        }

        private Texture2D ExportTextureSmoothnessHDRP(Terrain terrainBase, string terrainName, int type)
        {
            previewScene = EditorSceneManager.NewPreviewScene();
            Material sky = RenderSettings.skybox;
            float ambient = RenderSettings.ambientIntensity;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientColor = RenderSettings.ambientLight;
            if (SceneView.lastActiveSceneView.camera != null)

            {
                lastScene = SceneView.lastActiveSceneView.camera.scene;
                SceneView.lastActiveSceneView.camera.scene = previewScene;

                RenderSettings.skybox = null;
                RenderSettings.ambientIntensity = 0;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = terrainManagerSettings.ambientLightColor;
            }


            GameObject light = new GameObject("light");
            EditorSceneManager.MoveGameObjectToScene(light, previewScene);
            light.transform.eulerAngles = new Vector3(90, 0, 0);

#if NM_HDRP
            var hdLight = light.AddHDLight(HDLightTypeAndShape.Directional);
            hdLight.intensity = 3.141593f;
#endif

            Terrain terrain = Instantiate(terrainBase);
            terrain.drawTreesAndFoliage = false;
            bool drawInstanced = terrain.drawInstanced;
            terrain.drawInstanced = false;

            bool blend = terrain.materialTemplate.HasProperty("_EnableHeightBlend") &&
                         terrain.materialTemplate.GetFloat("_EnableHeightBlend") > 0;
            float heightTransition = 0;
            if (blend)
            {
                heightTransition = terrain.materialTemplate.GetFloat("_HeightTransition");
            }


            terrain.materialTemplate = new Material(Shader.Find("HDRP/TerrainLitNM Normal"));
            terrain.materialTemplate.SetFloat("_metallicMap", type);

#if NM_HDRP || NM_URP
            if (blend)
            {
                CoreUtils.SetKeyword(terrain.materialTemplate, "_TERRAIN_BLEND_HEIGHT", blend);
                terrain.materialTemplate.SetFloat("_HeightTransition", heightTransition);
            }
#endif

            GameObject go = terrain.gameObject;
            EditorSceneManager.MoveGameObjectToScene(go, previewScene);
            go.transform.position = Vector3.zero;

            GameObject cameraGo = new GameObject("PreviewCamera");
            EditorSceneManager.MoveGameObjectToScene(cameraGo, previewScene);


            Camera cam = cameraGo.AddComponent<Camera>();
#if NM_HDRP
            HDAdditionalCameraData cameraData = cameraGo.AddComponent<HDAdditionalCameraData>();
            cameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.None;
            FrameSettings frameSettings = cameraData.renderingPathCustomFrameSettings;

            FrameSettingsOverrideMask frameSettingsOverrideMask =
 cameraData.renderingPathCustomFrameSettingsOverrideMask;
            cameraData.customRenderingSettings = true;

            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Postprocess] = true;
            frameSettings.SetEnabled(FrameSettingsField.Postprocess, false);


            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.ExposureControl] = true;
            frameSettings.SetEnabled(FrameSettingsField.ExposureControl, false);


            frameSettingsOverrideMask.mask[(uint)FrameSettingsField.DirectSpecularLighting] = true;
            frameSettings.SetEnabled(FrameSettingsField.DirectSpecularLighting, false);

            cameraData.renderingPathCustomFrameSettings = frameSettings;
            cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
#endif


            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographic = true;
            cam.depthTextureMode = DepthTextureMode.Depth;


            cam.rect = new Rect(0, 0, 1, 1);

            Bounds currentBounds = terrain.terrainData.bounds;

            cam.transform.eulerAngles = new Vector3(0, 0, 0);


            cam.transform.position = currentBounds.center - cam.transform.forward * currentBounds.max.y;

            GameObject centerObject = new GameObject("Center");
            centerObject.transform.position = currentBounds.center;
            cam.transform.parent = centerObject.transform;
            centerObject.transform.eulerAngles = new Vector3(90, 0, 0);

            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = cam.transform.position.y + 10.0f;


            cam.orthographicSize = Mathf.Max((currentBounds.max.x - currentBounds.min.x) / 2.0f,
                (currentBounds.max.z - currentBounds.min.z) / 2.0f);

            cam.scene = previewScene;

            RenderTexture rt = new RenderTexture(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, 32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(terrainManagerSettings.basemapResolution,
                terrainManagerSettings.basemapResolution, TextureFormat.ARGB32, false);

            cam.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(
                new Rect(0, 0, terrainManagerSettings.basemapResolution, terrainManagerSettings.basemapResolution), 0,
                0);

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);


            cam.transform.parent = null;
            DestroyImmediate(centerObject);
            DestroyImmediate(cameraGo);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();

            if (previewScene != null)
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                SceneView.lastActiveSceneView.camera.scene = lastScene;
                RenderSettings.skybox = sky;

                RenderSettings.ambientIntensity = ambient;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientColor;
            }

            terrainBase.drawInstanced = drawInstanced;

            return screenShot;
        }

        void TerrainToMeshRim(bool allTerrains = false)
        {
            meshFiltersToFix.Clear();
            if (!Directory.Exists("Assets/" + terrainManagerSettings.terrainPath))
            {
                Directory.CreateDirectory("Assets/" + terrainManagerSettings.terrainPath);
            }

            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);

            if (allTerrains)
                terrains = Terrain.activeTerrains;

            int idTerrain = 0;
            int countTerrain = terrains.Length;
            foreach (var terrain in terrains)
            {
                EditorUtility.DisplayProgressBar("Terrain mesh generation",
                    "Exporting terrain " + idTerrain + "/" + countTerrain + "\n Exporting mesh",
                    idTerrain / (float) countTerrain);
                TerrainData terrainData = terrain.terrainData;
                float[,] heightmapData = terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                    terrainData.heightmapResolution);

                float sizeX = terrainData.size.x;
                float sizeY = terrainData.size.y;
                float sizeZ = terrainData.size.z;
                float terrainTowidth = (1 / sizeX * (terrainData.heightmapResolution - 1));
                float terrainToheight = (1 / sizeZ * (terrainData.heightmapResolution - 1));

                Vector3 position = Vector3.zero;

                int lod = 1;

                Vector4[,] positionArray = new Vector4[heightmapData.GetLength(0) + 2, heightmapData.GetLength(1) + 2];
                int addxz = 1;


                for (int x = 0; x < heightmapData.GetLength(0); x += lod)
                {
                    //List<Vector3> positionArrayRow = new List<Vector3>();
                    for (int z = 0; z < heightmapData.GetLength(1); z += lod)
                    {
                        position.x = z / (float) terrainToheight; //, polygonHeight
                        position.y = heightmapData[x, z] * sizeY;
                        position.z = x / (float) terrainTowidth;


                        positionArray[x / lod + addxz, z / lod + addxz] =
                            new Vector4(position.x, position.y, position.z);


                        if (x == 0)
                            positionArray[0, z / lod + addxz] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (x == heightmapData.GetLength(0) - 1)
                            positionArray[x / lod + 2, z / lod + addxz] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (z == 0)
                            positionArray[x / lod + 1, 0] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (z == heightmapData.GetLength(1) - 1)
                            positionArray[x / lod + addxz, z / lod + 2] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (x == 0 && z == 0)
                            positionArray[0, 0] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (x == 0 && z == heightmapData.GetLength(1) - 1)
                            positionArray[0, z / lod + 2] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (x == heightmapData.GetLength(0) - 1 && z == 0)
                            positionArray[x / lod + 2, 0] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);

                        if (x == heightmapData.GetLength(0) - 1 && z == heightmapData.GetLength(1) - 1)
                            positionArray[x / lod + 2, z / lod + 2] = new Vector4(position.x,
                                position.y - terrainManagerSettings.verticesDownDistance, position.z);
                    }
                    //positionArray.Add(positionArrayRow);
                }


                Mesh meshTerrain = new Mesh();
                meshTerrain.indexFormat = IndexFormat.UInt32;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<int> triangles = new List<int>();

                Vector3 normal;
                Vector3 vert;


                int id = 0;
                Vector2 uv;
                for (int x = 0; x < positionArray.GetLength(0); x++)
                {
                    for (int z = 0; z < positionArray.GetLength(1); z++)
                    {
                        if (x > 1 && x < positionArray.GetLength(0) - 2 && z > 1 && z < positionArray.GetLength(1) - 2)
                            continue;


                        vert = positionArray[x, z];
                        positionArray[x, z].w = id;
                        id++;
                        vertices.Add(vert);

                        uv = new Vector2(vert.x / (float) sizeX, vert.z / (float) sizeZ);
                        if (uv.x > 0.99)
                            uv.x = 1;
                        if (uv.y > 0.99)
                            uv.y = 1;
                        if (uv.x < 0.01)
                            uv.x = 0;
                        if (uv.y < 0.01)
                            uv.y = 0;
                        uvs.Add(uv);


                        normal = terrainData.GetInterpolatedNormal(vert.x / (float) sizeX, vert.z / (float) sizeZ);

                        if (x == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z].x / (float) sizeX,
                                positionArray[x + 1, z].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z].x / (float) sizeX,
                                positionArray[x - 1, z].z / (float) sizeZ);

                        if (z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z + 1].x / (float) sizeX,
                                positionArray[x, z + 1].z / (float) sizeZ);

                        if (z == positionArray.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z - 1].x / (float) sizeX,
                                positionArray[x, z - 1].z / (float) sizeZ);

                        if (x == 0 && z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z + 1].x / (float) sizeX,
                                positionArray[x + 1, z + 1].z / (float) sizeZ);

                        if (x == 0 && z == positionArray.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z - 1].x / (float) sizeX,
                                positionArray[x, z - 1].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1 && z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z + 1].x / (float) sizeX,
                                positionArray[x - 1, z + 1].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1 && z == heightmapData.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z - 1].x / (float) sizeX,
                                positionArray[x - 1, z - 1].z / (float) sizeZ);


                        if (x == 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 2, z].x / (float) sizeX,
                                positionArray[x + 2, z].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 2)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 2, z].x / (float) sizeX,
                                positionArray[x - 2, z].z / (float) sizeZ);

                        if (z == 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z + 2].x / (float) sizeX,
                                positionArray[x, z + 2].z / (float) sizeZ);

                        if (z == positionArray.GetLength(1) - 2)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z - 2].x / (float) sizeX,
                                positionArray[x, z - 2].z / (float) sizeZ);

                        if (x == 1 && z == 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 2, z + 2].x / (float) sizeX,
                                positionArray[x + 2, z + 2].z / (float) sizeZ);

                        if (x == 1 && z == positionArray.GetLength(1) - 2)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 2, z - 2].x / (float) sizeX,
                                positionArray[x, z - 2].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 2 && z == 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 2, z + 2].x / (float) sizeX,
                                positionArray[x - 2, z + 2].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 2 && z == heightmapData.GetLength(1) - 2)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 2, z - 2].x / (float) sizeX,
                                positionArray[x - 2, z - 2].z / (float) sizeZ);


                        normals.Add(normal);
                    }
                }


                int rowPositionCount = positionArray.GetLength(1);

                Debug.Log(positionArray.GetLength(0) + " " + positionArray.GetLength(1));

                for (int i = 0; i < positionArray.GetLength(1) - 1; i++)
                {
                    for (int j = 0; j < positionArray.GetLength(0) - 1; j++)
                    {
                        if (j > 0 && j < positionArray.GetLength(0) - 2 && i > 0 && i < positionArray.GetLength(1) - 2)
                            continue;


                        triangles.Add((int) positionArray[j, i].w);
                        triangles.Add((int) positionArray[(j + 1), i].w);
                        triangles.Add((int) positionArray[j, (i + 1)].w);

                        triangles.Add((int) positionArray[(j + 1), i].w);
                        triangles.Add((int) positionArray[(j + 1), (i + 1)].w);
                        triangles.Add((int) positionArray[j, (i + 1)].w);
                    }
                }


                //if (j > 0 && j < positionArray.GetLength(1) - 2 && i > 0 && i < positionArray.GetLength(0) - 2)
                //    continue;

                meshTerrain.SetVertices(vertices);
                meshTerrain.SetTriangles(triangles, 0);
                meshTerrain.SetUVs(0, uvs);
                meshTerrain.SetNormals(normals);
                //meshTerrain.RecalculateNormals();
                meshTerrain.RecalculateTangents();
                meshTerrain.RecalculateBounds();

                EditorUtility.DisplayProgressBar("Terrain mesh generation",
                    "Exporting terrain " + idTerrain + "/" + countTerrain + "\n Exporting textures",
                    (idTerrain + 0.5f) / (float) countTerrain);
                string terrainName = terrainManagerSettings.terrainPrefixName + terrain.gameObject.name;
                GameObject meshGO = new GameObject(terrainName);

                MeshFilter meshfilter = meshGO.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
                Material terrainMaterial = new Material(Shader.Find("Standard"));

                if (RenderPipelineManager.currentPipeline == null)
                {
                    terrainMaterial = new Material(Shader.Find("Standard"));
                }
                else
                {
                    var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

                    if (srpType.Contains("HDRenderPipelineAsset"))
                    {
                        terrainMaterial = new Material(Shader.Find("HDRP/TerrainLit"));
                    }
                    else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                             srpType.Contains("LightweightRenderPipelineAsset"))
                    {
                        terrainMaterial = new Material(Shader.Find("LWRP/TerrainLit"));
                    }
                }


                MaterialPropertyBlock block = new MaterialPropertyBlock();

                terrain.GetSplatMaterialPropertyBlock(block);


                Texture mask;

                Texture basemap = ExportBaseMap(terrain, terrainName, out mask);


                terrainMaterial.SetTexture("_MainTex", basemap);

                Texture texture = ExportNormalMapHeightMap(terrain, terrainName);

                terrainMaterial.SetTexture("_BumpMap", texture);

                terrainMaterial.SetInt("_SmoothnessTextureChannel", 1);
                terrainMaterial.SetFloat("_Glossiness", .2f);
                terrainMaterial.SetFloat("_GlossMapScale", 0.5f);
                terrainMaterial.SetFloat("_Metallic", .0750f);

                terrainMaterial.EnableKeyword("_NORMALMAP");


#if NM_URP
                terrainMaterial.SetFloat("_Smoothness", 1f);
                UnityEditor.Rendering.Universal.ShaderGUI.LitGUI.SetMaterialKeywords(terrainMaterial);
                //CoreUtils.SetKeyword(terrainMaterial, "_SPECULAR_SETUP", false);

                //CoreUtils.SetKeyword(terrainMaterial, "_METALLICSPECGLOSSMAP", true);
#endif


                string name = terrainManagerSettings.terrainPath + "/M_" + terrainName + ".mat";
                string path = "Assets/" + name;

                AssetDatabase.CreateAsset(terrainMaterial, path);
                terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);

                meshRenderer.sharedMaterial = terrainMaterial;

                meshGO.transform.position = terrain.transform.position;
                meshfilter.sharedMesh = meshTerrain;


                meshFiltersToFix.Add(meshfilter);

                AssetDatabase.Refresh();

                idTerrain++;
            }

            EditorUtility.ClearProgressBar();

            Debug.Log("Clear fix normals");
            FixMeshNormals(meshFiltersToFix);


            string pathMesh;
            foreach (var item in meshFiltersToFix)
            {
                name = terrainManagerSettings.terrainPath + item.name + ".asset";
                pathMesh = "Assets/" + name;
                AssetDatabase.CreateAsset(item.sharedMesh, pathMesh);
            }

            AssetDatabase.SaveAssets();
            System.GC.Collect();
        }


        void TerrainToMesh(bool exportTrees = false, bool allTerrains = false)
        {
            meshFiltersToFix.Clear();
            if (!Directory.Exists("Assets/" + terrainManagerSettings.terrainPath))
            {
                Directory.CreateDirectory("Assets/" + terrainManagerSettings.terrainPath);
            }

            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);

            if (allTerrains)
                terrains = Terrain.activeTerrains;

            int idTerrain = 0;
            int countTerrain = terrains.Length;
            foreach (var terrain in terrains)
            {
                EditorUtility.DisplayProgressBar("Terrain mesh generation",
                    "Exporting terrain " + idTerrain + "/" + countTerrain + "\n Exporting mesh",
                    idTerrain / (float) countTerrain);
                TerrainData terrainData = terrain.terrainData;
                float[,] heightmapData = terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                    terrainData.heightmapResolution);

                float sizeX = terrainData.size.x;
                float sizeY = terrainData.size.y;
                float sizeZ = terrainData.size.z;
                float terrainTowidth = (1 / sizeX * (terrainData.heightmapResolution - 1));
                float terrainToheight = (1 / sizeZ * (terrainData.heightmapResolution - 1));

                Vector3 position = Vector3.zero;

                int lod = (int) Mathf.Pow(2, terrainManagerSettings.terrainLod);

                Vector3[,] positionArray =
                    new Vector3[heightmapData.GetLength(0) / lod + (lod == 1 ? -1 : 0) + 1 +
                                (terrainManagerSettings.addVerticesDown ? 2 : 0), heightmapData.GetLength(1) / lod +
                        (lod == 1 ? -1 : 0) + 1 + (terrainManagerSettings.addVerticesDown ? 2 : 0)];
                int addxz = +(terrainManagerSettings.addVerticesDown ? 1 : 0);


                for (int x = 0; x < heightmapData.GetLength(0); x += lod)
                {
                    //List<Vector3> positionArrayRow = new List<Vector3>();
                    for (int z = 0; z < heightmapData.GetLength(1); z += lod)
                    {
                        position.x = z / (float) terrainToheight; //, polygonHeight
                        position.y = heightmapData[x, z] * sizeY;
                        position.z = x / (float) terrainTowidth;


                        positionArray[x / lod + addxz, z / lod + addxz] =
                            new Vector4(position.x, position.y, position.z);


                        if (terrainManagerSettings.addVerticesDown)
                        {
                            if (x == 0)
                                positionArray[0, z / lod + addxz] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (x == heightmapData.GetLength(0) - 1)
                                positionArray[x / lod + 2, z / lod + addxz] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (z == 0)
                                positionArray[x / lod + 1, 0] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (z == heightmapData.GetLength(1) - 1)
                                positionArray[x / lod + addxz, z / lod + 2] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (x == 0 && z == 0)
                                positionArray[0, 0] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (x == 0 && z == heightmapData.GetLength(1) - 1)
                                positionArray[0, z / lod + 2] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (x == heightmapData.GetLength(0) - 1 && z == 0)
                                positionArray[x / lod + 2, 0] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);

                            if (x == heightmapData.GetLength(0) - 1 && z == heightmapData.GetLength(1) - 1)
                                positionArray[x / lod + 2, z / lod + 2] = new Vector4(position.x,
                                    position.y - terrainManagerSettings.verticesDownDistance, position.z);
                        }
                    }
                    //positionArray.Add(positionArrayRow);
                }


                Mesh meshTerrain = new Mesh();
                meshTerrain.indexFormat = IndexFormat.UInt32;
                List<Vector3> vertices = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<int> triangles = new List<int>();

                Vector3 normal;
                Vector3 vert;

                //Debug.Log(sizeX);
                Vector2 uv;
                for (int x = 0; x < positionArray.GetLength(0); x++)
                {
                    for (int z = 0; z < positionArray.GetLength(1); z++)
                    {
                        vert = positionArray[x, z];
                        vertices.Add(vert);
                        uv = new Vector2(vert.x / (float) sizeX, vert.z / (float) sizeZ);
                        //if (uv.x > 0.99)
                        //    uv.x = 1;
                        //if (uv.y > 0.99)
                        //    uv.y = 1;
                        //if (uv.x < 0.01)
                        //    uv.x = 0;
                        //if (uv.y < 0.01)
                        //    uv.y = 0;
                        uvs.Add(uv);


                        normal = terrainData.GetInterpolatedNormal(vert.x / (float) sizeX, vert.z / (float) sizeZ);

                        if (x == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z].x / (float) sizeX,
                                positionArray[x + 1, z].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z].x / (float) sizeX,
                                positionArray[x - 1, z].z / (float) sizeZ);

                        if (z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z + 1].x / (float) sizeX,
                                positionArray[x, z + 1].z / (float) sizeZ);

                        if (z == positionArray.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x, z - 1].x / (float) sizeX,
                                positionArray[x, z - 1].z / (float) sizeZ);

                        if (x == 0 && z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z + 1].x / (float) sizeX,
                                positionArray[x + 1, z + 1].z / (float) sizeZ);

                        if (x == 0 && z == positionArray.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x + 1, z - 1].x / (float) sizeX,
                                positionArray[x, z - 1].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1 && z == 0)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z + 1].x / (float) sizeX,
                                positionArray[x - 1, z + 1].z / (float) sizeZ);

                        if (x == positionArray.GetLength(0) - 1 && z == heightmapData.GetLength(1) - 1)
                            normal = terrainData.GetInterpolatedNormal(positionArray[x - 1, z - 1].x / (float) sizeX,
                                positionArray[x - 1, z - 1].z / (float) sizeZ);


                        if (terrainManagerSettings.addVerticesDown)
                        {
                            if (x == 1)
                                normal = terrainData.GetInterpolatedNormal(positionArray[x + 2, z].x / (float) sizeX,
                                    positionArray[x + 2, z].z / (float) sizeZ);

                            if (x == positionArray.GetLength(0) - 2)
                                normal = terrainData.GetInterpolatedNormal(positionArray[x - 2, z].x / (float) sizeX,
                                    positionArray[x - 2, z].z / (float) sizeZ);

                            if (z == 1)
                                normal = terrainData.GetInterpolatedNormal(positionArray[x, z + 2].x / (float) sizeX,
                                    positionArray[x, z + 2].z / (float) sizeZ);

                            if (z == positionArray.GetLength(1) - 2)
                                normal = terrainData.GetInterpolatedNormal(positionArray[x, z - 2].x / (float) sizeX,
                                    positionArray[x, z - 2].z / (float) sizeZ);

                            if (x == 1 && z == 1)
                                normal = terrainData.GetInterpolatedNormal(
                                    positionArray[x + 2, z + 2].x / (float) sizeX,
                                    positionArray[x + 2, z + 2].z / (float) sizeZ);

                            if (x == 1 && z == positionArray.GetLength(1) - 2)
                                normal = terrainData.GetInterpolatedNormal(
                                    positionArray[x + 2, z - 2].x / (float) sizeX,
                                    positionArray[x, z - 2].z / (float) sizeZ);

                            if (x == positionArray.GetLength(0) - 2 && z == 1)
                                normal = terrainData.GetInterpolatedNormal(
                                    positionArray[x - 2, z + 2].x / (float) sizeX,
                                    positionArray[x - 2, z + 2].z / (float) sizeZ);

                            if (x == positionArray.GetLength(0) - 2 && z == heightmapData.GetLength(1) - 2)
                                normal = terrainData.GetInterpolatedNormal(
                                    positionArray[x - 2, z - 2].x / (float) sizeX,
                                    positionArray[x - 2, z - 2].z / (float) sizeZ);
                        }


                        normals.Add(normal);
                    }
                }

                int rowPositionCount = positionArray.GetLength(1);
                for (int i = 0; i < positionArray.GetLength(1) - 1; i++)
                {
                    for (int j = 0; j < positionArray.GetLength(0) - 1; j++)
                    {
                        triangles.Add(j + i * rowPositionCount);
                        triangles.Add(j + (i + 1) * rowPositionCount);
                        triangles.Add((j + 1) + i * rowPositionCount);

                        triangles.Add((j + 1) + i * rowPositionCount);
                        triangles.Add(j + (i + 1) * rowPositionCount);
                        triangles.Add((j + 1) + (i + 1) * rowPositionCount);
                    }
                }

                meshTerrain.SetVertices(vertices);
                meshTerrain.SetTriangles(triangles, 0);
                meshTerrain.SetUVs(0, uvs);
                meshTerrain.SetNormals(normals);
                //meshTerrain.RecalculateNormals();
                meshTerrain.RecalculateTangents();
                meshTerrain.RecalculateBounds();

                EditorUtility.DisplayProgressBar("Terrain mesh generation",
                    "Exporting terrain " + idTerrain + "/" + countTerrain + "\n Exporting textures",
                    (idTerrain + 0.5f) / (float) countTerrain);
                string terrainName = terrainManagerSettings.terrainPrefixName + terrain.gameObject.name;
                GameObject meshGO = new GameObject(terrainName);

                MeshFilter meshfilter = meshGO.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();

                Texture texture = null;
                if (terrainManagerSettings.useTerrainNormal || terrainManagerSettings.useTextureNormal)
                {
                    texture = ExportNormalMapHeightMap(terrain, terrainName);
                }

                Material terrainMaterial = new Material(Shader.Find("Standard"));

                if (RenderPipelineManager.currentPipeline == null)
                {
                    terrainMaterial = new Material(Shader.Find("Standard"));
                    Texture mask;
                    Texture basemap = ExportBaseMap(terrain, terrainName, out mask);
                    terrainMaterial.SetTexture("_MainTex", basemap);


                    if (terrainManagerSettings.useTerrainNormal || terrainManagerSettings.useTextureNormal)
                    {
                        terrainMaterial.SetTexture("_BumpMap", texture);
                    }

                    terrainMaterial.SetInt("_SmoothnessTextureChannel", 1);
                    terrainMaterial.SetFloat("_Glossiness", .2f);
                    terrainMaterial.SetFloat("_GlossMapScale", 0.5f);
                    terrainMaterial.SetFloat("_Metallic", .0750f);
                    terrainMaterial.SetFloat("_Smoothness", 1f);
                    terrainMaterial.EnableKeyword("_NORMALMAP");
                    terrainMaterial.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                }
                else
                {
                    var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

                    if (srpType.Contains("HDRenderPipelineAsset"))
                    {
                        terrainMaterial = new Material(Shader.Find("HDRP/Lit"));
                        Texture mask;
                        Texture basemap = ExportBaseMapHDRP(terrain, terrainName, out mask);
                        terrainMaterial.SetTexture("_BaseColorMap", basemap);
                        terrainMaterial.SetTexture("_MaskMap", mask);
                        terrainMaterial.SetFloat("_Metallic", 1f);
                        terrainMaterial.SetFloat("_MetallicRemapMin", 0f);
                        terrainMaterial.SetFloat("_MetallicRemapMax", 1f);


                        if (terrainManagerSettings.useTerrainNormal || terrainManagerSettings.useTextureNormal)
                        {
                            terrainMaterial.SetTexture("_NormalMap", texture);
                        }

                        terrainMaterial.EnableKeyword("_NORMALMAP");
                        terrainMaterial.EnableKeyword("_MASKMAP");
                    }
                    else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                             srpType.Contains("LightweightRenderPipelineAsset"))
                    {
                        terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        Texture mask;
                        Texture basemap = ExportBaseMap(terrain, terrainName, out mask);

                        Debug.Log(basemap);

                        terrainMaterial.SetTexture("_BaseMap", basemap);
                        terrainMaterial.SetTexture("_MainTex", basemap);

                        terrainMaterial.SetTexture("_MetallicGlossMap", mask);
                        terrainMaterial.SetTexture("_OcclusionMap", mask);

                        if (terrainManagerSettings.useTerrainNormal || terrainManagerSettings.useTextureNormal)
                        {
                            terrainMaterial.SetTexture("_BumpMap", texture);
                        }

                        if (terrainManagerSettings.useMaskSmoothnessURP)
                            terrainMaterial.SetInt("_SmoothnessTextureChannel", 0);
                        else
                            terrainMaterial.SetInt("_SmoothnessTextureChannel", 1);

                        terrainMaterial.SetFloat("_Glossiness", .2f);
                        terrainMaterial.SetFloat("_GlossMapScale", 0.5f);
                        terrainMaterial.SetFloat("_Metallic", .0750f);
                        terrainMaterial.EnableKeyword("_NORMALMAP");


#if NM_URP
                        terrainMaterial.SetFloat("_Smoothness", 1f);
                        UnityEditor.Rendering.Universal.ShaderGUI.LitGUI.SetMaterialKeywords(terrainMaterial);

                        //CoreUtils.SetKeyword(terrainMaterial, "_SPECULAR_SETUP", false);

                        //CoreUtils.SetKeyword(terrainMaterial, "_METALLICSPECGLOSSMAP", true);


#endif
                    }
                }

                MaterialPropertyBlock block = new MaterialPropertyBlock();

                terrain.GetSplatMaterialPropertyBlock(block);


                string name = terrainManagerSettings.terrainPath + "/M_" + terrainName + ".mat";
                string path = "Assets/" + name;

                AssetDatabase.CreateAsset(terrainMaterial, path);
                terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);

                meshRenderer.sharedMaterial = terrainMaterial;

                meshGO.transform.position =
                    terrain.transform.position + new Vector3(0, terrainManagerSettings.yOffset, 0);
                meshfilter.sharedMesh = meshTerrain;

                if (exportTrees)
                {
                    EditorUtility.DisplayProgressBar("Terrain mesh generation",
                        "Exporting terrain " + idTerrain + "/" + countTerrain + "\n Exporting trees",
                        (idTerrain + 0.75f) / (float) countTerrain);


                    List<GameObject> trees = ExportTrees(terrain);
                    foreach (var treeBatcher in trees)
                    {
                        treeBatcher.transform.parent = meshGO.transform;
                        treeBatcher.transform.localPosition = Vector3.zero;
                    }
                }

                meshFiltersToFix.Add(meshfilter);

                AssetDatabase.Refresh();

                idTerrain++;
            }

            EditorUtility.ClearProgressBar();

            FixMeshNormals(meshFiltersToFix);


            string pathMesh;
            foreach (var item in meshFiltersToFix)
            {
                name = terrainManagerSettings.terrainPath + item.name + ".asset";
                pathMesh = "Assets/" + name;
                AssetDatabase.CreateAsset(item.sharedMesh, pathMesh);
            }

            AssetDatabase.SaveAssets();
            System.GC.Collect();
        }

        void FixMeshNormals(List<MeshFilter> meshFiltersToFix)
        {
            MeshFilter meshFilter;
            Mesh mesh;
            Vector3[] vertices;
            Vector3[] normals;
            Bounds meshBounds;

            Matrix4x4 matrix4;

            MeshFilter meshFilterSecond;
            Mesh meshSecond;
            Vector3[] verticesSecond;
            Vector3[] normalsSecond;
            Bounds meshBoundsSecond;
            Matrix4x4 matrix4Second;
            Vector3 pos;
            Vector4 posRob;
            Vector3 pos2;
            double editorTime = EditorApplication.timeSinceStartup;

            try
            {
                bool cancel = false;
                AssetDatabase.StartAssetEditing();


                for (int i = 0; i < meshFiltersToFix.Count - 1; i++)
                {
                    meshFilter = meshFiltersToFix[i];

                    mesh = meshFilter.sharedMesh;
                    vertices = mesh.vertices;
                    normals = mesh.normals;
                    meshBounds = meshFilter.GetComponent<Renderer>().bounds;
                    matrix4 = meshFilter.transform.localToWorldMatrix;


                    cancel = EditorUtility.DisplayCancelableProgressBar("Terrain mesh generation",
                        "Normals calculating " + i + "/" + meshFiltersToFix.Count,
                        (i) / (float) meshFiltersToFix.Count);

                    for (int j = i + 1; j < meshFiltersToFix.Count; j++)
                    {
                        meshFilterSecond = meshFiltersToFix[j];

                        meshBoundsSecond = meshFilterSecond.GetComponent<Renderer>().bounds;
                        Vector3 baseSize = meshBoundsSecond.size;

                        meshBoundsSecond.size = baseSize * 0.99f;
                        bool outer = meshBounds.Intersects(meshBoundsSecond);
                        meshBoundsSecond.size = baseSize * 1.01f;
                        bool inner = meshBounds.Intersects(meshBoundsSecond);


                        if (!outer && inner)
                        {
                            meshSecond = meshFilterSecond.sharedMesh;
                            verticesSecond = meshSecond.vertices;
                            normalsSecond = meshSecond.normals;
                            matrix4Second = meshFilterSecond.transform.localToWorldMatrix;


                            for (int v1 = 0; v1 < vertices.Length; v1++)
                            {
                                posRob = vertices[v1];
                                posRob.w = 1;
                                pos = matrix4 * posRob;

                                if (meshBoundsSecond.SqrDistance(pos) < 1)
                                {
                                    for (int v2 = 0; v2 < verticesSecond.Length; v2++)
                                    {
                                        if (v2 % 1000 == 0)
                                            cancel = EditorUtility.DisplayCancelableProgressBar(
                                                "Terrain mesh generation",
                                                "Normals calculating " + i + "/" + meshFiltersToFix.Count,
                                                (i + v1 / (float) vertices.Length) / (float) meshFiltersToFix.Count);

                                        posRob = verticesSecond[v2];
                                        posRob.w = 1;
                                        pos2 = matrix4Second * posRob;
                                        if ((pos - pos2).sqrMagnitude < 0.01f)
                                        {
                                            normalsSecond[v2] = normals[v1];
                                        }

                                        if (cancel)
                                            break;
                                    }

                                    meshSecond.normals = normalsSecond;
                                }

                                if (cancel)
                                    break;
                            }
                        }

                        if (cancel)
                            break;
                    }

                    mesh.RecalculateTangents();

                    if (cancel)
                        break;
                }

                EditorUtility.ClearProgressBar();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        void CalculateTerrainMaxMinHeight(Terrain terrain)
        {
            int myIndex = 0;
            int heightMapResolution = terrain.terrainData.heightmapResolution;
            float[,] rawHeights = terrain.terrainData.GetHeights(0, 0, heightMapResolution, heightMapResolution);
            float height = 0;


            float min = float.MaxValue;
            float max = float.MinValue;


            for (int y = 0; y < heightMapResolution; y++)
            {
                for (int x = 0; x < heightMapResolution; x++)
                {
                    rawHeights[y, x] = Mathf.Clamp01(rawHeights[y, x]);
                    height = rawHeights[y, x];


                    if (height > max)
                        max = height;
                    if (height < min)
                        min = height;

                    myIndex++;
                }
            }

            if (max > maxHeightTerrain)
                maxHeightTerrain = max;
            if (min < minHeightTerrain)
                minHeightTerrain = min;
        }

        Texture2D ExportNormalMapHeightMap(Terrain terrain, string terrainName)
        {
            int heightMapResolution = terrain.terrainData.heightmapResolution;
            float[,] rawHeights = terrain.terrainData.GetHeights(0, 0, heightMapResolution, heightMapResolution);


            for (int y = 0; y < heightMapResolution; y++)
            {
                for (int x = 0; x < heightMapResolution; x++)
                {
                    rawHeights[y, x] = 1 - (rawHeights[y, x] - minHeightTerrain) /
                        (float) (maxHeightTerrain - minHeightTerrain);
                }
            }


            string name = terrainManagerSettings.terrainPath + "/T_" + terrainName + "_N.png";
            string path = Application.dataPath + name;


            var extension = Path.GetExtension(path);


            Texture2D normalTerrain = GetNormalMap(rawHeights, 20 * terrainManagerSettings.terrainNormalStrength);


            Texture2D normalTextures = null;

            if (RenderPipelineManager.currentPipeline == null)
            {
                normalTextures = ExportTextureNormalmap(terrain, terrainName);
            }
            else
            {
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    normalTextures = ExportTextureNormalmapHDRP(terrain, terrainName);
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                         srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    normalTextures = ExportTextureNormalmap(terrain, terrainName);
                }
            }


            Texture2D normalFinal = new Texture2D(normalTextures.width, normalTextures.height);

            float resize = normalTextures.width / (float) normalTerrain.width;


            Vector4 colorNormalTexture = Vector4.zero;

            Vector2 blueCount;

            for (int x = 0; x < normalTextures.width; x++)
            {
                for (int y = 0; y < normalTextures.height; y++)
                {
                    if (terrainManagerSettings.useTerrainNormal && terrainManagerSettings.useTextureNormal)
                    {
                        Vector4 colorNormal =
                            normalTerrain.GetPixel(Mathf.FloorToInt(x / resize), Mathf.FloorToInt(y / resize));

                        colorNormalTexture = normalTextures.GetPixel(x, y);


                        colorNormalTexture = LinearLightAddSub(colorNormalTexture, colorNormal);

                        //colorNormalTexture.z = colorNormal.z;

                        //colorNormalTexture.z = Mathf.Sqrt(1 - Mathf.Clamp01(colorNormalTexture.x * colorNormalTexture.x + colorNormalTexture.y * colorNormalTexture.y)) * 0.5f + 0.5f;
                        blueCount = new Vector2(colorNormalTexture.x, colorNormalTexture.y);
                        blueCount = blueCount * 2 - Vector2.one;
                        colorNormalTexture.z = Mathf.Clamp01(Vector2.Dot(blueCount, blueCount));
                        colorNormalTexture.z = Mathf.Sqrt(Mathf.Sqrt(1 - colorNormalTexture.z));

                        //colorNormalTexture.Normalize();
                        colorNormalTexture.w = 1;
                    }

                    if (!terrainManagerSettings.useTerrainNormal && terrainManagerSettings.useTextureNormal)
                    {
                        colorNormalTexture = normalTextures.GetPixel(x, y);

                        blueCount = new Vector2(colorNormalTexture.x, colorNormalTexture.y);
                        blueCount = blueCount * 2 - Vector2.one;
                        colorNormalTexture.z = Mathf.Clamp01(Vector2.Dot(blueCount, blueCount));
                        colorNormalTexture.z = Mathf.Sqrt(Mathf.Sqrt(1 - colorNormalTexture.z));

                        //colorNormalTexture = LinearLightAddSub(colorNormalTexture, colorNormalTexture);
                    }

                    if (terrainManagerSettings.useTerrainNormal && !terrainManagerSettings.useTextureNormal)
                    {
                        colorNormalTexture =
                            normalTerrain.GetPixel(Mathf.FloorToInt(x / resize), Mathf.FloorToInt(y / resize));

                        blueCount = new Vector2(colorNormalTexture.x, colorNormalTexture.y);
                        blueCount = blueCount * 2 - Vector2.one;
                        colorNormalTexture.z = Mathf.Clamp01(Vector2.Dot(blueCount, blueCount));
                        colorNormalTexture.z = Mathf.Sqrt(Mathf.Sqrt(1 - colorNormalTexture.z));
                    }

                    normalFinal.SetPixel(x, y, colorNormalTexture);
                }
            }


            byte[]
                pngData = normalFinal
                    .EncodeToPNG(); // GetNormalMap(rawHeights, 20 * terrainManagerSettings.terrainNormalStrength).EncodeToPNG();
            //byte[] pngData = normalTextures.EncodeToPNG();// GetNormalMap(rawHeights, 20 * terrainManagerSettings.terrainNormalStrength).EncodeToPNG();


            File.WriteAllBytes(path, pngData);
            //Debug.Log(path);

            AssetDatabase.Refresh();
            TextureImporter importer = (TextureImporter) AssetImporter.GetAtPath("Assets/" + name);
            importer.textureType = TextureImporterType.NormalMap;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            importer.streamingMipmaps = true;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
            //importer.mipMapsPreserveCoverage = true;
            //importer.alphaTestReferenceValue = 0.85f;

            //importer.convertToNormalmap = true;
            //importer.normalmapFilter = TextureImporterNormalFilter.Sobel;
            //importer.heightmapScale = 0.2f;


            AssetDatabase.ImportAsset("Assets/" + name, ImportAssetOptions.ForceUpdate);

            AssetDatabase.Refresh();

            return (Texture2D) AssetDatabase.LoadAssetAtPath("assets" + name, typeof(Texture2D));
        }

        private Texture2D HeightMapToNormal(Texture2D sourceImage, float strength = 1f)
        {
            Mathf.Clamp(strength, 0.0f, 10.0f);
            int w = sourceImage.width;
            int h = sourceImage.height;
            Texture2D normal = new Texture2D(w, h);
            Color[] currentColors = null;
            Color[] lColors = null;
            Color[] rColors = null;
            float sampleL;
            float sampleR;
            float sampleU;
            float sampleD;
            float xVector, yVector;

            for (int x = 0; x < w; x++)
            {
                if (currentColors != null) lColors = currentColors;
                if (rColors != null) currentColors = rColors;
                else currentColors = sourceImage.GetPixels(x, 0, 1, h);
                if (lColors == null) lColors = currentColors;
                if (x < w - 1) rColors = sourceImage.GetPixels(x + 1, 0, 1, h);
                else rColors = currentColors;

                for (int y = 0; y < currentColors.Length; y++)
                {
                    sampleL = lColors[y].r * strength;
                    sampleR = rColors[y].r * strength;
                    if (y < h - 1) sampleU = currentColors[y + 1].r * strength;
                    else sampleU = currentColors[y].r * strength;
                    if (y > 0) sampleD = currentColors[y - 1].r * strength;
                    else sampleD = currentColors[y].r * strength;

                    xVector = Mathf.Clamp01((((sampleL - sampleR) + 1) * 0.5f));
                    yVector = Mathf.Clamp01((((sampleD - sampleU) + 1) * 0.5f));
                    Color col = new Color(xVector, yVector, 1f, 1f);
                    normal.SetPixel(x, y, col);
                }
            }

            normal.Apply();
            return normal;
        }

        Texture2D GetNormalMap(float[,] rawHeights, float str = 2.0f)
        {
            int width = rawHeights.GetLength(0);
            int height = rawHeights.GetLength(0);


            Texture2D normal = new Texture2D(width, height, TextureFormat.ARGB32, true);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    normal.SetPixel(x, y, new Color(rawHeights[y, x], rawHeights[y, x], rawHeights[y, x]));
                }
            }

            normal.Apply();

            normal = HeightMapToNormal(normal, str / 20f);

            normal = Resize(normal, Mathf.ClosestPowerOfTwo(normal.width), Mathf.ClosestPowerOfTwo(normal.width));

            normal.Apply(true);

            return normal;

            /*normal.anisoLevel = 9;
            normal.mipMapBias = 1;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                //using Sobel operator
                float tl, t, tr, l, right, bl, bot, br;


                int xPlus = Mathf.Clamp(x + 1, 0, width - 1);
                int xMinus = Mathf.Clamp(x - 1, 0, width - 1);
                int yPlus = Mathf.Clamp(y + 1, 0, height - 1);
                int yMinus = Mathf.Clamp(y - 1, 0, height - 1);


                tl = rawHeights[yMinus, xMinus];
                t = rawHeights[yMinus, x];
                tr = rawHeights[yMinus, xPlus];
                right = rawHeights[y, xPlus];
                br = rawHeights[yPlus, xPlus];
                bot = rawHeights[yPlus, x];
                bl = rawHeights[yPlus, xMinus];
                l = rawHeights[y, xMinus];

                // tl = rawHeights[y - 1, x - 1];
                // t = rawHeights[y - 1, x];
                // tr = rawHeights[y - 1, x + 1];
                // right = rawHeights[y, x + 1];
                // br = rawHeights[y + 1, x + 1];
                // bot = rawHeights[y + 1, x];
                // bl = rawHeights[y + 1, x - 1];
                // l = rawHeights[y, x - 1];

                //Sobel filter
                float dX = (tr + 2.0f * right + br) - (tl + 2.0f * l + bl);
                float dY = (bl + 2.0f * bot + br) - (tl + 2.0f * t + tr);
                float dZ = 1;

                Vector3 vc = new Vector3(str * dX, str * dY, dZ);
                vc.Normalize();

                normal.SetPixel(x, y, new Color((vc.x + 1f) / 2f, (vc.y + 1f) / 2f, (vc.z + 1f) / 2f, 1));
            }


            normal.Apply(true);


            int range = 1;
            if (width <= 128)
                range = 1;

            for (int x = 0; x < width; x++)
            for (int i = 0; i < range; i++)
                normal.SetPixel(x, i, normal.GetPixel(x, range));

            for (int x = 0; x < width; x++)
            for (int i = 0; i < range; i++)
                normal.SetPixel(x, height - 1 - i, normal.GetPixel(x, height - 1 - range));

            for (int y = 0; y < height; y++)
            for (int i = 0; i < range; i++)
                normal.SetPixel(i, y, normal.GetPixel(range, y));

            for (int y = 0; y < height; y++)
            for (int i = 0; i < range; i++)
                normal.SetPixel(width - 1 - i, y, normal.GetPixel(width - 1 - range, y));

            normal.Apply(true);

            /*Color[] colors = normal.GetPixels();

            Texture2D mipTexture = Resize(normal, normal.width / terrainManagerSettings.terrainNormalDetails,
                normal.width / terrainManagerSettings.terrainNormalDetails);
            mipTexture = Resize(mipTexture, normal.width, normal.width);

            Color[] colorsMipMap = mipTexture.GetPixels();


            Color col;
            Color colmm;

            for (int i = 0, j = normal.width; i < j; i += 1)
            {
                for (int k = 0, l = normal.height; k < l; k += 1)
                {
                    col = colors[i + (k * normal.width)];

                    colmm = colorsMipMap[i + (k * normal.width)];

                    colmm.r = 1 - colmm.r;
                    colmm.g = 1 - colmm.g;

                    colors[i + (k * normal.width)] = LinearLightAddSub(col, colmm);
                }
            }

            normal.SetPixels(colors);#1#


            //normal = Resize(normal, Mathf.ClosestPowerOfTwo(normal.width), Mathf.ClosestPowerOfTwo(normal.width));

            // normal.Apply(true);

            return normal;*/
        }

        Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }

        Color Overlay(Color baseCol, Color blendCol)
        {
            Color outColor = new Color();
            outColor.a = 1;
            if (baseCol.r < 0.5f && baseCol.g < 0.5f && baseCol.b < 0.5f)
            {
                outColor.r = 2 * baseCol.r * blendCol.r;
                outColor.g = 2 * baseCol.g * blendCol.g;
                outColor.b = 2 * baseCol.b * blendCol.b;
            }
            else
            {
                outColor.r = 1 - 2 * (1 - baseCol.r) * (1 - blendCol.r);
                outColor.g = 1 - 2 * (1 - baseCol.g) * (1 - blendCol.g);
                outColor.b = 1 - 2 * (1 - baseCol.b) * (1 - blendCol.b);
            }

            return outColor;
        }

        Color LinearLightAddSub(Color baseCol, Color blendCol)
        {
            Color outColor = blendCol;
            outColor.a = 1;

            if (blendCol.r > 0.5f)
            {
                outColor.r = baseCol.r + (blendCol.r * 2 - 1) * 0.5f;
            }
            else
            {
                outColor.r = baseCol.r - (1 - blendCol.r * 2) * 0.5f;
            }

            if (blendCol.g > 0.5f)
            {
                outColor.g = baseCol.g + (blendCol.g * 2 - 1) * 0.5f;
            }
            else
            {
                outColor.g = baseCol.g - (1 - blendCol.g * 2) * 0.5f;
            }

            outColor.b = blendCol.b;
            if (blendCol.b > 0.5f)
            {
                outColor.b = baseCol.b + (blendCol.b * 2 - 1) * 0.5f;
            }
            else
            {
                outColor.b = baseCol.b - (1 - blendCol.b * 2) * 0.5f;
            }

            return outColor;
        }


        void SetInstancing(bool instanced, bool allTerrains = false)
        {
            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);
            if (allTerrains)
                terrains = Terrain.activeTerrains;
            foreach (var terrain in terrains)
            {
                terrain.drawInstanced = instanced;
            }
        }


        void SetTerrainSettings(bool allTerrains = false)
        {
            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);

            if (allTerrains)
                terrains = Terrain.activeTerrains;

            foreach (var terrain in terrains)
            {
                Undo.RegisterCompleteObjectUndo(terrain, "Modify Terrain");
                Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Modify Terrain");

                SetTerrainSettings(terrain);
            }
        }

        private void SetTerrainSettings(Terrain terrain)
        {
            terrain.groupingID = terrainManagerSettings.groupingID;
            terrain.allowAutoConnect = terrainManagerSettings.allowAutoConnect;
            terrain.drawHeightmap = terrainManagerSettings.drawHeightmap;
            terrain.drawInstanced = terrainManagerSettings.drawInstanced;
            terrain.heightmapPixelError = terrainManagerSettings.heightmapPixelError;
            terrain.basemapDistance = terrainManagerSettings.basemapDistance;
            terrain.shadowCastingMode = terrainManagerSettings.shadowCastingMode;
            terrain.reflectionProbeUsage = terrainManagerSettings.reflectionProbeUsage;
            terrain.materialTemplate = terrainManagerSettings.materialTemplate;


            terrain.drawTreesAndFoliage = terrainManagerSettings.drawTreesAndFoliage;
            terrain.bakeLightProbesForTrees = terrainManagerSettings.bakeLightProbesForTrees;
            terrain.deringLightProbesForTrees = terrainManagerSettings.deringLightProbesForTrees;


            terrain.preserveTreePrototypeLayers = terrainManagerSettings.preserveTreePrototypeLayers;
            terrain.detailObjectDistance = terrainManagerSettings.detailObjectDistance;
            terrain.detailObjectDensity = terrainManagerSettings.detailObjectDensity;
            terrain.treeDistance = terrainManagerSettings.treeDistance;
            terrain.treeBillboardDistance = terrainManagerSettings.treeBillboardDistance;
            terrain.treeCrossFadeLength = terrainManagerSettings.treeCrossFadeLength;
            terrain.treeMaximumFullLODCount = terrainManagerSettings.treeMaximumFullLODCount;

            terrain.terrainData.wavingGrassStrength = terrainManagerSettings.wavingGrassStrength;
            terrain.terrainData.wavingGrassSpeed = terrainManagerSettings.wavingGrassSpeed;
            terrain.terrainData.wavingGrassAmount = terrainManagerSettings.wavingGrassAmount;
            terrain.terrainData.wavingGrassTint = terrainManagerSettings.wavingGrassTint;
        }

        void SplitTerrain(bool allTerrains = false)
        {
            if (!Directory.Exists("Assets/" + terrainManagerSettings.terrainsDataPath))
            {
                Directory.CreateDirectory("Assets/" + terrainManagerSettings.terrainsDataPath);
            }

            Undo.SetCurrentGroupName("Split Terrain");

            terrainManagerSettings.terrainsCount = terrainManagerSettings.splitSize * terrainManagerSettings.splitSize;

            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);

            if (allTerrains)
                terrains = Terrain.activeTerrains;

            var currentObjectIndex = 0;
            List<TreeInstance> treeInstancesList = new List<TreeInstance>();
            TreeInstance[] treeInstances;
            TreeInstance ti;

            foreach (var terrain in terrains)
            {
                currentObjectIndex++;


                if (terrain == null)
                {
                    continue;
                }

                var progressCaption = "Spliting terrain " + terrain.name + " (" + currentObjectIndex.ToString() +
                                      " of " + terrains.Length.ToString() + ")";

                for (int i = 0; i < terrainManagerSettings.terrainsCount; i++)
                {
                    int xI = (i % terrainManagerSettings.splitSize);
                    int yI = (i / terrainManagerSettings.splitSize);

                    float size = 1 / (float) terrainManagerSettings.splitSize;

                    string progressText = "Generating terrain split " + i + "/terrainsCount";
                    float progress = i / (float) terrainManagerSettings.terrainsCount;

                    EditorUtility.DisplayProgressBar(progressCaption, progressText, progress);

                    TerrainData td = new TerrainData();
                    GameObject tgo = Terrain.CreateTerrainGameObject(td);

                    Undo.RegisterCreatedObjectUndo(td, "Create terrain data");
                    Undo.RegisterCreatedObjectUndo(tgo, "Create terrain split");

                    tgo.name = terrain.name + " " + xI + "_" + yI;


                    Terrain newTerrain = tgo.GetComponent<Terrain>();
                    newTerrain.terrainData = td;

                    AssetDatabase.CreateAsset(td,
                        "Assets" + terrainManagerSettings.terrainsDataPath + newTerrain.name + ".asset");


                    //copy all prototypes
                    newTerrain.terrainData.terrainLayers = terrain.terrainData.terrainLayers;

                    newTerrain.terrainData.detailPrototypes = terrain.terrainData.detailPrototypes;

                    newTerrain.terrainData.treePrototypes = terrain.terrainData.treePrototypes;


                    SetTerrainSettings(newTerrain);
                    newTerrain.materialTemplate = terrain.materialTemplate;


                    Vector3 parentPosition = terrain.GetPosition();


                    float spaceShiftX = terrain.terrainData.size.z / terrainManagerSettings.splitSize;
                    float spaceShiftY = terrain.terrainData.size.x / terrainManagerSettings.splitSize;

                    float xWShift = (i % terrainManagerSettings.splitSize) * spaceShiftX;
                    float zWShift = (i / terrainManagerSettings.splitSize) * spaceShiftY;

                    //Debug.Log(xWShift + " " + zWShift);


                    td.heightmapResolution = terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize;

                    td.size = new Vector3(terrain.terrainData.size.x / terrainManagerSettings.splitSize,
                        terrain.terrainData.size.y,
                        terrain.terrainData.size.z / terrainManagerSettings.splitSize
                    );


                    float[,] terrainHeight = terrain.terrainData.GetHeights(0, 0,
                        terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

                    float[,] partHeight =
                        new float[terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize + 1,
                            terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize + 1];

                    
                    int heightShift = terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize;

                    int startX = 0;
                    int startY = 0;

                    int end = terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize + 1;

            
                    
      
                    

                    for (int x = startX; x < end; x++)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(progressCaption,
                            progressText + " (Split height)",
                            progress + (((float) x / (end - startX)) / 5f) /
                            (float) terrainManagerSettings.terrainsCount);

                        if (cancel)
                        {
                            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                            EditorUtility.ClearProgressBar();
                            return;
                        }

                        for (int y = startY; y < end; y++)
                        {
                            float ph = terrainHeight[x + heightShift * xI, y + heightShift * yI];

                            partHeight[x, y] = ph;
                            
                        }
                    }

                    newTerrain.terrainData.SetHeights(0, 0, partHeight);
                    
                    
                    
                    //hole data
                    bool[,] terrainHole = terrain.terrainData.GetHoles(0, 0,
                        terrain.terrainData.holesResolution, terrain.terrainData.holesResolution);

                    bool[,] partHole =
                        new bool[terrain.terrainData.holesResolution / terrainManagerSettings.splitSize,
                            terrain.terrainData.holesResolution / terrainManagerSettings.splitSize];

                    int endHole = terrain.terrainData.holesResolution / terrainManagerSettings.splitSize;
                    int holeShift = terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize;
                    
                    for (int x = startX; x < endHole; x++)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(progressCaption,
                            progressText + " (Split height)",
                            progress + (1 / 5f +((float) x / (endHole - startX)) / 5f) /
                            (float) terrainManagerSettings.terrainsCount);

                        if (cancel)
                        {
                            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                            EditorUtility.ClearProgressBar();
                            return;
                        }

                        for (int y = startY; y < endHole; y++)
                        {
                            partHole[x, y] =  terrainHole[x + holeShift * xI, y + holeShift * yI];
                        }
                    }
                    
                    newTerrain.terrainData.SetHoles(0, 0, partHole);

                  
                    
                    //td.holesResolution = terrain.terrainData.heightmapResolution / terrainManagerSettings.splitSize;
                    
                    
                   // terrain.terrainData.holesTexture


                    td.alphamapResolution = terrain.terrainData.alphamapResolution / terrainManagerSettings.splitSize;

                    float[,,] terrainSplat = terrain.terrainData.GetAlphamaps(0, 0,
                        terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution);

                    float[,,] partSplat =
                        new float[terrain.terrainData.alphamapResolution / terrainManagerSettings.splitSize,
                            terrain.terrainData.alphamapResolution / terrainManagerSettings.splitSize,
                            terrain.terrainData.alphamapLayers
                        ];

                    int splatShift = terrain.terrainData.alphamapResolution / terrainManagerSettings.splitSize;

                    int start = 0;
                    end = terrain.terrainData.alphamapResolution / terrainManagerSettings.splitSize;


                    for (int s = 0; s < terrain.terrainData.alphamapLayers; s++)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(progressCaption,
                            progressText + " (Processing splat " + s + ")",
                            progress + (2 / 5f + (s / (float) terrain.terrainData.alphamapLayers) / 5f) /
                            (float) terrainManagerSettings.terrainsCount);
                        if (cancel)
                        {
                            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                            EditorUtility.ClearProgressBar();
                            return;
                        }

                        for (int x = start; x < end; x++)
                        {
                            for (int y = start; y < end; y++)
                            {
                                partSplat[x, y, s] = terrainSplat[x + splatShift * xI, y + splatShift * yI, s];
                            }
                        }
                    }

                    newTerrain.terrainData.SetAlphamaps(0, 0, partSplat);

                    td.SetDetailResolution(terrain.terrainData.detailResolution / terrainManagerSettings.splitSize,
                        terrain.terrainData.detailResolutionPerPatch);
                    int detailShift = terrain.terrainData.detailResolution / terrainManagerSettings.splitSize;
                    for (int d = 0; d < terrain.terrainData.detailPrototypes.Length; d++)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(progressCaption,
                            progressText + " (Processing detail " + d + ")",
                            progress + (3 / 5f + (d / (float) terrain.terrainData.detailPrototypes.Length) / 5f) /
                            (float) terrainManagerSettings.terrainsCount);
                        if (cancel)
                        {
                            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                            EditorUtility.ClearProgressBar();
                            return;
                        }

                        int[,] terrainDetail = terrain.terrainData.GetDetailLayer(0, 0,
                            terrain.terrainData.detailResolution, terrain.terrainData.detailResolution, d);

                        int[,] partDetail = new int[detailShift, detailShift];


                        startX = startY = 0;


                        for (int x = startX; x < detailShift; x++)
                        {
                            for (int y = startY; y < detailShift; y++)
                            {
                                int detail = terrainDetail[x + detailShift * xI, y + detailShift * yI];
                                partDetail[x, y] = detail;
                            }
                        }

                        newTerrain.terrainData.SetDetailLayer(0, 0, d, partDetail);
                    }

                    float sizeCheckX = size * xI;
                    float sizeCheckX1 = size * (xI + 1);

                    float sizeCheckY = size * yI;
                    float sizeCheckY1 = size * (yI + 1);


                    treeInstances = terrain.terrainData.treeInstances;
                    treeInstancesList.Clear();
                    for (int t = 0; t < treeInstances.Length; t++)
                    {
                        if (t % 1000 == 0)
                        {
                            bool cancel = EditorUtility.DisplayCancelableProgressBar(progressCaption,
                                progressText + " (Processing trees " + t + "/" + treeInstances.Length + ")",
                                progress + (4 / 5f + ((float) t / treeInstances.Length) / 5f) /
                                (float) terrainManagerSettings.terrainsCount);
                            if (cancel)
                            {
                                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                                EditorUtility.ClearProgressBar();
                                return;
                            }
                        }

                        ti = treeInstances[t];

                        if (ti.position.x >= sizeCheckY && ti.position.x <= sizeCheckY1 &&
                            ti.position.z >= sizeCheckX && ti.position.z <= sizeCheckX1)
                        {
                            ti.position = new Vector3((ti.position.x * terrainManagerSettings.splitSize) % 1,
                                ti.position.y, (ti.position.z * terrainManagerSettings.splitSize) % 1);

                            treeInstancesList.Add(ti);
                        }
                    }

                    //Debug.Log("end");

                    newTerrain.terrainData.SetTreeInstances(treeInstancesList.ToArray(), true);

                    newTerrain.gameObject.AddComponent<TerrainCullingSystem>();


                    tgo.transform.position = parentPosition + new Vector3(tgo.transform.position.x + zWShift,
                        tgo.transform.position.y,
                        tgo.transform.position.z + xWShift);

                    /*
                    tgo.transform.position = new Vector3(tgo.transform.position.x + parentPosition.x,
                                                          tgo.transform.position.y + parentPosition.y,
                                                          tgo.transform.position.z + parentPosition.z
                                                         );
                                                          */

                    terrain.gameObject.SetActive(false);


                    AssetDatabase.SaveAssets();
                    GC.Collect();
                }


                EditorUtility.ClearProgressBar();
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        public void ExportTreesForSelectedTerrain(bool allTerrains = false)
        {
            Terrain[] terrains = Selection.GetFiltered<Terrain>(SelectionMode.TopLevel);
            if (allTerrains)
                terrains = Terrain.activeTerrains;

            foreach (var terrain in terrains)
            {
                ExportTrees(terrain);
            }
        }


        public List<GameObject> ExportTrees(Terrain _terrain)
        {
            if (!Directory.Exists("Assets/" + terrainManagerSettings.terrainPath))
            {
                Directory.CreateDirectory("Assets/" + terrainManagerSettings.terrainPath);
            }

            if (_terrain == null)
                return null;

            List<GameObject> treePrototypes = new List<GameObject>();
            foreach (var item in terrainManagerSettings.terrainTrees)
            {
                if (item.active)
                    treePrototypes.Add(item.tree);
            }

            List<GameObject> treesBatchList = new List<GameObject>();


            bool backFace = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;

            TerrainData data = _terrain.terrainData;
            float width = data.size.x;
            float height = data.size.z;
            float y = data.size.y;

            GameObject[] trees = new GameObject[data.treePrototypes.Length];
            string pathMesh;

            // Debug.Log("Exporting " + data.treeInstances.Length + " trees");
            List<TreeInstance> treeInstanceNew = new List<TreeInstance>();
            for (int tID = 0; tID < data.treePrototypes.Length; tID++)
            {
                if (!treePrototypes.Contains(data.treePrototypes[tID].prefab))
                    continue;

                //Debug.Log(tID);

                List<MeshFilter> meshFilters = new List<MeshFilter>();
                foreach (TreeInstance tree in data.treeInstances)
                {
                    if (tree.prototypeIndex == tID)
                    {
                        Vector3 position = new Vector3(tree.position.x * width, tree.position.y * y,
                            tree.position.z * height); // + _terrain.transform.position;
                        bool treeInRange = true;


                        if (treeInRange)
                        {
                            //Debug.Log(treeInRange);
                            if (trees[tree.prototypeIndex] == null)
                            {
                                GameObject treePrefab = data.treePrototypes[tree.prototypeIndex].prefab;
                                LODGroup lODGroup = treePrefab.GetComponent<LODGroup>();
                                if (lODGroup != null)
                                {
                                    LOD[] lods = lODGroup.GetLODs();
                                    trees[tree.prototypeIndex] = lods[lods.Length - 1].renderers[0].gameObject;
                                }
                                else
                                {
                                    trees[tree.prototypeIndex] = treePrefab;
                                }
                            }

                            GameObject treeG = (GameObject) GameObject.Instantiate(trees[tree.prototypeIndex]);
                            //Debug.Log(treeG.name);
                            treeG.transform.position = position;
                            treeG.transform.rotation = Quaternion.Euler(0, tree.rotation, 0);
                            treeG.transform.localScale =
                                new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
                            meshFilters.Add(treeG.GetComponent<MeshFilter>());
                        }
                    }
                }


                if (trees[tID] != null)
                {
                    //List<Matrix4x4> matrices = new List<Matrix4x4>();

                    Material treeMaterial = Instantiate(trees[tID].GetComponent<MeshRenderer>().sharedMaterial);

                    string shaderName = treeMaterial.shader.name;
                    if (shaderName.Contains("NatureManufacture") && shaderName.Contains("Cross"))
                    {
                        // Debug.Log(RenderPipelineManager.currentPipeline);
                        if (RenderPipelineManager.currentPipeline == null)
                        {
                            if (shaderName.Contains("Snow"))
                                treeMaterial.shader = Shader.Find("NatureManufacture Shaders/Trees/Cross Snow WS");
                            else
                                treeMaterial.shader = Shader.Find("NatureManufacture Shaders/Trees/Cross WS");
                        }
                        else
                        {
                            var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                            if (srpType.Contains("HDRenderPipelineAsset"))
                            {
                                if (shaderName.Contains("Snow"))
                                    treeMaterial.shader = Shader.Find("NatureManufacture/HDRP/Foliage/Cross Snow WS");
                                else
                                    treeMaterial.shader = Shader.Find("NatureManufacture/HDRP/Foliage/Cross WS");
                            }
                            else if (srpType.Contains("UniversalRenderPipelineAsset") ||
                                     srpType.Contains("LightweightRenderPipelineAsset"))
                            {
                                if (shaderName.Contains("Snow"))
                                    treeMaterial.shader = Shader.Find("NatureManufacture/URP/Foliage/Cross Snow WS");
                                else
                                    treeMaterial.shader = Shader.Find("NatureManufacture/URP/Foliage/Cross WS");
                            }
                        }
                    }

                    name = terrainManagerSettings.terrainPath + data.treePrototypes[tID].prefab.name + "_" +
                           treeMaterial.name + ".mat";
                    pathMesh = "Assets/" + name;

                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(pathMesh);
                    if (mat == null)
                        AssetDatabase.CreateAsset(treeMaterial, pathMesh);


                    //Debug.Log(parent.name);

                    List<CombineInstance> combine = new List<CombineInstance>();
                    int id = 0;
                    int i = 0;
                    int meshVertsCount = 0;
                    while (i < meshFilters.Count)
                    {
                        CombineInstance combineInstance = new CombineInstance();
                        combineInstance.mesh = meshFilters[i].sharedMesh;
                        meshVertsCount += combineInstance.mesh.vertexCount;
                        combineInstance.transform = meshFilters[i].transform.localToWorldMatrix;
                        combine.Add(combineInstance);
                        //matrices.Add(meshFilters[i].transform.localToWorldMatrix);
                        i++;

                        if (meshVertsCount > terrainManagerSettings.trainglesTreesMax)
                        {
                            GameObject parent = new GameObject(_terrain.name + "_Tree_Batch_" +
                                                               data.treePrototypes[tID].prefab.name + "_" + id);
                            parent.transform.position = _terrain.transform.position;
                            treesBatchList.Add(parent);

                            MeshFilter filter = parent.AddComponent<MeshFilter>();

                            Mesh combinedAllMesh = new Mesh();
                            if (meshVertsCount > 65000)
                                combinedAllMesh.indexFormat = IndexFormat.UInt32;

                            combinedAllMesh.CombineMeshes(combine.ToArray(), true, true);
                            filter.sharedMesh = combinedAllMesh;

                            name = terrainManagerSettings.terrainPath + filter.name + "_" + id + ".asset";
                            pathMesh = "Assets/" + name;
                            AssetDatabase.CreateAsset(filter.sharedMesh, pathMesh);

                            MeshRenderer meshRenderer = parent.AddComponent<MeshRenderer>();

                            meshRenderer.sharedMaterial = treeMaterial;
                            Undo.RegisterCreatedObjectUndo(parent, "Create object trees");
                            id++;
                            combine.Clear();
                            meshVertsCount = 0;
                        }
                    }

                    if (meshVertsCount > 0)
                    {
                        GameObject parent = new GameObject(_terrain.name + "_Tree_Batch_" +
                                                           data.treePrototypes[tID].prefab.name + "_" + id);
                        parent.transform.position = _terrain.transform.position;
                        treesBatchList.Add(parent);

                        MeshFilter filter = parent.AddComponent<MeshFilter>();

                        Mesh combinedAllMesh = new Mesh();
                        if (meshVertsCount > 65000)
                            combinedAllMesh.indexFormat = IndexFormat.UInt32;

                        combinedAllMesh.CombineMeshes(combine.ToArray(), true, true);
                        filter.sharedMesh = combinedAllMesh;

                        name = terrainManagerSettings.terrainPath + filter.name + "_" + id + ".asset";
                        pathMesh = "Assets/" + name;
                        AssetDatabase.CreateAsset(filter.sharedMesh, pathMesh);


                        MeshRenderer meshRenderer = parent.AddComponent<MeshRenderer>();

                        // Debug.Log(trees[tID].name + " " + trees[tID].GetComponent<MeshRenderer>().sharedMaterial.name);
                        meshRenderer.sharedMaterial = treeMaterial;
                        Undo.RegisterCreatedObjectUndo(parent, "Create object trees");
                    }

                    i = 0;
                    while (i < meshFilters.Count)
                    {
                        DestroyImmediate(meshFilters[i].gameObject);
                        i++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Physics.queriesHitBackfaces = backFace;
            return treesBatchList;
        }
    }
}