using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

[ExecuteInEditMode]
public class MeshPositionSampler : MonoBehaviour
{
    private VisualEffect m_VisualEffect;
    public Transform raccoonPosition;
    public Transform[] raccoonMesh;
    private MeshFilter[] targetMeshFilters;
    public MeshFilter[] sourceMeshFilters;

    public Vector3[] sourceVertices;

    public float interval = 0.1f;

    public bool isActive; // Debug to toggle the calculations off

    [Tooltip("At what height are raccoon vertices considered.")]
    public Vector2 minMaxHeightThreshold = new Vector2(0f, 1f);

    [Tooltip("At what distance are cable vertices considered.")]
    public Vector2 minMaxDistanceThreshold = new Vector2(0.2f, 1.4f);

    public int seed;
    private int stepTarget, stepTargetVert, stepOrigin, stepOriginVert = 0;

    private enum RandomType {Origin, OriginVert, Target, TargetVert };

    private Vector3 newSourcePosition, newTargetPosition;

    void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    [ContextMenu("Manually Initialize")]
    void Initialize()
    {
        m_VisualEffect = GetComponent<VisualEffect>();

        if (raccoonMesh.Length > 0)
        {
            List<MeshFilter> tempList = new List<MeshFilter>();
            foreach (Transform t in raccoonMesh)
            {
                MeshFilter[] tempFilters = t.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter mf in tempFilters)
                {
                    tempList.Add(mf);
                }
            }

            targetMeshFilters = tempList.ToArray();
        }
        else
            Debug.Log("Raccoon transform not assigned; please assign the raccoon to target specific vertices of its mesh.");

        stepTarget = stepTargetVert = stepOrigin = stepOriginVert = 0;
        newSourcePosition = newTargetPosition = Vector3.zero;

        StartCoroutine(GetSourcePosition());
        StartCoroutine(GetTargetPosition());
    }

    // Randomness controlled by a seed, to give the same result each time
    int SeededRandom(int maxCount, RandomType type)
    {
        int step = 0;

        switch (type)
        {
            case RandomType.Origin:
                stepOrigin++;
                step = stepOrigin;
            break;

            case RandomType.OriginVert:
                stepOriginVert++;
                step = stepOriginVert;
                break;

            case RandomType.Target:
                stepTarget++;
                step = stepTarget;
                break;

            case RandomType.TargetVert:
                stepTargetVert++;
                step = stepTargetVert;
                break;
        }

        float newNumber = (float)Mathf.Abs(Mathf.Sin(Vector2.Dot(new Vector2(seed, seed) * step, new Vector2(12.9898f, 78.233f)) * 43758.5453f) % 1);
        
        return Mathf.RoundToInt(Mathf.Lerp(0f, (float)maxCount - 1f, newNumber));
    }

    [ContextMenu("Get source vertices.")]
    void GetSourceVertices()
    {
        List<Vector3> sourceVerts = new List<Vector3>();

        // Grab all vertices in all relevant meshes
        for (int i = 0; i < sourceMeshFilters.Length; i++)
        {
            Vector3[] tempSourceVerts;

            Matrix4x4 sourceLocalToWorld = sourceMeshFilters[i].transform.localToWorldMatrix;

            if (Application.isPlaying)
                tempSourceVerts = sourceMeshFilters[i].mesh.vertices;
            else
                tempSourceVerts = sourceMeshFilters[i].sharedMesh.vertices;

            // Add all vertices within the distance threshold to the list
            for (int j = 0; j < tempSourceVerts.Length; j++)
            {
                Vector3 tempWorldPos = sourceLocalToWorld.MultiplyPoint3x4(tempSourceVerts[j]);

                // Compare distance only based on X and Y (to allow spawning from all wires)
                Vector3 twoAxisTempWorldPos = new Vector3(tempWorldPos.x, tempWorldPos.y, raccoonPosition.position.z);

                float distance = Vector3.Distance(twoAxisTempWorldPos, raccoonPosition.position);

                if (distance > minMaxDistanceThreshold.x && distance < minMaxDistanceThreshold.y)
                    sourceVerts.Add(tempWorldPos);            
            }    
        }

        sourceVertices = sourceVerts.ToArray();
    }

    IEnumerator GetSourcePosition()
    {
        while (true)
        {
            if (!isActive)
                yield return null;

            newSourcePosition = Vector3.zero;

            if (sourceVertices.Length > 0)
            {
                // grab only vertices within the distance threshold
                while (newSourcePosition == Vector3.zero)
                {
                    Vector3 tempPos = sourceVertices[SeededRandom(sourceVertices.Length, RandomType.OriginVert)];


                    if (SameSideCheck(newTargetPosition, tempPos))
                        newSourcePosition = tempPos;

                    yield return null;
                }
            }
            else GetSourceVertices();

            m_VisualEffect.SetVector3("SourcePosition", newSourcePosition);
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator GetTargetPosition()
    {
        while (true)
        {
            if (!isActive)
                yield return null;

            newTargetPosition = Vector3.zero;

            if (targetMeshFilters.Length > 0)
            {
                MeshFilter randomMeshFilter = targetMeshFilters[SeededRandom(targetMeshFilters.Length, RandomType.Target)];
                Vector3[] targetVertices;

                // Grab mesh verts if palying, else shared mesh in edit mode (to prevent leak)
                if (Application.isPlaying)
                    targetVertices = randomMeshFilter.mesh.vertices;
                else
                    targetVertices = randomMeshFilter.sharedMesh.vertices;

                Matrix4x4 targetLocalToWorld = randomMeshFilter.transform.localToWorldMatrix;

                // grab only vertices below the vertical threshold
                while (newTargetPosition == Vector3.zero)
                {
                    Vector3 tempPos = targetVertices[SeededRandom(targetVertices.Length, RandomType.TargetVert)];
                    if (tempPos.y > minMaxHeightThreshold.x && tempPos.y < minMaxHeightThreshold.y)
                        newTargetPosition = targetLocalToWorld.MultiplyPoint3x4(tempPos);
                    yield return null;
                }
            }
            else
            {
                newTargetPosition = raccoonPosition.position;
                Debug.Log("No suitable verts for the raccoon found; please ensure raccoon mesh(es) have been assigned and that the minMaxHeightThreshold is reasonable.");
            }

            m_VisualEffect.SetVector3("TargetPosition", newTargetPosition);
            yield return new WaitForSeconds(interval);
        }
    }

    // Ensure that the electricity strikes the closest point (i.e. an arc from the left shouldn't consider a target on the right)
    bool SameSideCheck(Vector3 targetPos, Vector3 originPos)
    {
        Vector3 perpendicular = Vector3.Cross(raccoonPosition.transform.forward, originPos - targetPos);
        float direction = Vector3.Dot(perpendicular, Vector3.up);

        float sideofTarget = targetPos.x > raccoonPosition.transform.position.x ? 1f : -1f;
        float sideOfOrigin = direction > 0f ? 1f : -1f;

        return sideofTarget == sideOfOrigin ? true : false;
    }


}
