using E2ETesting.Steps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit;

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
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 600 });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
            pageObject = _page;


            // Om taggen 'AdminLogin' finns, logga in och gå till "Movies" sidan
            if (_scenarioContext.ScenarioInfo.Tags.Contains("AdminLogin"))
            {
                await _page.GotoAsync("https://localhost:7295/Identity/Account/Login");
                await _page.FillAsync("input[name='Input.Email']", "peter.bergqvist@edu.newton.se");
                await _page.FillAsync("input[name='Input.Password']", "Peter_123");
                await _page.ClickAsync("#login-submit");
                await _page.WaitForURLAsync("**/Movies");
            }

            // Om taggen 'DeleteReviewAdmin' finns, logga in och lägg till en review
            if (_scenarioContext.ScenarioInfo.Tags.Contains("DeleteReviewAdmin"))
            {
                await _page.GotoAsync("https://localhost:7295/Identity/Account/Login");
                await _page.FillAsync("input[name='Input.Email']", "peter.bergqvist@edu.newton.se");
                await _page.FillAsync("input[name='Input.Password']", "Peter_123");
                await _page.ClickAsync("#login-submit");
                await _page.WaitForURLAsync("**/Movies");
                await _page.FillAsync("#search-input", "Kopps");
                await _page.ClickAsync("#search-btn");
                await _page.ClickAsync("article.search-result-movie-row");
                await _page.ClickAsync("label.review-input-star[for='star1']");
                await _page.FillAsync("#Review_Title", "Världens sämsta film");
                await _page.FillAsync("#Review_Text", "Slösa inte med er tid!");
                _page.Dialog += async (_, dialog) =>
                {
                    Console.WriteLine("Dialog type: " + dialog.Type);
                    if (dialog.Type == "confirm")
                    {
                        await dialog.AcceptAsync(); // Tryck på "OK" för att acceptera dialogen
                    }
                };
                await _page.ClickAsync("button.btn-design.btn-black[title='Publicera recension']");

            }

            // Om taggen 'AdminReviewFilter' finns, logga in
            if (_scenarioContext.ScenarioInfo.Tags.Contains("AdminReviewFilter"))
            {
                await _page.GotoAsync("https://localhost:7295/Identity/Account/Login");
                await _page.FillAsync("input[name='Input.Email']", "peter.bergqvist@edu.newton.se");
                await _page.FillAsync("input[name='Input.Password']", "Peter_123");
                await _page.ClickAsync("#login-submit");
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
