using E2ETesting.Steps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace E2ETesting.Hooks
{
    [Binding]
    public class HooksSetup : BaseSteps
    {
        public HooksSetup(ScenarioContext scenarioContext) : base(scenarioContext) { }

        public static IPage pageObject;

        [BeforeScenario]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 200 });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
            pageObject = _page;
           

            // Om taggen 'AdminLogin' finns, logga in
            if (_scenarioContext.ScenarioInfo.Tags.Contains("AdminLogin"))
            {
                await _page.GotoAsync("https://localhost:7295/Identity/Account/Login");
                await _page.FillAsync("input[name='Input.Email']", "peter.bergqvist@edu.newton.se");
                await _page.FillAsync("input[name='Input.Password']", "Peter_123");
                await _page.ClickAsync("#login-submit");
                await _page.WaitForURLAsync("**/Movies");
            }
        }
       
        [AfterScenario]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}
