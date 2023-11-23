using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

Console.WriteLine("Hello CODECON Žilina");

#region Time provider

// Príklad máš v projekte dotnet8.tests/BasketShould.cs

#endregion

#region Randomness

int[] numbers = [ 1, 2, 3, 4, 5 ];
Random.Shared.Shuffle(numbers);
Console.WriteLine(string.Join(", ", numbers));

string[] fruitsArray = ["Apple", "Banana", "Cherry", "Date"];
string[] selectedFruits = Random.Shared.GetItems(fruitsArray, 2);
Console.WriteLine(string.Join(", ", selectedFruits));

#endregion

#region Reflection

// Old way
//var person = new Person();
//typeof(Person)
//    .GetField("_age", BindingFlags.Instance | BindingFlags.NonPublic)!
//    .SetValue(person, 100);

// New way
var person = new Person();
GetAgeField(person) = 100;

[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_age")]
static extern ref int GetAgeField(Person person);

//| Method   | Mean        | Error      | StdDev     | Gen0    | Allocated  |
//| -------- | -----------:| ----------:| ----------:| -------:| ----------:|
//| DotNet7  | 32.6988 ns  | 0.5648 ns  | 0.5283 ns  | 0.0029  | 24 B       |
//| DotNet8  | 0.0725 ns   | 0.0058 ns  | 0.0051 ns  | -       | -          |

public class Person
{
    private readonly int _age = 0;

    public int Age => _age;
}

#endregion

#region Data annotations

public class Product
{
    [Length(minimumLength: 4, maximumLength: 100)]
    public string Name { get; set; }

    [DeniedValues(0)]
    public decimal Price { get; set; }

    [Range(1, 100, MinimumIsExclusive = true, MaximumIsExclusive = true)]
    public int StockQuantity { get; set; }

    [Base64String]
    public string ImageBase64 { get; set; }

    [AllowedValues("Electronics", "Clothing", "Food", "Books")]
    public string Category { get; set; }

    [AllowedValues(0.10d, 0.15d, 0.20d)]
    public double VatRate { get; set; }

    [DeniedValues("Transparent", "Invisible")]
    public string Color { get; set; }

    [Length(minimumLength: 1, maximumLength: 5)]
    public List<int> RelatedProductIds { get; set; }
}

#endregion