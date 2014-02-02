using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Question3 
{
    static public void Main()
    {

        AntColony ac = new AntColony(); //initialize ants
        Locations loc = new Locations(); //initialize cities and distances

        double[,] Distances = loc.DistanceCalc();

        //set up parameters
        int population = 10;
        double alpha = 1; //pheromone influence
        double beta = 5; //node influence
        double rho = 0.001; //decay factor, pheromone persistence
        double Q = 2; //using ant density model for online updating

        Console.WriteLine("TSP - Ant Colony\n");
        Console.WriteLine("Population - " + population);
        Console.WriteLine("Alpha - " + alpha);
        Console.WriteLine("Beta - " + beta);
        Console.WriteLine("Rho (Pheromone Persistence) - " + rho);
        Console.WriteLine("Q (State Transition) - " + Q);
        Console.WriteLine(" ");

        //initialize pheromone trails
        Console.WriteLine("Initializing ant trails...\n");
        int[][] ants = ac.Initialize(population, 29);
        double[][] pheromones = ac.InitPheromones(29);

        int[] bestTrail = ac.BestSolution(ants, Distances);

        //initial best solution
        double bestLength = ac.TrailLength(bestTrail, Distances);

        Console.WriteLine("Best Initial trail length: " + bestLength + "\n");

        int iterations = 0;
        int end = 0;

        while (iterations < 2500)
        {
            ac.UpdatePheromones(ants, pheromones, Distances, rho, Q);
            ac.UpdatePheromonesOffline(ants, pheromones, Distances, rho, Q, population);
            ac.UpdateAnts(ants, pheromones, Distances, alpha, beta);

            int[] newBestTrail = ac.BestSolution(ants, Distances);
            double newBestLength = ac.TrailLength(newBestTrail, Distances);
            //Console.WriteLine(newBestLength);
            if (newBestLength < bestLength)
            {
                bestLength = newBestLength;
                bestTrail = newBestTrail;
                Console.WriteLine("New best length found: " + bestLength);
                end = iterations;
            }
            ++iterations;
        }

        Console.WriteLine("\nBest Length Found " + bestLength + " at iteration " + end);
        Console.WriteLine("\nBest Trail Found: ");
        loc.PrintTrail(bestTrail);
        Console.WriteLine();
        //Console.ReadLine();
    }
}

public class AntColony
{
    public int[][] Initialize(int population, int cities)
    {
        int[][] ants = new int[population][];
        Random rand = new Random();

        int initial = rand.Next(0, cities);
        for (int i = 0; i < population; i++)
        {
            ants[i] = RandomTrail(cities, initial, rand);
        }

        /*for (int i = 0; i < ants.Length; i++)
        {
            Console.Write("Ant({0}): ", i);

            for (int j = 0; j < ants[i].Length; j++)
            {
                Console.Write("{0:00}{1:00}", ants[i][j], j == (ants[i].Length - 1) ? "" : " ");
            }
            Console.WriteLine();
        }*/

        return ants;
    }

    public int[] RandomTrail(int cities, int initial, Random rand)
    {
        //Random rand = new Random();
        List<int> list = new List<int>();
        HashSet<int> used = new HashSet<int>();

        //random starting point for all ants
        for (int i = 0; i < cities; i++)
        {
            int val = rand.Next(0, 29);

            while (used.Contains(val))
            {
                val = rand.Next(0, 29);
            }

            list.Add(val);
            used.Add(val);
        }

        return list.ToArray();
    }

    public double[][] InitPheromones(int cities)
    {
        double[][] pheromones = new double[cities][];
        for (int i = 0; i <= 28; i++)
        {
            pheromones[i] = new double[cities];
        }
        for (int i = 0; i < pheromones.Length; i++) {
            for (int j = 0; j < pheromones[i].Length; j++)
            {
                pheromones[i][j] = 0.01; //initializing initial pherome level for all paths
            }
        }

        return pheromones;
    }

    public double TrailLength(int[] trail, double[,] Distances)
    {
        double length = 0;

        for (int i = 0; i < trail.Length - 2; i++)
        {
            length = length + Distances[trail[i], trail[i + 1]];
        }

        //length = length + Distances[0, trail.Length - 1]; //add for roundtrip back to initial city

        return length;
    }

