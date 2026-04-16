using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using SharpGLTF.Schema2;

namespace Volumetric.Samples.SpatialPad
{
    public static class Data
    {
        // Create button options
        public enum ButtonType
        {
            none,
            win_desktop,
            win_clipboard,
            win_explorer,
            win_settings,
            win_snippingTool,
            word_bold,
            word_selectAll,
            word_find,
            word_replace,
            word_pageBreak,
            excel_currency,
            excel_percent,
            excel_sum,
            excel_table,
            excel_date,
            powertoys_alwaysOnTop,
            powertoys_colorPicker,
            powertoys_screenRuler,
            powertoys_shortcutGuide,
            powertoys_advancedPaste,
            teams_enableMic,
            teams_enableCamera,
            teams_enableCaptions,
            teams_chat,
            teams_raiseHand
        };
        public static Dictionary<ButtonType, ButtonData> ButtonsData = new Dictionary<ButtonType, ButtonData>
        {
            { ButtonType.none, new ButtonData("Assets/Models/EnabledKeys/Windows_Key1.glb", "Assets/Models/EnabledKeys/Windows_Key1.glb", "Assets/Images/slot.png") },

            { ButtonType.win_desktop, new ButtonData("Assets/Models/EnabledKeys/Windows_Key1.glb", "Assets/Models/DisabledKeys/Windows_OP_Key1.glb", "Assets/Images/Keys/key_windows_1.png") },
            { ButtonType.win_clipboard, new ButtonData("Assets/Models/EnabledKeys/Windows_Key2.glb", "Assets/Models/DisabledKeys/Windows_OP_Key2.glb", "Assets/Images/Keys/key_windows_2.png") },
            { ButtonType.win_explorer, new ButtonData("Assets/Models/EnabledKeys/Windows_Key3.glb", "Assets/Models/DisabledKeys/Windows_OP_Key3.glb", "Assets/Images/Keys/key_windows_3.png") },
            { ButtonType.win_settings, new ButtonData("Assets/Models/EnabledKeys/Windows_Key4.glb", "Assets/Models/DisabledKeys/Windows_OP_Key4.glb", "Assets/Images/Keys/key_windows_4.png") },
            { ButtonType.win_snippingTool, new ButtonData("Assets/Models/EnabledKeys/Windows_Key5.glb", "Assets/Models/DisabledKeys/Windows_OP_Key5.glb", "Assets/Images/Keys/key_windows_5.png") },

            { ButtonType.word_bold, new ButtonData("Assets/Models/EnabledKeys/word_Key1.glb", "Assets/Models/DisabledKeys/word_OP_Key1.glb", "Assets/Images/Keys/key_word_1.png") },
            { ButtonType.word_find, new ButtonData("Assets/Models/EnabledKeys/word_Key2.glb", "Assets/Models/DisabledKeys/word_OP_Key2.glb", "Assets/Images/Keys/key_word_2.png") },
            { ButtonType.word_pageBreak, new ButtonData("Assets/Models/EnabledKeys/word_Key3.glb", "Assets/Models/DisabledKeys/word_OP_Key3.glb", "Assets/Images/Keys/key_word_3.png") },
            { ButtonType.word_replace, new ButtonData("Assets/Models/EnabledKeys/word_Key4.glb", "Assets/Models/DisabledKeys/word_OP_Key4.glb", "Assets/Images/Keys/key_word_4.png") },
            { ButtonType.word_selectAll, new ButtonData("Assets/Models/EnabledKeys/word_Key5.glb",  "Assets/Models/DisabledKeys/word_OP_Key5.glb", "Assets/Images/Keys/key_word_5.png") },

            { ButtonType.excel_sum, new ButtonData("Assets/Models/EnabledKeys/excel_Key1.glb", "Assets/Models/DisabledKeys/excel_OP_Key1.glb", "Assets/Images/Keys/key_excel_1.png") },
            { ButtonType.excel_table, new ButtonData("Assets/Models/EnabledKeys/excel_Key2.glb", "Assets/Models/DisabledKeys/excel_OP_Key2.glb", "Assets/Images/Keys/key_excel_2.png") },
            { ButtonType.excel_currency, new ButtonData("Assets/Models/EnabledKeys/excel_Key3.glb", "Assets/Models/DisabledKeys/excel_OP_Key3.glb", "Assets/Images/Keys/key_excel_3.png") },
            { ButtonType.excel_date, new ButtonData("Assets/Models/EnabledKeys/excel_Key4.glb", "Assets/Models/DisabledKeys/excel_OP_Key4.glb", "Assets/Images/Keys/key_excel_4.png") },
            { ButtonType.excel_percent, new ButtonData("Assets/Models/EnabledKeys/excel_Key5.glb", "Assets/Models/DisabledKeys/excel_OP_Key5.glb", "Assets/Images/Keys/key_excel_5.png") },

            { ButtonType.powertoys_advancedPaste, new ButtonData("Assets/Models/EnabledKeys/PT_Key1.glb", "Assets/Models/DisabledKeys/PT_OP_Key1.glb", "Assets/Images/Keys/key_PT_1.png") },
            { ButtonType.powertoys_alwaysOnTop, new ButtonData("Assets/Models/EnabledKeys/PT_Key2.glb", "Assets/Models/DisabledKeys/PT_OP_Key2.glb", "Assets/Images/Keys/key_PT_2.png") },
            { ButtonType.powertoys_colorPicker, new ButtonData("Assets/Models/EnabledKeys/PT_Key3.glb", "Assets/Models/DisabledKeys/PT_OP_Key3.glb", "Assets/Images/Keys/key_PT_3.png") },
            { ButtonType.powertoys_screenRuler, new ButtonData("Assets/Models/EnabledKeys/PT_Key4.glb", "Assets/Models/DisabledKeys/PT_OP_Key4.glb", "Assets/Images/Keys/key_PT_4.png") },
            { ButtonType.powertoys_shortcutGuide, new ButtonData("Assets/Models/EnabledKeys/PT_Key5.glb", "Assets/Models/DisabledKeys/PT_OP_Key5.glb", "Assets/Images/Keys/key_PT_5.png") },

            { ButtonType.teams_enableCamera, new ButtonData("Assets/Models/EnabledKeys/teams_Key1.glb", "Assets/Models/DisabledKeys/teams_OP_Key1.glb", "Assets/Images/Keys/key_teams_1.png") },
            { ButtonType.teams_enableCaptions, new ButtonData("Assets/Models/EnabledKeys/teams_Key2.glb", "Assets/Models/DisabledKeys/teams_OP_Key2.glb", "Assets/Images/Keys/key_teams_2.png") },
            { ButtonType.teams_enableMic, new ButtonData("Assets/Models/EnabledKeys/teams_Key3.glb", "Assets/Models/DisabledKeys/teams_OP_Key3.glb", "Assets/Images/Keys/key_teams_3.png") },
            { ButtonType.teams_chat, new ButtonData("Assets/Models/EnabledKeys/teams_Key4.glb", "Assets/Models/DisabledKeys/teams_OP_Key4.glb", "Assets/Images/Keys/key_teams_4.png") },
            { ButtonType.teams_raiseHand, new ButtonData("Assets/Models/EnabledKeys/teams_Key5.glb", "Assets/Models/DisabledKeys/teams_OP_Key5.glb", "Assets/Images/Keys/key_teams_5.png") },
        };
    }

