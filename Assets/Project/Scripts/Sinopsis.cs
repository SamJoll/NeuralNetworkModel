using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sinopsis : MonoBehaviour
{
    [Header("Sinapsis settings")]
    //���� ����������� ���������
    [SerializeField]Color DeactivatedColor;
    //���� ��������� ���������
    [SerializeField]Color ActivateColor;
    //��� �� �������� ����, ������� �� ������
    public bool isActive = false;
    //�������� ��������� ���������
    public ParticleSystem ActiveSisnopsisSparks;
    
    //������� ����� ����� � ���������
    IEnumerator ChangeColorAnim()
    {
        while (true)
        {
            if (isActive)
            {
                while (GetComponent<Renderer>().material.color != ActivateColor)
                {
                    GetComponent<Renderer>().material.color =
                        Color.Lerp(GetComponent<Renderer>().material.color, ActivateColor, .05f);

                    yield return new WaitForEndOfFrame();
                }
            }
            else if (!isActive)
            {
                while (GetComponent<Renderer>().material.color != DeactivatedColor)
                {
                    GetComponent<Renderer>().material.color =
                        Color.Lerp(GetComponent<Renderer>().material.color, DeactivatedColor, .05f);

                    yield return new WaitForEndOfFrame();
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator SisnapsisSparksAnim(Transform startPos, Transform endPos)
    {
        GameObject sparks = Instantiate(ActiveSisnopsisSparks.gameObject, startPos.position, Quaternion.identity);

        while(sparks.transform.position != endPos.position)
        {
            sparks.transform.position = Vector2.Lerp(sparks.transform.position, endPos.position, 0.02f);

            yield return new WaitForEndOfFrame();
        }

        Destroy(sparks);

        yield break;
    }

    private void Awake()
    {
        StartCoroutine(ChangeColorAnim());
    }
}
