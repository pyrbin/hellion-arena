using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

public enum CommentType { None, Info, Warning, Error }

public class Comment : PropertyAttribute
{
    public string text;
    public CommentType messageType;

    public Comment(string text, CommentType messageType = CommentType.None)
    {
        this.text = text;
        this.messageType = messageType;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Comment))]
public class HelpBoxAttributeDrawer : DecoratorDrawer
{
    public override float GetHeight()
    {
        var helpBoxAttribute = attribute as Comment;
        if (helpBoxAttribute == null) return base.GetHeight();
        var helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;

        helpBoxStyle.padding.top = 5;
        helpBoxStyle.padding.bottom = 5;

        if (helpBoxStyle == null) return base.GetHeight();
        return Mathf.Max(40f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4);
    }

    public override void OnGUI(Rect position)
    {
        var helpBoxAttribute = attribute as Comment;
        if (helpBoxAttribute == null) return;
        EditorGUI.HelpBox(position, helpBoxAttribute.text, GetMessageType(helpBoxAttribute.messageType));
    }

    private MessageType GetMessageType(CommentType helpBoxMessageType)
    {
        switch (helpBoxMessageType)
        {
            default:
            case CommentType.None: return MessageType.None;
            case CommentType.Info: return MessageType.Info;
            case CommentType.Warning: return MessageType.Warning;
            case CommentType.Error: return MessageType.Error;
        }
    }
}

#endif