    // Set data for each button type
    public class ButtonData
    {
        public string ModelUri { get; set; }
        public string DisabledModelUri { get; set; }
        public string ModelMorphUri { get; set; }
        public string NormalImage { get; set; }
        public string ShadowImage { get; set; }
        public string ShadowHoverImage { get; set; }

        // Store vertex data for each button model and morph model
        public List<Vector3> MeshVertexPositions { get; private set; } = new List<Vector3>();
        public List<Vector3> MeshVertexNormals { get; private set; } = new List<Vector3>();
        public List<Vector4> MeshVertexTangents { get; private set; } = new List<Vector4>();

        public List<Vector3> MorphVertexPositions { get; private set; } = new List<Vector3>();
        public List<Vector3> MorphVertexNormals { get; private set; } = new List<Vector3>();
        public List<Vector4> MorphVertexTangents { get; private set; } = new List<Vector4>();

        public int VertexCount;
        public float[] BlendedNormals = Array.Empty<float>();
        public float[] BlendedTangents = Array.Empty<float>();
        public float[] BlendedVertices = Array.Empty<float>();

        public ButtonData(string modelUri, string disabledModelUri, string normalImage)
        {
            ModelUri = modelUri;
            DisabledModelUri = disabledModelUri;
            ModelMorphUri = "Assets/Models/Key_morph.glb";
            NormalImage = normalImage;
            ShadowImage = "Assets/Images/key_shadow.png";
            ShadowHoverImage = "Assets/Images/key_shadow_hover.png";
            _ = getMorphInfo(ModelMorphUri);
        }

