# CinemaDwellers

# Online

## Algorithm

The online algorithm is implemented as a C# solution (Online.cs)

## C#

### Experiments:

Our program runs the 18 given test cases by default. If you want to run a different test case, replace the contents of the Main method with the following code snippet:

```csharp
var reader = new StreamReader("file location");
var cinema = readCinema(reader);

var people = readPeople(reader);
var solver = new Solver(cinema, people);

solver.Solve();

Console.WriteLine("Seated " + Solver.countSeated(cinema) + " people out of " + people.Sum() + " total people");
```