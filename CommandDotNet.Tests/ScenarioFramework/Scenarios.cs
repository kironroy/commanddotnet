using System.Collections;
using System.Collections.Generic;

namespace CommandDotNet.Tests.ScenarioFramework
{
    public class Scenarios : IEnumerable<IScenario>
    {
        private List<IScenario> _scenarios = new List<IScenario>();

        public void Add(IScenario scenario)
        {
            _scenarios.Add(scenario);
        }

        public IEnumerator<IScenario> GetEnumerator() =>
            _scenarios.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}