#if OVERLAYS_ANIMATION_RIGGING
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

namespace TeckArtist.Overlays.Editor
{
    [Overlay(typeof(SceneView), "Rigs")]
    public class RiggingOverlay : Overlay
    {
        private VisualTreeAsset ProgressSlider;
        private VisualElement m_Panel;
        private ProgressBar activeProgressBar;

        public override VisualElement CreatePanelContent()
        {
            ProgressSlider = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ProgressSlider.uxml");
            Selection.selectionChanged += UpdateSelection;
            EditorApplication.playModeStateChanged += state => EditorApplication.delayCall += UpdateSelection;
            UpdateSelection();
            return m_Panel;
        }

        private void UpdateSelection()
        {
            if (m_Panel == null)
            {
                m_Panel = new VisualElement();
            }
            m_Panel.Clear();
            m_Panel.style.maxHeight = 200;
            var rigBuilder = Selection.activeGameObject?.GetComponentInParent<RigBuilder>();
            if (rigBuilder)
            // if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<Rig>(out var rig))
            {
                var scrollview = new ScrollView();
                scrollview.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                // var rigs = rigBuilder.GetComponentsInChildren<Rig>();
                var rigs = rigBuilder.layers.Where(l => l.active).Select(l => l.rig);
                foreach (var rig in rigs)
                {
                    // m_Panel.Add(new Label(rig.name));
                    {
                        var pbs = ProgressSlider.CloneTree();
                        pbs.style.width = 200;
                        var pb = pbs.Q<ProgressBar>();
                        // pb.title = rig.name;
                        pbs.Q<Label>("Label").text = rig.name;
                        pbs.style.unityFontStyleAndWeight = FontStyle.Bold;
                        pb.lowValue = 0;
                        pb.highValue = 1;
                        var weightProp = new SerializedObject(rig).FindProperty("m_Weight");
                        pb.BindProperty(weightProp);
                        pb.title = string.Empty;
                        var slider = pbs.Q<Slider>();
                        slider.lowValue = 0;
                        slider.highValue = 1;
                        slider.BindProperty(weightProp);
                        pbs.RegisterCallback<MouseDownEvent>(e =>
                        {
                            if (e.button == 1)
                            {
                                Selection.objects = new Object[] { rig.gameObject };
                            }
                        });
                        scrollview.Add(pbs);
                    }
                    var dict = new Dictionary<GameObject, List<IRigConstraint>>();
                    var constraints = rig.GetComponentsInChildren<IRigConstraint>();
                    foreach (var constraint in constraints)
                    {
                        if (!dict.TryGetValue(constraint.component.gameObject, out var cList))
                        {
                            dict.Add(constraint.component.gameObject, new List<IRigConstraint>());
                        }
                        dict[constraint.component.gameObject].Add(constraint);
                    }
                    foreach (var kvp in dict)
                    {
                        var label = new Label($"- {kvp.Key.name}");
                        label.style.marginLeft = 4;
                        scrollview.Add(label);
                        foreach (var constraint in kvp.Value)
                        {
                            var cType = Regex.Replace(constraint.component.GetType().ToString(), ".*\\.", "");
                            var pbs = ProgressSlider.CloneTree();
                            pbs.style.marginLeft = 8;
                            var pb = pbs.Q<ProgressBar>();
                            pbs.Q<Label>("Label").text = cType;
                            pb.lowValue = 0;
                            pb.highValue = 1;
                            var weightProp = new SerializedObject(constraint.component).FindProperty("m_Weight");
                            pb.BindProperty(weightProp);
                            pb.title = string.Empty;
                            var slider = pbs.Q<Slider>();
                            slider.lowValue = 0;
                            slider.highValue = 1;
                            slider.BindProperty(weightProp);
                            pbs.RegisterCallback<MouseDownEvent>(e =>
                            {
                                if (e.button == 1)
                                {
                                    Selection.objects = new Object[] { constraint.component.gameObject };
                                }
                            });
                            scrollview.Add(pbs);
                        }
                    }
                }
                m_Panel.Add(scrollview);
            }
        }
    }
}
#endif