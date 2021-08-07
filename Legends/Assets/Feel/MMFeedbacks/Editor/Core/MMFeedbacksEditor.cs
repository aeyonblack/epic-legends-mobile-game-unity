using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A custom editor displaying a foldable list of MMFeedbacks, a dropdown to add more, as well as test buttons to test your feedbacks at runtime
    /// </summary>
    [CustomEditor(typeof(MMFeedbacks))]
    public class MMFeedbacksEditor : Editor
    {
        /// <summary>
        /// A data structure to store types and names
        /// </summary>
        public class FeedbackTypePair
        {
            public System.Type FeedbackType;
            public string FeedbackName;
        }

        /// <summary>
        /// A helper class to copy and paste feedback properties
        /// </summary>
        static class FeedbackCopy
        {
            // Single Copy --------------------------------------------------------------------

            static public System.Type Type { get; private set; }
            static List<SerializedProperty> Properties = new List<SerializedProperty>();
            
            static string[] IgnoreList = new string[]
            {
            "m_ObjectHideFlags",
            "m_CorrespondingSourceObject",
            "m_PrefabInstance",
            "m_PrefabAsset",
            "m_GameObject",
            "m_Enabled",
            "m_EditorHideFlags",
            "m_Script",
            "m_Name",
            "m_EditorClassIdentifier"
            };

            static public void Copy(SerializedObject serializedObject)
            {
                Type = serializedObject.targetObject.GetType();
                Properties.Clear();

                SerializedProperty property = serializedObject.GetIterator();
                property.Next(true);
                do
                {
                    if (!IgnoreList.Contains(property.name))
                    {
                        Properties.Add(property.Copy());
                    }
                } while (property.Next(false));
            }

            static public void Paste(SerializedObject target)
            {
                if (target.targetObject.GetType() == Type)
                {
                    for (int i = 0; i < Properties.Count; i++)
                    {
                        target.CopyFromSerializedProperty(Properties[i]);
                    }
                }
            }

            static public bool HasCopy()
            {
                return Properties != null && Properties.Count > 0;
            }

            // Multiple Copy ----------------------------------------------------------

            static public void CopyAll(MMFeedbacks sourceFeedbacks)
            {
                MMFeedbacksConfiguration.Instance._mmFeedbacks = sourceFeedbacks;
            }

            static public bool HasMultipleCopies()
            {
                return (MMFeedbacksConfiguration.Instance._mmFeedbacks != null);
            }

            static public void PasteAll(MMFeedbacksEditor targetEditor)
            {
                var sourceFeedbacks = new SerializedObject(MMFeedbacksConfiguration.Instance._mmFeedbacks);
                SerializedProperty feedbacks = sourceFeedbacks.FindProperty("Feedbacks");

                for (int i = 0; i < feedbacks.arraySize; i++)
                {
                    MMFeedback arrayFeedback = (feedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);

                    FeedbackCopy.Copy(new SerializedObject(arrayFeedback));
                    MMFeedback newFeedback = targetEditor.AddFeedback(arrayFeedback.GetType());
                    SerializedObject serialized = new SerializedObject(newFeedback);
                    serialized.Update();
                    FeedbackCopy.Paste(serialized);
                    serialized.ApplyModifiedProperties();
                }
                MMFeedbacksConfiguration.Instance._mmFeedbacks = null;
            }
        }

        protected MMFeedbacks _targetMMFeedbacks;
        protected SerializedProperty _mmfeedbacks;
        protected SerializedProperty _mmfeedbacksInitializationMode;
        protected SerializedProperty _mmfeedbacksSafeMode;
        protected SerializedProperty _mmfeedbacksAutoPlayOnStart;
        protected SerializedProperty _mmfeedbacksAutoPlayOnEnable;
        protected SerializedProperty _mmfeedbacksDirection;
        protected SerializedProperty _mmfeedbacksFeedbacksIntensity;
        protected SerializedProperty _mmfeedbacksAutoChangeDirectionOnEnd;
        protected SerializedProperty _mmfeedbacksDurationMultiplier;
        protected SerializedProperty _mmfeedbacksDisplayFullDurationDetails;
        protected SerializedProperty _mmfeedbacksCooldownDuration;
        protected SerializedProperty _mmfeedbacksInitialDelay;
        protected SerializedProperty _mmfeedbacksEvents;
        
        protected Dictionary<MMFeedback, Editor> _editors;
        protected List<FeedbackTypePair> _typesAndNames = new List<FeedbackTypePair>();
        protected string[] _typeDisplays;
        protected int _draggedStartID = -1;
        protected int _draggedEndID = -1;
        private static bool _debugView = false;
        protected Color _originalBackgroundColor;
        protected Color _scriptDrivenBoxColor;
        protected Texture2D _scriptDrivenBoxBackgroundTexture;
        protected Color _scriptDrivenBoxColorFrom = new Color(1f,0f,0f,1f);
        protected Color _scriptDrivenBoxColorTo = new Color(0.7f,0.1f,0.1f,1f);
        protected Color _playButtonColor = new Color32(193, 255, 2, 255);
        private static bool _settingsMenuDropdown;
        private static bool _eventsMenuDropdown;
        protected GUIStyle _directionButtonStyle;

        /// <summary>
        /// On Enable, grabs properties and initializes the add feedback dropdown's contents
        /// </summary>
        void OnEnable()
        {
            // Get properties
            _targetMMFeedbacks = target as MMFeedbacks;
            _mmfeedbacks = serializedObject.FindProperty("Feedbacks");
            _mmfeedbacksInitializationMode = serializedObject.FindProperty("InitializationMode");
            _mmfeedbacksSafeMode = serializedObject.FindProperty("SafeMode");
            _mmfeedbacksAutoPlayOnStart = serializedObject.FindProperty("AutoPlayOnStart");
            _mmfeedbacksAutoPlayOnEnable = serializedObject.FindProperty("AutoPlayOnEnable");
            _mmfeedbacksDirection = serializedObject.FindProperty("Direction");
            _mmfeedbacksAutoChangeDirectionOnEnd = serializedObject.FindProperty("AutoChangeDirectionOnEnd");
            _mmfeedbacksDurationMultiplier = serializedObject.FindProperty("DurationMultiplier");
            _mmfeedbacksDisplayFullDurationDetails = serializedObject.FindProperty("DisplayFullDurationDetails");
            _mmfeedbacksCooldownDuration = serializedObject.FindProperty("CooldownDuration");
            _mmfeedbacksInitialDelay = serializedObject.FindProperty("InitialDelay");
            _mmfeedbacksFeedbacksIntensity = serializedObject.FindProperty("FeedbacksIntensity");

            _mmfeedbacksEvents = serializedObject.FindProperty("Events");
            
            // store GUI bg color
            _originalBackgroundColor = GUI.backgroundColor;
            
            // Repair routine to catch feedbacks that may have escaped due to Unity's serialization issues
            RepairRoutine();

            // Create editors
            _editors = new Dictionary<MMFeedback, Editor>();
            for (int i = 0; i < _mmfeedbacks.arraySize; i++)
            {
                AddEditor(_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);
            }

            // Retrieve available feedbacks
            List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
                     from assemblyType in domainAssembly.GetTypes()
                     where assemblyType.IsSubclassOf(typeof(MMFeedback))
                     select assemblyType).ToList();

            // Create display list from types
            List<string> typeNames = new List<string>();
            for (int i = 0; i < types.Count; i++)
            {
                FeedbackTypePair newType = new FeedbackTypePair();
                newType.FeedbackType = types[i];
                newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
                if (newType.FeedbackName == "MMFeedbackBase")
                {
                    continue;
                }
                _typesAndNames.Add(newType);
            }

            _typesAndNames = _typesAndNames.OrderBy(t => t.FeedbackName).ToList();
            
            typeNames.Add("Add new feedback...");
            for (int i = 0; i < _typesAndNames.Count; i++)
            {
                typeNames.Add(_typesAndNames[i].FeedbackName);
            }

            _typeDisplays = typeNames.ToArray();

            _directionButtonStyle = new GUIStyle();
            _directionButtonStyle.border.left = 0;
            _directionButtonStyle.border.right = 0;
            _directionButtonStyle.border.top = 0;
            _directionButtonStyle.border.bottom = 0;

        }

        /// <summary>
        /// Calls the repair routine if needed
        /// </summary>
        protected virtual void RepairRoutine()
        {
            _targetMMFeedbacks = target as MMFeedbacks;
            if ((_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.EditorOnly) || (_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.Full))
            {
                _targetMMFeedbacks.AutoRepair();
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the inspector, complete with helpbox, init mode selection, list of feedbacks, feedback selection and test buttons 
        /// </summary>
        public override void OnInspectorGUI()
        {
            var e = Event.current;
            serializedObject.Update();
            Undo.RecordObject(target, "Modified Feedback Manager");

            EditorGUILayout.Space();

            if (!MMFeedbacks.GlobalMMFeedbacksActive)
            {
                Color baseColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.HelpBox("All MMFeedbacks, including this one, are currently disabled. This is done via script, by changing the value of the MMFeedbacks.GlobalMMFeedbacksActive boolean. Right now this value has been set to false. Setting it back to true will allow MMFeedbacks to play again.", MessageType.Warning);
                EditorGUILayout.Space();
                GUI.color = baseColor;
            }

            if (MMFeedbacksConfiguration.Instance.ShowInspectorTips)
            {
                EditorGUILayout.HelpBox("Select feedbacks from the 'add a feedback' dropdown and customize them. Remember, if you don't use auto initialization (Awake or Start), " +
                                        "you'll need to initialize them via script.", MessageType.None);    
            }

            Rect helpBoxRect = GUILayoutUtility.GetLastRect();

            // Settings dropdown -------------------------------------------------------------------------------------

            _settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, "Settings", true, EditorStyles.foldout);
            if (_settingsMenuDropdown)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Initialization", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_mmfeedbacksSafeMode);
                EditorGUILayout.PropertyField(_mmfeedbacksInitializationMode);
                EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnStart);
                EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnEnable);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Direction", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_mmfeedbacksDirection);
                EditorGUILayout.PropertyField(_mmfeedbacksAutoChangeDirectionOnEnd);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Intensity", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_mmfeedbacksFeedbacksIntensity);    
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_mmfeedbacksDurationMultiplier);
                EditorGUILayout.PropertyField(_mmfeedbacksDisplayFullDurationDetails);
                EditorGUILayout.PropertyField(_mmfeedbacksCooldownDuration);
                EditorGUILayout.PropertyField(_mmfeedbacksInitialDelay);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_mmfeedbacksEvents);
            }

            // Duration ----------------------------------------------------------------------------------------------
            
            float durationRectWidth = 70f;
            Rect durationRect = new Rect(helpBoxRect.xMax - durationRectWidth, helpBoxRect.yMax + 6, durationRectWidth, 17f);
            durationRect.xMin = helpBoxRect.xMax - durationRectWidth;
            durationRect.xMax = helpBoxRect.xMax;

            // Direction ----------------------------------------------------------------------------------------------

            float directionRectWidth = 16f;
            Rect directionRect = new Rect(helpBoxRect.xMax - directionRectWidth, helpBoxRect.yMax + 5, directionRectWidth, 17f);
            directionRect.xMin = helpBoxRect.xMax - directionRectWidth;
            directionRect.xMax = helpBoxRect.xMax;
            GUI.Label(durationRect, "["+_targetMMFeedbacks.TotalDuration.ToString("F2")+"s]");

            if (_targetMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop)
            {
                float rotationAngle = -90f;
                var pivotPoint = new Vector2(directionRect.xMin + 7, directionRect.yMax - 8);
                var matrixBackup = GUI.matrix;
                GUIUtility.RotateAroundPivot(rotationAngle, pivotPoint);
                GUIContent directionIcon = EditorGUIUtility.IconContent("d_PlayButton@2x", "BottomToTop");

                if (GUI.Button(directionRect, directionIcon, _directionButtonStyle))
                {
                    _targetMMFeedbacks.Revert();
                }
                GUI.matrix = matrixBackup;
            }
            else
            {
                float rotationAngle = 90f;
                var pivotPoint = new Vector2(directionRect.xMin + 7, directionRect.yMax - 8);
                var matrixBackup = GUI.matrix;
                GUIUtility.RotateAroundPivot(rotationAngle, pivotPoint);
                GUIContent directionIcon = EditorGUIUtility.IconContent("d_PlayButton@2x", "TopToBottom");

                if (GUI.Button(directionRect, directionIcon, _directionButtonStyle))
                {
                    _targetMMFeedbacks.Revert();
                }
                GUI.matrix = matrixBackup;
            }

            // Draw list ------------------------------------------------------------------------------------------

            MMFeedbackStyling.DrawSection("Feedbacks");

            for (int i = 0; i < _mmfeedbacks.arraySize; i++)
            {
                MMFeedbackStyling.DrawSplitter();

                SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(i);

                // Failsafe but should not happen
                if (property.objectReferenceValue == null)
                {
                    continue;
                }                    

                // Retrieve feedback

                MMFeedback feedback = property.objectReferenceValue as MMFeedback;
                feedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;
                
                Undo.RecordObject(feedback, "Modified Feedback");

                // Draw header

                int id = i;
                bool isExpanded = property.isExpanded;
                string label = feedback.Label;
                bool pause = false;

                if (feedback.Pause != null)
                {
                    pause = true;
                }
                if ((feedback.LooperPause == true) && (Application.isPlaying))
                {
                    if ((feedback as MMFeedbackLooper).InfiniteLoop)
                    {
                        label = label + "[Infinite Loop] ";
                    }
                    else
                    {
                        label = label + "[ " + (feedback as MMFeedbackLooper).NumberOfLoopsLeft + " loops left ] ";
                    }                    
                }

                Rect headerRect = MMFeedbackStyling.DrawHeader(
                    ref isExpanded,
                    ref feedback.Active,
                    label,
                    feedback.FeedbackColor,
                    (GenericMenu menu) =>
                    {
                        if (Application.isPlaying)
                            menu.AddItem(new GUIContent("Play"), false, () => PlayFeedback(id));
                        else
                            menu.AddDisabledItem(new GUIContent("Play"));
                        menu.AddSeparator(null);
                        //menu.AddItem(new GUIContent("Reset"), false, () => ResetFeedback(id));
                        menu.AddItem(new GUIContent("Remove"), false, () => RemoveFeedback(id));
                        menu.AddSeparator(null);
                        menu.AddItem(new GUIContent("Copy"), false, () => CopyFeedback(id));
                        if (FeedbackCopy.HasCopy() && FeedbackCopy.Type == feedback.GetType())
                            menu.AddItem(new GUIContent("Paste"), false, () => PasteFeedback(id));
                        else
                            menu.AddDisabledItem(new GUIContent("Paste"));
                    },
                    feedback.FeedbackStartedAt,
                    feedback.FeedbackDuration,
                    feedback.TotalDuration,
                        feedback.Timing,
                        pause,
                    _targetMMFeedbacks 
                        );

                // Check if we start dragging this feedback

                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (headerRect.Contains(e.mousePosition))
                        {
                            _draggedStartID = i;
                            e.Use();
                        }
                        break;
                    default:
                        break;
                }

                // Draw blue rect if feedback is being dragged

                if (_draggedStartID == i && headerRect != Rect.zero)
                {
                    Color color = new Color(0, 1, 1, 0.2f);
                    EditorGUI.DrawRect(headerRect, color);
                }

                // If hovering at the top of the feedback while dragging one, check where the feedback should be dropped : top or bottom

                if (headerRect.Contains(e.mousePosition))
                {
                    if (_draggedStartID >= 0)
                    {
                        _draggedEndID = i;

                        Rect headerSplit = headerRect;
                        headerSplit.height *= 0.5f;
                        headerSplit.y += headerSplit.height;
                        if (headerSplit.Contains(e.mousePosition))
                            _draggedEndID = i + 1;
                    }
                }

                // If expanded, draw feedback editor

                property.isExpanded = isExpanded;
                if (isExpanded)
                {
                    EditorGUI.BeginDisabledGroup(!feedback.Active);

                    string helpText = FeedbackHelpAttribute.GetFeedbackHelpText(feedback.GetType());
                    
                    if ( (!string.IsNullOrEmpty(helpText)) && (MMFeedbacksConfiguration.Instance.ShowInspectorTips))
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                        style.richText = true;
                        float newHeight = style.CalcHeight(new GUIContent(helpText), EditorGUIUtility.currentViewWidth);
                        EditorGUILayout.LabelField(helpText, style);
                    }                    

                    EditorGUILayout.Space();

                    if (!_editors.ContainsKey(feedback))
                    {
                        AddEditor(feedback);
                    }

                    Editor editor = _editors[feedback];
                    CreateCachedEditor(feedback, feedback.GetType(), ref editor);

                    editor.OnInspectorGUI();

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Play", EditorStyles.miniButtonMid))
                        {
                            PlayFeedback(id);
                        }
                        if (GUILayout.Button("Stop", EditorStyles.miniButtonMid))
                        {
                            StopFeedback(id);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            }

            // Draw add new item

            if (_mmfeedbacks.arraySize > 0)
            {
                MMFeedbackStyling.DrawSplitter();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                // Feedback list

                int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;
                if (newItem >= 0)
                {
                    AddFeedback(_typesAndNames[newItem].FeedbackType);
                }

                // Paste feedback copy as new

                if (FeedbackCopy.HasCopy())
                {
                    if (GUILayout.Button("Paste as new", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x)))
                    {
                        PasteAsNew();
                    }                        
                }

                if (FeedbackCopy.HasMultipleCopies())
                {
                    if (GUILayout.Button("Paste all as new", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste all as new")).x)))
                    {
                        PasteAllAsNew();
                    }                        
                }
            }

            if (!FeedbackCopy.HasMultipleCopies())
            {
                if (GUILayout.Button("Copy all", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x)))
                {
                    CopyAll();
                }
            }                

            EditorGUILayout.EndHorizontal();

            // Reorder

            if (_draggedStartID >= 0 && _draggedEndID >= 0)
            {
                if (_draggedEndID != _draggedStartID)
                {
                    if (_draggedEndID > _draggedStartID)
                        _draggedEndID--;
                    _mmfeedbacks.MoveArrayElement(_draggedStartID, _draggedEndID);
                    _draggedStartID = _draggedEndID;
                }
            }

            if (_draggedStartID >= 0 || _draggedEndID >= 0)
            {
                switch (e.type)
                {
                    case EventType.MouseUp:
                        _draggedStartID = -1;
                        _draggedEndID = -1;
                        e.Use();
                        break;
                    default:
                        break;
                }
            }

            // Clean up

            bool wasRemoved = false;
            for (int i = _mmfeedbacks.arraySize - 1; i >= 0; i--)
            {
                if (_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    wasRemoved = true;
                    _mmfeedbacks.DeleteArrayElementAtIndex(i);
                }
            }

            if (wasRemoved)
            {
                GameObject gameObject = (target as MMFeedbacks).gameObject;
                foreach (var c in gameObject.GetComponents<Component>())
                {
                    if (c != null)
                    {
                        c.hideFlags = HideFlags.None;    
                    }
                }
            }

            // Apply changes

            serializedObject.ApplyModifiedProperties();

            // Draw debug

            MMFeedbackStyling.DrawSection("All Feedbacks Debug");

            // Testing buttons

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            {
                // initialize button
                if (GUILayout.Button("Initialize", EditorStyles.miniButtonLeft))
                {
                    (target as MMFeedbacks).Initialization();
                }

                // play button
                _originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _playButtonColor;
                if (GUILayout.Button("Play", EditorStyles.miniButtonMid))
                {
                    (target as MMFeedbacks).PlayFeedbacks();
                }
                GUI.backgroundColor = _originalBackgroundColor;
                
                // pause button
                if ((target as MMFeedbacks).ContainsLoop)
                {
                    if (GUILayout.Button("Pause", EditorStyles.miniButtonMid))
                    {
                        (target as MMFeedbacks).PauseFeedbacks();
                    }   
                }
                
                // stop button
                if (GUILayout.Button("Stop", EditorStyles.miniButtonMid))
                {
                    (target as MMFeedbacks).StopFeedbacks();
                }
                
                // reset button
                if (GUILayout.Button("Reset", EditorStyles.miniButtonMid))
                {
                    (target as MMFeedbacks).ResetFeedbacks();
                }
                EditorGUI.EndDisabledGroup();
                
                // reverse button
                if (GUILayout.Button("Revert", EditorStyles.miniButtonMid))
                {
                    (target as MMFeedbacks).Revert();
                }

                // debug button
                EditorGUI.BeginChangeCheck();
                {
                    _debugView = GUILayout.Toggle(_debugView, "Debug View", EditorStyles.miniButtonRight);

                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var f in (target as MMFeedbacks).Feedbacks)
                            f.hideFlags = _debugView ? HideFlags.HideInInspector : HideFlags.None;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();


            float pingPong = Mathf.PingPong(Time.unscaledTime, 0.25f);
            
            // if in pause, we display additional controls
            if (_targetMMFeedbacks.InScriptDrivenPause)
            {
                // draws a warning box
                _scriptDrivenBoxColor = Color.Lerp(_scriptDrivenBoxColorFrom, _scriptDrivenBoxColorTo, pingPong);
                GUI.skin.box.normal.background = Texture2D.whiteTexture;
                GUI.backgroundColor = _scriptDrivenBoxColor;
                GUI.skin.box.normal.textColor = Color.black;
                GUILayout.Box("Script driven pause in progress, call Resume() to exit pause", GUILayout.ExpandWidth(true));
                GUI.backgroundColor = _originalBackgroundColor;
                GUI.skin.box.normal.background = _scriptDrivenBoxBackgroundTexture; 
                
                // draws resume button
                if (GUILayout.Button("Resume"))
                {
                    _targetMMFeedbacks.ResumeFeedbacks();
                }
            }

            // Debug draw
            if (_debugView)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_mmfeedbacks, true);
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// We need to repaint constantly if dragging a feedback around
        /// </summary>
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        /// <summary>
        /// Add a feedback to the list
        /// </summary>
        protected virtual MMFeedback AddFeedback(System.Type type)
        {
            GameObject gameObject = (target as MMFeedbacks).gameObject;

            MMFeedback newFeedback = Undo.AddComponent(gameObject, type) as MMFeedback;
            newFeedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;
            newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(type);

            AddEditor(newFeedback);

            _mmfeedbacks.arraySize++;
            _mmfeedbacks.GetArrayElementAtIndex(_mmfeedbacks.arraySize - 1).objectReferenceValue = newFeedback;

            return newFeedback;
        }

        //
        // Editors management
        //

        /// <summary>
        /// Create the editor for a feedback
        /// </summary>
        protected virtual void AddEditor(MMFeedback feedback)
        {
            if (feedback == null)
                return;

            if (!_editors.ContainsKey(feedback))
            {
                Editor editor = null;
                CreateCachedEditor(feedback, null, ref editor);

                _editors.Add(feedback, editor as Editor);
            }
        }

        /// <summary>
        /// Destroy the editor for a feedback
        /// </summary>
        protected virtual void RemoveEditor(MMFeedback feedback)
        {
            if (feedback == null)
                return;

            if (_editors.ContainsKey(feedback))
            {
                DestroyImmediate(_editors[feedback]);
                _editors.Remove(feedback);
            }
        }

        //
        // Feedback generic menus
        //

        /// <summary>
        /// Play the selected feedback
        /// </summary>
        protected virtual void InitializeFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;
            feedback.Initialization(feedback.gameObject);
        }

        /// <summary>
        /// Play the selected feedback
        /// </summary>
        protected virtual void PlayFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;
            feedback.Play(feedback.transform.position, _targetMMFeedbacks.FeedbacksIntensity);
        }

        /// <summary>
        /// Play the selected feedback
        /// </summary>
        protected virtual void StopFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;
            feedback.Stop(feedback.transform.position);
        }

        /// <summary>
        /// Resets the selected feedback
        /// </summary>
        /// <param name="id"></param>
        protected virtual void ResetFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;
            feedback.ResetFeedback();
        }

        /// <summary>
        /// Remove the selected feedback
        /// </summary>
        protected virtual void RemoveFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;

            (target as MMFeedbacks).Feedbacks.Remove(feedback);

            _editors.Remove(feedback);
            Undo.DestroyObjectImmediate(feedback);
        }

        /// <summary>
        /// Copy the selected feedback
        /// </summary>
        protected virtual void CopyFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;

            FeedbackCopy.Copy(new SerializedObject(feedback));
        }

        /// <summary>
        /// Asks for a full copy of the source
        /// </summary>
        protected virtual void CopyAll()
        {
            FeedbackCopy.CopyAll(target as MMFeedbacks);
        }

        /// <summary>
        /// Paste the previously copied feedback values into the selected feedback
        /// </summary>
        protected virtual void PasteFeedback(int id)
        {
            SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
            MMFeedback feedback = property.objectReferenceValue as MMFeedback;

            SerializedObject serialized = new SerializedObject(feedback);

            FeedbackCopy.Paste(serialized);
            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Creates a new feedback and applies the previoulsy copied feedback values
        /// </summary>
        protected virtual void PasteAsNew()
        {
            MMFeedback newFeedback = AddFeedback(FeedbackCopy.Type);
            SerializedObject serialized = new SerializedObject(newFeedback);

            serialized.Update();
            FeedbackCopy.Paste(serialized);
            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Asks for a paste of all feedbacks in the source
        /// </summary>
        protected virtual void PasteAllAsNew()
        {
            serializedObject.Update();
            Undo.RecordObject(target, "Paste all MMFeedbacks");
            FeedbackCopy.PasteAll(this);
            serializedObject.ApplyModifiedProperties();
        }
    }
}