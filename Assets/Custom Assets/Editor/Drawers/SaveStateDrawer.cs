using UnityEditor;
using UnityEngine;
using Tomba;

[CustomPropertyDrawer(typeof(Tomba.SaveState))]
public class SaveStateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty saveStateID = property.FindPropertyRelative("ID");

        label.text = "Save State: " + saveStateID.intValue;

        EditorGUI.PropertyField(position, property, label, true);

        if (property.isExpanded)
        {
            int previousIndentLevel = EditorGUI.indentLevel;




            SerializedProperty saveStateProperty = property.FindPropertyRelative("thumbnail");
            SerializedProperty thumbnailSize = property.FindPropertyRelative("thumbnailSize");
            SerializedProperty areaName = property.FindPropertyRelative("areaName");

            Texture saveStateThumbnail = (Texture)saveStateProperty.objectReferenceValue;
            Rect texturePosition = new Rect(0,0,0,0);

            Texture thumbnail;

            if (saveStateThumbnail != null)
            {
                thumbnail = saveStateThumbnail;
            }
            else
            {
                thumbnail = (Texture)Resources.Load("Icons/Editor_MissingThumbnail");
            }

            EditorGUI.indentLevel = previousIndentLevel + 1;
            Rect indentedRect = EditorGUI.IndentedRect(position);
            float fieldHeight = base.GetPropertyHeight(property, label) + 2;

            texturePosition = new Rect(indentedRect.x, indentedRect.y + fieldHeight * 3, thumbnailSize.vector2Value.x, thumbnailSize.vector2Value.y);
            EditorGUI.DropShadowLabel(texturePosition, new GUIContent(thumbnail));



            Rect afterTexturePosition;

            if (texturePosition.y == 0)
            {
                afterTexturePosition = new Rect(texturePosition.x , texturePosition.y + texturePosition.height, texturePosition.width, 20);
            }
            else
            {
                afterTexturePosition = new Rect(texturePosition.x , texturePosition.y + texturePosition.height, texturePosition.width, 20);
            }

            if (saveStateThumbnail != null)
            {
                EditorGUI.DropShadowLabel(texturePosition, new GUIContent("Area: " + areaName.stringValue));
                if (GUI.Button(afterTexturePosition, "Load"))
                {
                    EditorHelper.Start();
                }
                GUILayout.Space(20);
            }


            EditorGUI.indentLevel = previousIndentLevel;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty saveStateProperty = property.FindPropertyRelative("thumbnail");
        SerializedProperty thumbnailSize = property.FindPropertyRelative("thumbnailSize");
        Texture saveStateThumbnail = (Texture)saveStateProperty.objectReferenceValue;

        if (property.isExpanded)
        {
            return EditorGUI.GetPropertyHeight(property) + thumbnailSize.vector2Value.y + 20;
        }
        else
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }

    //static Image FixedSize(Image imgPhoto, int Width, int Height)
    //{
    //    int sourceWidth = imgPhoto.Width;
    //    int sourceHeight = imgPhoto.Height;
    //    int sourceX = 0;
    //    int sourceY = 0;
    //    int destX = 0;
    //    int destY = 0;

    //    float nPercent = 0;
    //    float nPercentW = 0;
    //    float nPercentH = 0;

    //    nPercentW = ((float)Width / (float)sourceWidth);
    //    nPercentH = ((float)Height / (float)sourceHeight);
    //    if (nPercentH < nPercentW)
    //    {
    //        nPercent = nPercentH;
    //        destX = System.Convert.ToInt16((Width -
    //                      (sourceWidth * nPercent)) / 2);
    //    }
    //    else
    //    {
    //        nPercent = nPercentW;
    //        destY = System.Convert.ToInt16((Height -
    //                      (sourceHeight * nPercent)) / 2);
    //    }

    //    int destWidth = (int)(sourceWidth * nPercent);
    //    int destHeight = (int)(sourceHeight * nPercent);

    //    Bitmap bmPhoto = new Bitmap(Width, Height,
    //                      PixelFormat.Format24bppRgb);
    //    bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
    //                     imgPhoto.VerticalResolution);

    //    Graphics grPhoto = Graphics.FromImage(bmPhoto);
    //    grPhoto.Clear(Color.Red);
    //    grPhoto.InterpolationMode =
    //            InterpolationMode.HighQualityBicubic;

    //    grPhoto.DrawImage(imgPhoto,
    //        new Rectangle(destX, destY, destWidth, destHeight),
    //        new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
    //        GraphicsUnit.Pixel);

    //    grPhoto.Dispose();
    //    return bmPhoto;
    //}
}