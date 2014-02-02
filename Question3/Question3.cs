// ECE457A Assignment 2 Question 3
// Submitted: July 5, 2013
// BY: Swapan Shah & Xiaotong Zhang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Question3 
{
	static public void Main() 
	{
		TabuSearch tabu = new TabuSearch();

		//set up the distance and flow arrays
		int[,] Distances = tabu.GetDistance();
		int[,] Flow = tabu.GetFlow();
		int[,] TabuList = new int[20,20];
		int[,] Frequency = new int[20,20];
		
		int TabuSize = 10;
		int RandomSolutions = 2;
		bool Dynamictabu = false;
		bool Aspiration = false;
		bool Partial = false;
		bool Diversify = false;

		//set up the menu and different variations to the code
		char menu = tabu.Menu();
		switch (menu)
		{
			case '0':
				break;
			case '1':
				RandomSolutions = 5;;
				break;
			case '2':
				TabuSize = 8;
				break;
			case '3':
				TabuSize = 12;
				break;
			case '4':
				Dynamictabu = true;
				break;
			case '5':
				Aspiration = true;
				break;
			case '6':
				Partial = true;
				break;
			case '7': 
				Diversify = true;
				break;
			default:
                Console.WriteLine("Invalid choice. Please enter again.");
                break;
		}

		int i = 0;
		int[] solution = new int[20];
		int Cost = 0;

		for (int j = 1; j < RandomSolutions; j++)
		{
			//random initial solution
			solution = tabu.UniqueRandomGenerator(1, 20);
			Console.WriteLine("Initial Solution: " + j);
			
			/*int[] solution = new int[20]
			{
				//12 ,01, 15, 04,09,11,13,08,03,17,02,18,20,06,14,10,07,05,16, 19
				//6, 1, 7, 5, 17, 13, 8, 20, 15, 19, 16, 11, 12, 2, 4, 9, 3, 10, 14, 18 
				//optimal solution
			};*/

			tabu.PrintSolution(solution);

			//determine suitable search and modify solution
			//determine new cost and print
			int[,] ModifiedFlow = tabu.ModifyFlow(solution, Flow);
			Cost = tabu.Cost(Distances, ModifiedFlow);
			Console.WriteLine("Total Cost " + Cost);
			int LowestCostSoFar = Cost;
			//solution = tabu.UniqueRandomGenerator(1, 20);
			//Console.WriteLine("Initial Solution: " + (j+1));

			while (Cost > 1285)
			//for (int i = 0; i < 5; i++)
			{
				if (i > 10000)
				{
					break; //don't go beyond 2500 iterations
				}

				if (Dynamictabu) //to randomize tabu size
				{
					if (i % 50 == 0)
					{
						Random r = new Random();
						TabuSize = r.Next(10, 15);
						Console.WriteLine("Random Tabu Size: " + TabuSize);
					}
				}

				solution = tabu.NewSolution(solution, Cost, Flow, Distances, TabuList, LowestCostSoFar, TabuSize, Aspiration, Partial, Frequency, Diversify);
				ModifiedFlow = tabu.ModifyFlow(solution, Flow);
				Cost = tabu.Cost(Distances, ModifiedFlow);

				if (Cost < LowestCostSoFar)
				{
					LowestCostSoFar = Cost;
					//Console.WriteLine("Lowest Cost So Far " + LowestCostSoFar);
				}
				//LowestCostSoFar = tabu.Cost(Distances, ModifiedFlow);
				//Console.WriteLine("Lowest Cost So Far " + LowestCostSoFar);

				++i;
			}
		

			Console.WriteLine("Final Solution: ");
			tabu.PrintSolution(solution);
			Console.WriteLine("Final Cost: " + Cost);
			if (Cost > 1285)
			{
				Console.WriteLine("Note: This solution is not optimal (within 10,000 iterations).");
			}
			Console.WriteLine("Total # of Iterations: " + (i-1));
			Console.WriteLine("END SEARCH");
			Console.WriteLine(" ");
		}
	}

}

public class TabuSearch
{
	public char Menu()
    {
        Console.WriteLine("1. 10 Random Starting Solutions");
        Console.WriteLine("2. Smaller Tabu List Size");
        Console.WriteLine("3. Larger Tabu List Size");
        Console.WriteLine("4. Dynamic Tabu List Size");
        Console.WriteLine("5. Aspiration Enabled");
        Console.WriteLine("6. Partial Neighborhood");
        Console.WriteLine("7. Frequency Based Tabu List");
        Console.WriteLine("0. Nothing");
        Console.Write("\nEnter your choice: "); 
        char ch = Convert.ToChar(Console.ReadLine()); 
      
        return ch;
    }

