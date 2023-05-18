using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Subsystems;
using Vectors;

////////////////////////////
/// Källor:              ///
/// https://gamemath.com ///
/// Föreläsningar        ///
////////////////////////////


[ExecuteAlways]
[RequireComponent(typeof(VectorRenderer))]
public class Main : MonoBehaviour
{
    private VectorRenderer vectors;
    
    [Range(0,1f)]public float Time = 0.0f;

    public bool doTranslation;
    public bool doScaling;
    public bool doRotation;
    public bool showCoordinateArrows;
    public bool drawCubeA;
    public bool drawCubeB;

    [SerializeField, HideInInspector]internal Vector3 coordinateArrowPosition;
    
    public Vector3 aTargetTranslation;
    public Vector3 bTargetTranslation;
    public Vector3 cTranslation;

    private Vector3 aTargetScale;
    private Vector3 bTargetScale;
    private Vector3 cScale;

    public Quaternion aTargetRotation;
    public Quaternion bTargetRotation;
    public Quaternion cRotation;

    public float determinantA;
    public float determinantB;
    public float determinantC;
    
    [SerializeField, HideInInspector]internal Matrix4x4 A;
    [SerializeField, HideInInspector]internal Matrix4x4 B;
    
    [SerializeField, HideInInspector]internal Matrix4x4 C;

    private Vector3[] columnVectorAarray;
    private Vector3[] columnVectorBarray;

    private Vector3 finalRotationX;
    private Vector3 finalRotationY;
    private Vector3 finalRotationZ;

    private void OnEnable()
    {
        vectors = GetComponent<VectorRenderer>();
    }
    
    void Update()
    {
        //// Default values, if any of the "Do X"-Bools are disabled.
        finalRotationX = Vector3.right;
        finalRotationY = Vector3.up;
        finalRotationZ = Vector3.forward;
        aTargetRotation = Quaternion.identity;
        bTargetRotation = Quaternion.identity;
        aTargetTranslation = Vector3.zero;
        bTargetTranslation = Vector3.zero;
        aTargetScale = Vector3.one;
        bTargetScale = Vector3.one;
        
        // Plocka ur de tre första kolumnvektorerna ur Matris A och B, sätt in i en Lista. 
        columnVectorAarray = new Vector3[3];
        columnVectorAarray = GetColumnsFromMatrix(A);
        
        columnVectorBarray = new Vector3[3];
        columnVectorBarray = GetColumnsFromMatrix(B);

        // Plocka ur Translationen
        if (doTranslation)
        {
            aTargetTranslation = GetTranslationFromMatrix(A);
            bTargetTranslation = GetTranslationFromMatrix(B);
        }

        // Beräkna magnituden för kolumn 0,1 och 2, både A och B matris
        if (doScaling)
        {
            aTargetScale.x = CalculateMagnitude(columnVectorAarray[0]); 
            aTargetScale.y = CalculateMagnitude(columnVectorAarray[1]); 
            aTargetScale.z = CalculateMagnitude(columnVectorAarray[2]);
        
            bTargetScale.x = CalculateMagnitude(columnVectorBarray[0]); 
            bTargetScale.y = CalculateMagnitude(columnVectorBarray[1]); 
            bTargetScale.z = CalculateMagnitude(columnVectorBarray[2]);   
        }

        Matrix4x4 tempNormA = new Matrix4x4();
        Matrix4x4 tempNormB = new Matrix4x4();

        // Normalisera vektorerna och stoppa in i temporära Normal-matriser 
        for (int i = 0; i < columnVectorAarray.Length; i++)
        {
            tempNormA.SetColumn(i,(columnVectorAarray[i]));
            tempNormB.SetColumn(i,(columnVectorBarray[i]));
        }
        
        //// Calculate the Quaternion from the normalized rotational matrices
        //// Only do rotations if Bool
        if (doRotation)
        {
            aTargetRotation = NormalizeQuaternion(CalculateQuaternion(tempNormA));
            bTargetRotation = NormalizeQuaternion(CalculateQuaternion(tempNormB));
            
            // aTargetRotation = CalculateQuaternion(tempNormA);
            // bTargetRotation = CalculateQuaternion(tempNormB);
        }
        
        
        // Debug.Log(CalculateQuaternion(tempNormA) +" - "+ NormalizeQuaternion(aTargetRotation));
        Debug.Log("X: "+NormalizeQuaternion(aTargetRotation).x + " Y: "+NormalizeQuaternion(aTargetRotation).y + " Z: "+NormalizeQuaternion(aTargetRotation).z + " W: "+NormalizeQuaternion(aTargetRotation).w);
        
        
        //// Lerp the transform-vectors and insert into Matrix C
        SetTranslationInMatrix(ref C, MyVectorLerp(aTargetTranslation, bTargetTranslation, Time));
        //// Lerp the scaling
        cScale = MyVectorLerp(aTargetScale, bTargetScale, Time);
        //// Lerp the rotation
        cRotation = MyQLerp(aTargetRotation, bTargetRotation, Time);
        
        //// Rotation for each axis 
        // finalRotationX = cRotation * Vector3.right;
        // finalRotationY = cRotation * Vector3.up;
        // finalRotationZ = cRotation * Vector3.forward;
        
        // Input the calculated rotation and scale into Matrix C
        SetRotationAndScaleInMatrix(ref C, cRotation, cScale);
        
        //Get the lerped translation from Matrix C. -Not sure why I did it this way on only the translation-
        cTranslation = GetTranslationFromMatrix(C);
        
        // Calculate the determinants so they are updated and seen in the inspector
        determinantA = CalculateDeterminant(A);
        determinantB = CalculateDeterminant(B);
        determinantC = CalculateDeterminant(C);
        
        
        using (vectors.Begin())
        {
            // Draw arrows corresponding to the calculated CURRENT LERPED rotation. They react to TIME variable
            // Only draw when bool is enabled. They have darker colours for easier discerning.
            if (showCoordinateArrows)
            {
                vectors.Draw(coordinateArrowPosition, coordinateArrowPosition+finalRotationX, new Color(0.45f, 0f, 0f));
                vectors.Draw(coordinateArrowPosition, coordinateArrowPosition+finalRotationY, new Color(0f, 0.45f, 0f));
                vectors.Draw(coordinateArrowPosition, coordinateArrowPosition+finalRotationZ, new Color(0f, 0f, 0.45f));   
            }
            
            // Draw cube from A-matrix and/or B-matrix. They have darker colours for easier discerning
            if (drawCubeA)
                DrawCubeDark(A);
            if (drawCubeB)
                DrawCubeDark(B);
            
            // Draw the interpolated cube
            DrawCube(C);
        }
    }

