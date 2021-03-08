using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NeuralNetwork : MonoBehaviour
{
    /*=================����������=================*/
    [Header("Neural Network Settings")]
    //���-�� �����
    [SerializeField, Min(0)]int layersCount;
    public int LayersCount
    {
        set
        {
            if(value > 0)
            {
                layersCount = value;
            } else
            {
                return;
            }
        }
        get
        {
            return layersCount;
        }
    }
    //����
    public List<NeuralNetworkLayer> Layers = new List<NeuralNetworkLayer>();
    //����
    Dictionary<string, double> weights = new Dictionary<string, double>();
    //������ ����
    [SerializeField] GameObject LayerPref;
    //������ ���������
    [SerializeField] GameObject SinapsisPref;
    [Space(50f)]
    [SerializeField] Text EpochDisplayText;
    [SerializeField] InputField EpochCountField;
    [SerializeField] InputField TrainSpeedField;
    [SerializeField] Text ErrorDisplayText;
    //�������
    Dictionary<string, LineRenderer> Sinopsises = new Dictionary<string, LineRenderer>();
    //������������� �������
    List<double[][]> trainingDataset = new List<double[][]>() { 
        new double[][] { 
            new double[] {1, 1},
            new double[] {1}
        },
        new double[][] {
            new double[] {1, 0},
            new double[] {0}
        }
    };

    string weightsFilePath;
    string weightsBackupFilePath;
    string trainingDatasetPath;

    /*=================������=================*/
    //���������� �����
    public void AddLayer()
    {
        NeuralNetworkLayer newLayer = Instantiate(LayerPref, transform).GetComponent<NeuralNetworkLayer>();
        Layers.Add(newLayer);
        layersCount++;

        UpdateLayersPos();

        if (layersCount > 1) RenderSinopsises();
    }
    //�������� �����
    public void DeleteLayer()
    {
        if(layersCount > 0)
        {
            Destroy(Layers.ToArray()[layersCount - 1].gameObject);

            Layers.RemoveAt(Layers.Count-1);
            layersCount--;

            UpdateLayersPos();

            if (layersCount >= 2) RenderSinopsises();
            else ClearSinopsises();
        }
    }
    //���������� ���������� �����
    void UpdateLayersPos()
    {
        if(layersCount % 2 != 0)
        {
            for (int lI = LayersCount; lI > 0; lI--)
            {
                Layers[lI-1].gameObject.transform.localPosition =
                    new Vector2((lI-1 -Mathf.CeilToInt(LayersCount / 2)) * 2.75f, 0);
            }
        } else if (LayersCount % 2 == 0)
        {
            for (int lI = LayersCount; lI > 0; lI--)
            {
                Layers[lI-1].gameObject.transform.localPosition =
                    new Vector2((lI - LayersCount/2) * 2.75f, 0);
            }
        }
    }
    //������������ �����
    void InitWeights()
    {
        weights.Clear();

        for(int lI = 1; lI < Layers.Count; lI++)
        { 
            for(int nI = 0; nI < Layers[lI].Neurons.Count; nI++)
            {
                for (int nI_Prev = 0; nI_Prev < Layers[lI-1].Neurons.Count; nI_Prev++)
                {
                    weights.Add($"{lI-1}-{nI_Prev}-{nI}", 0);
                }
            }
        }
    }
    //�������� ���� �� �����
    void ReadWeights()
    {
        weights.Clear();

        string[] weightsDataLines = File.ReadAllLines(weightsFilePath);

        foreach(string weightDataLine in weightsDataLines)
        {
            string weightKey = weightDataLine.Split(':')[0];
            double weightValue = Convert.ToDouble(weightDataLine.Split(':')[1]);

            weights.Add(weightKey, weightValue);
        }
    }
    //��������� ���������� ����
    void SaveBackupWeights()
    {
        File.WriteAllText(weightsBackupFilePath, "");

        if(!File.Exists(weightsBackupFilePath)) {
            File.Create(weightsBackupFilePath);
        }

        if(File.Exists(weightsBackupFilePath) && File.Exists(weightsFilePath)) {
            File.AppendAllLines(weightsBackupFilePath, File.ReadAllLines(weightsFilePath));
        } else
        {
            Debug.LogError("Can't write backup weights");
        }
    }
    //��������� ����
    void SaveWeights()
    {
        if(!File.Exists(weightsFilePath))
        {
            File.Create(weightsFilePath);
        }

        File.WriteAllText(weightsFilePath, "");

        foreach(KeyValuePair<string, double> weight in weights)
        {
            File.AppendAllText(weightsFilePath, $"{weight.Key}:{weight.Value}\n");
        }
    }
    //������������� ��������� ����
    void GenerateRandWeights(double maxNum, double minNum = 0)
    {
        System.Random rand = new System.Random();

        for (int lI = 1; lI < Layers.Count; lI++)
        {
            for (int nI = 0; nI < Layers[lI].Neurons.Count; nI++)
            {
                for (int nI_Prev = 0; nI_Prev < Layers[lI - 1].Neurons.Count; nI_Prev++)
                {
                    weights[$"{lI - 1}-{nI_Prev}-{nI}"] = rand.NextDouble() * 2 * maxNum + minNum;
                }
            }
        }
    }
    //�������� ������������� ���
    void GetTrainingDataset()
    {
        foreach(string trainingDataLine in File.ReadAllLines(trainingDatasetPath))
        {
            string[] inputsString = trainingDataLine.Split(':')[0].Split(',');
            string[] outputsString = trainingDataLine.Split(':')[1].Split(',');

            List<double> inputs = new List<double>();
            List<double> outputs = new List<double>();

            foreach(string inputString in inputsString)
            {
                inputs.Add(Convert.ToDouble(inputString));
            }
            foreach (string outputString in outputsString)
            {
                outputs.Add(Convert.ToDouble(outputString));
            }

            trainingDataset.Add(new double[][] { inputs.ToArray(), outputs.ToArray() });
        }
    }
    //�������� �������
    void ClearSinopsises()
    {
        foreach (LineRenderer sinopsis in Sinopsises.Values)
        {
            Destroy(sinopsis.gameObject);
        }

        Sinopsises.Clear();
    }
    //����� ���� �������� ��������
    void ClearSinopsisesEffects()
    {
        foreach (LineRenderer sinapsis in Sinopsises.Values)
        {
            sinapsis.GetComponent<Sinopsis>().isActive = false;
        }
        if (GameObject.FindGameObjectsWithTag("Sinopsis Effect").Length > 0)
        {
            foreach (GameObject sinapsisEffect in GameObject.FindGameObjectsWithTag("Sinopsis Effect"))
            {
                Destroy(sinapsisEffect);
            }
        }
    }
    //������ �����
    public void RenderSinopsises()
    {
        ClearSinopsises();

        for (int lI = 1; lI < Layers.Count; lI++)
        {
            for (int nI = 0; nI < Layers[lI].Neurons.Count; nI++)
            {
                for (int nI_Prev = 0; nI_Prev < Layers[lI - 1].Neurons.Count; nI_Prev++)
                {
                    if (!Sinopsises.ContainsKey($"{lI - 1}-{nI_Prev}-{nI}"))
                    {
                        Sinopsises.Add($"{lI - 1}-{nI_Prev}-{nI}",
                        Instantiate(SinapsisPref, Layers[lI - 1].Neurons[nI_Prev].transform).GetComponent<LineRenderer>());
                    }

                    Sinopsises[$"{lI - 1}-{nI_Prev}-{nI}"].SetPosition(0, Layers[lI - 1].Neurons[nI_Prev].transform.position + Vector3.forward);
                    Sinopsises[$"{lI - 1}-{nI_Prev}-{nI}"].SetPosition(1, Layers[lI].Neurons[nI].transform.position + Vector3.forward);
                }
            }
        }
    }
    //������
    public double Error(double[][] trainData)
    {
        double error = 0;

        for (int nI = 0; nI < trainData[1].Length; nI++)
        {
            error += (Math.Pow(trainData[1][nI] - Layers[LayersCount - 1].Neurons[nI].OutputValue, 2));
        }

        return Math.Round(error, 2) / trainData[1].Length;
    }
    //������ ���������������
    void ForwardPropagation()
    {
        for (int lI = 1; lI < LayersCount; lI++)
        {
            for(int nI = 0; nI < Layers[lI].Neurons.Count; nI++)
            {
                double result = 0;

                for (int nI_Prev = 0; nI_Prev < Layers[lI-1].Neurons.Count; nI_Prev++)
                {
                    result += Layers[lI-1].Neurons[nI_Prev].OutputValue * weights[$"{lI-1}-{nI_Prev}-{nI}"];
                }

                Layers[lI].Neurons[nI].InputValue = result; 
            }
        }
    }
    //������ ��������������� � ���������
    IEnumerator ForwardPropagationDelayed()
    {
        for (int lI = 1; lI < LayersCount; lI++)
        {
            for (int nI = 0; nI < Layers[lI].Neurons.Count; nI++)
            {
                double result = 0;

                for (int nI_Prev = 0; nI_Prev < Layers[lI - 1].Neurons.Count; nI_Prev++)
                {
                    Sinopsises[$"{lI - 1}-{nI_Prev}-{nI}"].GetComponent<Sinopsis>().isActive = true;

                    StartCoroutine(
                        Sinopsises[$"{lI - 1}-{nI_Prev}-{nI}"].GetComponent<Sinopsis>().SisnapsisSparksAnim(Layers[lI-1].Neurons[nI_Prev].transform, 
                        Layers[lI].Neurons[nI].transform));

                    result += Layers[lI - 1].Neurons[nI_Prev].OutputValue * weights[$"{lI - 1}-{nI_Prev}-{nI}"];

                    yield return new WaitForSeconds(1f);

                    Sinopsises[$"{lI - 1}-{nI_Prev}-{nI}"].GetComponent<Sinopsis>().isActive = false;
                }

                Layers[lI].Neurons[nI].InputValue = result;
            }
        }

        yield break;
    }
    //�������� ��������������� ������
    void BackPropagation(double[][] trainingData)
    {
        for(int nI = 0; nI < Layers[LayersCount-1].NeuronsCount; nI++)
        {
            //����������� ��������
            double derivative_sigmoid = 
                (1 - Layers[LayersCount - 1].Neurons[nI].OutputValue) * Layers[LayersCount - 1].Neurons[nI].OutputValue;
            //����������� ������ (ideal_Val - sigm(nI))^2
            double derivative_error = 
                -2 * (Layers[LayersCount - 1].Neurons[nI].OutputValue - trainingData[1][nI]) * derivative_sigmoid;

            for (int nI_Prev = 0; nI_Prev < Layers[LayersCount-2].NeuronsCount; nI_Prev++)
            {
                weights[$"{LayersCount - 2}-{nI_Prev}-{nI}"] -= 
                    derivative_error * Layers[LayersCount - 2].Neurons[nI_Prev].OutputValue * Convert.ToDouble(TrainSpeedField.text);
            }
        }
    }
    //�������� ��������� ����
    IEnumerator Train(int epochCount)
    {
        GetTrainingDataset();

        for (int epoch = 1; epoch <= epochCount; epoch++)
        {
            foreach (double[][] trainData in trainingDataset)
            {
                for (int nI = 0; nI < Layers[0].NeuronsCount; nI++)
                {
                    Layers[0].Neurons[nI].InputValue = trainData[0][nI];
                }

                ForwardPropagation();

                BackPropagation(trainData);

                if(EpochDisplayText != null)
                {
                    EpochDisplayText.text = $"����� #{Convert.ToString(epoch)}";
                }
                if(ErrorDisplayText != null)
                {
                    ErrorDisplayText.text = $"������ {Error(trainData) * 100}%";
                }
            }
            yield return new WaitForEndOfFrame();
        }

        SaveWeights();

        yield break;
    }
    //������ ��������� ����
    public void StartPredict()
    {
        ClearSinopsisesEffects();

        StopAllCoroutines();   

        InitWeights();
        ReadWeights();

        StartCoroutine(ForwardPropagationDelayed());
    }
    //������ ��������
    public void StartTrain()
    {
        StopAllCoroutines();

        ClearSinopsisesEffects();

        InitWeights();

        GenerateRandWeights(1, -1);

        StartCoroutine(Train(Convert.ToInt32(EpochCountField.text)));
    }
    //���������� ��������
    public void StopTrain()
    {
        StopAllCoroutines();

        SaveWeights();
    }

    private void Start()
    {
        weightsFilePath = $"{Application.dataPath}/StreamingAssets/NeuralNetworkData/weights.txt";
        weightsBackupFilePath = $"{Application.dataPath}/StreamingAssets/NeuralNetworkData/weightsBackup.txt";
        trainingDatasetPath = $"{Application.dataPath}/StreamingAssets/NeuralNetworkData/trainingDataset.txt";
    }
}
