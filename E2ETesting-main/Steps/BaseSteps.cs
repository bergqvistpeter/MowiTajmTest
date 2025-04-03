using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace E2ETesting.Steps
{
    public class BaseSteps
    {
        public IPlaywright _playwright;
        public IBrowser _browser;
        public IBrowserContext _context;
        public IPage _page;
        public readonly ScenarioContext _scenarioContext;

       


        public BaseSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        public BaseSteps() { }
    }
}