        //// FUNKTIONER ////
    
    //// Determinantfunktion för 4x4 matriser ////
    private float CalculateDeterminant(Matrix4x4 m)
    {
        float d=m[0,0] * (m[1,1]*(m[2,2]*m[3,3]-m[2,3]*m[3,2]) + m[1,2]*(m[2,3]*m[3,1]-m[2,1]*m[3,3]) + m[1,3]*(m[2,1]*m[3,2]-m[2,2]*m[3,1]))
              - m[0,1] * (m[1,0]*(m[2,2]*m[3,3]-m[2,3]*m[3,2]) + m[1,2]*(m[2,3]*m[3,0]-m[2,0]*m[3,3]) + m[1,3]*(m[2,0]*m[3,2]-m[2,2]*m[3,0]))
              + m[0,2] * (m[1,0]*(m[2,1]*m[3,3]-m[2,3]*m[3,1]) + m[1,1]*(m[2,3]*m[3,0]-m[2,0]*m[3,3]) + m[1,3]*(m[2,0]*m[3,1]-m[2,1]*m[3,0]))
              - m[0,3] * (m[1,0]*(m[2,1]*m[3,2]-m[2,2]*m[3,1]) + m[1,1]*(m[2,2]*m[3,0]-m[2,0]*m[3,2]) + m[1,2]*(m[2,0]*m[3,1]-m[2,1]*m[3,0]));
        
        return d;
    }
    
    //// Magnitud-funktion ////
    private float CalculateMagnitude(Vector3 v)
    {
        float u;

        u = (float)Math.Sqrt(CalculateDotProductV3(v,v));
        
        return u;
    }
    
    //// Beräkna Skalärprodukten av 3D-vektorer ////
    private float CalculateDotProductV3(Vector3 v, Vector3 u)
    {
        float d = v.x * u.x + v.y * u.y + v.z * u.z;
        return d;
    }
    
