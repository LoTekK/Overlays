using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace TeckArtist.Overlays.Editor
{
    [Overlay(typeof(SceneView), "SceneView Shader")]
    public class SceneViewVisualiser : Overlay
    {
        private SceneViewShaderConfig m_ShaderConfig;
        private Shader m_Shader;
        // TODO: switch this out for SettingsProvider?
        private IEnumerable<SceneViewShaderConfig> m_ShaderConfigs;
        private DropdownField m_ShaderConfigDropdown;
        private VisualElement m_Root;
        private VisualElement m_Properties;

        public override VisualElement CreatePanelContent()
        {
            m_ShaderConfigs = AssetDatabase.FindAssets("t:SceneViewShaderConfig").Select(g => AssetDatabase.GUIDToAssetPath(g)).Select(p => AssetDatabase.LoadAssetAtPath<SceneViewShaderConfig>(p));
            m_Root = new VisualElement();
            m_ShaderConfigDropdown = new DropdownField("Shader")
            {
                choices = m_ShaderConfigs.Select(c => Regex.Replace(c.Shader.name, ".*\\/", "")).ToList(),
                index = m_ShaderConfigs.ToList().IndexOf(m_ShaderConfig)
            };
            m_ShaderConfigDropdown.RegisterValueChangedCallback(e =>
                {
                    m_ShaderConfig = m_ShaderConfigs.ElementAt(m_ShaderConfigDropdown.index);
                    m_Shader = m_ShaderConfig.Shader;
                    InitialiseShader(m_Shader);
                    SetShader(m_ShaderConfig);
                    m_Properties.Clear();
                    m_Properties.Add(BuildGUI());
                    m_Properties.MarkDirtyRepaint();
                });
            m_Root.Add(m_ShaderConfigDropdown);
            m_Properties = new HelpBox();
            m_Properties.Add(BuildGUI());
            m_Root.Add(m_Properties);
            return m_Root;
        }

        private VisualElement BuildGUI()
        {
            var root = new VisualElement();
            if (m_Shader)
            {
                var count = ShaderUtil.GetPropertyCount(m_Shader);
                for (int i = 0; i < count; ++i)
                {
                    var field = CreateShaderField(m_Shader, i);
                    if (field == null)
                    {
                        continue;
                    }
                    root.Add(field);
                }
            }
            root.style.width = 250;
            if (root.childCount == 0)
            {
                var label = new Label("No tweakable properties");
                label.style.unityFontStyleAndWeight = FontStyle.Italic;
                root.Add(label);
            }
            return root;
        }

        private VisualElement CreateShaderField(Shader shader, int index)
        {
            if (ShaderUtil.IsShaderPropertyHidden(shader, index) || ShaderUtil.IsShaderPropertyNonModifiableTexureProperty(shader, index))
            {
                return null;
            }
            var root = new VisualElement();
            var desc = ObjectNames.NicifyVariableName(shader.GetPropertyDescription(index));
            var name = shader.GetPropertyName(index);
            switch (shader.GetPropertyType(index))
            {
                case ShaderPropertyType.Color:
                    var colorField = new ColorField(desc)
                    {
                        value = Shader.GetGlobalColor(name)
                    };
                    colorField.RegisterValueChangedCallback(e => Shader.SetGlobalColor(name, e.newValue));
                    root.Add(colorField);
                    break;
                case ShaderPropertyType.Float:
                    var floatField = new FloatField(desc)
                    {
                        value = Shader.GetGlobalFloat(name)
                    };
                    floatField.RegisterValueChangedCallback(e => Shader.SetGlobalFloat(name, e.newValue));
                    root.Add(floatField);
                    break;
                case ShaderPropertyType.Int:
                    var intField = new IntegerField(desc)
                    {
                        value = Shader.GetGlobalInteger(name)
                    };
                    intField.RegisterValueChangedCallback(e => Shader.SetGlobalInteger(name, e.newValue));
                    root.Add(intField);
                    break;
                case ShaderPropertyType.Range:
                    var limits = shader.GetPropertyRangeLimits(index);
                    var rangeField = new Slider(desc, limits.x, limits.y)
                    {
                        value = Shader.GetGlobalFloat(name),
                        showInputField = true
                    };
                    rangeField.RegisterValueChangedCallback(e => Shader.SetGlobalFloat(name, e.newValue));
                    root.Add(rangeField);
                    break;
                case ShaderPropertyType.Texture:
                    break;
                case ShaderPropertyType.Vector:
                    var vecField = new Vector4Field(desc)
                    {
                        value = Shader.GetGlobalVector(name)
                    };
                    vecField.RegisterValueChangedCallback(e => Shader.SetGlobalVector(name, e.newValue));
                    root.Add(vecField);
                    break;
            }
            return root;
        }

        private void InitialiseShader(Shader shader)
        {
            if (shader == null)
            {
                return;
            }
            for (int i = 0; i < shader.GetPropertyCount(); ++i)
            {
                var name = shader.GetPropertyName(i);
                var type = shader.GetPropertyType(i);
                switch (type)
                {
                    case ShaderPropertyType.Color:
                        Shader.SetGlobalColor(name, shader.GetPropertyDefaultVectorValue(i));
                        break;
                    case ShaderPropertyType.Float:
                        Shader.SetGlobalFloat(name, shader.GetPropertyDefaultFloatValue(i));
                        break;
                    case ShaderPropertyType.Int:
                        Shader.SetGlobalInteger(name, (int)shader.GetPropertyDefaultFloatValue(i));
                        break;
                    case ShaderPropertyType.Range:
                        Shader.SetGlobalFloat(name, shader.GetPropertyDefaultFloatValue(i));
                        break;
                    case ShaderPropertyType.Texture:
                        // Debug.Log(shader.GetPropertyTextureDefaultName(i));
                        break;
                    case ShaderPropertyType.Vector:
                        Shader.SetGlobalVector(name, shader.GetPropertyDefaultVectorValue(i));
                        break;
                }
            }
        }

        public override void OnCreated()
        {
            m_ShaderConfig = AssetDatabase.LoadAssetAtPath<SceneViewShaderConfig>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:SceneViewShaderConfig").FirstOrDefault()));
            m_Shader = m_ShaderConfig?.Shader;
            InitialiseShader(m_Shader);
            EditorApplication.delayCall += () => { if (displayed) SetShader(m_ShaderConfig); };
            displayedChanged += display =>
            {
                if (display)
                {
                    SetShader(m_ShaderConfig);
                }
                else
                {
                    SetShader(null);
                }
            };
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                if (displayed)
                {
                    SetShader(m_ShaderConfig);
                }
                else
                {
                    SetShader(null);
                }
            };
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    if (displayed)
                    {
                        SetShader(m_ShaderConfig);
                    }
                    else
                    {
                        SetShader(null);
                    }
                }
            };
        }

        private void SetShader(SceneViewShaderConfig shaderConfig)
        {
            m_Shader = shaderConfig?.Shader;
            foreach (SceneView view in SceneView.sceneViews)
            {
                // view.cameraMode.drawMode = DrawCameraMode.UserDefined <-- can we add something to the scene view dropdown list?
                view.SetSceneViewShaderReplace(m_Shader, null);
            }
            if (shaderConfig == null)
            {
                return;
            }
            foreach (var tex in shaderConfig.DefaultTextures)
            {
                Shader.SetGlobalTexture(tex.Name, tex.Texture);
            }
        }
    }
}