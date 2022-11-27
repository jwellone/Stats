using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using Unity.Profiling;

#nullable enable

namespace jwellone.Debug
{
    public class Stats : MonoBehaviour
    {
        public enum Layout
        {
            None,
            Statistics,
            Memory,
            All
        }

        [SerializeField] Layout _layout = Layout.Statistics;
        [SerializeField] RectTransform _root = null!;
        [SerializeField] Text _text = null!;
        [SerializeField] Image _image = null!;

        static Stats? _instance = null;

        float _avgTime;
        float _prevRealTime;
        RectTransform? _rectTransformForText;
        readonly FrameTiming[] _frameTimings = new FrameTiming[1];

        ProfilerRecorder _prDynamicBathcedDrawCallsCount;
        ProfilerRecorder _prDynamicBatchesCount;
        ProfilerRecorder _prStaticBatchedDrawCallsCount;
        ProfilerRecorder _prStaticBatchedCount;
        ProfilerRecorder _prInstancedBatchedDrawCallsCount;
        ProfilerRecorder _prInstancedBatchedCount;

        ProfilerRecorder _prBatchesCount;
        ProfilerRecorder _prTrianglesCount;
        ProfilerRecorder _prVerticesCount;
        ProfilerRecorder _prSetPassCallsCount;
        ProfilerRecorder _prShadowCastersCount;
        ProfilerRecorder _prVisibleSkinnedMeshesCount;

        Recorder _rAnimationUpdate = null!;
        Recorder _rAnimatorsUpdate = null!;

        ProfilerRecorder _prTotalUsedMemory;
        ProfilerRecorder _prGCUsedMemory;
        ProfilerRecorder _prGfxUsedMemory;
        ProfilerRecorder _prAudioUsedMemory;
        ProfilerRecorder _prVideoUsedMemory;
        ProfilerRecorder _prProfilerUsedMemory;

        ProfilerRecorder _prTotalReservedMemory;
        ProfilerRecorder _prGCReservedMemory;
        ProfilerRecorder _prGfxReservedMemory;
        ProfilerRecorder _prAudioReservedMemory;
        ProfilerRecorder _prVideoReservedMemory;
        ProfilerRecorder _prProfilerReservedMemory;
        ProfilerRecorder _prSystemUsedMemory;

        ProfilerRecorder _prTextureCount;
        ProfilerRecorder _prTextureMemory;
        ProfilerRecorder _prMeshCount;
        ProfilerRecorder _prMeshMemory;
        ProfilerRecorder _prMaterialCount;
        ProfilerRecorder _prMaterialMemory;
        ProfilerRecorder _prAnimationClipCount;
        ProfilerRecorder _prAnimationClipMemory;
        ProfilerRecorder _prAssetCount;
        ProfilerRecorder _prGameObjectCount;
        ProfilerRecorder _prSceneObjectCount;
        ProfilerRecorder _prObjectCount;
        ProfilerRecorder _prGCAllocationInFrameCount;
        ProfilerRecorder _prGCAllocatedInFrame;

        RectTransform rectTransformForText
        {
            get
            {
                if (_rectTransformForText == null)
                {
                    _rectTransformForText = _text.GetComponent<RectTransform>();
                }

                return _rectTransformForText;
            }
        }

        float cpuFrameTime
        {
            get;
            set;
        }

        float gpuFrameTime
        {
            get;
            set;
        }

        public static Layout layout
        {
            get => (_instance == null) ? Layout.None : _instance._layout;
            set
            {
                if (_instance != null) { _instance._layout = value; }
            }
        }

        void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        void Start()
        {
            _rAnimationUpdate = Recorder.Get("Animation.Update");
            _rAnimatorsUpdate = Recorder.Get("Animators.Update");

            _root.anchoredPosition = Vector2.zero;
            _root.sizeDelta = Vector2.zero;
            _root.anchorMin = new Vector2(Screen.safeArea.xMin / Screen.width, Screen.safeArea.yMin / Screen.height);
            _root.anchorMax = new Vector2(Screen.safeArea.xMax / Screen.width, Screen.safeArea.yMax / Screen.height);
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void OnEnable()
        {
            _prDynamicBathcedDrawCallsCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Dynamic Batched Draw Calls Count");
            _prDynamicBatchesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Dynamic Batches Count");
            _prStaticBatchedDrawCallsCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Static Batched Draw Calls Count");
            _prStaticBatchedCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Static Batches Count");
            _prInstancedBatchedDrawCallsCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Instanced Batched Draw Calls Count");
            _prInstancedBatchedCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Instanced Batches Count");

            _prBatchesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");

            _prTrianglesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            _prVerticesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");

            _prSetPassCallsCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _prShadowCastersCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
            _prVisibleSkinnedMeshesCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Visible Skinned Meshes Count");

            _prTotalUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _prGCUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            _prGfxUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            _prAudioUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Used Memory");
            _prVideoUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Used Memory");
            _prProfilerUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Profiler Used Memory");

            _prTotalReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            _prGCReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _prGfxReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Reserved Memory");
            _prAudioReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Reserved Memory");
            _prVideoReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Reserved Memory");
            _prProfilerReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Profiler Reserved Memory");

            _prSystemUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

            _prTextureCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Count");
            _prTextureMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            _prMeshCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Count");
            _prMeshMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            _prMaterialCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Count");
            _prMaterialMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Memory");
            _prAnimationClipCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "AnimationClip Count");
            _prAnimationClipMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "AnimationClip Memory");
            _prAssetCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Asset Count");
            _prGameObjectCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Game Object Count");
            _prSceneObjectCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Scene Object Count");
            _prObjectCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Object Count");
            _prGCAllocationInFrameCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocation In Frame Count");
            _prGCAllocatedInFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        }