    //// Beräkna Skalärprodukten av 4D-vektorer ////
    private float CalculateDotProductV4(Vector4 v, Vector4 u)
    {
        float d = v.x * u.x  +  v.y * u.y  +  v.z * u.z  +  v.w * u.w;
        return d;
    }
    
    //// Beräkna Kryssprodukten av 3D-vektorer ////
    private Vector3 CalculateCrossProduct(Vector3 lhs, Vector3 rhs)
    {
        Vector3 cross = new Vector3(lhs.y * rhs.z - lhs.z * rhs.y,
                                    lhs.z * rhs.x - lhs.x * rhs.z,
                                    lhs.x * rhs.y - lhs.y * rhs.x);
        return cross;
    }
    
    //// Beräkna fram en Kvaternion från en matris //// UPPDATERAD!!!!
    private Quaternion CalculateQuaternion(Matrix4x4 m)
    {
        Quaternion result = Quaternion.identity;

        float fourXSquaredMinus1 = m[0, 0] - m[1, 1] - m[2, 2];
        float fourYSquaredMinus1 = m[1, 1] - m[0, 0] - m[2, 2];
        float fourZSquaredMinus1 = m[2, 2] - m[0, 0] - m[1, 1];
        float fourWSquaredMinus1 = m[0, 0] + m[1, 1] + m[2, 2];

        int biggestIndex = 0;

        float fourBiggestSquaredMinus1 = fourWSquaredMinus1;
        if (fourXSquaredMinus1 > fourBiggestSquaredMinus1)
        {
            fourBiggestSquaredMinus1 = fourXSquaredMinus1;
            biggestIndex = 1;
        }
        if (fourYSquaredMinus1 > fourBiggestSquaredMinus1)
        {
            fourBiggestSquaredMinus1 = fourYSquaredMinus1;
            biggestIndex = 2;
        }
        if (fourZSquaredMinus1 > fourBiggestSquaredMinus1)
        {
            fourBiggestSquaredMinus1 = fourZSquaredMinus1;
            biggestIndex = 3;
        }

        float biggestVal = (float)Math.Sqrt(fourBiggestSquaredMinus1 + 1.0f) * 0.5f;
        float mult = 0.25f / biggestVal;

        switch (biggestIndex)
        {
            //För icke transponerad matris
            // case 0:
            //     result.w = biggestVal;
            //     result.x = (m[1, 2] - m[2, 1]) * mult;
            //     result.y = (m[2, 0] - m[0, 2]) * mult;
            //     result.z = (m[0, 1] - m[1, 0]) * mult;
            //     Debug.Log(biggestIndex+" - "+result.z);
            //     break;
            //
            // case 1:
            //     result.x = biggestVal;
            //     result.w = (m[1, 2] - m[2, 1]) * mult;
            //     result.y = (m[0, 1] + m[1, 0]) * mult;
            //     result.z = (m[2, 0] + m[0, 2]) * mult;
            //     Debug.Log("1 - "+result.z);
            //     break;
            //
            // case 2:
            //     result.y = biggestVal;
            //     result.x = (m[0, 1] + m[1, 0]) * mult;
            //     result.w = (m[2, 0] - m[0, 2]) * mult;
            //     result.z = (m[1, 2] + m[2, 1]) * mult;
            //     Debug.Log("2 - "+result.z);
            //     break;
            //
            // case 3:
            //     result.z = biggestVal;
            //     result.x = (m[2, 0] + m[0, 2]) * mult;
            //     result.y = (m[1, 2] + m[2, 1]) * mult;
            //     result.w = (m[0, 1] - m[1, 0]) * mult;
            //     Debug.Log("3 - "+result.z);
            //     break;
            
            //Fär transponerad matris
            case 0:
                result.w = biggestVal;
                result.x = (m[2, 1] - m[1, 2]) * mult;
                result.y = (m[0, 2] - m[2, 0]) * mult;
                result.z = (m[1, 0] - m[0, 1]) * mult;
                break;
            
            case 1:
                result.x = biggestVal;
                result.w = (m[2, 1] - m[1, 2]) * mult;
                result.y = (m[1, 0] + m[0, 1]) * mult;
                result.z = (m[0, 2] + m[2, 0]) * mult;
                break;
            
            case 2:
                result.y = biggestVal;
                result.x = (m[1, 0] + m[0, 1]) * mult;
                result.w = (m[0, 2] - m[2, 0]) * mult;
                result.z = (m[2, 1] + m[1, 2]) * mult;
                break;
            
            case 3:
                result.z = biggestVal;
                result.x = (m[0, 2] + m[2, 0]) * mult;
                result.y = (m[2, 1] + m[1, 2]) * mult;
                result.w = (m[1, 0] - m[0, 1]) * mult;
                break;
        }
        return result;
    }
    
