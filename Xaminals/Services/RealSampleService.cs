using System;
using System.Collections.Generic;
using System.Text;

namespace Xaminals.Services
{
    public class RealSampleService : ISampleService
    {
        public int Add(int num1, int num2) => num1 + num2;
    }
}
