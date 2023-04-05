using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;

namespace VoidTools
{
    internal sealed class VSTaskListCore
    {
        internal static List<GuiContainer> TaskList = new List<GuiContainer>();

        internal static void AsyncSearchForScripts()
        {
            bool doLog = EditorPrefs.GetBool(VSTaskListConsts.pref_Show_Log);
            Task.Factory.StartNew(() =>
                ScanScripts(doLog)
            );
        }

        internal static void ScanScripts(bool showStats)
        {
            VSTaskListConsts.stopwatch.Reset();
            VSTaskListConsts.stopwatch.Start();

            Dictionary<string, string> results = new Dictionary<string, string>(20);

            string[] paths = Directory.GetFiles(
                Directory.GetCurrentDirectory() + "\\Assets\\",
                $"*{VSTaskListConsts.FileExtension}",
                SearchOption.AllDirectories
                );

            foreach (string path in paths)
            {
                if (path.EndsWith(VSTaskListConsts.FileExtension))
                {
                    results.Add(
                        path,
                        File.ReadAllText(path)
                        );
                }
            }

            int nCores = Environment.ProcessorCount > 2 ? Environment.ProcessorCount / 2 : 1;

            results
                .Values
                .AsParallel()
                .WithDegreeOfParallelism(nCores)
                .ForAll((rawScript) =>
                {
                    GuiContainer container = new GuiContainer(100);
                    int start = 0;

                    List<string> lines = rawScript.Split(VSTaskListConsts.NewlineChar).ToList();

                    for (int i = 0; i < lines.Count; i++)
                    {
                        string line = lines[i];

                        foreach (string todoType in VSTaskListConsts.TodoTypes)
                        {
                            if (line.Contains(todoType))
                            {
                                start = line.IndexOf(todoType) + todoType.Length;

                                MessagePriorityLevel level = MessagePriorityLevel.None;

                                switch (todoType)
                                {
                                    case VSTaskListConsts.Comment_exclamation:
                                        {
                                            level = MessagePriorityLevel.High;
                                            break;
                                        }
                                    case VSTaskListConsts.Comment_Todo:
                                        {
                                            level = MessagePriorityLevel.TODO;
                                            break;
                                        }
                                    case VSTaskListConsts.Comment_Question:
                                        {
                                            level = MessagePriorityLevel.Question;
                                            break;
                                        }
                                    default:
                                        {
                                            level = MessagePriorityLevel.None;
                                            Debug.LogError("VSTaskList [ERROR] something went wrong: " + todoType);
                                            break;
                                        }
                                }

                                if (level != MessagePriorityLevel.None && line.Length - start > 3)
                                {
                                    container.Data.Add(new GuiData
                                    {
                                        PriorityLevel = level,
                                        LineNumber = i + 1,
                                        Message = line.Remove(0, start).TrimStart('/', '?', '!'),
                                        FileName = DictExtension.GetKeyByValueInput(results, rawScript),
                                    });
                                    break;
                                }
                            }
                        }
                    }

                    if (container.Data.Count > 0)
                    {
                        container.Path = DictExtension.GetKeyByValueInput(results, rawScript);
                        container.Data = container
                            .Data
                            .OrderBy(x => x.PriorityLevel)
                            .ThenBy(x => x.LineNumber)
                            .ToList();

                        TaskList.Add(container);
                    }
                }
            );

            TaskList = TaskList.OrderBy(x => x.Path).ToList();

            VSTaskListConsts.stopwatch.Stop();

            if (showStats)
            {
                int numComments = 0;
                TaskList.ForEach(x => numComments += x.Data.Count);
                Debug.Log(string.Format("Finished searching through {0} files and found {1} comments in {2}ms.", paths.Length, numComments, VSTaskListConsts.stopwatch.ElapsedMilliseconds));
            }

            VSTaskListUI.ResizeFoldoutlist(TaskList.Count);
        }
    }

    internal static class DictExtension
    {
        //found this function on stack overflow. couldn`t find the source
        internal static T GetKeyByValueInput<T, W>(this Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }
    }
}
