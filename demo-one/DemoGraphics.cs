using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace demo_one;

public class DemoGraphics
{
    [KernelFunction]
    public int GetPersonsAge([Description("Name of the person")]string name) 
    {
        return name switch
        {
            "Eduward" => 30,
            "John" => 30,
            "Jane" => 28,
            _ => 0
        };
    }

}