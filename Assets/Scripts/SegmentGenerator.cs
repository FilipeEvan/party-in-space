using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SegmentGenerator : MonoBehaviour
{
    public GameObject[] segment;
    [SerializeField] int xPos = 50;
    [SerializeField] bool creatingSegment = false;
    [SerializeField] int segmentNum;

    // Controle para evitar repetir o mesmo segmento mais de 2 vezes seguidas
    int lastSegmentIndex = -1;
    int sameSegmentCount = 0;

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
        int len = (segment == null) ? 0 : segment.Length;
        if (len <= 0)
        {
            creatingSegment = false;
            yield break;
        }

        segmentNum = PickNextIndex(len);

        Instantiate(segment[segmentNum], new Vector3(xPos, 0, 0), Quaternion.identity);

        if (segmentNum == lastSegmentIndex)
            sameSegmentCount++;
        else
        {
            lastSegmentIndex = segmentNum;
            sameSegmentCount = 1;
        }
        xPos += 50;
        yield return new WaitForSeconds(3);
        creatingSegment = false;
    }

    int PickNextIndex(int len)
    {
        if (len <= 1) return 0;

        // Se já repetimos 2 vezes, força um diferente
        if (sameSegmentCount >= 2 && lastSegmentIndex >= 0)
        {
            int idx;
            do { idx = Random.Range(0, len); } while (idx == lastSegmentIndex);
            return idx;
        }
        // Caso normal: qualquer índice
        return Random.Range(0, len);
    }

}
