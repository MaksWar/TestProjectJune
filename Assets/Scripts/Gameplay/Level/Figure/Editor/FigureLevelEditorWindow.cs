using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gameplay.Level.Models.Public;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Level.Editor
{
    public class FigureLevelEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Figure Level Editor";
        private const string MenuPath = "Tools/Level/Figure Level Editor";
        private const string AssetPathPrefix = "Assets/";
        private const string SavePathRoot = "Configs/Levels";
        private const string LevelCatalogDirectory = "AddressableAssets/InBuild/Levels";
        private const string LevelCatalogAssetName = "LevelCatalog.asset";
        private const int BezierSamplesPerSegment = 24;
        private const float DefaultHandleLength = 0.5f;
        private const float MinimumHandleLength = 0f;

        [SerializeField] private string levelId = "level_001";
        [SerializeField] private Sprite sprite;
        [SerializeField] private bool syncFigureIdWithSprite = true;
        [SerializeField] private string figureId;
        [SerializeField] private FigureType figureType = FigureType.Letter;
        [SerializeField] private Color viewColor = Color.white;
        [SerializeField] private string savePath;
        [SerializeField] private PathEntryType newPathType = PathEntryType.Linear;
        [SerializeField] private List<EditablePathEntry> paths = new();
        [SerializeField] private bool showSceneHandles = true;
        [SerializeField] private string currentLevelAssetPath;

        private GameObject previewRoot;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            FigureLevelEditorWindow window = GetWindow<FigureLevelEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DrawSceneHandles;
            EnsureSavePathInitialized();
            EnsureLevelIdInitialized();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneHandles;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawLevelInfo();
            EditorGUILayout.Space(8);
            DrawPathEditor();
            EditorGUILayout.Space(8);
            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawLevelInfo()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);

            levelId = EditorGUILayout.TextField("Level ID", levelId);

            EditorGUI.BeginChangeCheck();
            figureType = (FigureType)EditorGUILayout.EnumPopup("Figure Type", figureType);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSavePathFromFigureType();
                UpdateLevelIdFromFigureType();
            }

            savePath = EditorGUILayout.TextField("Save Path", NormalizeDisplayedSavePath(savePath));

            EditorGUI.BeginChangeCheck();
            sprite = (Sprite)EditorGUILayout.ObjectField("Sprite Selector", sprite, typeof(Sprite), false);
            if (EditorGUI.EndChangeCheck() && syncFigureIdWithSprite)
            {
                figureId = sprite != null ? sprite.name : string.Empty;
            }

            syncFigureIdWithSprite = EditorGUILayout.Toggle("Sync Figure ID", syncFigureIdWithSprite);

            using (new EditorGUI.DisabledScope(syncFigureIdWithSprite))
            {
                figureId = EditorGUILayout.TextField("Figure ID", figureId);
            }

            if (syncFigureIdWithSprite && sprite != null)
            {
                figureId = sprite.name;
            }

            viewColor = EditorGUILayout.ColorField("View Color", viewColor);

            if (string.IsNullOrWhiteSpace(levelId))
            {
                EditorGUILayout.HelpBox("Level ID is required for save filename and LevelEntry.LevelID.", MessageType.Warning);
            }

            if (string.IsNullOrWhiteSpace(savePath))
            {
                EditorGUILayout.HelpBox("Save Path is required. It will be reset from Figure Type before saving.", MessageType.Warning);
            }

            if (sprite == null)
            {
                EditorGUILayout.HelpBox("Sprite is optional for JSON, but required for scene preview.", MessageType.Info);
            }
        }

        private void DrawPathEditor()
        {
            EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                newPathType = (PathEntryType)EditorGUILayout.EnumPopup("New Path Type", newPathType);

                if (GUILayout.Button("Add Path", GUILayout.Width(110)))
                {
                    AddPath(newPathType);
                }
            }

            if (paths.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one path. Linear paths connect points directly. Bezier paths use every point as an anchor and draw a smooth curve through them.", MessageType.Info);
            }

            for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                EditablePathEntry path = paths[pathIndex];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                using (new EditorGUILayout.HorizontalScope())
                {
                    path.Foldout = EditorGUILayout.Foldout(path.Foldout, $"Path {pathIndex + 1}", true);

                    if (GUILayout.Button("Up", GUILayout.Width(45)))
                    {
                        MovePath(pathIndex, -1);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button("Down", GUILayout.Width(55)))
                    {
                        MovePath(pathIndex, 1);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        paths.RemoveAt(pathIndex);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUIUtility.ExitGUI();
                    }
                }

                if (path.Foldout)
                {
                    path.Type = (PathEntryType)EditorGUILayout.EnumPopup("Path Type", path.Type);
                    path.Closed = EditorGUILayout.Toggle("Closed Path", path.Closed);
                    DrawPathHelper(path);

                    int pointCount = Mathf.Max(0, EditorGUILayout.IntField("Point Count", path.Points.Count));
                    ResizePointList(path.Points, pointCount);

                    for (int pointIndex = 0; pointIndex < path.Points.Count; pointIndex++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditablePathPoint point = path.Points[pointIndex];
                            point.Position = EditorGUILayout.Vector2Field($"Point {pointIndex + 1}", point.Position);

                            if (GUILayout.Button("+", GUILayout.Width(24)))
                            {
                                EditablePathPoint nextPoint = CreatePoint(point.Position + Vector2.right, point.Angle, point.HandleLength);
                                path.Points.Insert(pointIndex + 1, nextPoint);
                                GUIUtility.ExitGUI();
                            }

                            if (GUILayout.Button("-", GUILayout.Width(24)))
                            {
                                path.Points.RemoveAt(pointIndex);
                                GUIUtility.ExitGUI();
                            }
                        }

                        if (path.Type == PathEntryType.Bezier)
                        {
                            EditablePathPoint point = path.Points[pointIndex];
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(18);
                                point.Angle = EditorGUILayout.FloatField("Angle", point.Angle);
                                point.HandleLength = Mathf.Max(MinimumHandleLength, EditorGUILayout.FloatField("Handle Length", point.HandleLength));
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add Point"))
                        {
                            AddPoint(path);
                        }

                        if (GUILayout.Button("Clear Points"))
                        {
                            path.Points.Clear();
                        }
                    }

                    string validation = ValidatePath(path);
                    if (string.IsNullOrEmpty(validation) == false)
                    {
                        EditorGUILayout.HelpBox(validation, MessageType.Warning);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            showSceneHandles = EditorGUILayout.Toggle("Show Scene Gizmos", showSceneHandles);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Path Line With Gizmo", GUILayout.Height(32)))
                {
                    CreateOrRefreshPreview();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import Level", GUILayout.Height(28)))
                {
                    ImportLevel();
                }

                if (GUILayout.Button("Save Level", GUILayout.Height(28)))
                {
                    SaveLevel();
                }
            }

            if (string.IsNullOrWhiteSpace(currentLevelAssetPath) == false)
            {
                EditorGUILayout.LabelField("Current File", currentLevelAssetPath);
            }
        }

        private static void DrawPathHelper(EditablePathEntry path)
        {
            path.HelperType = (PathHelperType)EditorGUILayout.EnumPopup("Path Helper", path.HelperType);

            switch (path.HelperType)
            {
                case PathHelperType.Circle:
                    DrawCircleHelper(path);
                    break;
                case PathHelperType.LinearAddition:
                    DrawLinearAdditionHelper(path);
                    break;
            }
        }

        private static void DrawCircleHelper(EditablePathEntry path)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            path.CircleCenter = EditorGUILayout.Vector2Field("Circle Center", path.CircleCenter);
            path.CircleDistanceByCenter = Mathf.Max(0f, EditorGUILayout.FloatField("Distance By Center", path.CircleDistanceByCenter));
            path.CirclePointCount = EditorGUILayout.IntPopup(
                "Count Of Points",
                NormalizeCirclePointCount(path.CirclePointCount),
                new[] { "4", "8", "16" },
                new[] { 4, 8, 16 });

            if (GUILayout.Button("Calculate Points"))
            {
                CalculateCirclePoints(path);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawLinearAdditionHelper(EditablePathEntry path)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            path.LinearStartPoint = EditorGUILayout.Vector2Field("Start Point", path.LinearStartPoint);
            path.LinearFinalPoint = EditorGUILayout.Vector2Field("Final Point", path.LinearFinalPoint);
            path.LinearPointCount = Mathf.Max(2, EditorGUILayout.IntField("Target Count", path.LinearPointCount));

            if (GUILayout.Button("Calculate Points"))
            {
                CalculateLinearAdditionPoints(path);
            }

            EditorGUILayout.EndVertical();
        }

        private static int NormalizeCirclePointCount(int pointCount)
        {
            return pointCount switch
            {
                4 => 4,
                16 => 16,
                _ => 8
            };
        }

        private static void CalculateCirclePoints(EditablePathEntry path)
        {
            int pointCount = NormalizeCirclePointCount(path.CirclePointCount);
            float radius = path.CircleDistanceByCenter;

            path.Closed = true;
            path.Points.Clear();

            for (int i = 0; i < pointCount; i++)
            {
                float angle = i * 360f / pointCount;
                Vector2 direction = GetDirection(angle);
                Vector2 position = path.CircleCenter + direction * radius;
                float tangentAngle = angle + 90f;
                float handleLength = CalculateCircleHandleLength(radius, pointCount);

                path.Points.Add(CreatePoint(position, tangentAngle, handleLength));
            }
        }

        private static float CalculateCircleHandleLength(float radius, int pointCount)
        {
            if (radius <= 0f || pointCount <= 0)
            {
                return DefaultHandleLength;
            }

            return 4f / 3f * Mathf.Tan(Mathf.PI / (2f * pointCount)) * radius;
        }

        private static void CalculateLinearAdditionPoints(EditablePathEntry path)
        {
            int pointCount = Mathf.Max(2, path.LinearPointCount);
            Vector2 startPoint = path.LinearStartPoint;
            Vector2 finalPoint = path.LinearFinalPoint;
            Vector2 direction = finalPoint - startPoint;
            float angle = direction.sqrMagnitude > 0.0001f
                ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
                : 0f;
            float handleLength = pointCount > 1
                ? direction.magnitude / (pointCount - 1) * 0.35f
                : DefaultHandleLength;

            path.Closed = false;
            path.Points.Clear();

            for (int i = 0; i < pointCount; i++)
            {
                float t = i / (float)(pointCount - 1);
                Vector2 position = Vector2.Lerp(startPoint, finalPoint, t);
                path.Points.Add(CreatePoint(position, angle, handleLength));
            }
        }

        private void AddPath(PathEntryType type)
        {
            EditablePathEntry path = new()
            {
                Type = type,
                Foldout = true
            };

            if (type == PathEntryType.Linear)
            {
                path.Points.Add(CreatePoint(new Vector2(-1f, 0f), 0f));
                path.Points.Add(CreatePoint(new Vector2(1f, 0f), 0f));
            }
            else
            {
                path.Points.Add(CreatePoint(new Vector2(-1.5f, 0f), 45f));
                path.Points.Add(CreatePoint(new Vector2(-0.75f, 1f), 0f));
                path.Points.Add(CreatePoint(new Vector2(0.75f, 1f), 0f));
                path.Points.Add(CreatePoint(new Vector2(1.5f, 0f), -45f));
            }

            paths.Add(path);
        }

        private static EditablePathPoint CreatePoint(Vector2 position, float angle, float handleLength = DefaultHandleLength)
        {
            return new EditablePathPoint
            {
                Position = position,
                Angle = angle,
                HandleLength = handleLength
            };
        }

        private static void AddPoint(EditablePathEntry path)
        {
            EditablePathPoint point = path.Points.Count > 0
                ? CreatePoint(path.Points[^1].Position + Vector2.right, path.Points[^1].Angle, path.Points[^1].HandleLength)
                : CreatePoint(Vector2.zero, 0f);

            path.Points.Add(point);
        }

        private void MovePath(int index, int direction)
        {
            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= paths.Count)
            {
                return;
            }

            (paths[index], paths[targetIndex]) = (paths[targetIndex], paths[index]);
        }

        private static void ResizePointList(List<EditablePathPoint> points, int count)
        {
            while (points.Count < count)
            {
                EditablePathPoint point = points.Count > 0
                    ? CreatePoint(points[^1].Position + Vector2.right, points[^1].Angle, points[^1].HandleLength)
                    : CreatePoint(Vector2.zero, 0f);

                points.Add(point);
            }

            while (points.Count > count)
            {
                points.RemoveAt(points.Count - 1);
            }
        }

        private static string ValidatePath(EditablePathEntry path)
        {
            if (path.Points.Count == 0)
            {
                return "Path has no points.";
            }

            if (path.Type == PathEntryType.Linear && path.Points.Count < 2)
            {
                return "Linear path needs at least 2 points.";
            }

            if (path.Type == PathEntryType.Bezier && path.Points.Count < 2)
            {
                return "Bezier path needs at least 2 points.";
            }

            return string.Empty;
        }

        private void CreateOrRefreshPreview()
        {
            if (previewRoot != null)
            {
                Undo.DestroyObjectImmediate(previewRoot);
            }

            string rootName = string.IsNullOrWhiteSpace(levelId)
                ? "FigureLevelPreview"
                : $"FigureLevelPreview_{levelId}";

            previewRoot = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(previewRoot, "Create Figure Level Preview");

            FigureComponent figureComponent = previewRoot.AddComponent<FigureComponent>();

            ViewComponent viewComponent = CreateView(previewRoot.transform);
            List<PathComponent> pathComponents = new();

            for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                PathComponent pathComponent = CreatePath(previewRoot.transform, pathIndex, paths[pathIndex]);
                pathComponents.Add(pathComponent);
            }

            SetSerializedField(figureComponent, "view", viewComponent);
            SetRuntimeField(figureComponent, "_paths", pathComponents);

            Selection.activeGameObject = previewRoot;
            SceneView.lastActiveSceneView?.FrameSelected();
            SceneView.RepaintAll();
        }

        private ViewComponent CreateView(Transform root)
        {
            GameObject viewObject = new("View");
            Undo.RegisterCreatedObjectUndo(viewObject, "Create Figure View");
            viewObject.transform.SetParent(root, false);

            SpriteRenderer spriteRenderer = viewObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = viewColor;
            spriteRenderer.sortingLayerName = "Figure";
            spriteRenderer.sortingOrder = 0;

            ViewComponent viewComponent = viewObject.AddComponent<ViewComponent>();
            SetSerializedField(viewComponent, "view", spriteRenderer);

            return viewComponent;
        }

        private PathComponent CreatePath(Transform root, int pathIndex, EditablePathEntry editablePath)
        {
            GameObject pathObject = new($"Path_{pathIndex + 1}_{editablePath.Type}");
            Undo.RegisterCreatedObjectUndo(pathObject, "Create Figure Path");
            pathObject.transform.SetParent(root, false);

            PathComponent pathComponent = pathObject.AddComponent<PathComponent>();
            LineRenderer lineRenderer = pathObject.AddComponent<LineRenderer>();
            ConfigureLineRenderer(lineRenderer, editablePath);

            List<PathPointComponent> pointComponents = new();

            for (int pointIndex = 0; pointIndex < editablePath.Points.Count; pointIndex++)
            {
                GameObject pointObject = new($"Point_{pointIndex + 1}");
                Undo.RegisterCreatedObjectUndo(pointObject, "Create Figure Path Point");
                pointObject.transform.SetParent(pathObject.transform, false);
                pointObject.transform.localPosition = editablePath.Points[pointIndex].Position;

                pointComponents.Add(pointObject.AddComponent<PathPointComponent>());
            }

            SetSerializedField(pathComponent, "points", pointComponents);

            return pathComponent;
        }

        private static void ConfigureLineRenderer(LineRenderer lineRenderer, EditablePathEntry path)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.widthMultiplier = 0.04f;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
            lineRenderer.sortingOrder = 10;

            Material spritesMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            if (spritesMaterial != null)
            {
                lineRenderer.sharedMaterial = spritesMaterial;
            }

            Vector3[] positions = BuildPreviewPositions(path);
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }

        private static Vector3[] BuildPreviewPositions(EditablePathEntry path)
        {
            if (path.Points.Count == 0)
            {
                return Array.Empty<Vector3>();
            }

            if (path.Type == PathEntryType.Bezier && path.Points.Count >= 2)
            {
                return BuildBezierPathPositions(path.Points, path.Closed);
            }

            int positionCount = path.Closed && path.Points.Count > 1
                ? path.Points.Count + 1
                : path.Points.Count;

            Vector3[] positions = new Vector3[positionCount];
            for (int i = 0; i < path.Points.Count; i++)
            {
                positions[i] = path.Points[i].Position;
            }

            if (path.Closed && path.Points.Count > 1)
            {
                positions[^1] = path.Points[0].Position;
            }

            return positions;
        }

        private static Vector3[] BuildBezierPathPositions(List<EditablePathPoint> points, bool closed)
        {
            List<Vector3> sampledPositions = new();
            int segmentCount = closed && points.Count > 1
                ? points.Count
                : points.Count - 1;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                EditablePathPoint start = points[segmentIndex];
                EditablePathPoint end = points[(segmentIndex + 1) % points.Count];
                Vector2 startPosition = start.Position;
                Vector2 endPosition = end.Position;
                Vector2 startHandle = startPosition + GetDirection(start.Angle) * start.HandleLength;
                Vector2 endHandle = endPosition - GetDirection(end.Angle) * end.HandleLength;

                for (int sample = 0; sample <= BezierSamplesPerSegment; sample++)
                {
                    if (segmentIndex > 0 && sample == 0)
                    {
                        continue;
                    }

                    float t = sample / (float)BezierSamplesPerSegment;
                    sampledPositions.Add(EvaluateCubicBezier(startPosition, startHandle, endHandle, endPosition, t));
                }
            }

            return sampledPositions.ToArray();
        }

        private static Vector2 EvaluateCubicBezier(Vector2 start, Vector2 startHandle, Vector2 endHandle, Vector2 end, float t)
        {
            float inverse = 1f - t;
            float t2 = t * t;
            float t3 = t2 * t;

            return inverse * inverse * inverse * start
                   + 3f * inverse * inverse * t * startHandle
                   + 3f * inverse * t2 * endHandle
                   + t3 * end;
        }

        private static Vector2 GetDirection(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private static void SetSerializedField(UnityEngine.Object target, string fieldName, object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogError($"Field '{fieldName}' was not found on {target.GetType().Name}.");
                return;
            }

            SetSerializedPropertyValue(property, value);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRuntimeField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogError($"Runtime field '{fieldName}' was not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }

        private static void SetSerializedPropertyValue(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.Generic:
                    if (value is List<PathComponent> pathsValue)
                    {
                        property.arraySize = pathsValue.Count;
                        for (int i = 0; i < pathsValue.Count; i++)
                        {
                            property.GetArrayElementAtIndex(i).objectReferenceValue = pathsValue[i];
                        }
                    }
                    else if (value is List<PathPointComponent> pointsValue)
                    {
                        property.arraySize = pointsValue.Count;
                        for (int i = 0; i < pointsValue.Count; i++)
                        {
                            property.GetArrayElementAtIndex(i).objectReferenceValue = pointsValue[i];
                        }
                    }
                    break;
                default:
                    Debug.LogError($"Unsupported serialized property type '{property.propertyType}' for field '{property.name}'.");
                    break;
            }
        }

        private void DrawSceneHandles(SceneView sceneView)
        {
            if (showSceneHandles == false || previewRoot == null)
            {
                return;
            }

            Handles.color = Color.cyan;
            Transform root = previewRoot.transform;

            for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                EditablePathEntry path = paths[pathIndex];
                if (DrawScenePath(root, path, pathIndex))
                {
                    break;
                }
            }
        }

        private bool DrawScenePath(Transform root, EditablePathEntry path, int pathIndex)
        {
            Vector3[] localPositions = BuildPreviewPositions(path);
            if (localPositions.Length > 1)
            {
                Vector3[] worldPositions = new Vector3[localPositions.Length];
                for (int i = 0; i < localPositions.Length; i++)
                {
                    worldPositions[i] = root.TransformPoint(localPositions[i]);
                }

                Handles.DrawAAPolyLine(4f, worldPositions);
            }

            for (int pointIndex = 0; pointIndex < path.Points.Count; pointIndex++)
            {
                EditablePathPoint point = path.Points[pointIndex];
                Vector3 worldPosition = root.TransformPoint(point.Position);
                float handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.08f;

                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPosition = Handles.FreeMoveHandle(
                    worldPosition,
                    handleSize,
                    Vector3.zero,
                    Handles.CircleHandleCap);

                Handles.Label(
                    worldPosition + Vector3.up * handleSize * 1.5f,
                    $"P{pathIndex + 1}.{pointIndex + 1}");

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Move Figure Path Point");
                    point.Position = root.InverseTransformPoint(newWorldPosition);
                    Repaint();
                    RefreshPreviewGeometry();
                    return true;
                }

                if (path.Type == PathEntryType.Bezier)
                {
                    Vector2 direction = GetDirection(point.Angle);
                    Vector3 localHandlePosition = point.Position + direction * point.HandleLength;
                    Vector3 worldHandlePosition = root.TransformPoint(localHandlePosition);

                    Handles.DrawLine(worldPosition, worldHandlePosition);

                    EditorGUI.BeginChangeCheck();
                    Vector3 newWorldHandlePosition = Handles.FreeMoveHandle(
                        worldHandlePosition,
                        handleSize * 0.75f,
                        Vector3.zero,
                        Handles.RectangleHandleCap);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Move Figure Tangent Handle");
                        Vector2 localHandleDelta = (Vector2)root.InverseTransformPoint(newWorldHandlePosition) - point.Position;

                        if (localHandleDelta.sqrMagnitude > 0.0001f)
                        {
                            point.Angle = Mathf.Atan2(localHandleDelta.y, localHandleDelta.x) * Mathf.Rad2Deg;
                            point.HandleLength = Mathf.Max(MinimumHandleLength, localHandleDelta.magnitude);
                        }

                        Repaint();
                        RefreshPreviewGeometry();
                        return true;
                    }
                }
            }

            return false;
        }

        private void RefreshPreviewGeometry()
        {
            if (previewRoot == null)
            {
                return;
            }

            PathComponent[] pathComponents = previewRoot.GetComponentsInChildren<PathComponent>();
            if (pathComponents.Length != paths.Count)
            {
                CreateOrRefreshPreview();
                return;
            }

            for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                EditablePathEntry path = paths[pathIndex];
                PathComponent pathComponent = pathComponents[pathIndex];

                LineRenderer lineRenderer = pathComponent.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    ConfigureLineRenderer(lineRenderer, path);
                }

                if (pathComponent.Points == null || pathComponent.Points.Count != path.Points.Count)
                {
                    CreateOrRefreshPreview();
                    return;
                }

                for (int pointIndex = 0; pointIndex < path.Points.Count; pointIndex++)
                {
                    pathComponent.Points[pointIndex].transform.localPosition = path.Points[pointIndex].Position;
                }
            }

            SceneView.RepaintAll();
        }

        private LevelEntry BuildLevelEntry()
        {
            LevelEntry levelEntry = new()
            {
                LevelID = levelId,
                FigureId = figureId,
                FigureType = figureType,
                ViewColor = viewColor,
                PathEntries = new List<PathEntry>()
            };

            for (int i = 0; i < paths.Count; i++)
            {
                levelEntry.PathEntries.Add(new PathEntry
                {
                    Order = i,
                    Type = paths[i].Type,
                    Closed = paths[i].Closed,
                    Path = BuildPositionList(paths[i].Points),
                    PointEntries = BuildPointEntryList(paths[i].Points)
                });
            }

            return levelEntry;
        }

        private static List<Vector2> BuildPositionList(List<EditablePathPoint> points)
        {
            List<Vector2> positions = new();

            foreach (EditablePathPoint point in points)
            {
                positions.Add(point.Position);
            }

            return positions;
        }

        private static List<PathPointEntry> BuildPointEntryList(List<EditablePathPoint> points)
        {
            List<PathPointEntry> pointEntries = new();

            foreach (EditablePathPoint point in points)
            {
                pointEntries.Add(new PathPointEntry
                {
                    Position = point.Position,
                    Angle = point.Angle,
                    HandleLength = point.HandleLength
                });
            }

            return pointEntries;
        }

        private void LoadFromLevelEntry(LevelEntry levelEntry)
        {
            levelId = levelEntry.LevelID;
            figureId = levelEntry.FigureId;
            figureType = levelEntry.FigureType;
            viewColor = levelEntry.ViewColor;
            syncFigureIdWithSprite = false;
            UpdateSavePathFromFigureType();
            sprite = FindSpriteByFigureId(figureId);

            paths.Clear();

            if (levelEntry.PathEntries != null)
            {
                levelEntry.PathEntries.Sort((left, right) => left.Order.CompareTo(right.Order));

                foreach (PathEntry pathEntry in levelEntry.PathEntries)
                {
                    paths.Add(new EditablePathEntry
                    {
                        Type = pathEntry.Type,
                        Closed = pathEntry.Closed,
                        Points = BuildEditablePointList(pathEntry),
                        Foldout = true
                    });
                }
            }
        }

        private static List<EditablePathPoint> BuildEditablePointList(PathEntry pathEntry)
        {
            List<EditablePathPoint> points = new();

            if (pathEntry.PointEntries != null && pathEntry.PointEntries.Count > 0)
            {
                foreach (PathPointEntry pointEntry in pathEntry.PointEntries)
                {
                    points.Add(CreatePoint(
                        pointEntry.Position,
                        pointEntry.Angle,
                        Mathf.Max(MinimumHandleLength, pointEntry.HandleLength)));
                }

                return points;
            }

            if (pathEntry.Path == null)
            {
                return points;
            }

            for (int i = 0; i < pathEntry.Path.Count; i++)
            {
                float angle = 0f;
                if (pathEntry.Path.Count > 1)
                {
                    Vector2 next = pathEntry.Path[Mathf.Min(i + 1, pathEntry.Path.Count - 1)];
                    Vector2 previous = pathEntry.Path[Mathf.Max(i - 1, 0)];
                    Vector2 direction = next - previous;
                    if (direction.sqrMagnitude > 0.0001f)
                    {
                        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    }
                }

                points.Add(CreatePoint(pathEntry.Path[i], angle));
            }

            return points;
        }

        private void ImportLevel()
        {
            string path = EditorUtility.OpenFilePanel("Import Figure Level", Application.dataPath, "json");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                LevelEntry levelEntry = JsonUtility.FromJson<LevelEntry>(json);

                if (levelEntry == null)
                {
                    Debug.LogError("Selected file does not contain a valid LevelEntry JSON.");
                    return;
                }

                LoadFromLevelEntry(levelEntry);
                currentLevelAssetPath = AbsolutePathToAssetPath(path);
                CreateOrRefreshPreview();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to import figure level: {exception.Message}");
            }
        }

        private void SaveLevel()
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                EditorUtility.DisplayDialog(WindowTitle, "Level ID is required before saving.", "OK");
                return;
            }

            EnsureSavePathInitialized();
            string saveDirectory = GetAssetSaveDirectory();
            EnsureSaveDirectoryExists(saveDirectory);
            string targetPath = $"{saveDirectory}/{levelId}.json";

            try
            {
                string directory = Path.GetDirectoryName(targetPath);
                if (string.IsNullOrWhiteSpace(directory) == false && Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }

                LevelEntry levelEntry = BuildLevelEntry();
                string json = JsonUtility.ToJson(levelEntry, true);
                File.WriteAllText(targetPath, json);

                currentLevelAssetPath = targetPath;
                AssetDatabase.Refresh();
                UpdateLevelCatalog(targetPath);

                Debug.Log($"Figure level saved to {targetPath}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save figure level: {exception.Message}");
            }
        }

        private void EnsureSavePathInitialized()
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                UpdateSavePathFromFigureType();
            }
        }

        private void EnsureLevelIdInitialized()
        {
            if (ShouldUseGeneratedLevelId(levelId))
            {
                UpdateLevelIdFromFigureType();
            }
        }

        private void UpdateSavePathFromFigureType()
        {
            savePath = GetBaseSavePath(figureType);
            currentLevelAssetPath = string.Empty;
        }

        private void UpdateLevelIdFromFigureType()
        {
            levelId = GenerateNextLevelId(figureType);
            currentLevelAssetPath = string.Empty;
        }

        private static string GetBaseSavePath(FigureType type) =>
            $"{SavePathRoot}/{type}";

        private static string GetLevelCatalogAssetPath() =>
            $"{AssetPathPrefix}{LevelCatalogDirectory}/{LevelCatalogAssetName}";

        private void UpdateLevelCatalog(string levelAssetPath)
        {
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(GetLevelCatalogAssetPath());
            if (catalog == null)
            {
                Debug.LogWarning($"Level catalog was not found at '{GetLevelCatalogAssetPath()}'. JSON was saved, but catalog was not updated.");
                return;
            }

            TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(levelAssetPath);
            if (jsonAsset == null)
            {
                Debug.LogWarning($"Saved level JSON was not imported as a TextAsset at '{levelAssetPath}'. Catalog was not updated.");
                return;
            }

            Undo.RecordObject(catalog, "Update Level Catalog");
            LevelGroupData group = GetOrCreateCatalogGroup(catalog, figureType);
            UpsertCatalogLevel(group, levelId, jsonAsset);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static LevelGroupData GetOrCreateCatalogGroup(LevelCatalog catalog, FigureType type)
        {
            catalog.Groups ??= new List<LevelGroupData>();

            LevelGroupData group = catalog.GetLevelGroupByType(type);
            if (group != null)
            {
                group.Levels ??= new List<LevelData>();
                return group;
            }

            group = new LevelGroupData
            {
                Type = type,
                Levels = new List<LevelData>()
            };

            catalog.Groups.Add(group);
            return group;
        }

        private static void UpsertCatalogLevel(LevelGroupData group, string id, TextAsset jsonAsset)
        {
            LevelData existingLevel = group.Levels.Find(level => level != null && level.Id == id);
            if (existingLevel != null)
            {
                existingLevel.Json = jsonAsset;
                existingLevel.Id = id;
                RemoveDuplicateCatalogLevels(group, id, existingLevel);
                return;
            }

            group.Levels.Add(new LevelData
            {
                Id = id,
                Json = jsonAsset
            });
        }

        private static void RemoveDuplicateCatalogLevels(LevelGroupData group, string id, LevelData keepLevel)
        {
            for (int i = group.Levels.Count - 1; i >= 0; i--)
            {
                LevelData level = group.Levels[i];
                if (level != keepLevel && level != null && level.Id == id)
                {
                    group.Levels.RemoveAt(i);
                }
            }
        }

        private static string GenerateNextLevelId(FigureType type)
        {
            int nextId = GetLastLevelNumber(type) + 1;
            return $"{type}_{nextId}";
        }

        private static int GetLastLevelNumber(FigureType type)
        {
            string assetDirectory = $"{AssetPathPrefix}{GetBaseSavePath(type)}";
            if (Directory.Exists(assetDirectory) == false)
            {
                return 0;
            }

            string prefix = $"{type}_";
            int lastLevelNumber = 0;

            foreach (string filePath in Directory.GetFiles(assetDirectory, "*.json", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                string idPart = fileName[prefix.Length..];
                if (int.TryParse(idPart, out int levelNumber))
                {
                    lastLevelNumber = Mathf.Max(lastLevelNumber, levelNumber);
                }
            }

            return lastLevelNumber;
        }

        private static bool ShouldUseGeneratedLevelId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id == "level_001")
            {
                return true;
            }

            foreach (FigureType type in Enum.GetValues(typeof(FigureType)))
            {
                string prefix = $"{type}_";
                if (id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(id[prefix.Length..], out _))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetAssetSaveDirectory()
        {
            string normalizedPath = NormalizeDisplayedSavePath(savePath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                normalizedPath = GetBaseSavePath(figureType);
            }

            return normalizedPath.StartsWith(AssetPathPrefix, StringComparison.OrdinalIgnoreCase)
                ? normalizedPath
                : $"{AssetPathPrefix}{normalizedPath}";
        }

        private static string NormalizeDisplayedSavePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalizedPath = path.Replace("\\", "/").Trim('/');
            if (normalizedPath.StartsWith(AssetPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath[AssetPathPrefix.Length..];
            }

            return normalizedPath;
        }

        private static void EnsureSaveDirectoryExists(string assetDirectory)
        {
            if (Directory.Exists(assetDirectory) == false)
            {
                Directory.CreateDirectory(assetDirectory);
                AssetDatabase.Refresh();
            }
        }

        private static string AbsolutePathToAssetPath(string absolutePath)
        {
            string normalizedAbsolutePath = absolutePath.Replace("\\", "/");
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName.Replace("\\", "/");

            if (string.IsNullOrEmpty(projectRoot) == false &&
                normalizedAbsolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedAbsolutePath[(projectRoot.Length + 1)..];
            }

            return normalizedAbsolutePath;
        }

        private static Sprite FindSpriteByFigureId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (id.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                Sprite assetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(id);
                if (assetSprite != null)
                {
                    return assetSprite;
                }
            }

            string[] guids = AssetDatabase.FindAssets($"{id} t:Sprite", new[] { "Assets/Graphics/InBuild/Gameplay/Levels" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite foundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (foundSprite != null && foundSprite.name == id)
                {
                    return foundSprite;
                }
            }

            return null;
        }

        [Serializable]
        private class EditablePathEntry
        {
            public PathEntryType Type;
            public bool Closed;
            public List<EditablePathPoint> Points = new();
            public bool Foldout = true;
            public PathHelperType HelperType;
            public Vector2 CircleCenter;
            public float CircleDistanceByCenter = 1f;
            public int CirclePointCount = 8;
            public Vector2 LinearStartPoint = new(-1f, 0f);
            public Vector2 LinearFinalPoint = new(1f, 0f);
            public int LinearPointCount = 2;
        }

        [Serializable]
        private class EditablePathPoint
        {
            public Vector2 Position;
            public float Angle;
            public float HandleLength = DefaultHandleLength;
        }

        private enum PathHelperType
        {
            None = 0,
            Circle = 1,
            LinearAddition = 2
        }
    }
}
