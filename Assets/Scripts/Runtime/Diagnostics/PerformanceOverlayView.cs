using System;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class PerformanceOverlayView : MonoBehaviour
    {
        private const float Margin = 8f;
        private const float Megabyte = 1024f * 1024f;
        private const float Kilobyte = 1024f;

        [SerializeField, Min(0.05f)] private float _interval = 0.5f;
        [SerializeField, Min(8)] private int _fontSize = 20;
        [SerializeField] private bool _open = true;

        private FrameSampler _sampler;
        private StringBuilder _text;
        private GUIContent _body;
        private GUIContent _header;
        private GUIStyle _style;

        private ProfilerRecorder _mainThread;
        private ProfilerRecorder _systemMemory;
        private ProfilerRecorder _heapMemory;
        private ProfilerRecorder _allocated;

        private int _collections;

        private void OnEnable()
        {
            _sampler = new FrameSampler(_interval);
            _text = new StringBuilder(256);
            _body = new GUIContent(string.Empty);
            _header = new GUIContent(string.Empty);
            _collections = GC.CollectionCount(0);

            _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            _systemMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _heapMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

            Rebuild();
        }

        private void OnDisable()
        {
            _mainThread.Dispose();
            _systemMemory.Dispose();
            _heapMemory.Dispose();
            _allocated.Dispose();

            _style = null;
        }

        private void Update()
        {
            if (_sampler.Add(Time.unscaledDeltaTime))
            {
                Rebuild();
            }
        }

        private void Rebuild()
        {
            int collections = GC.CollectionCount(0);
            int sinceLast = collections - _collections;
            _collections = collections;

            _header.text = _open ? "Stats  [-]" : "Stats  [+]  " + _sampler.Fps.ToString("F0") + " fps";

            _text.Length = 0;
            _text.Append("FPS      ").Append(_sampler.Fps.ToString("F1")).Append('\n');
            _text.Append("Frame    ").Append(_sampler.AverageMs.ToString("F2")).Append(" ms\n");
            _text.Append("Worst    ").Append(_sampler.WorstMs.ToString("F2")).Append(" ms\n");
            _text.Append("CPU      ").Append(Milliseconds(_mainThread)).Append('\n');
            _text.Append("RAM      ").Append(Megabytes(_systemMemory)).Append('\n');
            _text.Append("GC heap  ").Append(Megabytes(_heapMemory)).Append('\n');
            _text.Append("Alloc    ").Append(Kilobytes(_allocated)).Append("/frame\n");
            _text.Append("Collects ").Append(sinceLast).Append(" / ").Append(_interval.ToString("F1")).Append('s');

            _body.text = _text.ToString();
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(8, 8, 6, 6),
                    richText = false
                };

                _style.normal.textColor = Color.white;
            }

            float line = _fontSize * 1.4f;
            float width = _fontSize * 14f;
            float height = _open ? line * 9.5f : line * 1.6f;

            Rect area = new Rect(Margin, Margin, width, height);
            GUI.Box(area, GUIContent.none);

            if (GUI.Button(new Rect(area.x, area.y, area.width, line * 1.4f), _header, _style))
            {
                _open = !_open;
                Rebuild();
            }

            if (!_open)
            {
                return;
            }

            GUI.Label(new Rect(area.x, area.y + line * 1.4f, area.width, area.height - line * 1.4f), _body, _style);
        }

        private static string Milliseconds(ProfilerRecorder recorder)
        {
            if (!recorder.Valid || recorder.Count == 0)
            {
                return "n/a";
            }

            double total = 0d;

            for (int i = 0; i < recorder.Count; i++)
            {
                total += recorder.GetSample(i).Value;
            }

            return (total / recorder.Count * 1e-6d).ToString("F2") + " ms";
        }

        private static string Megabytes(ProfilerRecorder recorder)
        {
            return recorder.Valid ? (recorder.LastValue / Megabyte).ToString("F1") + " MB" : "n/a";
        }

        private static string Kilobytes(ProfilerRecorder recorder)
        {
            return recorder.Valid ? (recorder.LastValue / Kilobyte).ToString("F1") + " KB" : "n/a";
        }
    }
}
