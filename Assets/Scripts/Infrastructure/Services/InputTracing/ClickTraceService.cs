using System.Collections.Generic;
using System.Text;
using Infrastructure.Services.Log;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Infrastructure.Services.InputTracing
{
    public class ClickTraceService : IClickTraceService, ITickable
    {
        private const float DebugRayLength = 100f;

        private readonly ILogService _logService;
        private readonly StringBuilder _builder = new StringBuilder(512);

        public ClickTraceService(ILogService logService)
        {
            _logService = logService;
        }

        public void Tick()
        {
            if (TryGetMouseClick(out Vector2 mousePosition))
            {
                TraceClick("Mouse", mousePosition, -1);
            }

            int touchCount = UnityEngine.Input.touchCount;
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began)
                {
                    continue;
                }

                TraceClick("Touch", touch.position, touch.fingerId);
            }
        }

        private static bool TryGetMouseClick(out Vector2 pointerPosition)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                pointerPosition = UnityEngine.Input.mousePosition;
                return true;
            }

            pointerPosition = default;
            return false;
        }

        private void TraceClick(string source, Vector2 screenPosition, int pointerId)
        {
            Camera worldCamera = Camera.main;

            _builder.Clear();
            _builder.Append("[ClickTrace] source=").Append(source);
            if (pointerId >= 0)
            {
                _builder.Append(" pointerId=").Append(pointerId);
            }

            _builder.Append(" screen=").Append(screenPosition);

            AppendUiHits(screenPosition, pointerId, worldCamera);

            if (worldCamera == null)
            {
                _builder.Append(" camera=<null>");
                //_logService.LogWarning(_builder.ToString());
                return;
            }

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            Debug.DrawRay(ray.origin, ray.direction * DebugRayLength, Color.red, 1.5f);

            _builder.Append(" camera=").Append(worldCamera.name);
            _builder.Append(" rayOrigin=").Append(ray.origin);
            _builder.Append(" rayDirection=").Append(ray.direction);

            Append3DHits(ray);
            Append2DHits(ray);

            //_logService.Log(_builder.ToString());
        }

        private void AppendUiHits(Vector2 screenPosition, int pointerId, Camera worldCamera)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                _builder.Append(" uiHits=<no_event_system>");
                return;
            }

            PointerEventData pointerEventData = new PointerEventData(eventSystem)
            {
                position = screenPosition,
                pointerId = pointerId
            };

            var results = new List<RaycastResult>();
            try
            {
                eventSystem.RaycastAll(pointerEventData, results);
                _builder.Append(" uiHits=").Append(results.Count);

                for (int i = 0; i < results.Count; i++)
                {
                    RaycastResult result = results[i];
                    _builder.Append(" | ui[")
                        .Append(i)
                        .Append("]=")
                        .Append(result.gameObject.name)
                        .Append("@")
                        .Append(GetHierarchyPath(result.gameObject.transform));
                }
            }
            finally
            {
                results.Clear();
            }
        }

        private void Append3DHits(Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            _builder.Append(" 3dHits=").Append(hits.Length);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                _builder.Append(" | 3d[")
                    .Append(i)
                    .Append("]=")
                    .Append(hit.collider.name)
                    .Append("@")
                    .Append(GetHierarchyPath(hit.collider.transform))
                    .Append(" dist=")
                    .Append(hit.distance.ToString("F3"))
                    .Append(" point=")
                    .Append(hit.point);
            }
        }

        private void Append2DHits(Ray ray)
        {
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
            _builder.Append(" 2dHits=").Append(hits.Length);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                _builder.Append(" | 2d[")
                    .Append(i)
                    .Append("]=")
                    .Append(hit.collider.name)
                    .Append("@")
                    .Append(GetHierarchyPath(hit.collider.transform))
                    .Append(" dist=")
                    .Append(hit.distance.ToString("F3"))
                    .Append(" point=")
                    .Append(hit.point);
            }
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return "<null>";
            }

            StringBuilder pathBuilder = new StringBuilder(target.name);
            Transform current = target.parent;

            while (current != null)
            {
                pathBuilder.Insert(0, '/');
                pathBuilder.Insert(0, current.name);
                current = current.parent;
            }

            return pathBuilder.ToString();
        }
    }
}