	public int[] NewSolution(int[] solution, int Cost, int[,] Flow, int[,] Distances, int[,] TabuList, int LowestCostSoFar, 
								int TabuSize, bool Aspiration, bool Partial, int[,] Frequency, bool Diversify)
	{
		TabuSearch tabu = new TabuSearch();
		//int[] UpdatedSolution = new int[20];
		int NewCost = int.MinValue;
		int PreviousCost = Cost;
		
		int[] NewSolution;
		
		while (NewCost < PreviousCost)
		{
			//Console.WriteLine("Iteration: " + iteration);
			NewSolution = tabu.PossibleMoves(solution, Distances, Flow, PreviousCost, TabuList, LowestCostSoFar, Aspiration, Partial, Frequency, Diversify);
			if (NewSolution == null)
			{
				return solution;
			}
			else 
			{
				TabuList = tabu.UpdateTabu(NewSolution[2], NewSolution[3], NewSolution[0], NewSolution[1], TabuList, TabuSize);
				Frequency = tabu.UpdateFrequency(NewSolution[0], NewSolution[1], Frequency);
				//tabu.PrintSolution(NewSolution);

				//Console.WriteLine("Tabu List");
				//tabu.PrintArray(TabuList);

				//Console.WriteLine("New Solution");
				solution = tabu.GetNewSolution(solution, NewSolution[0], NewSolution[1]);
				int[,] ModifiedFlow = tabu.ModifyFlow(solution,Flow);
				PreviousCost = NewCost;
				NewCost = tabu.Cost(Distances,ModifiedFlow);	
				//Console.WriteLine("New Cost: " + NewCost);

				if (NewCost < LowestCostSoFar)
				{
					LowestCostSoFar = NewCost;
				}
			}
			//tabu.PrintArray(TabuList);
		}

		/*NOTES:
		modify possible move function to return array of the best move swaps
		check tabulist and update accordingly
		then do the rest so that everything is all connected... */

		return solution;
	}

	public void PrintArray(int[,] a)
	{
		int height = a.GetLength(0);
		int width = a.GetLength(1);
		for (int i=0; i < height; i++)
		{
			for (int j=0; j < width; j++)
			{
				//Console.Write("{0} | ", a[i,j]);
				Console.Write("{0} | ", String.Format("{0:00}", a[i,j]));
			}
			Console.WriteLine("");
		}
	}

	public void PrintSolution(int[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			Console.Write("{0} | ", String.Format("{0:00}", s[i]));
		}
		Console.WriteLine("");
	}

	public int[,] GetDistance() 
	{
		string fullpath = Environment.CurrentDirectory + @"/" + "distance.csv";
		StreamReader reader = new StreamReader(fullpath);
		string line;
		int lineCount = 0;
		int[,] distances = new int[20,20];

		while ((line = reader.ReadLine()) != null)
		{
			//Console.Write(line);
			string[] values = line.Split(',');
			//Console.WriteLine("");

			for (int i = 0; i<values.Length; i++)
			{
				distances[lineCount,i] = int.Parse(values[i]);
			}

			++lineCount;
		}

		return distances;
	}

	public int[,] GetFlow()
	{
		string fullpath = Environment.CurrentDirectory + @"/" + "flow.csv";
		StreamReader reader = new StreamReader(fullpath);
		string line;
		int lineCount = 0;
		int[,] flow = new int[20,20];

		while ((line = reader.ReadLine()) != null)
		{
			//Console.Write(line);
			string[] values = line.Split(',');
			//Console.WriteLine("");

			for (int i = 0; i<values.Length; i++)
			{
				flow[lineCount,i] = int.Parse(values[i]);
			}

			++lineCount;
		}

		return flow;
	}