    public int[] BestSolution(int[][] ants, double[,] Distances)
    {
        double bestSolution = TrailLength(ants[0], Distances);

        int bestAnt = 0;

        for(int i = 1; i < ants.Length - 1; i++)
        {
            double currentLength = TrailLength(ants[i], Distances);
            //update best trail solution if one exists
            if (currentLength < bestSolution)
            {
                bestSolution = currentLength;
                bestAnt = i; //ant i is travelling most optimal pat
            }
        }

        int[] best = new int[29];
        best = ants[bestAnt]; 

        return best;
    }

    public void UpdatePheromones(int[][] ants, double[][] pheromones, double[,] Distances,
                                    double rho, double Q)
    {
        for (int i = 0; i < pheromones.Length; i++)
        {
            for (int j = i + 1; j < pheromones[i].Length; j++)
            {
                for (int x = 0; x < ants.Length; x++)
                {
                    double length = TrailLength(ants[x], Distances);
                    double dec = (1 - rho) * pheromones[i][j];
                    double inc = 0;

                    if (Connected(i, j, ants[x]))
                    {
                        inc = Q / length;
                    }

                    pheromones[i][j] = dec + inc;

                    if (pheromones[i][j] < 0.0001)
                    {
                        pheromones[i][j] = 0.0001;
                    }
                    else if (pheromones[i][j] > 100000.0)
                    {
                        pheromones[i][j] = 100000.0;
                    }

                    pheromones[j][i] = pheromones[i][j];
                }
            }
        }
    }

    public void UpdatePheromonesOffline(int[][] ants, double[][] pheromones, double[,] Distances, double rho, double Q, int population)
    {
        for (int i = 0 ; i < pheromones.Length; i++)
        {
            for (int j = 0; j < pheromones[i].Length; j++)
            {
                for (int x = 0; x < 10; x++)
                {
                    double length = TrailLength(ants[x], Distances);
                    pheromones[i][j] = (1 - rho) + (Q/length) * rho;
                    pheromones[j][i] = pheromones[i][j];
                }
            }
        }   
    }

    public double[] MoveProbability(int ant, int city, bool[] visited, double[][] pheromones, double[,] Distances
                                    , double alpha, double beta)
    {
        double[] probability = new double[29]; //the probably for each edge
        double total = 0;

        //calculate total to determine different propabilities for each edge
        for (int i = 0; i < probability.Length; i++)
        {
            if (i == city || visited[i] == true)
            {
                probability[i] = 0; // to avoid staying in same city or going to an already visited city 
            }
            else
            {
                //edit this tau as necessary
                probability[i] = Math.Pow(pheromones[city][i], alpha) * Math.Pow((1.0 / Distances[i, city]), beta);
            }
            total = total + probability[i]; //total of the probablities
        }
        
        double[] actualProb = new double[29];

        for (int i = 0; i < actualProb.Length; i++)
        {
            actualProb[i] = probability[i] / total; 
        }

        return actualProb;
    }

    public void UpdateAnts(int[][] ants, double[][] pheromones, double[,] Distances, double alpha, double beta)
    {
        for (int k = 0; k < ants.Length; ++k)
        {
            int start = ants[k][0];
            //int start = rand.Next(0, 29);
            int[] newTrail = NewTrail(k, start, pheromones, Distances, alpha, beta);
            ants[k] = newTrail;
        }
    }

    public int SelectNext(int ant, int city, bool[] visited, double[][] pheromones, double[,] Distances
                            , double alpha, double beta)
    {
        double[] probability = MoveProbability(ant, city, visited, pheromones, Distances, alpha, beta);

        var weighted = new Double[probability.Length + 1];

        for(int i = 0; i < probability.Length; i++)
        {
            weighted[i + 1] = weighted[i] + probability[i];
        }
        
        weighted[weighted.Length - 1] = Math.Round(weighted[weighted.Length - 1]);

        Random rand = new Random();
        double randomProb = rand.NextDouble(); //to randomly select a probability value
        int nextCity = 100; 

        for (int j = 0; j < weighted.Length - 1; j++)
        {
            if (randomProb >= weighted[j] && randomProb < weighted[j + 1]) //check where random value is
            {
                //nextCity = j;
                return j;
            }         
        }

        return nextCity;
    }

