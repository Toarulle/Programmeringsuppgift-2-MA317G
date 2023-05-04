using System;
using System.Collections;
using System.Collections.Generic;
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

    private Vector4 actualTransform;
    
    [SerializeField, HideInInspector]internal Matrix4x4 A;
    [SerializeField, HideInInspector]internal Matrix4x4 B;
    
    [SerializeField, HideInInspector]internal Matrix4x4 C;

    private void OnEnable()
    {
        vectors = GetComponent<VectorRenderer>();
    }
    
    void Update()
    {
        aTargetTransform = GetTransformFromMatrix(A);
        bTargetTransform = GetTransformFromMatrix(B);
        
        //lerp-transform the vectors.
        cTransform = MyVectorLerp(aTargetTransform, bTargetTransform, Time);
        
        SetTransformInMatrix(C, cTransform);
        C[0,3] = cTransform.x;
        C[1,3] = cTransform.y;
        C[2,3] = cTransform.z;
        
        actualTransform = C * (new Vector4(1f, 1f, 1f, 1f));
        Debug.Log(actualTransform);
        Vector3 test = -actualTransform;

        using (vectors.Begin())
        {
            vectors.Draw(test, test+ new Vector3(A[0,1],A[0,2],A[0,3]),Color.blue);
            vectors.Draw(test, test+ new Vector3(A[1,1],A[1,2],A[1,3]),Color.blue);
            vectors.Draw(test, test+ new Vector3(A[2,1],A[2,2],A[2,3]),Color.blue);
            DrawCube(cTransform, vectors);
        }
    }

    public void SetTransformInMatrix(Matrix4x4 x, Vector3 v)
    {
        x[0,3] = v.x;
        //Debug.Log(C.m03 + " - " + x.m03 + " - " + v.x);
        x[1,3] = v.y;
        x[2,3] = v.z;
    }
    public Vector3 GetTransformFromMatrix(Matrix4x4 m)
    {
        return new Vector3(m.m03, m.m13, m.m23);
    }
    
    //Egenbyggd Lerpfunktion för vektorer
    private static Vector3 MyVectorLerp(Vector3 a, Vector3 b, float t)
    {
        Vector3 c;
        c = (1.0f - t) * a + t * b;
        return c;
    }
    
    private void DrawCube(Vector3 position, VectorRenderer vectors)
    {
        //Parallel to X-axis arrows
        vectors.Draw(position, new Vector3(position.x + cubeSize, position.y, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z),Color.red);
        vectors.Draw(position+new Vector3(0,0,cubeSize), new Vector3(position.x + cubeSize, position.y, position.z + cubeSize),Color.red);
        vectors.Draw(position+new Vector3(0,cubeSize,cubeSize), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.red);
        
        //Parallel to Y-axis arrows
        vectors.Draw(position, new Vector3(position.x, position.y + cubeSize, position.z),Color.green);
        vectors.Draw(position+new Vector3(cubeSize,0,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z),Color.green);
        vectors.Draw(position+new Vector3(0,0,cubeSize), new Vector3(position.x, position.y + cubeSize, position.z + cubeSize),Color.green);
        vectors.Draw(position+new Vector3(cubeSize,0,cubeSize), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.green);

        //Parallel to Z-axis arrows
        vectors.Draw(position, new Vector3(position.x, position.y, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize,0,0), new Vector3(position.x + cubeSize, position.y, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(0,cubeSize,0), new Vector3(position.x, position.y + cubeSize, position.z + cubeSize),Color.blue);
        vectors.Draw(position+new Vector3(cubeSize,cubeSize,0), new Vector3(position.x + cubeSize, position.y + cubeSize, position.z + cubeSize),Color.blue);
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