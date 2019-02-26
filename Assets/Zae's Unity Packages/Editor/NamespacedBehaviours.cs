using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace ZaeTools.Unity.Editor
{
    public static class NamespacedBehaviours
    {
        private const string FILENAME = "NewBehaviour.cs";
        private const string TEMPLATE =
            "using System.Collections;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using UnityEngine;\r\n" +
            "\r\n" +
            "namespace {0}\r\n" +
            "{{\r\n" +
            "    public class {1} : MonoBehaviour\r\n" +
            "    {{\r\n" +
            "        public void Start()\r\n" +
            "        {{\r\n" +
            "            \r\n" +
            "        }}\r\n" +
            "        \r\n" +
            "        public void Update()\r\n" +
            "        {{\r\n" +
            "            \r\n" +
            "        }}\r\n" +
            "    }}\r\n" +
            "}}\r\n";

        private static readonly Regex NamespaceRegex = new Regex("[^a-zA-Z0-9.]");

        [MenuItem("Assets/Create/C# Script (namespaced)", priority = 80)]
        public static void CreateScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<ScriptNamedAction>(), FILENAME, null, null);
        }

        internal static void CreateScript(string path, string scriptName, string scriptNamespace)
        {
            string template = GetTemplateWithCorrectLineEndings(EditorSettings.lineEndingsForNewScripts);
            string fullPath = Path.GetFullPath(path);

            string content;
            if (scriptNamespace.Equals(string.Empty))
                scriptNamespace = Application.productName;
            scriptNamespace = NamespaceRegex.Replace(scriptNamespace, string.Empty);
            content = string.Format(template, scriptNamespace, scriptName);

            File.WriteAllText(fullPath, content, new System.Text.UTF8Encoding(true));
            AssetDatabase.ImportAsset(path);
        }

        private static string GetTemplateWithCorrectLineEndings(LineEndingsMode lineEndingsMode)
        {
            bool removeLineFeed = false;

            switch (lineEndingsMode)
            {
                case LineEndingsMode.Windows:
                    removeLineFeed = false;
                    break;

                case LineEndingsMode.OSNative:
                    if (Application.platform != RuntimePlatform.WindowsEditor)
                        removeLineFeed = true;
                    break;

                case LineEndingsMode.Unix:
                default:
                    removeLineFeed = true;
                    break;
            }

            if (removeLineFeed)
                return TEMPLATE.Replace("\r", string.Empty);
            else
                return TEMPLATE;
        }

        public class ScriptNamedAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                int fileNamePoint = pathName.LastIndexOf('/') + 1;
                string fileName = pathName.Remove(pathName.Length - 3).Remove(0, fileNamePoint);
                string _namespace = EditorSettings.projectGenerationRootNamespace;

                if (fileName.Contains("."))
                {
                    pathName = pathName.Remove(fileNamePoint);

                    int splitPoint = fileName.LastIndexOf('.');
                    _namespace += "." + fileName.Remove(splitPoint);
                    fileName = fileName.Remove(0, splitPoint + 1);

                    pathName += fileName + ".cs";
                }

                CreateScript(pathName, fileName, _namespace);
            }
        }
    }
}