    //// Normalisera en 3D-vektor ////
    private Vector3 NormalizeVector3(Vector3 v)
    {
        Vector3 n = v / CalculateMagnitude(v);
        return n;
    }

    //// Normalisera en Kvaternion ////
    private Quaternion NormalizeQuaternion(Quaternion q)
    {
        Quaternion qn;
        float magnitude = (float)Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        qn.x = q.x/magnitude;
        qn.y = q.y/magnitude;
        qn.z = q.z/magnitude;
        qn.w = q.w/magnitude;

        return qn;
    }
    
    //// Sätt Translationkomponenten i en Matris. ////
    private void SetTranslationInMatrix(ref Matrix4x4 x, Vector3 v)
    {
        Matrix4x4 temp = x;
        temp[0,3] = v.x;
        //Debug.Log(C.m03 + " - " + x.m03 + " - " + v.x);
        temp[1,3] = v.y;
        temp[2,3] = v.z;
        x = temp;
    }
    
    //// Sätt rotation och skalnings-komponenten i en Matris. ////
    private void SetRotationAndScaleInMatrix(ref Matrix4x4 x, Quaternion q, Vector3 s)
    {
        
        float magnitude = (float)Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

        //För transponerad matris
        //Sätt diagonalen
        x[0, 0] = (1 - 2*(q.y * q.y) - 2*(q.z * q.z));
        x[1, 1] = (1 - 2*(q.x * q.x) - 2*(q.z * q.z));
        x[2, 2] = (1 - 2*(q.x * q.x) - 2*(q.y * q.y));
        
        //Resten av första raden
        x[0, 1] = (2*(q.x * q.y) - 2*(q.w * q.z));
        x[0, 2] = (2*(q.x * q.z) + 2*(q.w * q.y));
        
        //Resten av andra raden
        x[1, 0] = (2*(q.x * q.y) + 2*(q.w * q.z));
        x[1, 2] = (2*(q.y * q.z) - 2*(q.w * q.x));
        
        //Resten av tredje raden
        x[2, 0] = (2*(q.x * q.z) - 2*(q.w * q.y));
        x[2, 1] = (2*(q.y * q.z) + 2*(q.w * q.x));
        
        //För icke transponerad matris
        // //Sätt diagonalen
        // x[0, 0] = (1 - 2*(q.y * q.y) - 2*(q.z * q.z))*s.x;
        // x[1, 1] = (1 - 2*(q.x * q.x) - 2*(q.z * q.z))*s.y;
        // x[2, 2] = (1 - 2*(q.x * q.x) - 2*(q.y * q.y))*s.z;
        // //Resten av första raden
        // x[0, 1] = (2*(q.x * q.y) + 2*(q.w * q.z))*s.y;
        // x[0, 2] = (2*(q.x * q.z) - 2*(q.w * q.y))*s.z;
        //
        // //Resten av andra raden
        // x[1, 0] = (2*(q.x * q.y) - 2*(q.w * q.z))*s.x;
        // x[1, 2] = (2*(q.y * q.z) + 2*(q.w * q.x))*s.z;
        //
        // //Resten av tredje raden
        // x[2, 0] = (2*(q.x * q.z) + 2*(q.w * q.y))*s.x;
        // x[2, 1] = (2*(q.y * q.z) - 2*(q.w * q.x))*s.y;
    }
    
    //// Hämta Translationkomponenten från en Matris. ////
    public Vector3 GetTranslationFromMatrix(Matrix4x4 m)
    {
        return new Vector3(m.m03, m.m13, m.m23);
    }
    
