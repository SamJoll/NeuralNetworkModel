using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]enum State {
    Active,
    Deactivated
}

public class Neuron : MonoBehaviour
{
    /*===============����������===============*/

    [Header("Neuron's values")]
    //������� ��������
    [SerializeField]double inputValue = 0;
    public double InputValue
    {
        get
        {
            return inputValue;
        }
        set
        {
            inputValue = value;
            OutputValue = value;
        }
    }
    //�������� ��������
    [SerializeField]double outputValue = 0;
    public double OutputValue
    {
        get
        {
            return outputValue;
        }
        set
        {
            outputValue = 1 / (1 + Math.Exp(value));

            displayValueText.text = Convert.ToString(Math.Round(outputValue, 2));
        }
    }

    [Space(4f)]

    [Header("Neuron settings")]
    //���� ����������� �������
    public Color idleNeuronColor;
    //���� ��������� �������
    public Color activeNeuronColor;
    [SerializeField] State NeuronState = new State();
    //�������� ����� ����� �������
    [Range(0, 1)]
    public float lerpTime = 0;
    //�����, ��� ����������� �������� �������
    public Text displayValueText;

    /*===============������===============*/
    IEnumerator ChangeColorAnim()
    {
        while (true)
        {

            if (outputValue >= .5d)
            {
                NeuronState = State.Active;
            }
            else if (outputValue < .5d)
            {
                NeuronState = State.Deactivated;
            }

            if (NeuronState == State.Active)
            {
                while (GetComponent<SpriteRenderer>().color != activeNeuronColor)
                {
                    GetComponent<SpriteRenderer>().color = Color.Lerp(GetComponent<SpriteRenderer>().color, activeNeuronColor, lerpTime);

                    yield return new WaitForFixedUpdate();
                }
            }
            else if (NeuronState == State.Deactivated)
            {
                while (GetComponent<SpriteRenderer>().color != idleNeuronColor)
                {
                    Debug.Log("s");

                    GetComponent<SpriteRenderer>().color = Color.Lerp(GetComponent<SpriteRenderer>().color, idleNeuronColor, lerpTime);

                    yield return new WaitForEndOfFrame();
                }
            }

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
    private void Awake()
    {
        //currentNeuronColor = GetComponent<SpriteRenderer>().color;

        InputValue = inputValue;

        StartCoroutine(ChangeColorAnim());
    }
}