        public async Task getMorphInfo(string uri)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uri).Replace("\\", "/");
            ModelRoot model = await Task.Run(() => ModelRoot.Load(assetPath));

            foreach (var node in model.LogicalNodes)
            {
                // Access the mesh of the model
                if (node.Mesh != null)
                {
                    // Loop through the primitives of the mesh
                    foreach (var primitive in node.Mesh.Primitives)
                    {
                        // Access vertex data
                        var positions = primitive.GetVertexAccessor("POSITION");

                        if (positions != null)
                        {
                            var posArray = positions.AsVector3Array();
                            if (posArray != null)
                                MeshVertexPositions = new List<Vector3>(posArray);
                        }

                        var normals = primitive.GetVertexAccessor("NORMAL");
                        if (normals != null)
                        {
                            var normArray = normals.AsVector3Array();
                            if (normArray != null)
                                MeshVertexNormals = new List<Vector3>(normArray);
                        }

                        var tangents = primitive.GetVertexAccessor("TANGENT");
                        if (tangents != null)
                        {
                            var tangArray = tangents.AsVector4Array();
                            if (tangArray != null)
                                MeshVertexTangents = new List<Vector4>(tangArray);
                        }

                        // Check if the primitive has any morph targets (blendshapes)
                        if (primitive.MorphTargetsCount > 0)
                        {
                            // Iterate through each morph target (blendshape)
                            for (int i = 0; i < primitive.MorphTargetsCount; i++)
                            {
                                var morphTarget = primitive.GetMorphTargetAccessors(i);

                                // Access morph data
                                if (morphTarget.TryGetValue("POSITION", out var positionAccessor))
                                {
                                    var morphPositions = positionAccessor.AsVector3Array();
                                    MorphVertexPositions = new List<Vector3>(morphPositions);
                                }

                                if (morphTarget.TryGetValue("NORMAL", out var normalAccessor))
                                {
                                    var morphNormals = normalAccessor.AsVector3Array();
                                    MorphVertexNormals = new List<Vector3>(morphNormals);
                                }

                                if (morphTarget.TryGetValue("TANGENT", out var tangentAccessor))
                                {
                                    var morphTangents = tangentAccessor.AsColorArray(0);
                                    MorphVertexTangents = new List<Vector4>(morphTangents);
                                }
                            }
                            VertexCount = Math.Min(MeshVertexPositions.Count, MorphVertexPositions.Count);
                            BlendedVertices = new float[VertexCount * 3];
                            BlendedNormals = new float[VertexCount * 3];
                            BlendedTangents = new float[VertexCount * 4];
                        }
                        else
                        {
                            Debug.WriteLine("No blendshapes found in " + node.Name);
                        }
                    }
                }
            }
        }
    }
}
