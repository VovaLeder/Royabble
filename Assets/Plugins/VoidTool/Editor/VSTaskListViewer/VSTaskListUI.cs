using System.Linq;

using UnityEditor;

using UnityEngine;

namespace VoidTools
{
    [InitializeOnLoad]
    internal sealed class InitScanOnLoad
    {
        static InitScanOnLoad()
        {
            VSTaskListCore.AsyncSearchForScripts();
        }
    }

    internal sealed class VSTaskListUI : EditorWindow
    {
        #region validation

#pragma warning disable IDE0051 // Remove unused private members

        [MenuItem("Void Tool/Visual Studio Task List View/Show Debug ON")]
        private static void VSPrefOnClicked() => EnableLogs(true);

        [MenuItem("Void Tool/Visual Studio Task List View/Show Debug ON", true)]
        private static bool VSPrefOn_validate() => !EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log, false);

        [MenuItem("Void Tool/Visual Studio Task List View/Show Debug OFF")]
        private static void VSPrefOffClicked() => EnableLogs(false);

        [MenuItem("Void Tool/Visual Studio Task List View/Show Debug OFF", true)]
        private static bool VSPrefOff_validate() => EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log, false);

        #endregion validation

        [MenuItem("Void Tool/Visual Studio Task List View/Open TaskList %T", priority = 1)]
        private static void ShowWindow()
        {
            VSTaskListUI window = GetWindow<VSTaskListUI>();
            window.titleContent = new GUIContent("VS TaskList");
            window.minSize = new Vector2(300, 100);

            TabIndex = EditorPrefs.GetInt(VSTaskListConsts.pref_Chosen_Display_Method);
            window.Show();
        }

        private void OnGUI()
