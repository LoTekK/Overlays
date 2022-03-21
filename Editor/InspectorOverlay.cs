using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace TeckArtist.Overlays.Editor
{
    [Overlay(typeof(SceneView), "Inspector")]
    public class InspectorOverlay : Overlay
    {
        private VisualElement m_Root;

        public override void OnCreated()
        {
            Selection.selectionChanged += OnSelectionChange;
            m_Root = new VisualElement();
        }

        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= OnSelectionChange;
        }

        public override VisualElement CreatePanelContent()
        {
            m_Root.Add(RebuildInspector());
            return m_Root;
        }

        private void OnSelectionChange()
        {
            m_Root.Clear();
            m_Root.Add(RebuildInspector());
            m_Root.MarkDirtyRepaint();
        }

        private VisualElement RebuildInspector()
        {
            if (Selection.gameObjects.Length == 0)
            {
                return null;
            }
            var container = new ScrollView(ScrollViewMode.Vertical);
            // container.style.maxHeight = 400;
            container.style.maxHeight = 400;
            container.style.width = 400;
            var components = Selection.activeGameObject?.GetComponents<Component>();
            foreach (var component in components)
            {
                var foldout = new Foldout
                {
                    text = ObjectNames.NicifyVariableName(component.GetType().ToString().Replace("UnityEngine.", ""))
                };
                var editor = UnityEditor.Editor.CreateEditor(component);
                var inspector = editor.CreateInspectorGUI();
                if (inspector == null)
                {
                    inspector = new IMGUIContainer(editor.OnInspectorGUI);
                }
                inspector.style.backgroundColor = Color.gray * 0.5f;
                foldout.Add(inspector);
                container.Add(foldout);
            }
            return container;
        }
    }
}