using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace UnityConstantsGenerator
{
    [Generator]
    public class UnityConstantsGenerator : ISourceGenerator
    {
        private enum AnimatorParameterType
        {
            Unknown = 0,
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9,
        }

        private static readonly DiagnosticDescriptor InputReadFailedDescriptor = new DiagnosticDescriptor(
            id: "UCG0002",
            title: "UnityConstant input read failed",
            messageFormat: "UnityConstant 입력 파일을 읽는 중 오류가 발생했습니다: {0}",
            category: "UnityConstantGenerator",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GeneratorFailedDescriptor = new DiagnosticDescriptor(
            id: "UCG0001",
            title: "UnityConstant generation failed",
            messageFormat: "UnityConstant 생성 실패: {0}",
            category: "UnityConstantGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly",
            "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
            "static", "string", "struct", "switch", "this", "throw", "true", "try",
            "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while"
        };

        private static readonly string[] BuiltInTags =
        {
            "Untagged",
            "Respawn",
            "Finish",
            "EditorOnly",
            "MainCamera",
            "Player",
            "GameController",
        };

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!IsSupportedAssembly(context.Compilation.AssemblyName))
            {
                return;
            }

            try
            {
                string sourceMethod;
                IReadOnlyDictionary<string, string> configConstants = CollectConfigConstants(context, out sourceMethod);
                IReadOnlyDictionary<string, IReadOnlyList<AnimatorParameterConstant>> animatorParametersByClass =
                    CollectAnimatorParameters(context);
                TagLayerConstantSet tagLayerConstants = CollectTagLayerConstants(context);
                IReadOnlyDictionary<string, string> sceneConstants = CollectSceneConstants(context);
                IReadOnlyDictionary<string, string> addressableLabelConstants = CollectAddressableLabelConstants(context);

                string generatedSource = BuildGeneratedSource(
                    sourceMethod,
                    configConstants,
                    animatorParametersByClass,
                    tagLayerConstants,
                    sceneConstants,
                    addressableLabelConstants);

                context.AddSource(
                    "UnityConstant_Generator.g.cs",
                    SourceText.From(generatedSource, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratorFailedDescriptor,
                    Location.None,
                    ex.ToString()));

                context.AddSource(
                    "UnityConstant_Generator_Error.g.cs",
                    SourceText.From(
                        $"/* error: {EscapeComment(ex.ToString())} */",
                        Encoding.UTF8));
            }
        }

        private static IReadOnlyDictionary<string, string> CollectConfigConstants(
            GeneratorExecutionContext context,
            out string sourceMethod)
        {
            string jsonText = null;
            sourceMethod = "Config source not found";

            AdditionalText configFile = context.AdditionalFiles.FirstOrDefault(
                file => file.Path.EndsWith("config.json", StringComparison.OrdinalIgnoreCase));

            if (configFile != null)
            {
                jsonText = configFile.GetText(context.CancellationToken)?.ToString();
                sourceMethod = $"AdditionalFiles: {configFile.Path}";
            }

            if (string.IsNullOrEmpty(jsonText))
            {
                foreach (string fallbackPath in EnumerateConfigPaths(context))
                {
                    try
                    {
                        if (!File.Exists(fallbackPath))
                        {
                            continue;
                        }

                        jsonText = File.ReadAllText(fallbackPath);
                        sourceMethod = $"Fallback (File.ReadAllText): {fallbackPath}";
                        break;
                    }
                    catch (Exception ex)
                    {
                        ReportReadFailure(context, fallbackPath, ex);
                    }
                }
            }

            if (string.IsNullOrEmpty(jsonText))
            {
                return new SortedDictionary<string, string>(StringComparer.Ordinal);
            }

            var constants = new SortedDictionary<string, string>(StringComparer.Ordinal);
            ParseJsonText(jsonText, constants);
            return constants;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<AnimatorParameterConstant>> CollectAnimatorParameters(
            GeneratorExecutionContext context)
        {
            var controllerToParameters =
                new SortedDictionary<string, List<AnimatorParameterConstant>>(StringComparer.Ordinal);

            foreach (string controllerPath in EnumerateAnimatorControllerPaths(context))
            {
                try
                {
                    string controllerClassName = BuildAnimatorClassName(controllerPath, controllerToParameters.Keys);
                    List<AnimatorParameterConstant> parameters = ParseAnimatorParameters(
                        File.ReadAllLines(controllerPath),
                        controllerClassName);

                    controllerToParameters[controllerClassName] = parameters;
                }
                catch (Exception ex)
                {
                    ReportReadFailure(context, controllerPath, ex);
                }
            }

            return controllerToParameters.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<AnimatorParameterConstant>)pair.Value
                    .OrderBy(item => item.ParameterName, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);
        }

        private static TagLayerConstantSet CollectTagLayerConstants(GeneratorExecutionContext context)
        {
            foreach (string tagManagerPath in EnumerateTagManagerPaths(context))
            {
                try
                {
                    if (!File.Exists(tagManagerPath))
                    {
                        continue;
                    }

                    return ParseTagManager(File.ReadAllLines(tagManagerPath));
                }
                catch (Exception ex)
                {
                    ReportReadFailure(context, tagManagerPath, ex);
                }
            }

            return TagLayerConstantSet.Empty;
        }

        private static IReadOnlyDictionary<string, string> CollectSceneConstants(GeneratorExecutionContext context)
        {
            foreach (string editorBuildSettingsPath in EnumerateEditorBuildSettingsPaths(context))
            {
                try
                {
                    if (!File.Exists(editorBuildSettingsPath))
                    {
                        continue;
                    }

                    return ParseEditorBuildSettings(File.ReadAllLines(editorBuildSettingsPath));
                }
                catch (Exception ex)
                {
                    ReportReadFailure(context, editorBuildSettingsPath, ex);
                }
            }

            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        private static IReadOnlyDictionary<string, string> CollectAddressableLabelConstants(
            GeneratorExecutionContext context)
        {
            var labels = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var usedIdentifiers = new HashSet<string>(StringComparer.Ordinal);
            var existingLabelNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (string addressableAssetPath in EnumerateAddressableAssetPaths(context))
            {
                try
                {
                    if (!File.Exists(addressableAssetPath))
                    {
                        continue;
                    }

                    ParseAddressableLabels(
                        File.ReadAllLines(addressableAssetPath),
                        labels,
                        usedIdentifiers,
                        existingLabelNames);
                }
                catch (Exception ex)
                {
                    ReportReadFailure(context, addressableAssetPath, ex);
                }
            }

            return labels;
        }

        private static bool IsSupportedAssembly(string assemblyName)
        {
            return string.Equals(assemblyName, "Assembly-CSharp", StringComparison.Ordinal) ||
                string.Equals(assemblyName, "TRPG.Runtime", StringComparison.Ordinal);
        }

        private static List<AnimatorParameterConstant> ParseAnimatorParameters(
            IEnumerable<string> lines,
            string controllerClassName)
        {
            var parameters = new List<AnimatorParameterConstant>();
            var usedIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            bool inParameters = false;
            string pendingParameterName = null;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (!inParameters)
                {
                    if (line.StartsWith("m_AnimatorParameters:", StringComparison.Ordinal))
                    {
                        inParameters = true;
                    }

                    continue;
                }

                if (rawLine.StartsWith("  m_AnimatorLayers:", StringComparison.Ordinal))
                {
                    break;
                }

                const string parameterNamePrefix = "- m_Name: ";
                if (line.StartsWith(parameterNamePrefix, StringComparison.Ordinal))
                {
                    pendingParameterName = line.Substring(parameterNamePrefix.Length).Trim();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(pendingParameterName))
                {
                    continue;
                }

                const string parameterTypePrefix = "m_Type: ";
                if (!line.StartsWith(parameterTypePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                int rawType;
                if (!int.TryParse(line.Substring(parameterTypePrefix.Length).Trim(), out rawType))
                {
                    pendingParameterName = null;
                    continue;
                }

                string parameterIdentifier = MakeUniqueIdentifier(
                    SanitizeIdentifier(pendingParameterName),
                    usedIdentifiers);

                parameters.Add(new AnimatorParameterConstant(
                    controllerClassName,
                    parameterIdentifier,
                    pendingParameterName,
                    MapAnimatorParameterType(rawType)));

                pendingParameterName = null;
            }

            return parameters;
        }

        private static TagLayerConstantSet ParseTagManager(IEnumerable<string> lines)
        {
            var tags = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var layers = new SortedDictionary<string, LayerConstant>(StringComparer.Ordinal);
            var usedTagIdentifiers = new HashSet<string>(StringComparer.Ordinal);
            var usedLayerIdentifiers = new HashSet<string>(StringComparer.Ordinal);
            var existingTagNames = new HashSet<string>(StringComparer.Ordinal);

            bool inTags = false;
            bool inLayers = false;
            int layerIndex = -1;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.Equals("tags:", StringComparison.Ordinal))
                {
                    inTags = true;
                    inLayers = false;
                    continue;
                }

                if (line.Equals("layers:", StringComparison.Ordinal))
                {
                    inTags = false;
                    inLayers = true;
                    layerIndex = -1;
                    continue;
                }

                if (line.StartsWith("m_SortingLayers:", StringComparison.Ordinal))
                {
                    break;
                }

                if (inTags && line.StartsWith("- ", StringComparison.Ordinal))
                {
                    string tagName = line.Substring(2).Trim();
                    if (string.IsNullOrWhiteSpace(tagName))
                    {
                        continue;
                    }

                    if (!existingTagNames.Add(tagName))
                    {
                        continue;
                    }

                    string tagIdentifier = MakeUniqueIdentifier(
                        SanitizeIdentifier(tagName),
                        usedTagIdentifiers);

                    tags[tagIdentifier] = tagName;
                    continue;
                }

                if (inLayers && line.StartsWith("-", StringComparison.Ordinal))
                {
                    layerIndex++;

                    string layerName = line.Length > 1 ? line.Substring(1).Trim() : string.Empty;
                    if (string.IsNullOrWhiteSpace(layerName))
                    {
                        continue;
                    }

                    if (layerIndex < 0 || layerIndex > 31)
                    {
                        continue;
                    }

                    string layerIdentifier = MakeUniqueIdentifier(
                        SanitizeIdentifier(layerName),
                        usedLayerIdentifiers);

                    layers[layerIdentifier] = new LayerConstant(layerIdentifier, layerName, layerIndex);
                }
            }

            foreach (string builtInTag in BuiltInTags)
            {
                if (!existingTagNames.Add(builtInTag))
                {
                    continue;
                }

                string tagIdentifier = MakeUniqueIdentifier(
                    SanitizeIdentifier(builtInTag),
                    usedTagIdentifiers);

                tags[tagIdentifier] = builtInTag;
            }

            return new TagLayerConstantSet(tags, layers.Values.ToList());
        }

        private static IReadOnlyDictionary<string, string> ParseEditorBuildSettings(IEnumerable<string> lines)
        {
            var scenes = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var usedSceneIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            bool inScenes = false;
            bool hasPendingScene = false;
            bool pendingSceneEnabled = false;
            string pendingScenePath = null;

            // EditorBuildSettings.asset is Unity YAML; only m_Scenes fields are needed here.
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.Equals("m_Scenes:", StringComparison.Ordinal))
                {
                    inScenes = true;
                    continue;
                }

                if (!inScenes)
                {
                    continue;
                }

                if (line.StartsWith("m_configObjects:", StringComparison.Ordinal))
                {
                    AddSceneConstant(
                        scenes,
                        usedSceneIdentifiers,
                        hasPendingScene,
                        pendingSceneEnabled,
                        pendingScenePath);
                    hasPendingScene = false;
                    break;
                }

                if (line.StartsWith("- ", StringComparison.Ordinal))
                {
                    AddSceneConstant(
                        scenes,
                        usedSceneIdentifiers,
                        hasPendingScene,
                        pendingSceneEnabled,
                        pendingScenePath);

                    hasPendingScene = true;
                    pendingSceneEnabled = false;
                    pendingScenePath = null;
                    line = line.Substring(2).Trim();
                }

                if (line.StartsWith("enabled:", StringComparison.Ordinal))
                {
                    pendingSceneEnabled = string.Equals(
                        line.Substring("enabled:".Length).Trim(),
                        "1",
                        StringComparison.Ordinal);
                    continue;
                }

                if (line.StartsWith("path:", StringComparison.Ordinal))
                {
                    pendingScenePath = line.Substring("path:".Length).Trim();
                }
            }

            AddSceneConstant(
                scenes,
                usedSceneIdentifiers,
                hasPendingScene,
                pendingSceneEnabled,
                pendingScenePath);

            return scenes;
        }

        private static void ParseAddressableLabels(
            IEnumerable<string> lines,
            IDictionary<string, string> labels,
            ISet<string> usedIdentifiers,
            ISet<string> existingLabelNames)
        {
            bool inLabelList = false;

            // Addressable labels are serialized in both the settings label table and group entries.
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("m_LabelNames:", StringComparison.Ordinal) ||
                    line.StartsWith("m_SerializedLabels:", StringComparison.Ordinal))
                {
                    AddInlineYamlLabels(line, labels, usedIdentifiers, existingLabelNames);
                    inLabelList = !line.EndsWith("[]", StringComparison.Ordinal);
                    continue;
                }

                if (!inLabelList)
                {
                    continue;
                }

                if (line.StartsWith("- ", StringComparison.Ordinal))
                {
                    AddAddressableLabelConstant(
                        line.Substring(2).Trim(),
                        labels,
                        usedIdentifiers,
                        existingLabelNames);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    inLabelList = false;
                }
            }
        }

        private static string BuildGeneratedSource(
            string sourceMethod,
            IReadOnlyDictionary<string, string> configConstants,
            IReadOnlyDictionary<string, IReadOnlyList<AnimatorParameterConstant>> animatorParametersByClass,
            TagLayerConstantSet tagLayerConstants,
            IReadOnlyDictionary<string, string> sceneConstants,
            IReadOnlyDictionary<string, string> addressableLabels)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine($"// Generated from: {sourceMethod}");
            sb.AppendLine("namespace UnityConstant");
            sb.AppendLine("{");

            AppendConfigClass(sb, configConstants);
            sb.AppendLine();
            AppendAnimatorClass(sb, animatorParametersByClass);
            sb.AppendLine();
            AppendScenesClass(sb, sceneConstants);
            sb.AppendLine();
            AppendAddressableClass(sb, addressableLabels);
            sb.AppendLine();
            AppendTagsClass(sb, tagLayerConstants.Tags);
            sb.AppendLine();
            AppendLayersClass(sb, tagLayerConstants.Layers);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendConfigClass(
            StringBuilder sb,
            IReadOnlyDictionary<string, string> configConstants)
        {
            sb.AppendLine("    public static partial class JsonConfig");
            sb.AppendLine("    {");

            foreach (KeyValuePair<string, string> pair in configConstants)
            {
                sb.Append("        public const string ");
                sb.Append(pair.Key);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(pair.Value));
                sb.AppendLine("\";");
            }

            sb.AppendLine("    }");
        }

        private static void AppendAnimatorClass(
            StringBuilder sb,
            IReadOnlyDictionary<string, IReadOnlyList<AnimatorParameterConstant>> animatorParametersByClass)
        {
            sb.AppendLine("    public static partial class Animator");
            sb.AppendLine("    {");
            sb.AppendLine("        public static partial class Parameters");
            sb.AppendLine("        {");

            foreach (KeyValuePair<string, IReadOnlyList<AnimatorParameterConstant>> pair in animatorParametersByClass)
            {
                sb.Append("            public static partial class ");
                sb.Append(pair.Key);
                sb.AppendLine();
                sb.AppendLine("            {");

                AppendAnimatorParameterTypeClass(sb, pair.Value, AnimatorParameterType.Bool, "Bool");
                AppendAnimatorParameterTypeClass(sb, pair.Value, AnimatorParameterType.Trigger, "Trigger");
                AppendAnimatorParameterTypeClass(sb, pair.Value, AnimatorParameterType.Int, "Int");
                AppendAnimatorParameterTypeClass(sb, pair.Value, AnimatorParameterType.Float, "Float");

                sb.AppendLine("            }");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static void AppendAnimatorParameterTypeClass(
            StringBuilder sb,
            IReadOnlyList<AnimatorParameterConstant> parameters,
            AnimatorParameterType parameterType,
            string typeClassName)
        {
            sb.Append("                public static partial class ");
            sb.Append(typeClassName);
            sb.AppendLine();
            sb.AppendLine("                {");

            foreach (AnimatorParameterConstant parameter in parameters)
            {
                if (parameter.ParameterType != parameterType)
                {
                    continue;
                }

                sb.Append("                    public const string ");
                sb.Append(parameter.ParameterIdentifier);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(parameter.ParameterName));
                sb.AppendLine("\";");
            }

            sb.AppendLine("                }");
        }

        private static void AppendScenesClass(
            StringBuilder sb,
            IReadOnlyDictionary<string, string> scenes)
        {
            sb.AppendLine("    public static partial class Scenes");
            sb.AppendLine("    {");

            foreach (KeyValuePair<string, string> pair in scenes)
            {
                sb.Append("        public const string ");
                sb.Append(pair.Key);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(pair.Value));
                sb.AppendLine("\";");
            }

            sb.AppendLine("    }");
        }

        private static void AppendAddressableClass(
            StringBuilder sb,
            IReadOnlyDictionary<string, string> labels)
        {
            sb.AppendLine("    public static partial class Addressable");
            sb.AppendLine("    {");
            sb.AppendLine("        public static partial class Label");
            sb.AppendLine("        {");

            foreach (KeyValuePair<string, string> pair in labels)
            {
                sb.Append("            public const string ");
                sb.Append(pair.Key);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(pair.Value));
                sb.AppendLine("\";");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static void AppendTagsClass(
            StringBuilder sb,
            IReadOnlyDictionary<string, string> tags)
        {
            sb.AppendLine("    public static partial class Tags");
            sb.AppendLine("    {");

            foreach (KeyValuePair<string, string> pair in tags)
            {
                sb.Append("        public const string ");
                sb.Append(pair.Key);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(pair.Value));
                sb.AppendLine("\";");
            }

            sb.AppendLine("    }");
        }

        private static void AppendLayersClass(
            StringBuilder sb,
            IReadOnlyList<LayerConstant> layers)
        {
            sb.AppendLine("    public static partial class Layers");
            sb.AppendLine("    {");

            foreach (LayerConstant layer in layers)
            {
                sb.Append("        public const string ");
                sb.Append(layer.Identifier);
                sb.Append(" = \"");
                sb.Append(EscapeStringLiteral(layer.Name));
                sb.AppendLine("\";");

                sb.Append("        public const int ");
                sb.Append(layer.Identifier);
                sb.Append("Index = ");
                sb.Append(layer.Index.ToString());
                sb.AppendLine(";");

                sb.Append("        public const int ");
                sb.Append(layer.Identifier);
                sb.Append("Mask = 1 << ");
                sb.Append(layer.Identifier);
                sb.AppendLine("Index;");
            }

            sb.AppendLine("    }");
        }

        private static void ParseJsonText(
            string jsonText,
            IDictionary<string, string> dict)
        {
            var parser = new JsonParser(jsonText);
            JsonValue root = parser.ParseValue();
            parser.EnsureFullyConsumed();
            FlattenJsonValue(root, string.Empty, dict);
        }

        private static void FlattenJsonValue(
            JsonValue value,
            string prefix,
            IDictionary<string, string> dict)
        {
            switch (value.Kind)
            {
                case JsonValueKind.Object:
                    foreach (KeyValuePair<string, JsonValue> prop in value.ObjectItems)
                    {
                        string propertyName = SanitizeIdentifier(prop.Key);
                        string newPrefix = string.IsNullOrEmpty(prefix)
                            ? propertyName
                            : $"{prefix}_{propertyName}";

                        FlattenJsonValue(prop.Value, newPrefix, dict);
                    }

                    break;

                case JsonValueKind.Array:
                    for (int index = 0; index < value.ArrayItems.Count; index++)
                    {
                        FlattenJsonValue(value.ArrayItems[index], $"{prefix}_{index}", dict);
                    }

                    break;

                default:
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        dict[prefix] = value.PrimitiveValue ?? string.Empty;
                    }

                    break;
            }
        }

        private static IEnumerable<string> EnumerateProjectRoots(GeneratorExecutionContext context)
        {
            var visitedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var baseDirectories = new List<string>
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory,
            };

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                "build_property.projectdir",
                out string projectDirectory))
            {
                baseDirectories.Add(projectDirectory);
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                "build_property.msbuildprojectdirectory",
                out string msbuildProjectDirectory))
            {
                baseDirectories.Add(msbuildProjectDirectory);
            }

            foreach (string syntaxTreePath in context.Compilation.SyntaxTrees
                .Select(tree => tree.FilePath)
                .Where(path => !string.IsNullOrWhiteSpace(path)))
            {
                string directoryPath = Path.GetDirectoryName(syntaxTreePath);
                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    baseDirectories.Add(directoryPath);
                }
            }

            foreach (string baseDirectory in baseDirectories.Where(path => !string.IsNullOrWhiteSpace(path)))
            {
                var current = new DirectoryInfo(baseDirectory);
                for (int depth = 0; current != null && depth < 8; depth++, current = current.Parent)
                {
                    string assetsPath = Path.Combine(current.FullName, "Assets");
                    if (!Directory.Exists(assetsPath))
                    {
                        continue;
                    }

                    if (visitedRoots.Add(current.FullName))
                    {
                        yield return current.FullName;
                    }
                }
            }
        }

        private static IEnumerable<string> EnumerateConfigPaths(GeneratorExecutionContext context)
        {
            foreach (string root in EnumerateProjectRoots(context))
            {
                yield return Path.Combine(root, "Assets", "config.json");
                yield return Path.Combine(root, "Assets", "Config.json");
            }
        }

        private static IEnumerable<string> EnumerateAnimatorControllerPaths(GeneratorExecutionContext context)
        {
            var yieldedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string root in EnumerateProjectRoots(context))
            {
                string assetsPath = Path.Combine(root, "Assets");
                if (!Directory.Exists(assetsPath))
                {
                    continue;
                }

                IEnumerable<string> controllerPaths;
                try
                {
                    controllerPaths = Directory.EnumerateFiles(
                        assetsPath,
                        "*.controller",
                        SearchOption.AllDirectories);
                }
                catch
                {
                    continue;
                }

                foreach (string controllerPath in controllerPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    if (yieldedPaths.Add(controllerPath))
                    {
                        yield return controllerPath;
                    }
                }
            }
        }

        private static IEnumerable<string> EnumerateTagManagerPaths(GeneratorExecutionContext context)
        {
            foreach (string root in EnumerateProjectRoots(context))
            {
                yield return Path.Combine(root, "ProjectSettings", "TagManager.asset");
            }
        }

        private static IEnumerable<string> EnumerateEditorBuildSettingsPaths(GeneratorExecutionContext context)
        {
            foreach (string root in EnumerateProjectRoots(context))
            {
                yield return Path.Combine(root, "ProjectSettings", "EditorBuildSettings.asset");
            }
        }

        private static IEnumerable<string> EnumerateAddressableAssetPaths(GeneratorExecutionContext context)
        {
            var yieldedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string root in EnumerateProjectRoots(context))
            {
                string addressablesDataPath = Path.Combine(root, "Assets", "AddressableAssetsData");
                if (!Directory.Exists(addressablesDataPath))
                {
                    continue;
                }

                string settingsPath = Path.Combine(addressablesDataPath, "AddressableAssetSettings.asset");
                if (yieldedPaths.Add(settingsPath))
                {
                    yield return settingsPath;
                }

                string groupsPath = Path.Combine(addressablesDataPath, "AssetGroups");
                if (!Directory.Exists(groupsPath))
                {
                    continue;
                }

                IEnumerable<string> groupAssetPaths;
                try
                {
                    groupAssetPaths = Directory.EnumerateFiles(
                        groupsPath,
                        "*.asset",
                        SearchOption.AllDirectories);
                }
                catch
                {
                    continue;
                }

                foreach (string groupAssetPath in groupAssetPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    if (yieldedPaths.Add(groupAssetPath))
                    {
                        yield return groupAssetPath;
                    }
                }
            }
        }

        private static void ReportReadFailure(
            GeneratorExecutionContext context,
            string path,
            Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InputReadFailedDescriptor,
                Location.None,
                $"{path} ({ex.Message})"));
        }

        private static string BuildAnimatorClassName(
            string controllerPath,
            IEnumerable<string> existingClassNames)
        {
            string fileName = Path.GetFileNameWithoutExtension(controllerPath);
            string sanitizedName = SanitizeIdentifier(fileName);
            var usedNames = new HashSet<string>(existingClassNames, StringComparer.Ordinal);

            if (!usedNames.Contains(sanitizedName))
            {
                return sanitizedName;
            }

            string[] segments = controllerPath
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .ToArray();

            for (int count = 2; count <= segments.Length; count++)
            {
                string suffix = string.Join("_", segments.Skip(segments.Length - count).Take(count - 1));
                string candidate = SanitizeIdentifier($"{fileName}_{suffix}");
                if (!usedNames.Contains(candidate))
                {
                    return candidate;
                }
            }

            int index = 1;
            while (true)
            {
                string candidate = $"{sanitizedName}_{index}";
                if (!usedNames.Contains(candidate))
                {
                    return candidate;
                }

                index++;
            }
        }

        private static string EscapeStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static string EscapeComment(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("*/", "* /");
        }

        private static void AddInlineYamlLabels(
            string line,
            IDictionary<string, string> labels,
            ISet<string> usedIdentifiers,
            ISet<string> existingLabelNames)
        {
            int openBracketIndex = line.IndexOf('[');
            int closeBracketIndex = line.LastIndexOf(']');
            if (openBracketIndex < 0 || closeBracketIndex <= openBracketIndex)
            {
                return;
            }

            string inlineList = line.Substring(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1);
            foreach (string label in inlineList.Split(','))
            {
                AddAddressableLabelConstant(
                    label.Trim(),
                    labels,
                    usedIdentifiers,
                    existingLabelNames);
            }
        }

        private static void AddAddressableLabelConstant(
            string labelName,
            IDictionary<string, string> labels,
            ISet<string> usedIdentifiers,
            ISet<string> existingLabelNames)
        {
            labelName = NormalizeYamlScalar(labelName);
            if (string.IsNullOrWhiteSpace(labelName) || !existingLabelNames.Add(labelName))
            {
                return;
            }

            string labelIdentifier = MakeUniqueIdentifier(
                SanitizeIdentifier(labelName),
                usedIdentifiers);

            labels[labelIdentifier] = labelName;
        }

        private static string NormalizeYamlScalar(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim();
            if (value.Length >= 2 &&
                ((value[0] == '\'' && value[value.Length - 1] == '\'') ||
                    (value[0] == '"' && value[value.Length - 1] == '"')))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        private static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "_";
            }

            var sb = new StringBuilder(value.Length + 8);

            char first = value[0];
            if (!char.IsLetter(first) && first != '_')
            {
                sb.Append('_');
            }

            foreach (char ch in value)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('_');
                }
            }

            string identifier = sb.ToString();
            if (ReservedKeywords.Contains(identifier))
            {
                identifier = "_" + identifier;
            }

            return identifier;
        }

        private static string MakeUniqueIdentifier(
            string baseIdentifier,
            ISet<string> usedIdentifiers)
        {
            string candidate = baseIdentifier;
            int index = 1;

            while (!usedIdentifiers.Add(candidate))
            {
                candidate = $"{baseIdentifier}_{index}";
                index++;
            }

            return candidate;
        }

        private static void AddSceneConstant(
            IDictionary<string, string> scenes,
            ISet<string> usedSceneIdentifiers,
            bool hasScene,
            bool isEnabled,
            string scenePath)
        {
            if (!hasScene || !isEnabled || string.IsNullOrWhiteSpace(scenePath))
            {
                return;
            }

            string sceneName = GetSceneName(scenePath);
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            string sceneIdentifier = MakeUniqueIdentifier(
                SanitizeIdentifier(sceneName),
                usedSceneIdentifiers);

            scenes[sceneIdentifier] = sceneName;
        }

        private static string GetSceneName(string scenePath)
        {
            string normalizedPath = scenePath.Replace('\\', '/');
            int slashIndex = normalizedPath.LastIndexOf('/');
            string fileName = slashIndex >= 0
                ? normalizedPath.Substring(slashIndex + 1)
                : normalizedPath;

            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static AnimatorParameterType MapAnimatorParameterType(int rawType)
        {
            switch (rawType)
            {
                case 1:
                    return AnimatorParameterType.Float;

                case 3:
                    return AnimatorParameterType.Int;

                case 4:
                    return AnimatorParameterType.Bool;

                case 9:
                    return AnimatorParameterType.Trigger;

                default:
                    return AnimatorParameterType.Unknown;
            }
        }

        private readonly struct AnimatorParameterConstant
        {
            public AnimatorParameterConstant(
                string controllerClassName,
                string parameterIdentifier,
                string parameterName,
                AnimatorParameterType parameterType)
            {
                ControllerClassName = controllerClassName;
                ParameterIdentifier = parameterIdentifier;
                ParameterName = parameterName;
                ParameterType = parameterType;
            }

            public string ControllerClassName { get; }

            public string ParameterIdentifier { get; }

            public string ParameterName { get; }

            public AnimatorParameterType ParameterType { get; }
        }

        private readonly struct TagLayerConstantSet
        {
            public static readonly TagLayerConstantSet Empty =
                new TagLayerConstantSet(
                    new SortedDictionary<string, string>(StringComparer.Ordinal),
                    new List<LayerConstant>());

            public TagLayerConstantSet(
                IReadOnlyDictionary<string, string> tags,
                IReadOnlyList<LayerConstant> layers)
            {
                Tags = tags;
                Layers = layers;
            }

            public IReadOnlyDictionary<string, string> Tags { get; }

            public IReadOnlyList<LayerConstant> Layers { get; }
        }

        private readonly struct LayerConstant
        {
            public LayerConstant(string identifier, string name, int index)
            {
                Identifier = identifier;
                Name = name;
                Index = index;
            }

            public string Identifier { get; }

            public string Name { get; }

            public int Index { get; }
        }

        private enum JsonValueKind
        {
            Object,
            Array,
            String,
            Number,
            Boolean,
            Null,
        }

        private sealed class JsonValue
        {
            public JsonValue(IReadOnlyDictionary<string, JsonValue> objectItems)
            {
                Kind = JsonValueKind.Object;
                ObjectItems = objectItems;
                ArrayItems = Array.Empty<JsonValue>();
                PrimitiveValue = null;
            }

            public JsonValue(IReadOnlyList<JsonValue> arrayItems)
            {
                Kind = JsonValueKind.Array;
                ObjectItems = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                ArrayItems = arrayItems;
                PrimitiveValue = null;
            }

            public JsonValue(JsonValueKind kind, string primitiveValue)
            {
                Kind = kind;
                ObjectItems = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                ArrayItems = Array.Empty<JsonValue>();
                PrimitiveValue = primitiveValue;
            }

            public JsonValueKind Kind { get; }

            public IReadOnlyDictionary<string, JsonValue> ObjectItems { get; }

            public IReadOnlyList<JsonValue> ArrayItems { get; }

            public string PrimitiveValue { get; }
        }

        private sealed class JsonParser
        {
            private readonly string text;
            private int index;

            public JsonParser(string text)
            {
                this.text = text ?? string.Empty;
            }

            public JsonValue ParseValue()
            {
                SkipWhitespace();

                if (index >= text.Length)
                {
                    throw new FormatException("JSON 입력이 비어 있습니다.");
                }

                char ch = text[index];
                switch (ch)
                {
                    case '{':
                        return ParseObject();

                    case '[':
                        return ParseArray();

                    case '"':
                        return new JsonValue(JsonValueKind.String, ParseString());

                    case 't':
                        ConsumeLiteral("true");
                        return new JsonValue(JsonValueKind.Boolean, "true");

                    case 'f':
                        ConsumeLiteral("false");
                        return new JsonValue(JsonValueKind.Boolean, "false");

                    case 'n':
                        ConsumeLiteral("null");
                        return new JsonValue(JsonValueKind.Null, "null");

                    default:
                        if (ch == '-' || char.IsDigit(ch))
                        {
                            return new JsonValue(JsonValueKind.Number, ParseNumber());
                        }

                        throw new FormatException($"지원되지 않는 JSON 토큰입니다. index={index}, char='{ch}'");
                }
            }

            public void EnsureFullyConsumed()
            {
                SkipWhitespace();
                if (index != text.Length)
                {
                    throw new FormatException($"JSON 파싱 후 남은 문자가 있습니다. index={index}");
                }
            }

            private JsonValue ParseObject()
            {
                Consume('{');
                SkipWhitespace();

                var items = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                if (TryConsume('}'))
                {
                    return new JsonValue(items);
                }

                while (true)
                {
                    SkipWhitespace();
                    string key = ParseString();
                    SkipWhitespace();
                    Consume(':');

                    JsonValue value = ParseValue();
                    items[key] = value;

                    SkipWhitespace();
                    if (TryConsume('}'))
                    {
                        break;
                    }

                    Consume(',');
                }

                return new JsonValue(items);
            }

            private JsonValue ParseArray()
            {
                Consume('[');
                SkipWhitespace();

                var items = new List<JsonValue>();
                if (TryConsume(']'))
                {
                    return new JsonValue(items);
                }

                while (true)
                {
                    items.Add(ParseValue());
                    SkipWhitespace();

                    if (TryConsume(']'))
                    {
                        break;
                    }

                    Consume(',');
                }

                return new JsonValue(items);
            }

            private string ParseString()
            {
                Consume('"');

                var sb = new StringBuilder();
                while (index < text.Length)
                {
                    char ch = text[index++];
                    if (ch == '"')
                    {
                        return sb.ToString();
                    }

                    if (ch != '\\')
                    {
                        sb.Append(ch);
                        continue;
                    }

                    if (index >= text.Length)
                    {
                        throw new FormatException("문자열 이스케이프가 비정상적으로 종료되었습니다.");
                    }

                    char escaped = text[index++];
                    switch (escaped)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            sb.Append(escaped);
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'u':
                            sb.Append(ParseUnicodeEscape());
                            break;
                        default:
                            throw new FormatException($"지원되지 않는 문자열 이스케이프입니다: \\{escaped}");
                    }
                }

                throw new FormatException("문자열이 닫히지 않았습니다.");
            }

            private char ParseUnicodeEscape()
            {
                if (index + 4 > text.Length)
                {
                    throw new FormatException("유니코드 이스케이프 길이가 부족합니다.");
                }

                string hex = text.Substring(index, 4);
                index += 4;

                ushort codePoint;
                if (!ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out codePoint))
                {
                    throw new FormatException($"잘못된 유니코드 이스케이프입니다: \\u{hex}");
                }

                return (char)codePoint;
            }

            private string ParseNumber()
            {
                int start = index;

                if (text[index] == '-')
                {
                    index++;
                }

                ConsumeDigits();

                if (index < text.Length && text[index] == '.')
                {
                    index++;
                    ConsumeDigits();
                }

                if (index < text.Length && (text[index] == 'e' || text[index] == 'E'))
                {
                    index++;
                    if (index < text.Length && (text[index] == '+' || text[index] == '-'))
                    {
                        index++;
                    }

                    ConsumeDigits();
                }

                return text.Substring(start, index - start);
            }

            private void ConsumeDigits()
            {
                int start = index;
                while (index < text.Length && char.IsDigit(text[index]))
                {
                    index++;
                }

                if (start == index)
                {
                    throw new FormatException($"숫자 파싱 중 숫자가 필요합니다. index={index}");
                }
            }

            private void ConsumeLiteral(string literal)
            {
                if (index + literal.Length > text.Length ||
                    !string.Equals(text.Substring(index, literal.Length), literal, StringComparison.Ordinal))
                {
                    throw new FormatException($"리터럴 '{literal}' 파싱 실패. index={index}");
                }

                index += literal.Length;
            }

            private void Consume(char expected)
            {
                SkipWhitespace();
                if (index >= text.Length || text[index] != expected)
                {
                    throw new FormatException($"'{expected}' 문자가 필요합니다. index={index}");
                }

                index++;
            }

            private bool TryConsume(char expected)
            {
                SkipWhitespace();
                if (index < text.Length && text[index] == expected)
                {
                    index++;
                    return true;
                }

                return false;
            }

            private void SkipWhitespace()
            {
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                {
                    index++;
                }
            }
        }
    }
}