	public int[] PossibleMoves(int[] solution, int[,] Distance, int[,] Flow, int Cost, int[,] TabuList, int LowestCostSoFar,
									bool Aspiration, bool Partial, int[,] Frequency, bool Diversify)
	{	
		TabuSearch ts = new TabuSearch();

		int[,] ModifiedFlow = new int[20,20];	

		//swapping is by location (1,2)(1,3)... everytime 
		//update and keep track of lowest option

		int[] NewSolution;
		int[] FinalSolution = new int[4];
		int LowestIterationCost = 3000;
		int NewCost = 0;
		int FinalSolutionCost = Cost;
		int x_pos = 0;
		int y_pos = 0;

		int TempCost = 0;
		ModifiedFlow = ts.ModifyFlow(solution, Flow);
		TempCost = ts.Cost(Distance, ModifiedFlow);

		int interval = 1;
		if (Partial == true)
		{
			interval = 2;
		}

		for (int i = 0; i < solution.Length; i++)
		{
			for (int j = 0; j < solution.Length; j+=interval)
			{	
				if (i < j)
				{

					//int temp = NewSolution[i];
					//NewSolution[i] = NewSolution[j];
					//NewSolution[j] = temp;
					NewSolution = ts.GetNewSolution(solution, i, j);
					//Console.WriteLine("{0},{1} -> ", i, j);
					ModifiedFlow = ModifyFlow(NewSolution, Flow);

					//check if tabu or not
					if (TabuList[NewSolution[i] - 1, i] == 0 && TabuList[NewSolution[j] - 1, j] == 0)
					{
						NewCost = ts.Cost(Distance, ModifiedFlow);
						//Console.WriteLine("Cost: " + NewCost);

						if (Diversify == true)
						{
							NewCost = NewCost + Frequency[i,j];
						}

						//if ((NewCost < LowestIterationCost && TabuList[NewSolution[i] - 1, i] == 0 && TabuList[NewSolution[j] - 1, j] == 0))
						if (NewCost < LowestIterationCost)
						{
							//Console.WriteLine(TabuList[NewSolution[i] - 1, i]);
							//Console.WriteLine(TabuList[NewSolution[j] - 1, j]);
							FinalSolutionCost = NewCost;
							LowestIterationCost = NewCost;
							//Console.WriteLine("Lowest Iteration Cost " + LowestIterationCost);
							//FinalSolution = NewSolution;
							x_pos = i;
							y_pos = j;
							//solution = NewSolution;
							//Console.WriteLine("New Solution ");
							//ts.PrintSolution(solution);
						}
					}

					else if (Aspiration == true)
					{
						if ((TempCost = ts.Cost(Distance, ModifiedFlow) + Frequency[i,j]) < LowestCostSoFar)
						{
							//update Tabu List accordingly 
							//Console.WriteLine(TabuList[NewSolution[i] - 1, i]);
							//Console.WriteLine(TabuList[NewSolution[j] - 1, j]);
							FinalSolutionCost = NewCost;
							LowestIterationCost = NewCost;
							//Console.WriteLine("Lowest Iteration Cost " + LowestIterationCost);
							//FinalSolution = NewSolution;
							x_pos = i;
							y_pos = j;
						}
					}
				}
			}
		}

		//Console.WriteLine("Swap: " + x_pos + "," + y_pos);
		//Console.WriteLine("Lowest Cost " + FinalSolutionCost);

		//Console.WriteLine(solution[x_pos] + ", " + x_pos);
		//Console.WriteLine(solution[y_pos] + ", " + y_pos);

		FinalSolution[0] = x_pos;
		FinalSolution[1] = y_pos;
		FinalSolution[2] = solution[x_pos] - 1;
		FinalSolution[3] = solution[y_pos] - 1;

		return FinalSolution;

		//change to return an array of necessary updates to the solution
		//use main function to do actual swapping of the solution and updating of the tabulist 

		//need to return TabuList as well somehow... 
	}
	
	public int[] GetNewSolution(int[] array, int position1, int position2)
	{
		int[] newArray = (int[])array.Clone();
		
		int temp = newArray[position1];
		newArray[position1] = newArray[position2];
		newArray[position2] = temp;
		
		return newArray; 
	}

	public int[,] ModifyFlow(int[] solution, int[,] Flow)
	{
		int[,] ModifiedRow = new int[20,20];
		int[,] ModifiedCol = new int[20,20];

		for(int i = 0; i < solution.Length; i++) 
		{
			int row = solution[i] - 1;
			//Console.WriteLine("i " + i);
			for (int j= 0; j < Flow.GetLength(0); j++)
			{
				//Console.WriteLine("j " + j);
				//Console.WriteLine("row " + row);
				ModifiedRow[i,j] = Flow[row,j];  
			}
		}

		for(int x = 0; x < solution.Length; x++)
		{
			int column = solution[x] - 1;
			for (int y = 0; y < Flow.GetLength(1); y++)
			{
				ModifiedCol[y,x] = ModifiedRow[y,column];
			}
		}

		return ModifiedCol;
	}

	public int Cost(int[,] Distance, int[,] Flow)
	{
		int Cost = 0;

		for (int i = 0; i < Distance.GetLength(0); i++)
		{
			for (int j = 0; j < Distance.GetLength(1); j++)
			{
				Cost = Cost + (Distance[i,j] * Flow[i,j]);
			}
		}

		return Cost/2;
	} 

	public int[,] UpdateTabu(int x, int y, int x_index, int y_index, int[,] TabuList, int TabuSize)
	{
		TabuSearch ts = new TabuSearch();

		for (int i = 0; i < TabuList.GetLength(0); i++)
		{
			for (int j = 0; j < TabuList.GetLength(1); j++)
			{
				//TabuList[x,x_index]--;
				//TabuList[y,y_index]--;
				if (TabuList[i,j] != 0)
				{
					int Value = TabuList[i,j];
					TabuList[i,j] = Value - 1;
				}
			}
		}

		TabuList[x,x_index] = TabuSize;
		TabuList[y,y_index] = TabuSize;

		return TabuList;
	}

	public int[,] UpdateFrequency(int x, int y, int[,] Frequency)
	{
		Frequency[x,y]++;

		return Frequency;
	}

	public int[] UniqueRandomGenerator(int minVal, int maxVal)
	{
	    Random rand = new Random();
	    SortedList<int, int> uniqueList = new SortedList<int, int>();
	    for (int i = minVal; i <= maxVal; i++) 
	    {
            uniqueList.Add(rand.Next(), i);
		}

	    return uniqueList.Values.ToArray();
	}

	public int RandomInteger(int lowerbound, int[] integers)
	{
		Random rand = new Random();
		int x = rand.Next(lowerbound, 19);

	    //Seeks if Number is Repeated
	    if (integers.Contains(x))
	    {
	        //If So, Recursion Takes Place
	        RandomInteger(lowerbound, integers);
	    }

	    //Returns Value to Original Loop to be Assigned to Array
	    return x;
	}

}