using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Subsystems;
using Vectors;


[ExecuteAlways]
[RequireComponent(typeof(VectorRenderer))]
public class Main : MonoBehaviour
{
    private VectorRenderer vectors;

    
    [Range(0,5f)]public float cubeSize = 1f;
    [Range(0,1f)]public float Time = 0.0f;

    public bool doATransforming;
    public bool doBTransforming;
    public bool doAScaling;
    public bool doBScaling;
    public bool doARotation;
    public bool doBRotation;
    
    public Vector3 aTargetTransform;
    public Vector3 bTargetTransform;
    public Vector3 cTransform;

    private Vector3 aTargetScale;
    private Vector3 bTargetScale;
    private Vector3 cScale;

    private Quaternion aTargetRotation;
    private Quaternion bTargetRotation;
    private Quaternion cRotation;

    private Vector3 baseXVector;
    private Vector3 baseYVector;
    private Vector3 baseZVector;

    public float determinantA;
    public float determinantB;
    public float determinantC;
    
    [SerializeField, HideInInspector]internal Matrix4x4 A;
    [SerializeField, HideInInspector]internal Matrix4x4 B;
    
    [SerializeField, HideInInspector]internal Matrix4x4 C;

    private Vector3[] columnVectorAarray;
    private Vector3[] columnVectorBarray;

    private void OnEnable()
    {
        vectors = GetComponent<VectorRenderer>();
        //Vector3.right
        baseXVector = new Vector3(1, 0, 0);
        //Vector3.up
        baseYVector = new Vector3(0, 1, 0);
        //Vector3.forward
        baseZVector = new Vector3(0, 0, 1);
    }
    
    void Update()
    {
        columnVectorAarray = new Vector3[3];
        columnVectorAarray = GetColumnsFromMatrix(A);
        
        columnVectorBarray = new Vector3[3];
        columnVectorBarray = GetColumnsFromMatrix(B);

        
        
        aTargetTransform = GetTransformFromMatrix(A);
        bTargetTransform = GetTransformFromMatrix(B);

        Vector3 tempXA;
        tempXA.x = CalculateDotProductV3(columnVectorAarray[0], baseXVector);
        tempXA.y = CalculateDotProductV3(columnVectorAarray[1], baseXVector);
        tempXA.z = CalculateDotProductV3(columnVectorAarray[2], baseXVector);

        //Debug.Log(tempXA + " - " + new Vector3(A[2,0],A[2,1],A[2,2]) + " - " + baseXVector);
        aTargetScale.x = CalculateMagnitude(columnVectorAarray[0]); 
        aTargetScale.y = CalculateMagnitude(columnVectorAarray[1]); 
        aTargetScale.z = CalculateMagnitude(columnVectorAarray[2]);
        
        bTargetScale.x = CalculateMagnitude(columnVectorBarray[0]); 
        bTargetScale.y = CalculateMagnitude(columnVectorBarray[1]); 
        bTargetScale.z = CalculateMagnitude(columnVectorBarray[2]);

        columnVectorAarray[0] = columnVectorAarray[0]/CalculateMagnitude(columnVectorAarray[0]);
        columnVectorAarray[1] = columnVectorAarray[1]/CalculateMagnitude(columnVectorAarray[1]);
        columnVectorAarray[2] = columnVectorAarray[2]/CalculateMagnitude(columnVectorAarray[2]);

        Matrix4x4 tempNormA = new Matrix4x4();
        tempNormA.SetColumn(0,columnVectorAarray[0]);
        tempNormA.SetColumn(1,columnVectorAarray[1]);
        tempNormA.SetColumn(2,columnVectorAarray[2]);

        Quaternion rotatexAxis = CalculateQuaternion(tempNormA);
        
        Vector3 rotationVectorX1 = CalculateCrossProduct(columnVectorAarray[0], baseXVector);
        Vector3 rotationVectorX2 = CalculateCrossProduct(columnVectorAarray[1], baseXVector);
        Vector3 rotationVectorX3 = CalculateCrossProduct(columnVectorAarray[2], baseXVector);
        Vector3 rotationVectorY = CalculateCrossProduct(columnVectorAarray[1], baseYVector);
        Vector3 rotationVectorZ = CalculateCrossProduct(columnVectorAarray[2], baseZVector);

        Vector3 finalRotation = rotationVectorX1 + rotationVectorX2 + rotationVectorX3;

        ////lerp the transform-vectors and insert into Matrix C////
        SetTransformInMatrix(ref C, MyVectorLerp(aTargetTransform, bTargetTransform, Time));

        cScale = MyVectorLerp(aTargetScale, bTargetScale, Time);
        
        
        cTransform = GetTransformFromMatrix(C);

        determinantA = CalculateDeterminant(A);
        determinantB = CalculateDeterminant(B);
        determinantC = CalculateDeterminant(C);
        
        
        using (vectors.Begin())
        {
            vectors.Draw(Vector3.zero, baseXVector, Color.magenta);
            vectors.Draw(Vector3.zero, columnVectorAarray[0], Color.cyan);
            vectors.Draw(Vector3.zero, rotationVectorX1, Color.white);
            vectors.Draw(Vector3.zero, rotationVectorX2, Color.white);
            vectors.Draw(Vector3.zero, rotationVectorX3, Color.white);
            vectors.Draw(Vector3.zero, finalRotation, Color.black);
            
            /*vectors.Draw(Vector3.zero, new Vector3(A[0,0],A[0,1],A[0,2]),Color.blue);
            vectors.Draw(Vector3.zero, new Vector3(A[1,0],A[1,1],A[1,2]),Color.blue);
            vectors.Draw(Vector3.zero, new Vector3(A[2,0],A[2,1],A[2,2]),Color.blue);
            
            vectors.Draw(Vector3.zero, new Vector3(A[0,0],A[1,0],A[2,0]),Color.magenta);
            vectors.Draw(Vector3.zero, new Vector3(A[0,1],A[1,1],A[2,1]),Color.magenta);
            vectors.Draw(Vector3.zero, new Vector3(A[0,2],A[1,2],A[2,2]),Color.magenta);*/
            DrawCube(cTransform, cScale, vectors);
        }
    }