        void OnDisable()
        {
            _prDynamicBathcedDrawCallsCount.Dispose();
            _prDynamicBatchesCount.Dispose();
            _prStaticBatchedDrawCallsCount.Dispose();
            _prStaticBatchedCount.Dispose();
            _prInstancedBatchedDrawCallsCount.Dispose();
            _prInstancedBatchedCount.Dispose();

            _prBatchesCount.Dispose();

            _prTrianglesCount.Dispose();
            _prVerticesCount.Dispose();

            _prSetPassCallsCount.Dispose();
            _prShadowCastersCount.Dispose();
            _prVisibleSkinnedMeshesCount.Dispose();

            _prTotalUsedMemory.Dispose();
            _prGCUsedMemory.Dispose();
            _prGfxUsedMemory.Dispose();
            _prAudioUsedMemory.Dispose();
            _prVideoUsedMemory.Dispose();
            _prProfilerUsedMemory.Dispose();

            _prTotalReservedMemory.Dispose();
            _prGCReservedMemory.Dispose();
            _prGfxReservedMemory.Dispose();
            _prAudioReservedMemory.Dispose();
            _prVideoReservedMemory.Dispose();
            _prProfilerReservedMemory.Dispose();
            _prSystemUsedMemory.Dispose();

            _prTextureCount.Dispose();
            _prTextureMemory.Dispose();
            _prMeshCount.Dispose();
            _prMeshMemory.Dispose();
            _prMaterialCount.Dispose();
            _prMaterialMemory.Dispose();
            _prAnimationClipCount.Dispose();
            _prAnimationClipMemory.Dispose();
            _prAssetCount.Dispose();
            _prGameObjectCount.Dispose();
            _prSceneObjectCount.Dispose();
            _prObjectCount.Dispose();
            _prGCAllocationInFrameCount.Dispose();
            _prGCAllocatedInFrame.Dispose();
        }

