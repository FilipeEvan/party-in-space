using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    [SerializeField] int xPos = 50;
    [SerializeField] bool creatingSegment = false;
    [SerializeField] int segmentNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (creatingSegment == false)
        {
            creatingSegment = true;
            StartCoroutine(SegmentGen());
        }
    }

    IEnumerator SegmentGen()
    {
        segmentNum = Random.Range(0, 3);
        Instantiate(segment[segmentNum], new Vector3(xPos, 0, 0), Quaternion.identity);
        xPos += 50;
        yield return new WaitForSeconds(3);
        creatingSegment = false;
    }

}