    public int[] NewTrail(int ant, int startnode, double[][] pheromones, double[,] Distances, double alpha, double beta)
    {
        int numCities = 29;
        int[] trail = new int[numCities];
        bool[] visited = new bool[numCities];

        trail[0] = startnode; //first city has already been determined
        //Console.WriteLine(startnode);
        visited[startnode] = true;

        for (int i = 0; i < numCities - 1; i++)
        {
            int nextCity = trail[i];
            //Console.WriteLine(nextCity);
            int next = SelectNext(ant, nextCity, visited, pheromones, Distances, alpha, beta);
            //Console.WriteLine(next);
            trail[i + 1] = next;
            //Console.WriteLine(i);
            visited[next] = true; 
        }

        //Console.WriteLine("Starting node " + trail[0]);
        return trail;
    }

    public bool Connected(int cityOne, int cityTwo, int[] trail)
    {
        int lastIndex = trail.Length - 1;
        int indexOne = ArrayIndex(trail, cityOne); //index of one of the cities
        int indexTwo = ArrayIndex(trail, cityTwo); //index of the other city

        if (indexOne + 1 == indexTwo)
        {
            return true;
        }
        else if (indexOne - 1 == indexTwo)
        {
            return true;
        }
        else if (indexOne == 0 && indexTwo == lastIndex)
        {
            return true;
        }
        else if (indexTwo == 0 && indexOne == lastIndex)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int ArrayIndex(int[] array, int value) // helper for RandomTrail
    {
        int index = 0;
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i] == value)
                index = i;
        }

        return index;
    }
}

public class Locations
{
    public int[,] Coordinates = new int[,] {
        {1150, 1760}, 
        {630, 1660},
        {40, 2090},
        {750, 1100},
        {750, 2030},
        {1030, 2070},
        {1650, 650},
        {1490, 1630},
        {790, 2260},
        {710, 1310},
        {840, 550},
        {1170, 2300},
        {970, 1340},
        {510, 700},
        {750, 900},
        {1280, 1200},
        {230, 590},
        {460, 860},
        {1040, 950},
        {590, 1390},
        {830, 1770},    
        {490, 500},
        {1840, 1240},
        {1260, 1500},
        {1280, 790},
        {490, 2130},
        {1460, 1420},
        {1260, 1910},
        {360, 1980}
    };

    public double[,] DistanceCalc() 
    {
        double [,] Distances = new double[29,29];

        for (int i = 0; i < Distances.GetLength(0); i++)
        {
            Distances[i,i] = 0; //no distance traveling between the cities

            for (int j = 0; j < Distances.GetLength(1); j++)
            {
                Distances[i, j] = Uclidean(Coordinates[i, 0] - Coordinates[j, 0], Coordinates[i, 1] - Coordinates[j, 1]);
                Distances[j, i] = Distances[i, j];
            }
        }

        return Distances;
    }

    public double Uclidean(int xdif, int ydif)
    {
        double distance = 0;

        distance = Math.Sqrt(Math.Pow(xdif, 2) + Math.Pow(ydif, 2));

        return Math.Round(distance, 3);
    }

    public void PrintArray(double[,] a)
    {
        int height = a.GetLength(0);
        int width = a.GetLength(1);
        for (int i=0; i < height; i++)
        {
            for (int j=0; j < width; j++)
            {
                //Console.Write("{0} | ", a[i,j]);
                Console.Write("{0} | ", String.Format("{0:0000}", a[i,j]));
            }
            Console.WriteLine("");
        }
    }

    public void PrintTrail(int[] trail)
    {
        for (int i = 0; i < trail.Length; i++)
        {
            Console.Write("{0} | ", String.Format("{0:00}", trail[i]));
        }
        Console.WriteLine(" ");
    }
}