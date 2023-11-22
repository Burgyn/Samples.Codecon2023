using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Grade = (decimal score, string grade);

Console.WriteLine("Hello CODECON Žilina");

#region Collection expressions

// Old way

//int[] ages = new int[] { 24, 34, 56 };

//List<string> names = new List<string> { "John", "Mary", "Bob" };

//int[][] points = new int[][]
//{
//    new int[] {1,2},
//    new int[] {3,4},
//    new int[] {5,6}
//};

//List<(string name, decimal[] score)> scores = new List<(string name, decimal[] score)>()
//{
//    ("John", new decimal[] {99M, 82.5M }),
//    ("Mary", new decimal[]{88M, 90M, 91M }),
//    ("Bob", new decimal[] {100M, 98M, 99M })
//};

// New way
//int[] ages = [24, 34, 56];

//List<string> names = ["John", "Mary", "Bob"];

//int[][] points = [[1, 2], [3, 4], [5, 6]];

//List<(string name, decimal[] score)> scores =
//[
//    ("John", [99M, 82.5M]),
//    ("Mary", [88M, 90M, 91M]),
//    ("Bob", [100M, 98M, 99M])
//];

//Print([3, 5, 7, 8]);

//static void Print(IEnumerable<int> ages)
//{
//    // implementation
//}

//List<int> list = [2, 3];
//Span<int> span = [6, 7];

//IEnumerable<int> allNumbers = [1, .. list, 4, 5, .. span];

//foreach (int age in (int[])[1, 2, 3, .. ages])
//{
//    Console.WriteLine(age);
//}

//MyCollection<int> myCollection = [1, 2, 3, 4, 5];

//[CollectionBuilder(typeof(MyCollection), "Create")]
//public class MyCollection<T> : IEnumerable<T>
//{
//    private readonly ImmutableArray<T> _items;

//    public MyCollection(ReadOnlySpan<T> items)
//    {
//        _items = items.ToImmutableArray();
//    }

//    public IEnumerator<T> GetEnumerator()
//    {
//        foreach (var item in _items)
//        {
//            yield return item;
//        }
//    }

//    IEnumerator IEnumerable.GetEnumerator()
//    {
//        return GetEnumerator();
//    }
//}

//public static class MyCollection
//{
//    public static MyCollection<T> Create<T>(ReadOnlySpan<T> items)
//        => new(items);
//}

#endregion

#region Primary constructor

// Old way
//public class Person
//{
//    public string FirstName { get; }
//
//    public string LastName { get; }
//
//    public int Age { get; }
//
//    public Person(string firstName, string lastName, int age)
//    {
//        FirstName = firstName;
//        LastName = lastName;
//        Age = age;
//    }
//}

// New way
//public class Person(string firstName, string lastName, int age)
//{
//    public string FirstName { get; } = firstName;
//
//    public string LastName { get; } = lastName;
//
//    public int Age { get; } = age;
//}

//public class Employee(string firstName, string lastName, int age, decimal salary)
//    : Person(firstName, lastName, age)
//{
//    public decimal Salary { get; } = salary;

//    public string? Department { get; }

//    public Employee(string firstName, string lastName, int age, decimal salary, string department)
//        : this(firstName, lastName, age, salary)
//    {
//        Department = department;
//    }
//}

//public class PeopleService(IPeopleRepository repository)
//{
//    public IEnumerable<Person> GetPeople()
//        => repository.GetPeople();

//    //private readonly IPeopleRepository _repository;

//    //public PeopleService(IPeopleRepository repository)
//    //{
//    //    _repository = repository;
//    //}
//}

#endregion

#region Using Alias types

//static Grade Grade(decimal score)
//    => score switch
//    {
//        >= 90M => (score, "A"),
//        >= 80M => (score, "B"),
//        >= 70M => (score, "C"),
//        >= 60M => (score, "D"),
//        _ => (score, "F")
//    };

//Grade grade1 = Grade(95M);
//var grade2 = new Grade(91, "A");

//static Task<IEnumerable<(decimal scode, string grade)>> GetGrades()
//    => Task.FromResult(Enumerable.Empty<(decimal score, string grade)>());

#endregion