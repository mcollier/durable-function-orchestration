using System;

namespace DurableFunctionOrchestration.Activities;
public class HotelFunctionException: Exception
{
    public HotelFunctionException(string msg): base(msg) { }
}
