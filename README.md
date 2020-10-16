# CinemaDwellers

# Offline

## Gurobi
Before proceeding you first need to install Gurobi: https://www.gurobi.com/documentation/9.0/quickstart_mac/software_installation_guid.html

You can use the academic license so you do not have to pay ;)

## Algorithm

The offline algorithm is implemented as a C# solution (Offline.cs) and a Python solution (Offline.py). 

## C#

### Parameters:

This program supports two modes:

1. Instance mode: Run a single instance and output the result to the console.
2. Experiment mode: Run a predifined experiment(s) and output the results to .csv file.

To tell the program what to run you can use arguements. Here is a breakdown of all the supported arguements:

| Parameter | Arguement | Description                                                                                                             |
|-----------|-----------|-------------------------------------------------------------------------------------------------------------------------|
| -d        | None      | Run the program in debug mode                                                                                           |
| -m        | String    | Mode to run the program in. Options: `Instance` \| `Experiments`                                                            |
| -e        | Integer   | The `Id` of the experiment to run. Can be repeated multiple times to run multiple experiments consecutively.              |
| -i        | String    | Path to instance file for Instance mode OR Path to instances folder containing `Exact{i}.txt` files for Experiments mode. |
| -c        | String    | Path to .prm file for Instance mode OR Path to configs folder containing `tune_Exact{i}.prm` files for Experiments mode.  |
| -ilpo     | None      | Solve the instance using only the ILP solver.(Instance mode only)                                                      |
| -go       | None      | Solve the instance using only the Greedy solver. (Instance mode only)                                                   |
| -t        | None      | Tune the model using the Gurobi tuning tool. Will only write the best one. (`Instance` mode only)                                              |
| -to       | String    | Output file where the tune parameters are written too. (`Instance` mode only)                                                                  |

### Experiments:

For convinience we have implemented 5 experiments. All results are written as a .csv to the folder called `results` in the same directory as the executable:

`1`: `Tuning vs No Tuning`: Run each test case with a tune file and then without a tune file.

`2`:  `Greedy vs ILP Solver`: Run each test case with the Greedy solver and then with the ILP solver.

`3`: `Tuning`: Tune all test cases and write the output to folder called `tune` in the same directory as the executable.

`4`: `ILP Solver`: Run all test cases using the ILP solver and configurations.

`5`: `Try Greedy First`: Run all test cases using the Greedy solver first and if the result is not optimal, run the ILP solver.

### Examples

1. Run a single instance using greedy first and then ILP solver.

```
./Offline.exe -d -m Instance -c ./example_config.prm -i ./example_instance.txt
```

2. Run a single instance using greedy only.

```
./Offline.exe -d -m Instance -c ./example_config.prm -i ./example_instance.txt -go
```

3. Run a single instance using ILP solver only.

```
./Offline.exe -d -m Instance -c ./example_config.prm -i ./example_instance.txt -ilpo
```

4. Tune a single instance.
```
./Offline.exe -d -m Instance -i ./example_instance.txt -t -to ./example_tune.prm
```

5. Run a single experiment.
```
./Offline.exe -d -m Experiments -i ./test_instances/ -c ./test_configs -e 1
```

6. Run multiple experiments.
```
./Offline.exe -d -m Experiments -i ./test_instances/ -c ./test_configs -e 1 -e 2 -e 3 -e 4 -e 5
```

## How to run it in visual studio

[Here](https://dailydotnettips.com/how-to-pass-command-line-arguments-using-visual-studio/) is a great tutorial on how to configure environment variables in visual studio :).

## Python

Current dependencies: `numpy>1.0` and `gurobipy`. Gurobi also needs a (free academic) license!

Also: probably use python3.8, the nicest one :) 

How to use greedy algorithm?

Run 

```main.py [--filename instances/myinstance.txt ]```

When no filename is provided, default instance from blackboard is used.

How to use the ILP solver?

Run

```ilp.py [--filename instances/mini.txt]```

