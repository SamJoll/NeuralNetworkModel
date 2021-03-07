using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkLayer : MonoBehaviour
{
    /*===================����������===================*/
    [Header("Layer Settings")]
    //���-�� �������� � ����
    [SerializeField, Min(0)]int neuronsCount = 0;
    public int NeuronsCount
    {
        set
        {
            if(value > 0)
            {
                neuronsCount = value;
            } else
            {
                return;
            }
        }
        get
        {
            return neuronsCount;
        }
    }
    //������� ����
    public List<Neuron> Neurons = new List<Neuron>();
    //������ �������
    [SerializeField] public GameObject NeuronPref;
    //��������� ����, ������� ����������� ����
    NeuralNetwork NeuralNetworkParent;

    /*=====================������=====================*/
    //���������� �������
    public void AddNeuron()
    {
        if (NeuronPref.GetComponent<Neuron>())
        {
            Neuron newNeuron = Instantiate(NeuronPref, transform).GetComponent<Neuron>();
            Neurons.Add(newNeuron);
            neuronsCount++;

            UpdateNeuronsPos();

            NeuralNetworkParent.RenderSinopsises();
        }
    }
    //�������� �������
    public void DeleteNeuron()
    {
        if (NeuronsCount > 0)
        {
            Destroy(Neurons[Neurons.Count - 1].gameObject);

            Neurons.RemoveAt(Neurons.Count - 1);
            neuronsCount--;

            UpdateNeuronsPos();

            NeuralNetworkParent.RenderSinopsises();
        }
    }
    //���������� ������� ��������
    void UpdateNeuronsPos()
    {
        if (NeuronsCount % 2 != 0)
        {
            for (int nI = 0; nI < NeuronsCount; nI++)
            {
                Neurons[nI].gameObject.transform.localPosition =
                    new Vector2(0, (Mathf.CeilToInt(NeuronsCount / 2) - nI) * 1.2f);
            }
        } else if(NeuronsCount % 2 == 0)
        {
            for (int nI = 1; nI <= NeuronsCount; nI++)
            {
                Neurons[nI - 1].gameObject.transform.localPosition =
                    new Vector2(0, ((NeuronsCount / 2 + NeuronPref.transform.localScale.y / 2) - nI) * 1.2f);
            }
        }
    }

    private void Awake()
    {
        NeuralNetworkParent = transform.parent.GetComponent<NeuralNetwork>();
    }
}