#pragma warning restore IDE0051 // Remove unused private members
        {
            // if null, length changed or refresh is requested using refreshFoldout
            if (DoFoldoutAt == null || refreshFoldout)
            {
                refreshFoldout = false;
                DoFoldoutAt = new bool[foldoutLength < 3 ? 3 : foldoutLength];
            }

            //saving the gui color for resetting
            m_editorColor = GUI.color;
            m_editorContentColor = GUI.contentColor;

            if (EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log))
            {
                GUI.backgroundColor = Color.clear;
            }
            else
            {
                GUI.backgroundColor = m_editorColor;
            }

            GUILayout.BeginHorizontal();
            //log display option
            if (GUILayout.Button(VSTaskListConsts.ButtonShowStatsContext, GUILayout.ExpandWidth(false)))
            {
                if (EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log))
                {
                    EnableLogs(false);
                }
                else
                {
                    EnableLogs(true);
                }
            }

            GUI.backgroundColor = m_editorColor;

            //making tabs in the gui window
            TabIndex = GUILayout.Toolbar(TabIndex, VSTaskListConsts.TabTitleText);

            GUILayout.EndHorizontal();

            //starting a scroll area
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            switch (TabIndex)
            {
                case 0:
                    {
                        OrderByFileGUI();
                        break;
                    }
                case 1:
                    {
                        OrderByPriorityGUI();
                        break;
                    }
                default:
                    break;
            }

            //ending a scroll area
            EditorGUILayout.EndScrollView();

            //refresh button
            if (GUILayout.Button(VSTaskListConsts.ButtonRefreshContext))
            {
                VSTaskListCore.TaskList.Clear();
                VSTaskListCore.ScanScripts(EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log));
            }

            if (EditorPrefs.GetInt(VSTaskListConsts.pref_Chosen_Display_Method) != TabIndex)
            {
                EditorPrefs.SetInt(VSTaskListConsts.pref_Chosen_Display_Method, TabIndex);
            }
        }

        #region displayMethods

        private void OrderByFileGUI()
        {
            foreach (GuiContainer container in VSTaskListCore.TaskList)
            {
                GUI.backgroundColor = m_editorColor;
                GUI.contentColor = m_editorContentColor;

                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();

                int index = VSTaskListCore.TaskList.IndexOf(container);
                DoFoldoutAt[index] = EditorGUILayout.BeginFoldoutHeaderGroup(
                    DoFoldoutAt[index],
                    string.Format(
                        "In Script \"{0}\"",
                        SplitandGetLastString(container.Data[0].FileName, '\\')
                    ),
                    EditorStyles.foldoutHeader
                );

                if (GUILayout.Button("Open File", GUILayout.ExpandWidth(false)))
                {
                    AssetDatabase.OpenAsset(
                        FromRelativePathToInstanceID(
                            container.Path
                        )
                    );
                }
                GUILayout.EndHorizontal();

                if (DoFoldoutAt[index])
                {
                    foreach (GuiData data in container.Data)
                    {
                        switch (data.PriorityLevel)
                        {
                            case MessagePriorityLevel.High:
                                {
                                    GUI.backgroundColor = VSTaskListConsts.m_priorityColorHigh;
                                    GUI.contentColor = VSTaskListConsts.m_contextColorHigh;
                                    break;
                                }
                            case MessagePriorityLevel.Question:
                                {
                                    GUI.backgroundColor = VSTaskListConsts.m_priorityColorQuestion;
                                    GUI.contentColor = VSTaskListConsts.m_contextColorQuestion;
                                    break;
                                }
                            case MessagePriorityLevel.TODO:
                                {
                                    GUI.backgroundColor = VSTaskListConsts.m_priorityColorNormal;
                                    GUI.contentColor = VSTaskListConsts.m_contextColorNormal;
                                    break;
                                }
                        }

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(
                            string.Format(
                                "{0} (on line {1})",
                                data.Message,
                                data.LineNumber
                                ),
                            EditorStyles.foldoutHeader
                            ))
                        {
                            AssetDatabase.OpenAsset(
                                FromRelativePathToInstanceID(
                                    container.Path
                                    ),
                                data.LineNumber
                                );
                        }

                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUI.contentColor = m_editorContentColor;
            GUI.backgroundColor = m_editorColor;
        }

        /// <summary>
        /// desplays the tasklist ordered by priority
        /// </summary>
        private void OrderByPriorityGUI()
        {
            for (int i = 0; i < VSTaskListConsts.TodoTypes.Length; i++)
            {
                MessagePriorityLevel level;

                level = (MessagePriorityLevel)i;
                EditorGUILayout.Space();
                DoFoldoutAt[i] = EditorGUILayout.BeginFoldoutHeaderGroup(
                    DoFoldoutAt[i],
                    $"{level}"
                    );

                if (DoFoldoutAt[i])
                {
                    //setting the style by type
                    switch (VSTaskListConsts.TodoTypes[i])
                    {
                        case VSTaskListConsts.Comment_exclamation:
                            {
                                GUI.backgroundColor = VSTaskListConsts.m_priorityColorHigh;
                                GUI.contentColor = VSTaskListConsts.m_contextColorHigh;
                                break;
                            }
                        case VSTaskListConsts.Comment_Question:
                            {
                                GUI.backgroundColor = VSTaskListConsts.m_priorityColorQuestion;
                                GUI.contentColor = VSTaskListConsts.m_contextColorQuestion;
                                break;
                            }
                        case VSTaskListConsts.Comment_Todo:
                            {
                                GUI.backgroundColor = VSTaskListConsts.m_priorityColorNormal;
                                GUI.contentColor = VSTaskListConsts.m_contextColorNormal;
                                break;
                            }
                    }

                    foreach (GuiContainer container in VSTaskListCore.TaskList)
                    {
                        foreach (GuiData data in container.Data)
                        {
                            if (data.PriorityLevel == (MessagePriorityLevel)i)
                            {
                                if (GUILayout.Button(
                                    string.Format(
                                        "{0}   ({1}, line: {2})",
                                        data.Message,
                                        SplitandGetLastString(data.FileName, '\\'),
                                        data.LineNumber
                                        ),
                                    EditorStyles.foldoutHeader
                                    ))
                                {
                                    AssetDatabase.OpenAsset(
                                        FromRelativePathToInstanceID(
                                            container.Path
                                            ),
                                        data.LineNumber
                                        );
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                GUI.backgroundColor = m_editorColor;
                GUI.contentColor = m_editorContentColor;
            }
        }

        #endregion displayMethods

        #region help functions

        internal static void ResizeFoldoutlist(int size, bool forceRefresh = false)
        {
            refreshFoldout = forceRefresh;
            foldoutLength = size;
        }

        /// <summary>
        /// gets the instance id using the guid using the relative path of the file
        /// </summary>
        /// <param name="relativePath">file path from within unity's "Assets" folder</param>
        /// <returns>instance id of the file</returns>
        internal static int GetInstance_IdFromGUIDAtRelativePath(string relativePath)
        {
            return AssetDatabase
                .LoadAssetAtPath<Object>(
                AssetDatabase.GUIDToAssetPath(
                    AssetDatabase.AssetPathToGUID(
                        relativePath
                        )
                    )
                )
                .GetInstanceID();
        }

        /// <param name="fullPath">file path (relative and full)</param>
        /// <returns>file name including extention</returns>
        internal static string GetFileName(string fullPath)
        {
            string[] s = fullPath.Split('\\');
            return s[s.Length - 1];
        }

        /// <summary>
        /// gets the path of the file from within the unity "Assets" folder
        /// </summary>
        /// <param name="fullPath">Full filepath on the storage medium</param>
        /// <returns>the path from the "Assets" folder to the file</returns>
        internal static string GetRelativePath(string fullPath)
        {
            string[] s = fullPath.Split('\\');
            string output = "";
            int k = 0;
            //cuts off every thing before "Assets"
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == "Assets")
                {
                    output += $"{s[i]}\\";
                    k = i;
                    break;
                }
            }
            //formats the string correctly
            for (int j = 1; j + k <= s.Length; j++)
            {
                if (j + k < s.Length - 1)
                {
                    output += $"{s[j + k]}\\";
                }
                else
                {
                    output += $"{s[j + k]}";
                    break;
                }
            }

            return output;
        }

        /// <summary>gets the InstanceId from the Relative Path used by the AssetDatabase</summary>
        /// <param name="relativePath">key value of outlook Dictionary</param>
        /// <returns>Returns the InstanceID of the c# file</returns>
        private static int FromRelativePathToInstanceID(string relativePath)
        {
            return GetInstance_IdFromGUIDAtRelativePath(
                GetRelativePath(
                    relativePath
                    )
                );
        }

        private static void EnableLogs(bool enable)
        {
            EditorPrefs.SetBool(VSTaskListConsts.pref_Show_Log, enable);
            Debug.Log(string.Format(
                "{0} Debug messages for VSTaskListView",
                enable ? "Enabled" : "Disabled"
                ));
        }

        #endregion help functions

        private Color m_editorColor;
        private Color m_editorContentColor;
        private Vector2 scrollPos;

        internal static bool[] DoFoldoutAt;
        private static int foldoutLength = 0;
        private static bool refreshFoldout = false;

        private static int tabIndex = 0;

        internal static int TabIndex
        {
            get => tabIndex;
            set
            {
                if (tabIndex != value)
                {
                    // resize so they all fold in
                    ResizeFoldoutlist(VSTaskListCore.TaskList.Count, true);
                    tabIndex = value;
                }
            }
        }

        private string SplitandGetLastString(string filePath, char spliter) =>
            filePath.Split(spliter).Last();
    }
}