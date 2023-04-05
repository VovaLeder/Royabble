using UnityEngine;

namespace VoidTools
{
    internal enum MessagePriorityLevel
    {
        High = 0,
        Question,
        TODO,
        None = -1
    }

    class VSTaskListConsts
    {
        // prefs
        internal const string pref_Show_Log = "EditorPref_Show_Log_On_AssetLoad";
        internal const string pref_Chosen_Display_Method = "Editorpref_Chosen_Display_Method_GUI";

        // index titles
        internal static readonly string[] TabTitleText = new string[]
        {
            "Order By File",
            "Order By Priority",
        };

        // file constents
        internal const string FileExtension = ".cs";
        internal const char NewlineChar = '\n';

        // Comment_ ordered by priority
        internal const string Comment_exclamation = "//! ";
        internal const string Comment_Question = "//? ";
        internal const string Comment_Todo = "//TODO ";

        internal static readonly string[] TodoTypes = new string[]
        {
            Comment_exclamation,
            Comment_Question,
            Comment_Todo
        };

        // colors
        internal static readonly Color m_priorityColorHigh = Color.red / 10 * 7;
        internal static readonly Color m_priorityColorQuestion = Color.magenta / 10 * 7;
        internal static readonly Color m_priorityColorNormal = Color.cyan / 10 * 7;
        internal static readonly Color m_contextColorHigh = Color.yellow;
        internal static readonly Color m_contextColorQuestion = Color.white;
        internal static readonly Color m_contextColorNormal = Color.white;

        // buttons
        internal const string ButtonRefreshContext = "---Refresh---";
        internal const string ButtonShowStatsContext = "Show Stats";

        // stats
        internal static readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    }
}