        void Update()
        {
            if (_layout == Layout.None)
            {
                _root.gameObject.SetActive(false);
                return;
            }

            _root.gameObject.SetActive(true);

            FrameTimingManager.CaptureFrameTimings();
            var num = FrameTimingManager.GetLatestTimings((uint)_frameTimings.Length, _frameTimings);
            if (num > 0)
            {
                var frame = _frameTimings[0];
                cpuFrameTime = (float)(frame.cpuFrameTime * 1000);
                gpuFrameTime = (float)(frame.gpuFrameTime * 1000);
            }

            var time = Time.realtimeSinceStartup;
            _avgTime *= (1f - 0.05f);
            _avgTime += (time - _prevRealTime) * 0.05f;
            _prevRealTime = time;

            var isStatistics = _layout == Layout.All || _layout == Layout.Statistics;
            var sb = new System.Text.StringBuilder();

            if (isStatistics)
            {
                sb.Append(" Graphics:\t\t\t\t").Append((1 / _avgTime).ToString("F1")).Append("FPS(").Append((_avgTime * 1000f).ToString("F1")).AppendLine("ms)");
                sb.Append($"  CPU: main ").Append(cpuFrameTime.ToString("F1")).Append("ms render thread ").Append(gpuFrameTime.ToString("F1")).AppendLine("ms  ");

                var batchesSavedByDynamicBatching = _prDynamicBathcedDrawCallsCount.LastValue - _prDynamicBatchesCount.LastValue;
                var batchesSavedByStaticBatching = _prStaticBatchedDrawCallsCount.LastValue - _prStaticBatchedCount.LastValue;
                var batchesSavedByInstancing = _prInstancedBatchedDrawCallsCount.LastValue - _prInstancedBatchedCount.LastValue;
                sb.Append("  Batches: ").Append(_prBatchesCount.LastValue).Append("\t\tSaved by batching: ").AppendLine((batchesSavedByDynamicBatching + batchesSavedByStaticBatching + batchesSavedByInstancing).ToString());

                sb.Append("  Tris: ").Append(MakeNumberText(_prTrianglesCount.LastValue)).Append("\t\t\tVerts: ").AppendLine(MakeNumberText(_prVerticesCount.LastValue));
                sb.Append("  SetPass calls: ").Append(_prSetPassCallsCount.LastValue).Append("\tShadow casters: ").AppendLine(_prShadowCastersCount.LastValue.ToString());
                sb.Append("  Screen: ").Append(Screen.width).Append("x").Append(Screen.height).AppendLine("");

                sb.Append("  Visible skinned meshes:").AppendLine(_prVisibleSkinnedMeshesCount.LastValue.ToString());
                sb.Append("  Animation components playing:").AppendLine(_rAnimationUpdate.sampleBlockCount.ToString());
                sb.Append("  Animator components playing:").Append(_rAnimatorsUpdate.sampleBlockCount.ToString());
            }

            if (_layout == Layout.All || _layout == Layout.Memory)
            {
                if (isStatistics)
                {
                    sb.AppendLine().AppendLine();
                }

                sb.AppendLine(" Memory");
                sb.Append("  Total Used: ").Append(MakeSizeText(_prTotalUsedMemory.LastValue));
                sb.Append("  GC: ").Append(MakeSizeText(_prGCUsedMemory.LastValue));
                sb.Append("  Gfx: ").Append(MakeSizeText(_prGfxUsedMemory.LastValue));
                sb.Append("  Aduio: ").Append(MakeSizeText(_prAudioUsedMemory.LastValue));
                sb.Append("  Video: ").Append(MakeSizeText(_prVideoUsedMemory.LastValue));
                sb.Append("  Profiler: ").Append(MakeSizeText(_prProfilerUsedMemory.LastValue));
                sb.Append("  ");
                sb.AppendLine();

                sb.Append("  Total Reserved: ").Append(MakeSizeText(_prTotalReservedMemory.LastValue));
                sb.Append("  GC: ").Append(MakeSizeText(_prGCReservedMemory.LastValue));
                sb.Append("  Gfx: ").Append(MakeSizeText(_prGfxReservedMemory.LastValue));
                sb.Append("  Aduio: ").Append(MakeSizeText(_prAudioReservedMemory.LastValue));
                sb.Append("  Video: ").Append(MakeSizeText(_prVideoReservedMemory.LastValue));
                sb.Append("  Profiler: ").Append(MakeSizeText(_prProfilerReservedMemory.LastValue));
                sb.Append("  ");

                sb.AppendLine();
                sb.Append("  System: ").AppendLine(MakeSizeText(_prSystemUsedMemory.LastValue));
                sb.AppendLine();

                sb.Append("  Textures:").Append(_prTextureCount.LastValue).Append("/").AppendLine(MakeSizeText(_prTextureMemory.LastValue));
                sb.Append("  Meshes:").Append(_prMeshCount.LastValue).Append("/").AppendLine(MakeSizeText(_prMeshMemory.LastValue));
                sb.Append("  Materials:").Append(_prMaterialCount.LastValue).Append("/").AppendLine(MakeSizeText(_prMaterialMemory.LastValue));
                sb.Append("  AnimationClips:").Append(_prAnimationClipCount.LastValue).Append("/").AppendLine(MakeSizeText(_prAnimationClipMemory.LastValue));
                sb.Append("  Asset Count:").AppendLine(_prAssetCount.LastValue.ToString());
                sb.Append("  Game Object Count:").AppendLine(_prGameObjectCount.LastValue.ToString());
                sb.Append("  Scene Object Count:").AppendLine(_prSceneObjectCount.LastValue.ToString());
                sb.Append("  Object Count:").AppendLine(_prObjectCount.LastValue.ToString());

                sb.Append("  GC Allocation In Frame:").Append(string.Format("{0, 4}", _prGCAllocationInFrameCount.LastValue)).Append("/").Append(MakeSizeText(_prGCAllocatedInFrame.LastValue));
            }

            _text.text = sb.ToString();

            var rectForText = rectTransformForText.rect;
            _image.rectTransform.sizeDelta = new Vector2(rectForText.width, rectForText.height);
        }

        string MakeNumberText(long value)
        {
            if (value >= 1000000)
            {
                return (value * 0.000001f).ToString("F1") + "M";
            }

            else if (value >= 1000)
            {
                return (value * 0.001f).ToString("F1") + "k";
            }

            return value.ToString();
        }

        string MakeSizeText(long value)
        {
            if (value >= 1024 * 1024 * 1024)
            {
                return (value / 1024f / 1024f / 1024f).ToString("F2") + "GB";
            }
            else if (value >= 1024 * 1024)
            {
                return (value / 1024f / 1024f).ToString("F1") + "MB";
            }
            else if (value >= 1024)
            {
                return (value / 1024f).ToString("F1") + "KB";
            }

            return value + "byte";
        }

        public static void Show()
        {
            _instance?._root.gameObject.SetActive(true);
        }

        public static void Hide()
        {
            _instance?._root.gameObject.SetActive(false);
        }
    }
}