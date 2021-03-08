using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NeuralNetwork : MonoBehaviour
{
    /*=================ПЕРЕМЕННЫЕ=================*/
    [Header("Neural Network Settings")]
    //Кол-во слоев
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
    //Слои
    public List<NeuralNetworkLayer> Layers = new List<NeuralNetworkLayer>();
    //Веса
    Dictionary<string, double> weights = new Dictionary<string, double>();
    //Префаб слоя
    [SerializeField] GameObject LayerPref;
    //Префаб синапсиса
    [SerializeField] GameObject SinapsisPref;
    //
    Dictionary<string, LineRenderer> Sinopsises = new Dictionary<string, LineRenderer>();

    string weightsFilePath;
    string weightsBackupFilePath;

    /*=================МЕТОДЫ=================*/
    //Добавление слоев
    public void AddLayer()
    {
        NeuralNetworkLayer newLayer = Instantiate(LayerPref, transform).GetComponent<NeuralNetworkLayer>();
        Layers.Add(newLayer);
        layersCount++;

        UpdateLayersPos();

        if (layersCount > 1) RenderSinopsises();
    }
    //Удаление слоев
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
    //Обновление расложения слоев
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
    //Иницилизация весов
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
    //Получить веса из файла
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
    //Сохранить предыдущие веса
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
    //Сохранить веса
    void SaveWeights()
    {
        File.WriteAllText(weightsFilePath, "");

        if(!File.Exists(weightsFilePath))
        {
            File.Create(weightsFilePath);
        }

        foreach(KeyValuePair<string, double> weight in weights)
        {
            File.AppendAllText(weightsFilePath, $"{weight.Key}:{weight.Value}\n");
        }
    }
    //Сгенерировать рандомные веса
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
    //Очистить синопсы
    void ClearSinopsises()
    {
        foreach (LineRenderer sinopsis in Sinopsises.Values)
        {
            Destroy(sinopsis.gameObject);
        }

        Sinopsises.Clear();
    }
    //Рендер весов
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
    //Прямое распространение
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
    //Прямое распространение с анимацией
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
    //Обратное распространение ошибки
    void BackPropagation()
    {

    }
    //Запуск нейронной сети
    public void StartPredict()
    {
        StopAllCoroutines();

        foreach(LineRenderer sinapsis in Sinopsises.Values)
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

        SaveBackupWeights();

        InitWeights();
        GenerateRandWeights(1, -1);

        SaveWeights();

        StartCoroutine(ForwardPropagationDelayed());
    }

    private void Start()
    {
        weightsFilePath = $"{Application.dataPath}/NeuralNetworkData/weights.txt";
        weightsBackupFilePath = $"{Application.dataPath}/NeuralNetworkData/weightsBackup.txt";
    }
}
