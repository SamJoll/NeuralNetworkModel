using System.Collections;
using UnityEngine;

public class NeuralNetworkPredictManager : MonoBehaviour
{
    [SerializeField] NeuralNetwork NeuralNetwork;

    IEnumerator EnableInputField()
    {
        while (true)
        {
            if (NeuralNetwork.Layers.Count > 0)
            {
                foreach (Neuron inputNeuron in NeuralNetwork.Layers[0].Neurons)
                {
                    inputNeuron.transform.Find("INPUTFIELD").gameObject.SetActive(true);
                }
            }
           

            yield return new WaitForEndOfFrame();
        }
    }

    private void Start()
    {
        StartCoroutine(EnableInputField());
    }
}