    //// Determinantfunktion för 4x4 matriser ////
    private float CalculateDeterminant(Matrix4x4 m)
    {
        float d=m[0,0] * (m[1,1]*(m[2,2]*m[3,3]-m[2,3]*m[3,2]) + m[1,2]*(m[2,3]*m[3,1]-m[2,1]*m[3,3]) + m[1,3]*(m[2,1]*m[3,2]-m[2,2]*m[3,1]))
              - m[0,1] * (m[1,0]*(m[2,2]*m[3,3]-m[2,3]*m[3,2]) + m[1,2]*(m[2,3]*m[3,0]-m[2,0]*m[3,3]) + m[1,3]*(m[2,0]*m[3,2]-m[2,2]*m[3,0]))
              + m[0,2] * (m[1,0]*(m[2,1]*m[3,3]-m[2,3]*m[3,1]) + m[1,1]*(m[2,3]*m[3,0]-m[2,0]*m[3,3]) + m[1,3]*(m[2,0]*m[3,1]-m[2,1]*m[3,0]))
              - m[0,3] * (m[1,0]*(m[2,1]*m[3,2]-m[2,2]*m[3,1]) + m[1,1]*(m[2,2]*m[3,0]-m[2,0]*m[3,2]) + m[1,2]*(m[2,0]*m[3,1]-m[2,1]*m[3,0]));
        
        return d;
    }
    private float CalculateMagnitude(Vector3 v)
    {
        float u;

        u = (float)Math.Sqrt(CalculateDotProductV3(v,v));
        
        return u;
    }
    private float CalculateDotProductV3(Vector3 v, Vector3 u)
    {
        float d = v.x * u.x + v.y * u.y + v.z * u.z;
        return d;
    }

    private float CalculateDotProductV4(Vector4 v, Vector4 u)
    {
        float d = v.x * u.x  +  v.y * u.y  +  v.z * u.z  +  v.w * u.w;
        return d;
    }
    private Vector3 CalculateCrossProduct(Vector3 lhs, Vector3 rhs)
    {
        Vector3 cross = new Vector3(lhs.y * rhs.z - lhs.z * rhs.y,
                                    lhs.z * rhs.x - lhs.x * rhs.z,
                                    lhs.x * rhs.y - lhs.y * rhs.x);
        return cross;
    }
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
            case 0:
                result.w = biggestVal;
                result.x = (m[1, 2] - m[2, 1]) * mult;
                result.y = (m[2, 0] - m[0, 2]) * mult;
                result.z = (m[0, 1] - m[1, 0]) * mult;
                break;
            
            case 1:
                result.x = biggestVal;
                result.w = (m[1, 2] - m[2, 1]) * mult;
                result.y = (m[0, 1] + m[1, 0]) * mult;
                result.z = (m[2, 0] + m[0, 2]) * mult;
                break;
            
            case 2:
                result.y = biggestVal;
                result.x = (m[0, 1] + m[1, 0]) * mult;
                result.w = (m[2, 0] - m[0, 2]) * mult;
                result.z = (m[1, 2] + m[2, 1]) * mult;
                break;
            
            case 3:
                result.z = biggestVal;
                result.x = (m[2, 0] + m[0, 2]) * mult;
                result.y = (m[1, 2] + m[2, 1]) * mult;
                result.w = (m[0, 1] - m[1, 0]) * mult;
                break;
        }
        return result;
    }
    private Vector3 NormalizeVector3(Vector3 v)
    {
        Vector3 n = v / CalculateMagnitude(v);
        return n;
    }
    //// Sätt Translationkomponenten i en Matris. ("Transform" är inte nödvändigtvis rätt ord, men det är det som används här!) ////
    public void SetTransformInMatrix(ref Matrix4x4 x, Vector3 v)
    {
        Matrix4x4 temp = x;
        temp[0,3] = v.x;
        //Debug.Log(C.m03 + " - " + x.m03 + " - " + v.x);
        temp[1,3] = v.y;
        temp[2,3] = v.z;
        x = temp;
    }
    //// Hämta Translationkomponenten från en Matris. ("Transform" är inte nödvändigtvis rätt ord, men det är det som används här!) ////
    public Vector3 GetTransformFromMatrix(Matrix4x4 m)
    {
        return new Vector3(m.m03, m.m13, m.m23);
    }
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
    
    private void DrawCube(Vector3 position, Vector3 size, VectorRenderer vectors)
    {
        //Parallel to X-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,0,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z + cubeSize * size.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.red);
        
        //Parallel to Y-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z),Color.green);
        vectors.Draw(position+new Vector3(0,0,cubeSize * size.z), 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,cubeSize * size.z), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.green);

        //Parallel to Z-axis arrows
        vectors.Draw(position, 
            new Vector3(position.x, position.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize * size.x,0,0), 
            new Vector3(position.x + cubeSize * size.x, position.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(0,cubeSize * size.y,0), 
            new Vector3(position.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize * size.x,cubeSize * size.y,0), 
            new Vector3(position.x + cubeSize * size.x, position.y + cubeSize * size.y, position.z + cubeSize * size.z),Color.blue);
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

        Vector3 bTransform = main.GetTransformFromMatrix(main.B); 
        
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
        
        Vector3 aTransform = main.GetTransformFromMatrix(main.A);

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