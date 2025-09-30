using GeneticSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceGA
{
    /// <summary>
    /// Genetic Algorithmを実装するクラス
    /// </summary>
    public class GA(IReadOnlyList<City> cities)
    {
        /// <summary>
        /// 都市リスト
        /// </summary>
        IReadOnlyList<City> m_CityList = cities;

        public IList<int> Run(int generationCount = 1000, int populationMin = 50, int populationMax = 100 , Action<IList<int>, int>? onGeneration = null, float mutationProbability = 0.08f)
        {
            var fitness = new MyFitness(m_CityList);
            var chromosome = new MyChromosome(m_CityList.Count);
            var population = new Population(populationMin, populationMax, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, new TournamentSelection(), new OrderedCrossover(), new ReverseSequenceMutation())
            {
                Termination = new GenerationNumberTermination(generationCount),
                MutationProbability = mutationProbability
            };
            
            ga.GenerationRan += (sender, e) =>
            {
                var best = ga.BestChromosome as MyChromosome;
                if (best != null)
                {
                    onGeneration?.Invoke(best.GetRoute(), ga.GenerationsNumber);
                }
            };

            ga.Start();

            var best = ga.BestChromosome as MyChromosome;
            return best?.GetRoute() ?? new List<int>();
        }
    }

    public class MyFitness(IReadOnlyList<City> cities) : IFitness
    {

        IReadOnlyList<City> m_CityList = cities;
        public double Evaluate(IChromosome chromosome)
        {
            var genes = chromosome.GetGenes();
            double totalDistance = 0.0;

            for (int i = 0; i < genes.Length - 1; i++)
            {
                var from = m_CityList[(int)genes[i].Value];
                var to = m_CityList[(int)genes[i + 1].Value];
                totalDistance += from.DistanceTo(to);
            }
            // 最後の都市から最初の都市に戻る
            var last = m_CityList[(int)genes[^1].Value];
            var first = m_CityList[(int)genes[0].Value];
            totalDistance += last.DistanceTo(first);

            // 距離が短いほど適応度が高くなるように逆数を返す
            return 1.0 / totalDistance;
        }
    }

    public sealed class MyChromosome : ChromosomeBase
    {
        private readonly int _cityCount;
        private static readonly Random _random = new();

        public MyChromosome(int cityCount) : base(cityCount)
        {
            _cityCount = cityCount;
            var order = Enumerable.Range(0, cityCount).OrderBy(_ => _random.Next()).ToArray();
            for (int i = 0; i < cityCount; i++)
            {
                ReplaceGene(i, new Gene(order[i]));
            }
        }

        public override Gene GenerateGene(int geneIndex)
        {
            // 都市インデックスをランダムに生成
            return new Gene(_random.Next(0, _cityCount));
        }

        public override IChromosome CreateNew()
        {
            return new MyChromosome(_cityCount);
        }

        public IList<int> GetRoute()
        {
            return GetGenes().Select(g => (int)g.Value).ToList();
        }
    }
}