    //// Hämta de tre första kolumnerna från en matris och returnera en VectorArray som innehåller dem.
    private Vector3[] GetColumnsFromMatrix(Matrix4x4 m)
    {
        Vector3[] vA = new Vector3[3];
        vA[0] = new Vector3(m[0, 0], m[1, 0], m[2, 0]);
        vA[1] = new Vector3(m[0, 1], m[1, 1], m[2, 1]);
        vA[2] = new Vector3(m[0, 2], m[1, 2], m[2, 2]);

        return vA;
    }
    
    //// Lerpfunktion för vektorer ////
    private static Vector3 MyVectorLerp(Vector3 a, Vector3 b, float t)
    {
        Vector3 c = (1.0f - t) * a + t * b;
        return c;
    }
    
    //// Lerpfuktion för Kvaternioner ////
    private static Quaternion MyQLerp(Quaternion a, Quaternion b, float t)
    {
        Quaternion lerpedQ;

        float cosTheta = a.w * b.w  +  a.x * b.x  +  a.y * b.y  +  a.z * b.z;

        //If the dot product is negative, inverse (multiply by -1) one of the quaternions
        //which will make it take a shorter path. 
        if (cosTheta < 0.0f)
        {
            b.w = -b.w;
            b.x = -b.x;
            b.y = -b.y;
            b.z = -b.z;
            cosTheta = -cosTheta;
        }
        
        //Check if they are very close together to protect against division with 0
        float ka, kb;
        if (cosTheta > 0.9999f)
        {
            //They are very close, just linear interpolation instead
            ka = 1.0f - t;
            kb = t;
        }
        else
        {
            //Calculate sin of angle from trig.
            float sinTheta = (float)Math.Sqrt(1.0f - cosTheta * cosTheta);

            //Calculate angle from its sin and cos
            float theta = (float)Math.Atan2(sinTheta, cosTheta);

            float oneOverSinTheta = 1.0f / sinTheta;

            ka = (float)Math.Sin((1.0f - t) * theta) * oneOverSinTheta;
            kb = (float)Math.Sin(t * theta) * oneOverSinTheta;
        }
        
        //Interpolate
        lerpedQ.x = a.x * ka + b.x * kb;
        lerpedQ.y = a.y * ka + b.y * kb;
        lerpedQ.z = a.z * ka + b.z * kb;
        lerpedQ.w = a.w * ka + b.w * kb;

        return lerpedQ;
    }
    
    //// Rita en Kub av koordinataxlar //// UPPDATERAD!
    private void DrawCube(Matrix4x4 m)
    {
        Vector3 ogCorner = m*new Vector4(0,0,0,1);
        Vector3 X = m*new Vector4(1,0,0,1);
        Vector3 Y = m*new Vector4(0,1,0,1);
        Vector3 Z = m*new Vector4(0,0,1,1);
        Vector3 XPlusY = m*new Vector4(1,1,0,1);
        Vector3 XPlusZ = m*new Vector4(1,0,1,1);
        Vector3 YPlusZ = m*new Vector4(0,1,1,1);
        Vector3 xPlusYPlusZ = m*new Vector4(1,1,1,1);
        
        
        //Parallel to X-axis arrows
        vectors.Draw(ogCorner,X, Color.red);
        vectors.Draw(Y, XPlusY, Color.red);
        vectors.Draw(Z, XPlusZ, Color.red);
        vectors.Draw(YPlusZ, xPlusYPlusZ,Color.red);
        
        //Parallel to Y-axis arrows
        vectors.Draw(ogCorner,Y, Color.green);
        vectors.Draw(X, XPlusY, Color.green);
        vectors.Draw(Z, YPlusZ, Color.green);
        vectors.Draw(XPlusZ, xPlusYPlusZ, Color.green);
        
        //Parallel to Z-axis arrows
        vectors.Draw(ogCorner,Z, Color.blue);
        vectors.Draw(Y, YPlusZ, Color.blue);
        vectors.Draw(X, XPlusZ, Color.blue);
        vectors.Draw(XPlusY, xPlusYPlusZ, Color.blue);
    }

