using UnityEditor;
using UnityEditor.UI;

namespace UIExtensions.Editor
{
    [CustomEditor(typeof(SkewedImage))]
    public class SkewedImageInspector : ImageEditor
    {
        private SkewedImage _skewedImage;
        private SerializedProperty _skewVector;
        private SerializedProperty _TrapezSkewTop;
        private SerializedProperty _TrapezSkewBottom;

        protected override void OnEnable()
        {
            base.OnEnable();
            _skewedImage = (SkewedImage)target;
            _skewVector = serializedObject.FindProperty("SkewVector");
            _TrapezSkewTop = serializedObject.FindProperty("TrapezSkewTop");
            _TrapezSkewBottom = serializedObject.FindProperty("TrapezSkewBottom");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(_skewVector);
            if (_skewVector.vector2Value != _skewedImage.SkewVector)
            {
                Undo.RecordObject(_skewedImage, "Changed Skew");
                _skewedImage.SkewVector = _skewVector.vector2Value;
                _skewedImage.OnRebuildRequested();
            }

            EditorGUILayout.PropertyField(_TrapezSkewTop);
            if (_TrapezSkewTop.floatValue != _skewedImage.TrapezSkewTop)
            {
                Undo.RecordObject(_skewedImage, "Changed TrapezSkewTop");
                _skewedImage.TrapezSkewTop = _TrapezSkewTop.floatValue;
                _skewedImage.OnRebuildRequested();
            }
            EditorGUILayout.PropertyField(_TrapezSkewBottom);
            if (_TrapezSkewBottom.floatValue != _skewedImage.TrapezSkewBottom)
            {
                Undo.RecordObject(_skewedImage, "Changed TrapezSkewBottom");
                _skewedImage.TrapezSkewBottom = _TrapezSkewBottom.floatValue;
                _skewedImage.OnRebuildRequested();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