    //// Rita en mörkare Kub av koordinataxlar //// UPPDATERAD!
    private void DrawCubeDark(Matrix4x4 m)
    {

        Vector3 ogCorner = m*new Vector4(0,0,0,1);
        Vector3 X = m*new Vector4(1,0,0,1);
        Vector3 Y = m*new Vector4(0,1,0,1);
        Vector3 Z = m*new Vector4(0,0,1,1);
        Vector3 XPlusY = m*new Vector4(1,1,0,1);
        Vector3 XPlusZ = m*new Vector4(1,0,1,1);
        Vector3 YPlusZ = m*new Vector4(0,1,1,1);
        Vector3 xPlusYPlusZ = m*new Vector4(1,1,1,1);
        
        
        //Parallel to X-axis arrows
        vectors.Draw(ogCorner,X, new Color(0.45f, 0f, 0f));
        vectors.Draw(Y, XPlusY, new Color(0.45f, 0f, 0f));
        vectors.Draw(Z, XPlusZ, new Color(0.45f, 0f, 0f));
        vectors.Draw(YPlusZ, xPlusYPlusZ,new Color(0.45f, 0f, 0f));
        
        //Parallel to Y-axis arrows
        vectors.Draw(ogCorner,Y, new Color(0f, 0.45f, 0f));
        vectors.Draw(X, XPlusY,  new Color(0f, 0.45f, 0f));
        vectors.Draw(Z, YPlusZ,  new Color(0f, 0.45f, 0f));
        vectors.Draw(XPlusZ, xPlusYPlusZ, new Color(0f, 0.45f, 0f));
        
        //Parallel to Z-axis arrows
        vectors.Draw(ogCorner,Z, new Color(0f, 0f, 0.45f));
        vectors.Draw(Y, YPlusZ, new Color(0f, 0f, 0.45f));
        vectors.Draw(X, XPlusZ, new Color(0f, 0f, 0.45f));
        vectors.Draw(XPlusY, xPlusYPlusZ, new Color(0f, 0f, 0.45f));
    }
}

[CustomEditor(typeof(Main))]
public class MainEditor : Editor
{
    private void OnSceneGUI()
    {
        var main = target as Main;
        if (!main) return;
        
        EditorGUI.BeginChangeCheck();

        Vector3 bTransform = main.GetTranslationFromMatrix(main.B); 
        
        var newBTarget = Handles.PositionHandle(bTransform, main.transform.rotation);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Moved Target");
            var copy = main.B;
            copy.m03 = newBTarget.x;
            copy.m13 = newBTarget.y;
            copy.m23 = newBTarget.z;
            main.B = copy;
            EditorUtility.SetDirty(main);
        }
        
        
        
        EditorGUI.BeginChangeCheck();
        
        Vector3 aTransform = main.GetTranslationFromMatrix(main.A);

        var newATarget = Handles.PositionHandle(aTransform, main.transform.rotation);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Moved Target");
            var copy = main.A;
            copy.m03 = newATarget.x;
            copy.m13 = newATarget.y;
            copy.m23 = newATarget.z;
            main.A = copy;
            EditorUtility.SetDirty(main);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var main = target as Main;
        if (!main) return;
        
        EditorGUI.BeginChangeCheck();

        if (main.showCoordinateArrows)
        {
            EditorGUILayout.BeginHorizontal();
            
            //EditorGUILayout.LabelField("Coordinate Arrow Position", GUILayout.MaxWidth(50));

            main.coordinateArrowPosition = EditorGUILayout.Vector3Field("Coordinate Arrow Position", main.coordinateArrowPosition);
            
            EditorGUILayout.EndHorizontal();
        }
        
        
        /////////////////////////////////
        //Produce A Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix A");
        EditorGUILayout.BeginVertical();

        var resultA = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultA[i, j] = EditorGUILayout.FloatField(main.A[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix A");
            main.A = resultA;
            EditorUtility.SetDirty(main);
        }
        
        EditorGUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        /////////////////////////////////
        //Produce B Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix B");
        EditorGUILayout.BeginVertical();

        var resultB = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultB[i, j] = EditorGUILayout.FloatField(main.B[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix B");
            main.B = resultB;
            EditorUtility.SetDirty(main);
        }        
        
        EditorGUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        /////////////////////////////////
        //Produce C Matrix in Inspector//
        /////////////////////////////////
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Matrix C (result)");
        EditorGUILayout.BeginVertical();

        var resultC = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 4; j++)
            {
                resultC[i, j] = EditorGUILayout.FloatField(main.C[i,j]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(main, "Change Matrix C");
            main.C = resultC;
            EditorUtility.SetDirty(main);
        }
        
        
    }
}